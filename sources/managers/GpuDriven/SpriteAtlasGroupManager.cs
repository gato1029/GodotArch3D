namespace GodotEcsArch.sources.managers.GpuDriven;

using Godot;
using System.Collections.Generic;

public class SpriteAtlasGroupManager
{
    public int MaxLayersPerArray { get; set; } = 256;

    // Diccionario de Materiales base registrados por PipelineId
    private readonly Dictionary<int, ShaderMaterial> _registeredMaterials = new();
    
    // Diccionario de Builders de Atlas por PipelineId
    private readonly Dictionary<int, List<GpuTextureArrayBuilder>> _pipelineBuilders = new();

    private readonly Dictionary<string, int> _nameToGlobalId = new();
    private readonly Dictionary<int, TextureLocation> _globalMap = new();

    /// <summary>
    /// Permite dar de alta cualquier Shader/Pipeline nuevo en tiempo de ejecución.
    /// </summary>
    public void RegisterPipelineMaterial(int pipelineId, ShaderMaterial baseMaterial)
    {
        _registeredMaterials[pipelineId] = baseMaterial;
        if (!_pipelineBuilders.ContainsKey(pipelineId))
        {
            _pipelineBuilders[pipelineId] = new List<GpuTextureArrayBuilder>();
        }
    }

    public void RegisterTexture(string idName, int globalId, string path, int pipelineId)
    {
        if (_nameToGlobalId.TryGetValue(idName, out int existingId) && existingId != globalId)
        {
            GD.PrintErr($"❌ ID inconsistente para {idName}: {existingId} vs {globalId}");
            return;
        }

        _nameToGlobalId[idName] = globalId;
        RegisterTexture(globalId, path, pipelineId);
    }

    public void RegisterTexture(int globalIndex, string path, int pipelineId)
    {
        if (!_registeredMaterials.TryGetValue(pipelineId, out var baseMaterial))
        {
            GD.PrintErr($"❌ El PipelineId {pipelineId} no ha sido registrado con un ShaderMaterial.");
            return;
        }

        var builders = _pipelineBuilders[pipelineId];
        int builderIndex = globalIndex / MaxLayersPerArray;
        int layerIndex = globalIndex % MaxLayersPerArray;

        while (builders.Count <= builderIndex)
        {
            builders.Add(new GpuTextureArrayBuilder(baseMaterial, pipelineId)
            {
                MaxLayers = MaxLayersPerArray
            });
        }

        builders[builderIndex].SetTextureAt(layerIndex, path);
        _globalMap[globalIndex] = new TextureLocation(builderIndex, layerIndex, pipelineId);
    }

    public void BuildAll()
    {
        foreach (var list in _pipelineBuilders.Values)
        {
            foreach (var builder in list)
            {
                builder.BuildAndApply();
            }
        }
    }
//  Devuelve todos los PipelineIds que han sido registrados
    public IEnumerable<int> GetRegisteredPipelineIds()
    {
        return _pipelineBuilders.Keys;
    }

    //  Obtiene el Global ID asignado a un nombre clave
    public int GetGlobalIdByName(string textureIdName)
    {
        if (_nameToGlobalId.TryGetValue(textureIdName, out int globalId))
        {
            return globalId;
        }

        GD.PrintErr($"❌ El nombre de textura '{textureIdName}' no está registrado.");
        return -1;
    }
    public TextureLocation GetLocation(string textureId)
    {
        if (_nameToGlobalId.TryGetValue(textureId, out int index))
            return GetLocation(index);

        return new TextureLocation(-1, -1, -1);
    }

    public TextureLocation GetLocation(int globalIndex)
    {
        if (_globalMap.TryGetValue(globalIndex, out var location))
            return location;

        return new TextureLocation(-1, -1, -1);
    }

    public IReadOnlyList<GpuTextureArrayBuilder> GetBuilders(int pipelineId)
    {
        if (_pipelineBuilders.TryGetValue(pipelineId, out var list))
            return list;

        return System.Array.Empty<GpuTextureArrayBuilder>();
    }
}