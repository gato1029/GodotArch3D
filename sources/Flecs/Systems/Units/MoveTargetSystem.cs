using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using RVO;
using SadRogue.Primitives;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Units;

public class MoveTargetSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()          
          .With<MoveTargetComponent>()
          .With<Components.CharacterComponent>()
          .With<SteeringComponent>() // <-- Añadido
          .With<MoveResolutorComponent>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var targetArray = it.Field<MoveTargetComponent>(1);
        var chaArray = it.Field<Components.CharacterComponent>(2);
        var steeringArray = it.Field<SteeringComponent>(3); // <-- Añadido
        var resolutorArray = it.Field<MoveResolutorComponent>(4);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var target = ref targetArray[i];
            ref var cha = ref chaArray[i];
            ref var steering = ref steeringArray[i];
            ref var resolutor = ref resolutorArray[i];

            if (resolutor.BlockedTimer>1)
            {
                steering.DesiredDir = Vector2.Zero;
                resolutor.BlockedTimer = 0;
                resolutor.Blocked = true;
                cha.characterStateType = CharacterStateType.IDLE;
                it.Entity(i).Remove<MoveTargetComponent>();
                it.Entity(i).Add<SleepTag>();
                continue;
            }
            Vector2 toTarget = target.Value - pos.position;
            float distSq = toTarget.LengthSquared();

            if (distSq < 0.05f) // Umbral de llegada
            {
                resolutor.BlockedTimer = 0;
                steering.DesiredDir = Vector2.Zero;
                resolutor.Blocked = true;
                cha.characterStateType = CharacterStateType.IDLE;
                it.Entity(i).Remove<MoveTargetComponent>();
                it.Entity(i).Add<SleepTag>();
                continue;
            }

            // Solo enviamos la DIRECCIÓN deseada al Steering
            steering.DesiredDir = (toTarget).Normalized();
            cha.characterStateType = CharacterStateType.MOVING;
        }
    }
}