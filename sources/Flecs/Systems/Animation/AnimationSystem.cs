using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Animation;
internal class AnimationSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<IdGenericComponent>()
           .With<AnimationComponent>()
           .With<DirectionComponent>()
           .With<RenderFrameDataComponent>();

    }

    protected override void OnIter(Iter it)
    {
        var idGen = it.Field<IdGenericComponent>(0);
        var animation = it.Field<AnimationComponent>(1);
        var dir = it.Field<DirectionComponent>(2);
        var frame = it.Field<RenderFrameDataComponent>(3);
        float delta = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var id = ref idGen[i];
            ref var anim = ref animation[i];
            ref var d = ref dir[i];
            ref var f = ref frame[i];

            AnimationType stateAnimation = anim.stateAnimation;
            var  animationData = AnimationCache.GetAnimation(anim.idSpriteOrAnimation, anim.entityType, anim.stateAnimation,  d);

            //var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(id.id);
            //AnimationStateData animationStateData = dataCharacterModel.animationCharacterBaseData.animationDataArray[stateAnimation];
            //AnimationData animationData = animationStateData.animationData[(int)d.animationDirection];


            if (anim.lastStateAnimation != stateAnimation)
            {
                anim.TimeSinceLastFrame = 0f;
                anim.animationComplete = false;
                anim.currentFrameIndex = 1;
                anim.active = true;
                anim.lastStateAnimation = stateAnimation;
                anim.frameDuration = animationData.frameDuration;
                f.uvMap =animationData.uvFramesArray[0];                
            }

            anim.TimeSinceLastFrame += delta;
            if (anim.TimeSinceLastFrame >= anim.frameDuration && anim.active)
            {
                anim.TimeSinceLastFrame = 0;
                if (anim.currentFrameIndex >= animationData.uvFramesArray.Length)
                {
                    if (animationData.loop)
                    {
                        f.uvMap = animationData.uvFramesArray[0];

                        anim.animationComplete = true;
                        anim.currentFrameIndex = 0;
                        anim.active = true;
                    }
                    else
                    {
                        anim.animationComplete = true;
                        anim.active = false;
                    }
                }
                else
                {
                    f.uvMap = animationData.uvFramesArray[anim.currentFrameIndex];                  
                    anim.currentFrameIndex++;
                }

            }
        }
    }



 
}
