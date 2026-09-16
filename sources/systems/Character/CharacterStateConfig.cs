using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;

public class CharacterStateRules
{
    // Función normal de transición de estados
    public StateType GetNextState(GodotFlecs.sources.Flecs.Components.StateComponent character, AnimationComponent anim)
    {
        return character.stateType switch
        {
            StateType.ATTACK => StateType.EXECUTE_ATTACK,
            StateType.EXECUTE_ATTACK => StateType.ATTACK,
            StateType.TAKE_HIT => StateType.IDLE,
            StateType.TAKE_STUN => StateType.IDLE,
            StateType.DIE => StateType.DIE,
            StateType.BLOCKED => StateType.IDLE,
            _ => character.stateType
        };
    }

    // Reemplazamos el Dictionary por un arreglo plano indexado por el enum (CharacterStateType)
    public AnimationType[] AnimationMap;
}

public static class CharacterStateConfig
{
    // Usamos un arreglo plano en lugar de un Dictionary. 
    // El tamaño debe ser igual o mayor al número total de elementos en el enum CharacterBehaviorType.
    private static readonly CharacterStateRules[] _rulesArray = new CharacterStateRules[16];

    static CharacterStateConfig()
    {
        // 1. Configurar Reglas para GENERICO
        var genericRules = new CharacterStateRules
        {
            AnimationMap = new AnimationType[16] // Tamaño basado en el número de estados
        };

        // Asignamos directamente por índice numérico del enum (Cero hashmaps)
        genericRules.AnimationMap[(int)StateType.IDLE] = AnimationType.PARADO;
        genericRules.AnimationMap[(int)StateType.MOVING] = AnimationType.CAMINANDO;
        genericRules.AnimationMap[(int)StateType.ATTACK] = AnimationType.ATACANDO_CUERPO;
        genericRules.AnimationMap[(int)StateType.TAKE_HIT] = AnimationType.RECIBE_DANIO;
        genericRules.AnimationMap[(int)StateType.EXECUTE_ATTACK] = AnimationType.NINGUNA;
        genericRules.AnimationMap[(int)StateType.TAKE_STUN] = AnimationType.STUNEADO;
        genericRules.AnimationMap[(int)StateType.DIE] = AnimationType.MUERTO;

        // 2. Configurar Reglas para PERSONAJE_PRINCIPAL
        var principalRules = new CharacterStateRules
        {
            AnimationMap = new AnimationType[16]
        };

        principalRules.AnimationMap[(int)StateType.IDLE] = AnimationType.PARADO;
        principalRules.AnimationMap[(int)StateType.MOVING] = AnimationType.CAMINANDO;
        principalRules.AnimationMap[(int)StateType.ATTACK] = AnimationType.ATACANDO_CUERPO;
        principalRules.AnimationMap[(int)StateType.EXECUTE_ATTACK] = AnimationType.NINGUNA;
        principalRules.AnimationMap[(int)StateType.TAKE_HIT] = AnimationType.RECIBE_DANIO;
        principalRules.AnimationMap[(int)StateType.DIE] = AnimationType.MUERTO;

        // Guardamos en el arreglo principal usando el enum casteado a entero como índice
        _rulesArray[(int)BehaviorType.GENERICO] = genericRules;
        _rulesArray[(int)BehaviorType.PERSONAJE_PRINCIPAL] = principalRules;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static CharacterStateRules GetRules(BehaviorType baseId)
    {
        int index = (int)baseId;
        if (index >= 0 && index < _rulesArray.Length && _rulesArray[index] != null)
        {
            return _rulesArray[index];
        }

        // Fallback por defecto si no existe
        return _rulesArray[(int)BehaviorType.GENERICO];
    }
}