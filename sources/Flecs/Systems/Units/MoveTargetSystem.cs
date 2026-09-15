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
using static System.Net.WebRequestMethods;
using CharacterComponent = GodotFlecs.sources.Flecs.Components.CharacterComponent;

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
          .With<MoveColliderComponent>()
          //.With<AttackPendingComponent>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var targetArray = it.Field<MoveTargetComponent>(1);
        var chaArray = it.Field<Components.CharacterComponent>(2);
        var steeringArray = it.Field<SteeringComponent>(3); // <-- Añadido
        var resolutorArray = it.Field<MoveResolutorComponent>(4);
        var moveArray = it.Field<MoveColliderComponent>(5);
        //var attackpendingArray = it.Field<AttackPendingComponent>(5);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var target = ref targetArray[i];
            ref var cha = ref chaArray[i];
            ref var steering = ref steeringArray[i];
            ref var resolutor = ref resolutorArray[i];
            ref var move = ref moveArray[i];
          //  ref var attackPending = ref attackpendingArray[i];

            if (resolutor.BlockedTimer>0.5f )
            {
                steering.DesiredDir = Vector2.Zero;
                resolutor.BlockedTimer = 0;
                resolutor.Blocked = true;
                cha.characterStateType = CharacterStateType.IDLE;
                it.Entity(i).Remove<MoveTargetComponent>();
                it.Entity(i).Add<StoppedTag>();

             
                // aqui quiere decir que choco con algo inesperado
                //if (attackPending.Active)
                //{
                //    // libero objetivo 
                //    attackPending.Active = false;
                //    attackPending.Target = default;
                //}
                continue;
            }
            Vector2 toTarget = target.Value - pos.position+move.Offset;
            float distSq = toTarget.LengthSquared();

            float umbralLlegada = 0.05f;
            //if (attackPending.Active && attackPending.Target.IsAlive() && !attackPending.Target.Has<DeadTag>())
            //{
            //    if (attackPending.isUnit)
            //    {
            //        if (attackPending.Target.Has<CharacterComponent>())
            //        {
            //            umbralLlegada = attackPending.Target.Get<MoveColliderComponent>().Radius;
            //        }
            //    }
            //    else
            //    {
            //        //if (attackPending.Target.Has<BuildingDefinitionComponent>())
            //        //{
            //        //    umbralLlegada = 2;
            //        //}                
            //    }
            //}

            if (distSq <= umbralLlegada)//0.05f) // Umbral de llegada
            {
                resolutor.BlockedTimer = 0;
                steering.DesiredDir = Vector2.Zero;
                resolutor.Blocked = true;
                cha.characterStateType = CharacterStateType.IDLE;
                it.Entity(i).Remove<MoveTargetComponent>();
                it.Entity(i).Add<StoppedTag>();
                //if (attackPending.Active)
                //{
                //    it.Entity(i).Add<AttackPendingTag>();
                //}                
                continue;
            }

            // Solo enviamos la DIRECCIÓN deseada al Steering
            steering.DesiredDir = (toTarget).Normalized();
            cha.characterStateType = CharacterStateType.MOVING;
        }
    }
}