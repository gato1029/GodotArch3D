using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using GodotEcsArch.sources.BlackyEngine.Core;
using StateComponent = GodotFlecs.sources.Flecs.Components.StateComponent;


namespace GodotEcsArch.sources.Flecs.Systems.Generic;

public class DirectionSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<VelocityComponent>()
          .With<DirectionComponent>()
          .With<StateComponent>()
          .With<SteeringComponent>() 
          .Without<DeadTag>()
          .Without<StoppedTag>();
    }

    protected override void OnIter(Iter it)
    {   var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld==null)
        {
            return;
        }
        var stateRegister = blackyWorld.State.stateChangeRegistry;
        var velArray = it.Field<VelocityComponent>(0);
        var dirArray = it.Field<DirectionComponent>(1);
        var chaArray = it.Field<StateComponent>(2);
        var steerArray = it.Field<SteeringComponent>(3); // <---
        for (int i = 0; i < it.Count(); i++)
        {
            Entity e = it.Entity(i);
            ref var vel = ref velArray[i];
            ref var dir = ref dirArray[i];
            ref var cha = ref chaArray[i];
            ref var steer = ref steerArray[i];
            // Dentro del loop de DirectionSystem
            ref var steering = ref steerArray[i];
            if (cha.stateType != StateType.MOVING)
            {
                continue;
            }
            // PRIORIDAD: 
            // 1. ¿A dónde quiero ir? (DesiredDir)
            // 2. Si no hay deseo, ¿A dónde miraba antes? (dir.value)
            Vector2 lookDir = steering.DesiredDir != Vector2.Zero
                ? steering.DesiredDir
                : dir.value;

            if (lookDir != Vector2.Zero)
            {
                dir.value = lookDir.Normalized();

                // Solo actualizamos el "normalized" (signo) si realmente hay intención activa
                // para evitar que Math.Sign(0) resetee el sprite.
                if (steering.DesiredDir != Vector2.Zero)
                {
                    dir.normalized = new Vector2(
                        MathF.Abs(steering.DesiredDir.X) < 0.01f ? 0 : MathF.Sign(steering.DesiredDir.X),
                        MathF.Abs(steering.DesiredDir.Y) < 0.01f ? 0 : MathF.Sign(steering.DesiredDir.Y)
                    );
                }

                switch (dir.directionType)
                {
                    case DirectionAnimationType.NINGUNO:
                        break;
                    case DirectionAnimationType.DOS:                        
                        dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.value); // antes estaba el norm
                        break;
                    case DirectionAnimationType.CUATRO:                        
                        dir.animationDirection = CommonOperations.GetDirectionAnimation(dir.value);
                        break;
                    case DirectionAnimationType.OCHO:
                        dir.animationDirection = CommonOperations.GetDirectionAnimationEight(dir.value);
                        break;
                    default:
                        break;
                }
                stateRegister.SetDirection(e.Id.Value, dir.animationDirection);
            }
        }
    }
}