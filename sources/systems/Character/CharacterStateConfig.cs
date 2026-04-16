// Define reglas por personaje
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;

public class CharacterStateRules
{
    public Func<GodotFlecs.sources.Flecs.Components.CharacterComponent, GodotFlecs.sources.Flecs.Components.AnimationComponent, CharacterStateType>? OnAnimationComplete;
    public Dictionary<CharacterStateType, AnimationType> StateToAnimation = new();
}

// Configuración centralizada
public static class CharacterStateConfig
{
    private static Dictionary<CharacterBehaviorType, CharacterStateRules> _rulesByCharacter = new();

    static CharacterStateConfig()
    {
        // Reglas del PersonajePrincipal (id = 1)
        rulePersonajePrincipal();
        ruleEstadoComun();
        
    }
    private static void ruleEstadoComun()
    {
        _rulesByCharacter[CharacterBehaviorType.GENERICO] = new CharacterStateRules
        {
            OnAnimationComplete = (character, anim) =>
            {
                return character.characterStateType switch
                {
                    CharacterStateType.ATTACK => CharacterStateType.IDLE,
                    CharacterStateType.EXECUTE_ATTACK => CharacterStateType.IDLE,
                    CharacterStateType.TAKE_HIT => CharacterStateType.IDLE,
                    CharacterStateType.TAKE_STUN => CharacterStateType.IDLE,
                    CharacterStateType.DIE => CharacterStateType.DIE,
                    CharacterStateType.BLOCKED => CharacterStateType.IDLE,// se queda en DIE, se destruye luego
                    _ => character.characterStateType
                };
            },
            StateToAnimation = new Dictionary<CharacterStateType, AnimationType>
                {
                    { CharacterStateType.IDLE,  AnimationType.PARADO  },
                    { CharacterStateType.MOVING, AnimationType.CAMINANDO },
                    { CharacterStateType.ATTACK, AnimationType.ATACANDO },               
                    { CharacterStateType.TAKE_HIT, AnimationType.RECIBE_DANIO },
                    { CharacterStateType.TAKE_STUN, AnimationType.STUNEADO },
                    { CharacterStateType.DIE, AnimationType.MUERTO },
                }
        };
    }
    private static void rulePersonajePrincipal()
    {
        _rulesByCharacter[CharacterBehaviorType.PERSONAJE_PRINCIPAL] = new CharacterStateRules
        {
            OnAnimationComplete = (character, anim) =>
            {
                return character.characterStateType switch
                {
                    CharacterStateType.ATTACK => CharacterStateType.ATTACK,
                    CharacterStateType.EXECUTE_ATTACK => CharacterStateType.ATTACK,
                    CharacterStateType.TAKE_HIT => CharacterStateType.IDLE,
                    CharacterStateType.DIE => CharacterStateType.DIE, // se queda en DIE, se destruye luego
                    _ => character.characterStateType
                };
            },
            StateToAnimation = new Dictionary<CharacterStateType, AnimationType>
                {
                    { CharacterStateType.IDLE,  AnimationType.PARADO  },
                    { CharacterStateType.MOVING, AnimationType.CAMINANDO },
                    { CharacterStateType.ATTACK, AnimationType.ATACANDO },
                    { CharacterStateType.EXECUTE_ATTACK, AnimationType.ATACANDO }, // opcional: animación especial // revisar luego para quitarlo
                    { CharacterStateType.TAKE_HIT, AnimationType.RECIBE_DANIO },
                    { CharacterStateType.DIE, AnimationType.MUERTO },
                }
        };
    }

    public static CharacterStateRules GetRules(CharacterBehaviorType baseId)
    {
        if (_rulesByCharacter.TryGetValue(baseId, out var rules))
            return rules;

        // Si no hay reglas específicas, usar reglas por defecto
        return new CharacterStateRules
        {
            OnAnimationComplete = (c, a) => c.characterStateType,
            StateToAnimation = new Dictionary<CharacterStateType, AnimationType>
                {
                    { CharacterStateType.IDLE, AnimationType.PARADO  }
                }
        };
    }



}
