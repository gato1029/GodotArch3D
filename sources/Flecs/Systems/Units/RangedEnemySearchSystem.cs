using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Components;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Units;

// busca objetivos tanto para unidades o edificios
internal class RangedEnemySearchSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()     
        .With<SpatialIDComponent>()        
        .With<RangedAttackComponent>() // Componente de ataque a distancia
        .With<TeamComponent>()
        .With<AttackPendingComponent>()        
        .Without<PlayerInputComponent>()
        .Without<DeadTag>()
        .Without<DestroyRequestTag>()
        .Without<AttackPendingTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var sim = world.Simulation.Tick;
        if (sim == null || sim.TickCount == 0) return;

        var dynGrid = world.State.DynamicHash;
        var posArray = it.Field<PositionComponent>(0);
        var spatialArray = it.Field<SpatialIDComponent>(1);        
        var rangedArray = it.Field<RangedAttackComponent>(2);
        var teamArray = it.Field<TeamComponent>(3);
        var attackPendArray = it.Field<AttackPendingComponent>(4);
        

        Span<int> neighbors = stackalloc int[8];

        float deltaTime = sim.FixedDelta; // 🔥 Usar solo el delta del tick actual, no el acumulado histórico
        int mask = sim.GetGroupMaskRango();
        int frame = sim.FrameIndex & mask;
        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var spatial = ref spatialArray[i];
            ref var ranged = ref rangedArray[i];
            ref var team = ref teamArray[i];
            ref var atp = ref attackPendArray[i];            

            var e = it.Entity(i);

            // 1. Acumular tiempo siempre para que el reloj avance en tiempo real
            ranged.Timer += deltaTime;

            // 2. Aplicar el staggering ANTES de evaluar o resetear el cooldown
            int group = ranged.NumberUnitRange & mask;
            if (group != frame) continue;

            // 3. Evaluar el cooldown solo en el frame que le toca a este grupo
            if (ranged.Timer >= ranged.Cooldown)
            {
                int times = (int)(ranged.Timer / ranged.Cooldown);
                ranged.Timer -= times * ranged.Cooldown;

                // SOLO UNA QUERY (optimización crítica)
                // solo busca unidades si quiero q luego busque edificios habria q ampliar el query
                int count = dynGrid.QueryNodesBounded(
                    pos.position.X,
                    pos.position.Y,
                    ranged.Range,
                    neighbors
                );

                for (int ii = 0; ii < count; ii++)
                {
                    var neighborId = neighbors[ii];
                    if (spatial.Value == neighborId) continue;

                    var targetEntity = dynGrid.GetEntity(neighborId);

                    // 🔥 1. Verifica que esté viva ANTES de extraer componentes
                    if (!targetEntity.IsAlive() || targetEntity.Has<DeadTag>()) continue;

                    // 🔥 2. Ahora es seguro consultar sus componentes
                    var otherTeam = targetEntity.Get<TeamComponent>();
                    if (team.TeamId == otherTeam.TeamId) continue;

                    // 🔥 3. Validar si el objetivo está realmente dentro del rango de ataque
                    // (Asumiendo que PositionComponent tiene un campo 'position' de tipo Vector2 o Vector3)
                    var targetPos = targetEntity.Get<PositionComponent>();

                    float rangeSqr = ranged.Range * ranged.Range;
                    float distSqr = pos.position.DistanceSquaredTo(targetPos.position);

                    if (distSqr > rangeSqr) continue; // Fuera de rango, pasamos al siguiente vecino

                    e.Set(new AttackPendingComponent(true, targetEntity,true, targetPos.position));
                    e.Add<AttackPendingTag>();
                    break;
                }
            }
        }
    }
}