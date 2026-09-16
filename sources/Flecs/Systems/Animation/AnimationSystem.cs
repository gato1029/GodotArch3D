using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.Flecs.Systems.Animation;
internal class AnimationSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<AnimationComponent>()
           .With<DirectionComponent>()
           .With<RenderFrameDataComponent>()
           .With<StateComponent>();

    }

    protected override void OnIter(Iter it)
    {
        
        var animation = it.Field<AnimationComponent>(0);
        var dir = it.Field<DirectionComponent>(1);
        var frameArray = it.Field<RenderFrameDataComponent>(2);
        var stateArray = it.Field<StateComponent>(3);
        float delta = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {            
            ref var anim = ref animation[i];
            ref var direction = ref dir[i];
            ref var frame = ref frameArray[i];
            ref var state = ref stateArray[i];
            SpriteAnimationData animationData;
            if (state.stateType != state.lastStateType) // estados diferentes
            {
                state.lastStateType = state.stateType;
                anim.lastAnimationType = anim.animationType;
                // busco cambiar animacion por que cambio estado
                CharacterStateRules rules = CharacterStateConfig.GetRules(state.behaviorType); // tipo de regla
                int stateIndex = (int)state.stateType;
                AnimationType currentAnimation = rules.AnimationMap[stateIndex];                
                if (currentAnimation == AnimationType.NINGUNA)
                {
                    // mantengo la ultima animacion
                    
                    state.stateType = rules.GetNextState(state, anim);
                    currentAnimation = anim.lastAnimationType;
                }
                animationData = AnimationCache.GetAnimation(anim.idSpriteOrAnimation, anim.entityType, currentAnimation, direction);
                //cambio de estado reinicializo frames
                //animationData = AnimationCache.GetAnimation(anim.idSpriteOrAnimation, anim.entityType, anim.animationType, direction);
                anim.TimeSinceLastFrame = 0f;
                anim.animationComplete = false;
                anim.active = true;                
                anim.animationType = currentAnimation;
                anim.frameDuration = animationData.frameDuration;
                frame.uvMap = animationData.uvFramesArray[0];
                anim.currentFrameIndex = 1;
                
            }
            else
            {
                 animationData = AnimationCache.GetAnimation(anim.idSpriteOrAnimation, anim.entityType, anim.animationType, direction);
            }

            //AnimationType currentAnimation = anim.animationType;
            

            anim.TimeSinceLastFrame += delta;
            if (anim.TimeSinceLastFrame >= anim.frameDuration && anim.active)
            {
                anim.TimeSinceLastFrame = 0;
                if (anim.currentFrameIndex >= animationData.uvFramesArray.Length)
                {
                    if (animationData.loop)
                    {
                        frame.uvMap = animationData.uvFramesArray[0];
                        anim.animationComplete = true;
                        anim.currentFrameIndex = 1;
                        anim.active = true;

                        CharacterStateRules rules = CharacterStateConfig.GetRules(state.behaviorType); // tipo de regla
                        //state.lastStateType = state.stateType;
                        state.stateType = rules.GetNextState(state, anim);
                        
                    }
                    else
                    {
                        anim.animationComplete = true;
                        anim.active = false;
                        CharacterStateRules rules = CharacterStateConfig.GetRules(state.behaviorType); // tipo de regla
                        //state.lastStateType = state.stateType;
                        state.stateType = rules.GetNextState(state, anim);
                    }
                }
                else
                {
                    frame.uvMap = animationData.uvFramesArray[anim.currentFrameIndex];                  
                    anim.currentFrameIndex++;
                }
                
            }
        }
    }



 
}
