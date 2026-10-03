using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Generic;

internal class FlowFieldUnstickSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

    private const float UnstickDelay = 1.0f; // segundos bloqueada antes de reintentar

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<FlowFieldFollowerComponent>()
          .With<MoveResolutorComponent>()
          .With<StoppedTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var resArray = it.Field<MoveResolutorComponent>(1);
        float dt = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var res = ref resArray[i];
            var e = it.Entity(i);

            res.StuckTimer += dt; // necesitas agregar este campo a MoveResolutorComponent

            if (res.StuckTimer >= UnstickDelay)
            {
                res.Blocked = false;
                res.BlockedTimer = 0f;
                res.StuckTimer = 0f;
                e.Remove<StoppedTag>();
            }
        }
    }
}

internal class FlowFieldFollowSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<FlowFieldFollowerComponent>()
          .With<SteeringComponent>()
          .With<StateComponent>() 
          .Without<StoppedTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;

        var flowFieldManager = blackyWorld.State.FlowFieldManager;

        var posArray = it.Field<PositionComponent>(0);
        var followerArray = it.Field<FlowFieldFollowerComponent>(1);
        var steeringArray = it.Field<SteeringComponent>(2);
        var stateArray = it.Field<StateComponent>(3);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var follower = ref followerArray[i];
            ref var steering = ref steeringArray[i];
            ref var state = ref stateArray[i];

            var field = flowFieldManager.GetField(follower.FlowFieldId);

            if (field == null)
            {
                steering.DesiredDir = Vector2.Zero;
                continue;
            }
            state.stateType = StateType.MOVING;
            steering.DesiredDir = field.SampleInterpolated(pos.position);
        }
    }
}