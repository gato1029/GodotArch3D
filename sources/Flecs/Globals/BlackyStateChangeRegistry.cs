using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.GpuDriven;
using GodotEcsArch.sources.managers.Mods;
using GodotFlecs.sources.Flecs.Components;

namespace GodotEcsArch.sources.Flecs.Globals;

public struct UnitRuntimeData
{
    public int GpuIndex;
    public int ShaderId;
    public int IdUnitAnimation;
    public BehaviorType BehaviorType;
    public StateType CurrentState;
    public AnimationDirection Direction;
}

public class BlackyStateChangeRegistry
{
    // Diccionario principal: La entidad (ulong) apunta directamente a su estructura completa
    private readonly ConcurrentDictionary<ulong, UnitRuntimeData> _entityData = new();
    
    // Cola o registro de entidades que cambiaron en este frame (guardamos solo el ulong del ID)
    private readonly ConcurrentDictionary<ulong, StateType> _pendingChanges = new();

    public void RegisterEntity(ulong entityId, int gpuIndex, int shaderId, int idUnitAnimation, BehaviorType behaviorType, StateType stateType = StateType.IDLE, AnimationDirection initialDirection = AnimationDirection.LEFT)
    {
        _entityData[entityId] = new UnitRuntimeData
        {
            GpuIndex = gpuIndex,
            ShaderId = shaderId,
            IdUnitAnimation = idUnitAnimation,
            BehaviorType = behaviorType,
            CurrentState = stateType,
            Direction = initialDirection
        };
        
        _pendingChanges[entityId] = stateType;
    }

    public void RegisterState(ulong entityId, StateType state)
    {
        if (_entityData.TryGetValue(entityId, out var data))
        {
            // Si el estado ya es el mismo, salimos de inmediato (cero trabajo innecesario)
            if (data.CurrentState == state) return;

            data.CurrentState = state;
            _entityData[entityId] = data;
            _pendingChanges[entityId] = state;
        }
    }

    public void SetDirection(ulong entityId, AnimationDirection direction)
    {
        if (_entityData.TryGetValue(entityId, out var data))
        {
            // Si la dirección ya es la misma, no hacemos nada
            if (data.Direction == direction) return;

            data.Direction = direction;
            _entityData[entityId] = data;
        
            // Marcamos como pendiente usando su estado actual para recalcular el LUT con la nueva dirección
            _pendingChanges[entityId] = data.CurrentState;
        }
    }

    public void ApplyChanges()
    {
        // Tomamos el snapshot instantáneo de los pendientes
        var changes = _pendingChanges.ToArray();
        _pendingChanges.Clear();

        if (changes.Length == 0) return;

        Parallel.ForEach(changes, change =>
        {
            ulong entityId = change.Key;
            StateType state = change.Value;

            // Único TryGetValue necesario en el hilo para rescatar de golpe GPUIndex, ShaderId y compañía
            if (!_entityData.TryGetValue(entityId, out var data))
                return;

            // Actualización directa a la GPU usando ambos valores empaquetados
            UpdateAnimation(data.ShaderId, state, data.Direction, data.IdUnitAnimation, data.GpuIndex, data.BehaviorType);
        });
    }

    public void Release(ulong entityId)
    {
        _entityData.TryRemove(entityId, out _);
        _pendingChanges.TryRemove(entityId, out _);
    }
    // Al iniciar tu juego o gestor, guarda el tiempo base:
  
    private void UpdateAnimation(int shaderId, StateType state, AnimationDirection direction, int idUnitAnimation, int gpuIndex, BehaviorType behaviorType)
    {
        var rules = CharacterStateConfig.GetRules(behaviorType);
        AnimationType newAnim = rules.AnimationMap[(int)state];
        
        int lutIndex = AtlasTexturesModsManager.Instance.DrivenRenderManager.UnitAnimations.GetLutIndex(idUnitAnimation, newAnim, direction);
        float currentTime = Time.GetTicksMsec() / 1000.0f; // Sincronizador de inicio de animación
        AtlasTexturesModsManager.Instance.DrivenRenderManager.GetGpuMeshData(gpuIndex)
            .SetBufferData(BufferSlot.AnimState, shaderId, lutIndex,currentTime);
    }
}