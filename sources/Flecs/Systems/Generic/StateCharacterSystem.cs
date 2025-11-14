using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
internal class StateCharacterSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; // debe ser single-thread


    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<CharacterComponent>()
           .With<AnimationComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var charArray = it.Field<CharacterComponent>(0);
        var aniArray = it.Field<AnimationComponent>(1);
        for (int i = 0; i < it.Count(); i++)
        {
            var e = it.Entity(i);
            ref var cha = ref charArray[i];
            ref var ani = ref aniArray[i];

            CharacterStateRules rules = CharacterStateConfig.GetRules(cha.characterBehaviorType);
        
            if (rules.StateToAnimation.TryGetValue(cha.characterStateType, out int newAnim))
            {
            
                if (newAnim != ani.stateAnimation)
                {
                    ani.lastStateAnimation = ani.stateAnimation;
                    ani.stateAnimation = newAnim;
                    ani.currentFrameIndex = 0;       // reset animación
                    ani.TimeSinceLastFrame = 0f;     // reset timer
                    ani.animationComplete = false;   // empezar de nuevo
                }
            }
            // transicion
            if (ani.animationComplete)
            {                    
                var newState = rules.OnAnimationComplete?.Invoke(cha, ani);
                if (newState.HasValue && newState.Value != cha.characterStateType)
                {
                    cha.characterStateType = newState.Value;

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
        qb.With<CharacterComponent>()
          .With<RenderLayerListComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var characters = it.Field<CharacterComponent>(0);
        var layersList = it.Field<RenderLayerListComponent>(1);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var cha = ref characters[i];
            ref var layers = ref layersList[i];

            // 1️⃣ Obtener las reglas del comportamiento del personaje
            CharacterStateRules rules = CharacterStateConfig.GetRules(cha.characterBehaviorType);

            // 2️⃣ Determinar animación base (cuerpo) según el estado
            if (rules.StateToAnimation.TryGetValue(cha.characterStateType, out int newAnim))
            {
                ref var mainAnim = ref layers.Animations[0]; // capa 0 = cuerpo principal

                if (newAnim != mainAnim.stateAnimation)
                {
                    mainAnim.lastStateAnimation = mainAnim.stateAnimation;
                    mainAnim.stateAnimation = newAnim;
                    mainAnim.currentFrameIndex = 0;
                    mainAnim.TimeSinceLastFrame = 0f;
                    mainAnim.animationComplete = false;
                }
            }

            // 3️⃣ Transiciones de estado (cuando termina la animación principal)
            ref var baseAnimation = ref layers.Animations[0];
            if (baseAnimation.animationComplete)
            {
                var newState = rules.OnAnimationComplete?.Invoke(cha, baseAnimation);
                if (newState.HasValue && newState.Value != cha.characterStateType)
                {
                    cha.characterStateType = newState.Value;
                }
            }           
        }
    }
}