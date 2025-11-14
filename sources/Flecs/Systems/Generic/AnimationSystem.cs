using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
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
            ref var a = ref animation[i];
            ref var d = ref dir[i];
            ref var f = ref frame[i];

            int stateAnimation = a.stateAnimation;
            var (animationStateData, animationData) = GetAnimationData((int)a.idSpriteOrAnimation, a.entityType, stateAnimation, ref d);

            //var dataCharacterModel = CharacterModelManager.Instance.GetCharacterModel(id.id);
            //AnimationStateData animationStateData = dataCharacterModel.animationCharacterBaseData.animationDataArray[stateAnimation];
            //AnimationData animationData = animationStateData.animationData[(int)d.animationDirection];


            if (a.lastStateAnimation != stateAnimation)
            {
                a.TimeSinceLastFrame = 0f;
                a.animationComplete = false;
                a.currentFrameIndex = 1;
                a.active = true;
                a.lastStateAnimation = stateAnimation;
                a.frameDuration = animationData.frameDuration;

                f.uvMap.R = animationData.frameDataArray[0].xFormat; // Reinicia al frame inicial
                f.uvMap.G = animationData.frameDataArray[0].yFormat; // Reinicia al frame inicial
                f.uvMap.B = animationData.frameDataArray[0].widhtFormat; // Reinicia al frame inicial
                f.uvMap.A = animationData.frameDataArray[0].heightFormat; // Reinicia al frame inicial
            }

            a.TimeSinceLastFrame += delta;
            if (a.TimeSinceLastFrame >= a.frameDuration && a.active)
            {
                a.TimeSinceLastFrame = 0;
                if (a.currentFrameIndex >= animationData.frameDataArray.Length)
                {
                    if (animationData.loop)
                    {
                        f.uvMap.R = animationData.frameDataArray[0].xFormat; // Reinicia al frame inicial
                        f.uvMap.G = animationData.frameDataArray[0].yFormat; // Reinicia al frame inicial
                        f.uvMap.B = animationData.frameDataArray[0].widhtFormat; // Reinicia al frame inicial
                        f.uvMap.A = animationData.frameDataArray[0].heightFormat; // Reinicia al frame inicial

                        a.animationComplete = true;
                        a.currentFrameIndex = 0;
                        a.active = true;
                    }
                    else
                    {
                        a.animationComplete = true;
                        a.active = false;
                    }
                }
                else
                {
                    f.uvMap.R = animationData.frameDataArray[a.currentFrameIndex].xFormat; // Reinicia al frame inicial
                    f.uvMap.G = animationData.frameDataArray[a.currentFrameIndex].yFormat; // Reinicia al frame inicial
                    f.uvMap.B = animationData.frameDataArray[a.currentFrameIndex].widhtFormat; // Reinicia al frame inicial
                    f.uvMap.A = animationData.frameDataArray[a.currentFrameIndex].heightFormat; // Reinicia al frame inicial

                    a.currentFrameIndex++;
                }

            }
        }
    }

    private static (AnimationStateData state, AnimationData data) GetAnimationData(int id, EntityType entityType,  int stateAnimation, ref DirectionComponent dir)
    {
        switch (entityType)
        {
            case EntityType.PERSONAJE:
                {
                    var model = CharacterModelManager.Instance.GetCharacterModel(id);
                    var animState = model.animationCharacterBaseData.animationDataArray[stateAnimation];
                    var animData = animState.animationData[(int)dir.animationDirection];
                    return (animState, animData);
                }    

            default:
                throw new InvalidOperationException($"EntityType {id} no soportado para animación.");
        }
    }

}
