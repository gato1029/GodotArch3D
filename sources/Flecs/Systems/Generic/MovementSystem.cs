using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using RVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
public class MovementSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()                        
          .With<MoveResolutorComponent>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);                        
        var moveExtraArray = it.Field<MoveResolutorComponent>(1);        
        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];            
            ref var movExtra = ref moveExtraArray[i];
            pos.position = movExtra.positionFuture;

        }
    }
}