using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;

using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;

internal class SpriteTransformSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<RenderGPUComponent>()
          .With<RenderTransformComponent>()
          .Without<StaticRenderTag>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>()
          .Without<TileSpriteAnimationTag>();


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
            
            //r.originOffset.Y 
            //float renderZ = ((p.position.Y + r.zOrdering) * CommonAtributes.LAYER_MULTIPLICATOR) + r.layerRender;
            var tt = t.transform;
            tt.Origin = new Vector3(p.position.X + r.originOffset.X, p.position.Y + r.originOffset.Y, renderZ);
            t.transform = tt; 
        }
    }
}

internal class SpriteTransformLayerSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<RenderLayerListComponent>()
          .Without<StaticRenderTag>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var pos = it.Field<PositionComponent>(0);
        var layers = it.Field<RenderLayerListComponent>(1);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var p = ref pos[i];
            ref var l = ref layers[i];

            for (int j = 0; j < l.GPUData.Length; j++)
            {
                ref var gpu = ref l.GPUData[j];
                ref var t = ref l.Transforms[j];

                float depthOffset = (gpu.zOrdering);
                float depthValue = p.position.Y 
                        + depthOffset
                        - p.height * GodotEcsArch.sources.utils.CommonAtributes.HEIGHT_OFFSET;

                float renderZ = depthValue * GodotEcsArch.sources.utils.CommonAtributes.LAYER_MULTIPLICATOR + gpu.layerRender * GodotEcsArch.sources.utils.CommonAtributes.LAYER_OFFSET;
                //GD.Print("character:"+renderZ);
                //float renderZ = (p.position.Y + gpu.zOrdering)
                //    * CommonAtributes.LAYER_MULTIPLICATOR + gpu.layerRender;

                var tt = t.transform;
                tt.Origin = new Vector3(
                    p.position.X + gpu.originOffset.X,
                    p.position.Y + gpu.originOffset.Y,
                    renderZ
                );
                t.transform = tt;
            }
        }
    }
}
internal class SpriteTransformStaticSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnAdd | flecs.EcsOnStart;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<RenderGPUComponent>()
          .With<RenderTransformComponent>()
          .With<StaticRenderTag>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>();

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
            //float renderZ = (p.position.Y  + r.zOrdering) * CommonAtributes.LAYER_MULTIPLICATOR + r.layerRender;


            var tt = t.transform;
            tt.Origin = new Vector3(p.position.X + r.originOffset.X, p.position.Y + r.originOffset.Y, renderZ);
            t.transform = tt;           
        }
    }
}