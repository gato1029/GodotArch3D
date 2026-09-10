using Arch.Core;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Units;
public class UnitMelleEnemySearchSystem: FlecsSystemBase
{
    // busca enemigos de las unidades que son melle, osea ataque cuerpo a cuerpo
    // encuentra objetivo y automaticamente ya asigna destino y objetivo
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;    
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<SpatialIDComponent>()
          .With<TeamComponent>()
          .With<CharacterComponent>()
          .With<EnemySearchComponent>()
          .With<MoveResolutorComponent>()
          .With<MeleeAttackComponent>()
          .Without<MoveTargetComponent>()
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
        var teamArray = it.Field<TeamComponent>(2);
        var charArray = it.Field<CharacterComponent>(3);
        var searchArray = it.Field<EnemySearchComponent>(4);        
        var moveResolutorArray = it.Field<MoveResolutorComponent>(5);

        Span<int> neighbors = stackalloc int[8];

        float deltaTime = sim.FixedDelta; // 🔥 Usar solo el delta del tick actual, no el acumulado histórico
        int mask = sim.GetGroupMask();
        int frame = sim.FrameIndex & mask;

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var spatial = ref spatialArray[i];
            ref var team = ref teamArray[i];
            ref var search = ref searchArray[i];
            ref var moveResolutor = ref moveResolutorArray[i];

            var e = it.Entity(i);

            // 🔥 acumular tiempo correctamente
            search.Timer += deltaTime;

            if (search.Timer >= search.Interval)
            {
                // 🔥 cuántas veces "debió" ejecutarse
                int times = (int)(search.Timer / search.Interval);

                // 🔥 conservar excedente (CLAVE)
                search.Timer -= times * search.Interval;
                // 🔥 👇 AQUI VA EL STAGGERING 👇
                int group = spatial.Value & mask;
                if (group != frame) continue;

                // 🔥 SOLO UNA QUERY (optimización crítica)
                int count = dynGrid.QueryNodesBounded(
                    pos.position.X,
                    pos.position.Y,
                    search.Radius,
                    neighbors
                );

                for (int ii = 0; ii < count; ii++)
                {
                    var targetEntity = dynGrid.GetEntity(neighbors[ii]);

                    if (spatial.Value == neighbors[ii]) continue;

                    var otherTeam = targetEntity.Get<TeamComponent>();
                    if (team.TeamId == otherTeam.TeamId) continue;

                    if (targetEntity.IsAlive() && !targetEntity.Has<DeadTag>())
                    {
                        moveResolutor.Blocked = false;
                        moveResolutor.BlockedTimer = 0f;

                        Vector2 targetPos = targetEntity.Get<PositionComponent>().position;

                        e.Set(new MoveTargetComponent(targetPos));
                        e.Remove<StoppedTag>();
                        e.Set(new AttackPendingComponent(true,targetEntity));                       
                        break;
                    }
                }
            }
        }
    }  
}
