using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Animation;
internal class AnimationLayerUpdateSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RenderLayerListComponent>()
            .With<DirectionComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var layers = it.Field<RenderLayerListComponent>(0);
        var dir = it.Field<DirectionComponent>(1);
        float delta = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var l = ref layers[i];
            ref var d = ref dir[i];

            for (int j = 0; j < l.Animations.Length; j++)
            {
                ref var a = ref l.Animations[j];
                ref var f = ref l.Frames[j];

                // Obtén los datos de animación según el tipo de entidad
                var animationData = AnimationCache.GetAnimation(
                    a.idSpriteOrAnimation,
                    a.entityType,                   
                    a.stateAnimation, 
                    d
                );

                // Si cambió el estado de animación → reiniciar
                if (a.lastStateAnimation != a.stateAnimation)
                {
                    a.TimeSinceLastFrame = 0f;
                    a.animationComplete = false;
                    a.currentFrameIndex = 1;
                    a.active = true;
                    a.lastStateAnimation = a.stateAnimation;
                    a.frameDuration = animationData.frameDuration;

                    f.uvMap =animationData.uvFramesArray[0];                    
                }

                // Avance de frames
                a.TimeSinceLastFrame += delta;
                if (a.TimeSinceLastFrame >= a.frameDuration && a.active)
                {
                    a.TimeSinceLastFrame = 0;

                    if (a.currentFrameIndex >= animationData.uvFramesArray.Length)
                    {
                        if (animationData.loop)
                        {
                            a.currentFrameIndex = 0;
                            a.animationComplete = true;
                            a.active = true;
                        }
                        else
                        {
                            a.animationComplete = true;
                            a.active = false;
                        }
                    }

                    if (a.active)
                    {
                        f.uvMap = animationData.uvFramesArray[a.currentFrameIndex];
                        //ref var frameData = ref animationData.frameDataArray[a.currentFrameIndex];
                        //f.uvMap = new Godot.Color(
                        //    frameData.xFormat,
                        //    frameData.yFormat,
                        //    frameData.widhtFormat,
                        //    frameData.heightFormat
                        //);

                        a.currentFrameIndex++;
                    }
                }
            }
        }
    }
}