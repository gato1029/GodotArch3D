using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Generic
{
    public class MovementFreeUnitTargetSytem : FlecsSystemBase
    {
        protected override ulong Phase => flecs.EcsOnUpdate;
        protected override bool MultiThreaded => true;
        protected override void BuildQuery(ref QueryBuilder qb)
        {
            qb.With<MoveTargetComponent>()                          
              .With<MoveResolutorComponent>()
              .With<UnitTag>()
              .Without<DeadTag>()
              .Without<SleepTag>();
        }

        protected override void OnIter(Iter it)
        {
            
            var moveTargetArray = it.Field<MoveTargetComponent>(0);
            var resolutorArray = it.Field<MoveResolutorComponent>(1);            

            for (int i = 0; i < it.Count(); i++)
            {
                ref var moveTarget = ref moveTargetArray[i];
                ref var resolutor = ref resolutorArray[i];

                Entity entity = it.Entity(i);
                if (resolutor.Blocked)
                {
                    entity.Remove<MoveTargetComponent>();
                    entity.Add<SleepTag>();
                    resolutor.Blocked = false;
                    resolutor.BlockedTimer = 0;
                }
            }
        }
    }
}
