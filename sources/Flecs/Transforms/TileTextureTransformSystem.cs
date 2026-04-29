using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Transforms;

internal class TileTextureTransformSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<RenderGPUComponent>()
          .With<RenderTransformComponent>()
          .With<TileTextureComponent>()
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
            float depthValue = p.position.Y
                   + depthOffset
                    - p.height * GodotEcsArch.sources.utils.CommonAtributes.HEIGHT_OFFSET;

            float renderZ = depthValue * GodotEcsArch.sources.utils.CommonAtributes.LAYER_MULTIPLICATOR + r.layerRender * GodotEcsArch.sources.utils.CommonAtributes.LAYER_OFFSET;

            Vector2 newWorldPos = TilesHelper.WorldPositionTile(p.tilePosition);
            p.position = newWorldPos;
            var tt = t.transform;
            tt.Origin = new Vector3(newWorldPos.X + r.originOffset.X, newWorldPos.Y + r.originOffset.Y, renderZ);
            t.transform = tt;
            RenderingServer.MultimeshInstanceSetTransform(r.rid, r.instance, t.transform);
            it.Entity(i).Remove<DirtyTransformTag>();
        }
    }
}
