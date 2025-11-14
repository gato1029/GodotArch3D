using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Human;
internal class LocalAvoidanceMainCharacterSystem:FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<VelocityComponent>()
          .With<MoveResolutorComponent>()
          .With<CharacterComponent>()
          .With<TeamComponent>()
          .With<DirectionComponent>()
          .With<ColliderComponent>()
          .With<UseBoidTag>()
          .With<PlayerInputComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var velArray = it.Field<VelocityComponent>(1);
        var moveArray = it.Field<MoveResolutorComponent>(2);
        var chaArray = it.Field<CharacterComponent>(3);
        var teamArray = it.Field<TeamComponent>(4);
        var dirArray = it.Field<DirectionComponent>(5);
        var colArray = it.Field<ColliderComponent>(6);

        for (int i = 0; i < it.Count(); i++)
        {
            var e = it.Entity(i);
            ref var pos = ref posArray[i];
            ref var vel = ref velArray[i];
            ref var move = ref moveArray[i];
            ref var cha = ref chaArray[i];
            ref var team = ref teamArray[i];
            ref var dir = ref dirArray[i];
            ref var col = ref colArray[i];
            if (move.Blocked || cha.characterStateType != GodotEcsArch.sources.managers.Characters.CharacterStateType.MOVING)
            {
                continue;
            }
            var posFuture = pos.position + vel.prefVel * it.DeltaTime();

            bool collision = CollisionManager.Instance.characterEntitiesFlecs.IntersecMoveAABBColliders(col.idCollider, posFuture);
            if (!collision)
            {  
                Rect2 rect2 = col.aabbMove;
                rect2.Position = posFuture - rect2.Size/2 + col.offsetMove;
                //var midlePointRectDirection = CommonOperations.GetMidPointByDirection(rect2, dir.animationDirection);
                //GameLog.LogCat("p:" + midlePointRectDirection);
                //midlePointRectDirection = midlePointRectDirection + vel.prefVel.Normalized() * it.DeltaTime() + col.offsetMove + pos.position;
                collision = CollisionManager.Instance.BuildingsCollidersFlecs.IntersectsMoveAABB(rect2);
            }
            if (collision)
            {
                cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE;
                move.Blocked = true;
                continue;
            }
            move.positionFuture = posFuture;
        }
    }
}
