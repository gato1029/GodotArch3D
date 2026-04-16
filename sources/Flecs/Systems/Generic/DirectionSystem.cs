using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using CharacterComponent = GodotFlecs.sources.Flecs.Components.CharacterComponent;


namespace GodotEcsArch.sources.Flecs.Systems.Generic;

public class DirectionSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<VelocityComponent>()
          .With<DirectionComponent>()
          .With<CharacterComponent>()
          .With<SteeringComponent>() // <--- Añadimos esto
          .Without<DeadTag>()
          .Without<SleepTag>();
    }

    protected override void OnIter(Iter it)
    {
        var velArray = it.Field<VelocityComponent>(0);
        var dirArray = it.Field<DirectionComponent>(1);
        var chaArray = it.Field<CharacterComponent>(2);
        var steerArray = it.Field<SteeringComponent>(3); // <---
        for (int i = 0; i < it.Count(); i++)
        {
            ref var vel = ref velArray[i];
            ref var dir = ref dirArray[i];
            ref var cha = ref chaArray[i];
            ref var steer = ref steerArray[i];

            // Dentro del loop de DirectionSystem
            ref var steering = ref steerArray[i];

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
                        dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.normalized);
                        break;
                    case DirectionAnimationType.CUATRO:                        
                        dir.animationDirection = CommonOperations.GetDirectionAnimation(dir.normalized);
                        break;
                    case DirectionAnimationType.OCHO:
                        break;
                    default:
                        break;
                }

                if (vel.desiredVel.LengthSquared() > 0.01f)
                    cha.characterStateType = CharacterStateType.MOVING;
                else
                    cha.characterStateType = CharacterStateType.IDLE;
                //cha.characterStateType = managers.Characters.CharacterStateType.MOVING;
            }
            else
            {
                // 🔹 sin movimiento
                cha.characterStateType = managers.Characters.CharacterStateType.IDLE;

                // ❗ opcional: NO tocar dir.value para mantener última dirección
            }
        }
    }
}