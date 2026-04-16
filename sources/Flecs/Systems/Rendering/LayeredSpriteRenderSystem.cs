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

namespace GodotEcsArch.sources.Flecs.Systems.Rendering;
internal class LayeredSpriteRenderSystem: FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RenderLayerListComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var layers = it.Field<RenderLayerListComponent>(0);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var l = ref layers[i];

            for (int j = 0; j < l.GPUData.Length; j++)
            {
                ref var ani = ref l.Animations[j];
                ref var gpu = ref l.GPUData[j];
                ref var frame = ref l.Frames[j];
                ref var transform = ref l.Transforms[j];

                RenderingServer.MultimeshInstanceSetTransform(gpu.rid, gpu.instance, transform.transform);
                
                if (ani.visible)
                {
                    RenderingServer.MultimeshInstanceSetCustomData(gpu.rid, gpu.instance, frame.uvMap);
                    RenderingServer.MultimeshInstanceSetColor(gpu.rid, gpu.instance, new Color(0, 0, 0, gpu.layerTextureMaterial));
                }
                else
                {
                    RenderingServer.MultimeshInstanceSetCustomData(gpu.rid, gpu.instance, new Color(-1,-1,-1,-1));
                }
                
            }
        }
    }
}
