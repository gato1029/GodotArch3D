using GodotEcsArch.sources.components;
using GodotFlecs.sources.Flecs.Components;

namespace GodotEcsArch.sources.managers.GpuDriven;
using Godot;
using System.Collections.Generic;

public static class PipelineBufferConfig
{
    private static readonly Dictionary<int, Dictionary<int, string>> _configs = new();

    static PipelineBufferConfig()
    {
        // 🔹 Pipeline Estático: Transform, StaticData y Flags
        _configs[(int)RenderPipelineType.Static] = new Dictionary<int, string>
        {
            { BufferSlot.Transform, "transform_texture" },
            { BufferSlot.StaticData, "static_data_texture" },
            { BufferSlot.Flags, "flags_texture" }
        };

        // 🔹 Pipeline Animado: Transform, AnimState y Flags
        _configs[(int)RenderPipelineType.AnimatedUnits] = new Dictionary<int, string>
        {
            { BufferSlot.Transform, "transform_texture" },
            { BufferSlot.AnimState, "anim_state_texture" },
            { BufferSlot.Flags, "flags_texture" }
        };
        // 🔹 Pipeline Animado: Transform, AnimState y Flags
        _configs[(int)RenderPipelineType.SimpleAnimation] = new Dictionary<int, string>
        {
            { BufferSlot.Transform, "transform_texture" },
            { BufferSlot.AnimState, "anim_state_texture" },
            { BufferSlot.Flags, "flags_texture" }
        };
    }

    public static void RegisterCustomPipeline(int pipelineId, Dictionary<int, string> requiredSlotUniforms)
    {
        _configs[pipelineId] = requiredSlotUniforms;
    }

    public static Dictionary<int, string> GetRequiredBuffers(int pipelineId)
    {
        if (_configs.TryGetValue(pipelineId, out var dict))
            return dict;

        return new Dictionary<int, string>
        {
            { BufferSlot.Transform, "transform_texture" },
            { BufferSlot.Flags, "flags_texture" }
        };
    }
}


public class GpuDrivenRenderManager
{
    // ============================================================
    // FIELDS & PROPERTIES
    // ============================================================

    private readonly SpriteAtlasGroupManager _atlasGroupManager;
 
    private readonly UnitAnimCatalog _unitAnimCatalog;
    private readonly SimpleAnimCatalog _simpleAnimCatalog;

  

    private readonly Dictionary<int, List<GpuMultiMeshGroup>> _pipelineGroups = new();

    // Sistema original: acceso mediante RID.
    private readonly Dictionary<Rid, GpuMultiMeshData> _ridToDataMap = new();

    // Nuevo sistema: acceso directo mediante índice.
    private readonly List<GpuMultiMeshData> _gpuDataList = new();

    public SpriteAtlasGroupManager AtlasManager => _atlasGroupManager;

    public UnitAnimCatalog UnitAnimations => _unitAnimCatalog;
    public SimpleAnimCatalog SimpleAnimations => _simpleAnimCatalog;
    public int GpuDataCount => _gpuDataList.Count;

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public GpuDrivenRenderManager(
        SpriteAtlasGroupManager atlasGroupManager,
        int initialMaxUnitTypes = 256)
    {
        _atlasGroupManager = atlasGroupManager;
        _unitAnimCatalog = new UnitAnimCatalog(initialMaxUnitTypes);
        _simpleAnimCatalog = new SimpleAnimCatalog();
    }

    // ============================================================
    // INITIALIZATION & BUILD
    // ============================================================

