using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Mods;
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
        var staticGrid = world.State.StaticSpatialBuildings;

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
                int count = dynGrid.QueryNodesBoundedClosestLayersFiltered(
                    pos.position.X,
                    pos.position.Y,
                    ranged.Range,
                    team.TeamId,
                    neighbors
                );
                Entity targetEntity = default;
                bool existTarget = false;
                
                Vector2 targetPos = Vector2.Zero;

                for (int ii = 0; ii < count; ii++)
                {
                    var neighborId = neighbors[ii];
                    if (spatial.Value == neighborId) continue;

                    targetEntity = dynGrid.GetEntity(neighborId);

                    // 🔥 1. Verifica que esté viva ANTES de extraer componentes
                    if (!targetEntity.IsAlive() || targetEntity.Has<DeadTag>()) continue;

                    //// 🔥 2. Ahora es seguro consultar sus componentes
                    //var otherTeam = targetEntity.Get<TeamComponent>();
                    //if (team.TeamId == otherTeam.TeamId) continue;

                    // 🔥 3. Validar si el objetivo está realmente dentro del rango de ataque
                    // (Asumiendo que PositionComponent tiene un campo 'position' de tipo Vector2 o Vector3)
                     targetPos = targetEntity.Get<PositionComponent>().position;

                    float rangeSqr = ranged.Range * ranged.Range;
                    float distSqr = pos.position.DistanceSquaredTo(targetPos);

                    if (distSqr > rangeSqr) continue; // Fuera de rango, pasamos al siguiente vecino
                    existTarget = true;
                    float impactThereshold = targetEntity.Get<MoveColliderComponent>().Radius;
                    e.Set(new AttackPendingComponent(true, targetEntity,true, targetPos,impactThereshold));
                    e.Add<AttackPendingTag>();
                    break;
                }

                if (!existTarget)
                {
                    int radius = (int)MathF.Ceiling((ranged.Range * 2f) / staticGrid._cellSize);
                    // aqui buscar edificos
                    foreach (var id in staticGrid.QueryNearbyUnique(pos.position.X, pos.position.Y, radius, team.TeamId))
                    {
                        if (staticGrid.TryGetValue(id, out Entity otherEntity))
                        {
                            if (!otherEntity.IsAlive()) continue;
                            targetEntity = otherEntity;
                            TileSpriteData sprite = null;

                            if (otherEntity.Has<BuildingDefinitionComponent>())
                            {
                                int idTemplate = otherEntity.Get<BuildingDefinitionComponent>().idSpriteTemplateNormal; // 🔹 para asegurar que es una entidad con collider
                                var template = AtlasModsManager.TryGetTileSprite(idTemplate, out sprite);
                            }
                            var posOther = otherEntity.Get<PositionComponent>();

                            targetPos = otherEntity.Get<PositionComponent>().position;
                            float rangeSqr = ranged.Range * ranged.Range;
                            float distSqr = pos.position.DistanceSquaredTo(targetPos);

                            if (distSqr > rangeSqr) continue; // Fuera del rango de melee, buscar siguiente
                            float impactThereshold = 0.5f;
                            e.Set(new AttackPendingComponent(true, targetEntity, false, targetPos,impactThereshold));
                            e.Add<AttackPendingTag>();

                            existTarget = true;
                            break;
                        }
                    }
                }

            }
        }
    }
}