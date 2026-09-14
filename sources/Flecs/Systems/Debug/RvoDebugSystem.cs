using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
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
            .With<MoveColliderComponent>()
            .With<UnitDefinitionComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var ageArray = it.Field<RvoAgentDebugComponent>(0);
        var posArray = it.Field<PositionComponent>(1);
        var colArray = it.Field<MoveColliderComponent>(2);
        var unitArray = it.Field<UnitDefinitionComponent>(3);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var age = ref ageArray[i];
            ref var col = ref colArray[i];
            ref var unit = ref unitArray[i];
            Entity entity = it.Entity(i);

            WireShape.Instance.UpdatePosition(age.idShapeMove, pos.position+col.Offset);
            if (age.idShapeRadiusAttack!=0)
            {
                if (entity.Has<MeleeAttackComponent>())
                {
                    var colMelle = entity.Get<MeleeAttackComponent>();
                    WireShape.Instance.UpdatePosition(age.idShapeRadiusAttack, pos.position + colMelle.OffSetRange);
                }
                if (entity.Has<RangedAttackComponent>())
                {
                    var colMelle = entity.Get<RangedAttackComponent>();
                    WireShape.Instance.UpdatePosition(age.idShapeRadiusAttack, pos.position);
                }
                
            }
            if (age.idShapeRadiusRangeSearch!=0)
            {
                WireShape.Instance.UpdatePosition(age.idShapeRadiusRangeSearch, pos.position);
            }
            if (age.idShapeBody!=0)
            {
                var template = BlackyPalletesPersistence.characterPalette.GetData(unit.idTemplate);
                var colBody = template.bodyColliders[0];                
                WireShape.Instance.UpdatePosition(age.idShapeBody, pos.position + colBody.Offset);
            }
           
        }
    }
}