    /// <summary>
    /// Construye los atlas, LUT y grupos MultiMesh.
    /// Registra cada GpuMultiMeshData mediante RID e índice.
    /// </summary>
    public void Build()
    {
        _pipelineGroups.Clear();
        _ridToDataMap.Clear();
        _gpuDataList.Clear();

        var registeredPipelineIds = _atlasGroupManager.GetRegisteredPipelineIds();

        foreach (int pipelineId in registeredPipelineIds)
        {
            var groupsForPipeline = new List<GpuMultiMeshGroup>();
            var builders = _atlasGroupManager.GetBuilders(pipelineId);
            var requiredBuffers = PipelineBufferConfig.GetRequiredBuffers(pipelineId);

            for (int i = 0; i < builders.Count; i++)
            {
                var builder = builders[i];

                if (builder.BaseMesh == null)
                    continue;

         
                var group = new GpuMultiMeshGroup(
                    builder.BaseMesh,
                    builder.TargetMaterial,
                    pipelineId,
                    requiredBuffers,
                    (gpuData) =>
                    {
                        // Asignamos un índice global estable.
                        int gpuDataIndex = _gpuDataList.Count;
                        gpuData.GpuDataIndex = gpuDataIndex;

                        // Nuevo sistema.
                        _gpuDataList.Add(gpuData);

                        // Sistema original.
                        _ridToDataMap[gpuData.MultimeshRid] = gpuData;
                    }
                );

                groupsForPipeline.Add(group);
            }

            _pipelineGroups[pipelineId] = groupsForPipeline;
        }

        GD.Print(
            $"✅ [GpuDrivenRenderManager] Inicializados " +
            $"{_pipelineGroups.Count} pipelines, " +
            $"{_gpuDataList.Count} GpuMultiMeshData y LUT de animaciones."
        );
    }
    public void PostBuild()
    {
        foreach (int pipelineId in _pipelineGroups.Keys)
        {
            var builders = _atlasGroupManager.GetBuilders(pipelineId);

            foreach (var builder in builders)
            {
                if (builder.BaseMesh == null)
                    continue;

                if (pipelineId == (int)RenderPipelineType.AnimatedUnits)
                {
                    _unitAnimCatalog.BuildAndApply(builder.TargetMaterial);
                }
                else if (pipelineId == (int)RenderPipelineType.SimpleAnimation)
                {
                    _simpleAnimCatalog.BuildAndApply(builder.TargetMaterial);
                }
            }
        }

        GD.Print("✅ [GpuDrivenRenderManager] PostBuild completado. Recursos de animación asignados.");
    }
    // ============================================================
    // GETTERS (GPU DATA)
    // ============================================================

    /// <summary>
    /// Obtiene GpuMultiMeshData mediante RID (Sistema original).
    /// </summary>
    public GpuMultiMeshData GetGpuMeshData(Rid multimeshRid)
    {
        if (_ridToDataMap.TryGetValue(multimeshRid, out var gpuData))
            return gpuData;

        GD.PrintErr($"❌ No se encontró GpuMultiMeshData para el RID: {multimeshRid}");
        return null;
    }

    /// <summary>
    /// Obtiene GpuMultiMeshData mediante índice (Acceso directo O(1)).
    /// </summary>
    public GpuMultiMeshData GetGpuMeshData(int gpuDataIndex)
    {
        if ((uint)gpuDataIndex < (uint)_gpuDataList.Count)
            return _gpuDataList[gpuDataIndex];

        GD.PrintErr($"❌ GpuDataIndex inválido: {gpuDataIndex}");
        return null;
    }

    // ============================================================
    // INSTANCE MANAGEMENT (CREATE)
    // ============================================================

    /// <summary>
    /// Crea una instancia mediante el nombre de textura.
    /// </summary>
    public (Rid multimeshRid, int gpuDataIndex, int instanceId, int layer, int pipelineId) CreateInstance(string textureIdName)
    {
        int globalId = _atlasGroupManager.GetGlobalIdByName(textureIdName);

        if (globalId < 0)
            return (default, -1, -1, -1, -1);

        return CreateInstance(globalId);
    }

    /// <summary>
    /// Crea una instancia mediante su ID global de textura.
    /// Devuelve RID e índice interno GpuDataIndex.
    /// </summary>
    public (Rid multimeshRid, int gpuDataIndex, int instanceId, int layer, int pipelineId) CreateInstance(int globalTextureId)
    {
        var loc = _atlasGroupManager.GetLocation(globalTextureId);

        if (!loc.IsValid() || !_pipelineGroups.TryGetValue(loc.PipelineId, out var groups))
        {
            GD.PrintErr($"❌ Error al ubicar la textura global ID: {globalTextureId}");
            return (default, -1, -1, -1, -1);
        }

        var group = groups[loc.ArrayIndex];
        var (gpuData, instanceId) = group.CreateInstance();

        return (
            gpuData.MultimeshRid,
            gpuData.GpuDataIndex,
            instanceId,
            loc.LayerIndex,
            loc.PipelineId
        );
    }

