using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Debug;
internal class RvoDebugSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RvoAgentDebugComponent>()
            .With<PositionComponent>()
            .With<MoveColliderComponent>();
          //  .With<BodyColliderComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var ageArray = it.Field<RvoAgentDebugComponent>(0);
        var posArray = it.Field<PositionComponent>(1);
        var colArray = it.Field<MoveColliderComponent>(2);
      //  var colBodyArray = it.Field<BodyColliderComponent>(3);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var age = ref ageArray[i];
            ref var col = ref colArray[i];
  //          ref var colBody = ref colBodyArray[i];
            WireShape.Instance.UpdatePosition(age.idShapeRadius, pos.position+col.Offset);
            if (age.idShapeRadiusAttack!=0)
            {
                WireShape.Instance.UpdatePosition(age.idShapeRadiusAttack, pos.position);
            }
            
           // WireShape.Instance.UpdatePosition(age.idShapeBody, pos.position + colBody.Shapes[0].Offset);
        }
    }
}
