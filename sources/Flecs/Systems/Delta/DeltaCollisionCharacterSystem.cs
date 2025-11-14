using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Delta;
internal class DeltaCollisionCharacterSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
            .With<CharacterComponent>()
            .With<ColliderComponent>()
            .With<MoveResolutorComponent>()
            .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var charArray = it.Field<CharacterComponent>(1);
        var colArray = it.Field<ColliderComponent>(2);


        // 3️⃣ Leer velocidades resultantes
        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];            
            ref var cha = ref charArray[i];
            ref var col = ref colArray[i];
            CollisionManager.Instance.characterEntitiesFlecs.UpdateColliderPosition(col.idCollider, pos.position);
        }
    }
}
