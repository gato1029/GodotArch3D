using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Building;
internal class BuildCleanupSystem:FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsPostUpdate; // se ejecuta al final del frame
    protected override bool MultiThreaded => false; // importante: sólo main thread

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<BuildingComponent>()
          .With<PositionComponent>()
          .With<ColliderComponent>()
          .With<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var buildArray = it.Field<BuildingComponent>(0);
        var posArray = it.Field<PositionComponent>(1);
        var colArray = it.Field<ColliderComponent>(2);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var build = ref buildArray[i];
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            var entity = it.Entity(i);
            MapManagerEditor.Instance.CurrentMapLevelData.mapBuildings.RemoveTile(pos.tilePosition);
            CollisionManager.Instance.BuildingsCollidersFlecs.RemoveCollider(col.idCollider);            
            // Eliminar del mundo Flecs
            entity.Destruct();
        }
    }
}
