namespace GodotEcsArch.sources.managers.GpuDriven;

using Godot;
using System;
using System.Collections.Generic;


public class GpuMultiMeshGroup
{
    public Mesh BaseMesh { get; private set; }
    public ShaderMaterial BaseMaterial { get; private set; }
    public int PipelineId { get; private set; }

    private readonly List<GpuMultiMeshData> _dataList = new();
    private readonly Action<GpuMultiMeshData> _onDataCreated;
    private readonly Dictionary<int, string> _requiredSlotUniforms;

    public GpuMultiMeshGroup(
        Mesh mesh, 
        ShaderMaterial baseMaterial, 
        int pipelineId, 
        Dictionary<int, string> requiredSlotUniforms, 
        Action<GpuMultiMeshData> onDataCreated)
    {
        BaseMesh = mesh;
        BaseMaterial = baseMaterial;
        PipelineId = pipelineId;
        _requiredSlotUniforms = requiredSlotUniforms;
        _onDataCreated = onDataCreated;
    }

    public IReadOnlyList<GpuMultiMeshData> DataList => _dataList;

    public (GpuMultiMeshData data, int instanceId) CreateInstance()
    {
        foreach (var data in _dataList)
        {
            if (data.HasAvailableSpace())
            {
                int id = data.CreateInstance();
                return (data, id);
            }
        }

        // Si se llenaron los bloques anteriores, crea un NUEVO GpuMultiMeshData
        var newData = new GpuMultiMeshData(BaseMesh, BaseMaterial, _requiredSlotUniforms);
        _dataList.Add(newData);
        _onDataCreated?.Invoke(newData);

        int newId = newData.CreateInstance();
        return (newData, newId);
    }
}