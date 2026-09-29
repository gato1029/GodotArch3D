using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Generic;

internal class FlowFieldFollowSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<FlowFieldFollowerComponent>()
          .With<SteeringComponent>()
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

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var follower = ref followerArray[i];
            ref var steering = ref steeringArray[i];

            var field = flowFieldManager.GetField(follower.FlowFieldId);

            if (field == null)
            {
                steering.DesiredDir = Vector2.Zero;
                continue;
            }

            steering.DesiredDir = field.SampleInterpolated(pos.position);
        }
    }
}