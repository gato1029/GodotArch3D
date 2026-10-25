using System;
using System.Collections.Generic;
using Godot;
namespace GodotEcsArch.sources.managers.Bars;

public class HealthBarMultiMeshGroup
{
    public Mesh BaseMesh;
    private List<HealthBarMultimeshData> _multimeshes = new();
    private Action<HealthBarMultimeshData> _onCreated;

    public HealthBarMultiMeshGroup(Mesh mesh, Action<HealthBarMultimeshData> onCreated)
    {
        BaseMesh = mesh;
        _onCreated = onCreated;
    }

    public (HealthBarMultimeshData mm, int instanceId) CreateInstance()
    {
        // Buscar un multimesh que tenga espacio libre
        foreach (var mm in _multimeshes)
        {
            if (mm.AvailableSpace())
            {
                int id = mm.CreateInstance();
                return (mm, id);
            }
        }

        // Si todos están llenos, creamos un nuevo bloque MultiMesh automáticamente
        var newMM = new HealthBarMultimeshData(BaseMesh);
        _multimeshes.Add(newMM);
        _onCreated?.Invoke(newMM);

        int newId = newMM.CreateInstance();
        return (newMM, newId);
    }
}