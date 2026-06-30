using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Transforms;

internal class TileSpriteTextureTransformSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<RenderGPUComponent>()
          .With<RenderTransformComponent>()
          .With<TileSpriteTextureComponent>()
          .With<DirtyTransformTag>();          
    }

    protected override void OnIter(Iter it)
    {
        var pos = it.Field<PositionComponent>(0);
        var ren = it.Field<RenderGPUComponent>(1);
        var trans = it.Field<RenderTransformComponent>(2);
        for (int i = 0; i < it.Count(); i++)
        {
            var e = it.Entity(i);
            ref var p = ref pos[i];
            ref var r = ref ren[i];
            ref var t = ref trans[i];

            float depthOffset = (r.zOrdering);
            float renderZ = CommonAtributes.Calculate(depthOffset, p.height, r.layerRender, p.position);

            Vector2 newWorldPos = TilesHelper.TilePositionToWorldPosition(p.tilePosition);
            p.position = newWorldPos;
            var tt = t.transform;
            tt.Origin = new Vector3(newWorldPos.X + r.originOffset.X, newWorldPos.Y + r.originOffset.Y, renderZ);
            t.transform = tt;
            RenderingServer.MultimeshInstanceSetTransform(r.rid, r.instance, t.transform);
            it.Entity(i).Remove<DirtyTransformTag>();
        }
    }
}
