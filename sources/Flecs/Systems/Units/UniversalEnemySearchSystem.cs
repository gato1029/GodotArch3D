using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using GodotFlecs.sources.Flecs.Components;

namespace GodotFlecs.sources.Flecs.Systems.Units;

internal class UniversalEnemySearchSystem : FlecsSystemBase
{
    [ThreadStatic]
    private static StaticSpatialChunkGrid<Entity>.QueryContext _buildingContext;

    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<SpatialIDComponent>()
          .With<TeamComponent>()
          .With<EnemySearchComponent>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>()
          // Solo busca si todavía NO tiene un objetivo activo / pendiente de persecución o ataque
          .Without<AttackPendingTag>()
          .Without<ChaseTargetTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var sim = world.Simulation.Tick;
        if (sim == null || sim.TickCount == 0) return;

        var dynGrid = world.State.DynamicHash;
        var staticGrid = world.State.StaticSpatialBuildings;
        var buildingContext = _buildingContext ??= new StaticSpatialChunkGrid<Entity>.QueryContext();

        var posArray = it.Field<PositionComponent>(0);
        var spatialArray = it.Field<SpatialIDComponent>(1);
        var teamArray = it.Field<TeamComponent>(2);
        var searchArray = it.Field<EnemySearchComponent>(3);

        Span<int> neighbors = stackalloc int[8];
        float deltaTime = sim.FixedDelta;
        
        // Máscara para dividir el staggering en 8 fotogramas (0 al 7)
        int mask = 0x7; 
        int frame = sim.FrameIndex & mask;

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var spatial = ref spatialArray[i];
            ref var team = ref teamArray[i];
            ref var search = ref searchArray[i];
            var e = it.Entity(i);

            // 1. Acumular tiempo del temporizador de búsqueda
            search.Timer += deltaTime;
            if (search.Timer < search.Interval) continue;

            int times = (int)(search.Timer / search.Interval);
            search.Timer -= times * search.Interval;

            // 2. Aplicar Staggering usando el índice secuencial limpio del componente
            int group = search.StaggerIndex & mask;
            if (group != frame) continue;

            // 3. Buscar en unidades dinámicas (grilla espacial)
            int count = dynGrid.QueryNodesBoundedClosestLayersFiltered(
                pos.position.X, pos.position.Y,
                search.Radius, team.TeamId, neighbors
            );

            bool foundTarget = false;
            Entity targetEntity = default;
            Vector2 targetPos = Vector2.Zero;
            float impactThreshold = 0.5f;
            bool isUnit = true;

            for (int ii = 0; ii < count; ii++)
            {
                var neighborId = neighbors[ii];
                int otherSpatialId = dynGrid.GetSpatialID(neighborId);
                if (spatial.Value == otherSpatialId) continue;

                targetEntity = dynGrid.GetEntity(neighborId);
                if (!targetEntity.IsAlive() || targetEntity.Has<DeadTag>()) continue;

                targetPos = targetEntity.Get<PositionComponent>().position;
                float rangeSqr = search.Radius * search.Radius;
                if (pos.position.DistanceSquaredTo(targetPos) > rangeSqr) continue;

                if (targetEntity.Has<MoveColliderComponent>())
                {
                    impactThreshold = targetEntity.Get<MoveColliderComponent>().Radius;
                }

                foundTarget = true;
                isUnit = true;
                break;
            }

            // 4. Si no hay unidades en rango, buscar en edificios estáticos
            if (!foundTarget)
            {
                int cellRadius = (int)MathF.Ceiling((search.Radius * 2f) / staticGrid._cellSize);
                foreach (var id in staticGrid.QueryNearbyUnique(pos.position.X, pos.position.Y, cellRadius, buildingContext, team.TeamId))
                {
                    if (staticGrid.TryGetValue(id, out Entity otherEntity))
                    {
                        if (!otherEntity.IsAlive() || otherEntity.Has<DeadTag>()) continue;

                        targetPos = otherEntity.Get<PositionComponent>().position;
                        float rangeSqr = search.Radius * search.Radius;
                        if (pos.position.DistanceSquaredTo(targetPos) > rangeSqr) continue;

                        targetEntity = otherEntity;
                        foundTarget = true;
                        isUnit = false;
                        break;
                    }
                }
            }

            // 5. Si se encontró un objetivo
            if (foundTarget)
            {
                e.Set(new AttackPendingComponent(true, targetEntity, isUnit, targetPos, impactThreshold));

                if (search.EnableChase)
                {
                    // Unidades móviles: activan persecución si el objetivo está lejos
                    e.Add<ChaseTargetTag>();
                }
                else
                {
                    // Estructuras / Torres defensivas: Atacan directo sin moverse ni perseguir
                    e.Add<AttackPendingTag>();
                    e.Add<StoppedTag>();
                }
            }
        }
    }
}