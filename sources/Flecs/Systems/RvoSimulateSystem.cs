using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using GodotFlecs.sources.Flecs.Systems.Human;
using RVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;

namespace GodotFlecs.sources.Flecs.Systems;
public class RvoSimulateSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; // debe ser single-thread

    const float BlockedThreshold = 0.8f;     // velocidad mínima considerada “quieto”
    const float UnblockTime = 10.5f;         // debe estar 10.5s libre para desbloquearse

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<VelocityComponent>()
          .With<RvoAgentIdComponent>()
          .With<MoveResolutorComponent>()
          .With<CharacterComponent>()
          .With<PositionComponent>()
          .With<UseRvoTag>()
          .Without<PlayerInputComponent>();  // <-- Solo procesamos agentes que tienen target
    }

    protected override void OnIter(Iter it)
    {        
        var velArray = it.Field<VelocityComponent>(0);
        var idArray = it.Field<RvoAgentIdComponent>(1);
        var extraArray = it.Field<MoveResolutorComponent>(2);
        var chaArray = it.Field<CharacterComponent>(3);
        var posArray = it.Field<PositionComponent>(4);

        // 3️⃣ Leer velocidades resultantes
        for (int i = 0; i < it.Count(); i++)
        {
            var e = it.Entity(i);
            ref var vel = ref velArray[i];
            ref var extraMov = ref extraArray[i];   
            ref var id = ref idArray[i];
            ref var cha = ref chaArray[i];
            ref var pos = ref posArray[i];
           

            if (extraMov.Blocked || cha.characterStateType != GodotEcsArch.sources.managers.Characters.CharacterStateType.MOVING)
            {
                extraMov.Blocked = true;
                vel.Value = Vector2.Zero;

                Simulator.Instance.setAgentMaxSpeed(id.Id, 0);                
                Simulator.Instance.doStepAgent(id.Id);
                Simulator.Instance.setAgentSleep(id.Id, true);
                //pos.position = extraMov.positionFuture;
                e.Remove<MoveResolutorComponent>();
                continue;
            }

            vel.Value = Simulator.Instance.getAgentVelocity(id.Id);
            extraMov.positionFuture = Simulator.Instance.getAgentPosition(id.Id);

            bool isMoving = vel.Value.Length() >= BlockedThreshold;

            if (!isMoving)
            {
                extraMov.Blocked = true;
                vel.Value = Vector2.Zero;
                extraMov.BlockedTimer = UnblockTime;
                
                Simulator.Instance.setAgentMaxSpeed(id.Id, 0);
                Simulator.Instance.doStepAgent(id.Id);
                Simulator.Instance.setAgentSleep(id.Id, true);
                cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE;
            }
            else
            {               
                if (extraMov.Blocked)
                {
                    extraMov.BlockedTimer -= it.DeltaTime();
                    if (extraMov.BlockedTimer <= 0)
                    {
                        extraMov.Blocked = false;
                        extraMov.BlockedTimer = 0f;
                        Simulator.Instance.setAgentMaxSpeed(id.Id, vel.MaxSpeed);
                    }
                }                               
            }
        }
    }
}
