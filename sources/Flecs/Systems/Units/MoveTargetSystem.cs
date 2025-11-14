using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using RVO;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Units
{
    public class MoveTargetSystem : FlecsSystemBase
    {
        protected override ulong Phase => flecs.EcsOnUpdate;
        protected override bool MultiThreaded => true;
        protected override void BuildQuery(ref QueryBuilder qb)
        {
            qb.With<PositionComponent>()              
              .With<VelocityComponent>()
              .With<DirectionComponent>()
              .With<MoveTargetComponent>()
              .With<CharacterComponent>()
              .With<MoveResolutorComponent>()
              .Without<DeadTag>();
        }

        protected override void OnIter(Iter it)
        {
            var posArray = it.Field<PositionComponent>(0);            
            var velArray = it.Field<VelocityComponent>(1);            
            var DirArray = it.Field<DirectionComponent>(2);
            var targetArray = it.Field<MoveTargetComponent>(3);            
            var chaArray = it.Field<CharacterComponent>(4);
            var moveExtraArray = it.Field<MoveResolutorComponent>(5);
            // 1️⃣ Sincronizar estado con el simulador

            for (int i = 0; i < it.Count(); i++)
            {
                var e = it.Entity(i);
                ref var pos = ref posArray[i];
                ref var target = ref targetArray[i];
                ref var vel = ref velArray[i];          
                ref var direc = ref DirArray[i];

                ref var cha = ref chaArray[i];
                ref var moveExtra = ref moveExtraArray[i];
                //if (cha.characterStateType != GodotEcsArch.sources.managers.Characters.CharacterStateType.MOVING)
                //{
                //    continue;
                //}
                // Si está bloqueado, no moverse
                if (moveExtra.Blocked)
                {
                    target.Value = pos.position;
                    vel.prefVel = Vector2.Zero;
                    e.Remove<MoveTargetComponent>();
                    
                    //Simulator.Instance.setAgentMaxSpeed(id.Id, 0);
                    //cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE;
                    continue;
                }

                // Dirección hacia el objetivo
                Vector2 dir = target.Value - pos.position;//  Simulator.Instance.getAgentPosition(id.Id);
                // pos.position;
                float distance = dir.Length();

                direc.value = dir.Normalized();
                direc.normalized = new Vector2(Math.Sign(dir.X), Math.Sign(dir.Y));
                direc.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(direc.normalized);

                if (dir.Length() > 0.1f)
                {
                    vel.prefVel = dir.Normalized()* vel.MaxSpeed;
                   // moveExtra.positionFuture += pos.position*vel.prefVel;
                    //Simulator.Instance.setAgentPrefVelocity(id.Id, vel.prefVel);
                    cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.MOVING;
                 
                }
                else
                {
                    e.Remove<MoveTargetComponent>();
                    //e.Remove<MoveResolutorComponent>();
                    moveExtra.Blocked = true;
                    vel.prefVel = Vector2.Zero;
                    //Simulator.Instance.setAgentMaxSpeed(id.Id, 0);
                    cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE;
                }
                           
            }

        }
    }
}
