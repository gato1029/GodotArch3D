using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Components;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Units;
internal class UnitRangedEnemySearchSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()     
        .With<SpatialIDComponent>()
        .With<CharacterComponent>()
        .With<RangedAttackComponent>() // Componente de ataque a distancia
        .With<TeamComponent>()
        .With<AttackPendingComponent>()
        .With<DirectionComponent>()
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
        var charArray = it.Field<CharacterComponent>(2);
        var rangedArray = it.Field<RangedAttackComponent>(3);
        var teamArray = it.Field<TeamComponent>(4);
        var attackPendArray = it.Field<AttackPendingComponent>(5);
        var dirArray = it.Field<DirectionComponent>(6);

        Span<int> neighbors = stackalloc int[8];

        float deltaTime = sim.FixedDelta; // 🔥 Usar solo el delta del tick actual, no el acumulado histórico
        int mask = sim.GetGroupMask();
        int frame = sim.FrameIndex & mask;
        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var spatial = ref spatialArray[i];
            ref var cha = ref charArray[i];
            ref var ranged = ref rangedArray[i];
            ref var team = ref teamArray[i];
            ref var atp = ref attackPendArray[i];
            ref var dir = ref dirArray[i];

            var e = it.Entity(i);

            // 1. Acumular tiempo siempre para que el reloj avance en tiempo real
            ranged.Timer += deltaTime;

            // 2. Aplicar el staggering ANTES de evaluar o resetear el cooldown
            int group = spatial.Value & mask;
            if (group != frame) continue;

            // 3. Evaluar el cooldown solo en el frame que le toca a este grupo
            if (ranged.Timer >= ranged.Cooldown)
            {
                int times = (int)(ranged.Timer / ranged.Cooldown);
                ranged.Timer -= times * ranged.Cooldown;

                // SOLO UNA QUERY (optimización crítica)
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

                    e.Set(new AttackPendingComponent(true, targetEntity));
                    e.Add<AttackPendingTag>();
                    break;
                }
            }
        }
    }
}