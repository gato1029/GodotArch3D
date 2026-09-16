using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;

namespace GodotFlecs.sources.Flecs.Systems.Generic;

internal class StateCharacterSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;


    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<StateComponent>()
           .With<AnimationComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var charArray = it.Field<StateComponent>(0);
        var aniArray = it.Field<AnimationComponent>(1);
        for (int i = 0; i < it.Count(); i++)
        {
            var e = it.Entity(i);
            ref var cha = ref charArray[i];
            ref var ani = ref aniArray[i];

            CharacterStateRules rules = CharacterStateConfig.GetRules(cha.behaviorType);

            int stateIndex = (int)cha.stateType;
            if (stateIndex >= 0 && stateIndex < rules.AnimationMap.Length)
            {
                AnimationType newAnim = rules.AnimationMap[stateIndex];
                if (newAnim == AnimationType.NINGUNA)
                {
                    ani.animationType = ani.lastAnimationType;
                    ani.lastAnimationType = AnimationType.NINGUNA;
                    ani.currentFrameIndex = 0;       // reset animación
                    ani.TimeSinceLastFrame = 0f;     // reset timer
                    ani.animationComplete = false;   // empezar de nuevo
                    var newState = rules.GetNextState(cha, ani);
                    if (newState != cha.stateType)
                    {
                        cha.stateType = newState;
                    }
                }
                else
                {
                    if (newAnim != ani.animationType)
                    {

                        ani.lastAnimationType = ani.animationType;
                        ani.animationType = newAnim;
                        ani.currentFrameIndex = 0;       // reset animación
                        ani.TimeSinceLastFrame = 0f;     // reset timer
                        ani.animationComplete = false;   // empezar de nuevo
                    }
                }

            }
            // transicion
            if (ani.animationComplete)
            {
                var newState = rules.GetNextState(cha, ani);
                if ( newState!= cha.stateType)
                {
                    cha.stateType = newState;
                }


            }



        }

    }
}

internal class CharacterStateLayerSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;

    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<StateComponent>()
          .With<RenderLayerListComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var characters = it.Field<StateComponent>(0);
        var layersList = it.Field<RenderLayerListComponent>(1);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var cha = ref characters[i];
            ref var layers = ref layersList[i];

            // 1️⃣ Obtener las reglas del comportamiento del personaje
            CharacterStateRules rules = CharacterStateConfig.GetRules(cha.behaviorType);


            int stateIndex = (int)cha.stateType;
            if (stateIndex >= 0 && stateIndex < rules.AnimationMap.Length)
            {
                AnimationType newAnim = rules.AnimationMap[stateIndex];
                ref var mainAnim = ref layers.Animations[0]; // capa 0 = cuerpo principal

                if (newAnim != mainAnim.animationType)
                {
                    mainAnim.lastAnimationType = mainAnim.animationType;
                    mainAnim.animationType = newAnim;
                    mainAnim.currentFrameIndex = 0;
                    mainAnim.TimeSinceLastFrame = 0f;
                    mainAnim.animationComplete = false;
                }
            }

            // 3️⃣ Transiciones de estado (cuando termina la animación principal)
            ref var baseAnimation = ref layers.Animations[0];
            if (baseAnimation.animationComplete)
            {
                var newState = rules.GetNextState(cha, baseAnimation);
                if (newState != cha.stateType)
                {
                    cha.stateType = newState;
                }
            }
        }
    }
}