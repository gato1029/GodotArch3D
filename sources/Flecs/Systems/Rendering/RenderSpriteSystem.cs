using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Metrics;

namespace GodotEcsArch.sources.Flecs.Systems.Rendering;
internal class RenderSpriteSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RenderGPUComponent>()
           .With<RenderFrameDataComponent>()
           .With<RenderTransformComponent>()
           .Without<TileSpriteAnimationTag>()
           .Without<RenderDisabledTag>();
    }

    protected override void OnIter(Iter it)
    {
        var gpu = it.Field<RenderGPUComponent>(0);
        var frame = it.Field<RenderFrameDataComponent>(1);
        var transform = it.Field<RenderTransformComponent>(2);
        int cont = it.Count();
        for (int i = 0; i < it.Count(); i++)
        {            
            ref var r = ref gpu[i];
            ref var f = ref frame[i];
            ref var t = ref transform[i];
            RenderingServer.MultimeshInstanceSetTransform(r.rid, r.instance, t.transform);
            RenderingServer.MultimeshInstanceSetCustomData(r.rid, r.instance, f.uvMap);
            RenderingServer.MultimeshInstanceSetColor(r.rid, r.instance, new Color(0, 0, 0, r.layerTextureMaterial));
        }
    }
}
internal class RenderSpriteTileSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RenderGPUComponent>()
           .With<RenderFrameDataComponent>()           
           .With<TileSpriteAnimationTag>()
           .Without<RenderDisabledTag>();
    }

    protected override void OnIter(Iter it)
    {
        var gpu = it.Field<RenderGPUComponent>(0);
        var frame = it.Field<RenderFrameDataComponent>(1);
        
        for (int i = 0; i < it.Count(); i++)
        {
            ref var r = ref gpu[i];
            ref var f = ref frame[i];            
            //RenderingServer.MultimeshInstanceSetTransform(r.rid, r.instance, t.transform);
            RenderingServer.MultimeshInstanceSetCustomData(r.rid, r.instance, f.uvMap);
            //RenderingServer.MultimeshInstanceSetColor(r.rid, r.instance, new Color(0, 0, 0, r.layerTextureMaterial));
        }
    }
}