    // ============================================================
    // INSTANCE MANAGEMENT (ANIMATION)
    // ============================================================

    /// <summary>
    /// Cambia animación mediante RID (Sistema original).
    /// </summary>
    public void SetInstanceAnimation(
        Rid multimeshRid,
        int instanceId,
        int unitTypeId,
        AnimationType animType,
        AnimationDirection direction)
    {
        var gpuData = GetGpuMeshData(multimeshRid);
        if (gpuData != null)
        {
            SetInstanceUnitAnimation(gpuData, instanceId, unitTypeId, animType, direction);
        }
    }

    /// <summary>
    /// Cambia animación mediante GpuDataIndex.
    /// </summary>
    public void SetInstanceUnitAnimation(
        int gpuDataIndex,
        int instanceId,
        int unitTypeId,
        AnimationType animType,
        AnimationDirection direction)
    {
        var gpuData = GetGpuMeshData(gpuDataIndex);
        if (gpuData != null)
        {
            SetInstanceUnitAnimation(gpuData, instanceId, unitTypeId, animType, direction);
        }
    }

    /// <summary>
    /// Sobrecarga directa utilizando GpuMultiMeshData.
    /// </summary>
    public void SetInstanceUnitAnimation(
        GpuMultiMeshData gpuData,
        int instanceId,
        int unitTypeId,
        AnimationType animType,
        AnimationDirection direction)
    {
        if (gpuData == null)
            return;

        int lutIndex = _unitAnimCatalog.GetLutIndex(unitTypeId, animType, direction);
        float currentTime = (float)Time.GetTicksMsec() / 1000.0f;

        gpuData.SetBufferData(
            BufferSlot.AnimState,
            instanceId,
            r: lutIndex,
            g: (float)animType,
            b: currentTime,
            a: 0.0f
        );

        float mirrorX = (direction == AnimationDirection.LEFT ||
                         direction == AnimationDirection.LEFTUP ||
                         direction == AnimationDirection.LEFTDOWN) ? 1.0f : 0.0f;

        gpuData.SetBufferData(
            BufferSlot.Flags,
            instanceId,
            r: mirrorX,
            g: 0.0f,
            b: 1.0f,
            a: 0.0f
        );
    }

    // ============================================================
    // INSTANCE MANAGEMENT (FREE)
    // ============================================================

    /// <summary>
    /// Libera una instancia mediante RID (Sistema original).
    /// </summary>
    public void FreeInstance(Rid multimeshRid, int instanceId)
    {
        if (instanceId < 0)
            return;

        if (_ridToDataMap.TryGetValue(multimeshRid, out var gpuData))
        {
            gpuData.FreeInstance(instanceId);
        }
        else
        {
            GD.PrintErr("❌ RID de MultiMesh no encontrado para liberar.");
        }
    }

    /// <summary>
    /// Libera una instancia mediante GpuDataIndex (Nuevo sistema).
    /// </summary>
    public void FreeInstance(int gpuDataIndex, int instanceId)
    {
        if (instanceId < 0)
            return;

        var gpuData = GetGpuMeshData(gpuDataIndex);
        gpuData?.FreeInstance(instanceId);
    }
    private float _globalTime;

    public void UpdateGlobalTime(float deltaTime)
    {
        _globalTime += deltaTime;

     //   float shaderTime = _globalTime;

        foreach (var gpuData in _gpuDataList)
        {
            if (gpuData.RequiresAnimationTime)
                gpuData.UpdateGlobalTime(_globalTime);
        }
    }
    // ============================================================
    // FLUSH
    // ============================================================

    /// <summary>
    /// Sube todos los buffers modificados a VRAM.
    /// Ejecutar después de finalizar todos los Jobs.
    /// </summary>
    public void Flush()
    {
        foreach (var groupList in _pipelineGroups.Values)
        {
            foreach (var group in groupList)
            {
                foreach (var gpuData in group.DataList)
                {
                    if (gpuData.HasDirtyBuffers)
                    {
                        gpuData.FlushToGPU();
                    }
                }
            }
        }
    }
}