using Godot;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Textures;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Mods;


public class RenderManagerOptimized
{
    private TextureArrayManager _textureManager;
    private List<MultiMeshGroup> _groups = new();
    private Dictionary<Rid, MultimeshData> _ridToMultimesh = new();
    public RenderManagerOptimized(TextureArrayManager textureManager)
    {
        _textureManager = textureManager;
    }

    /// <summary>
    /// Construye los grupos de render basados en los builders
    /// </summary>
    public void Build()
    {
        _groups.Clear();

        var builders = _textureManager.GetBuilders();

        for (int i = 0; i < builders.Count; i++)
        {
            var builder = builders[i];

            if (builder.mesh == null)
            {
                GD.PrintErr($"❌ Builder {i} no tiene mesh.");
                continue;
            }

            var group = new MultiMeshGroup(builder.mesh, (mm) =>
            {
                _ridToMultimesh[mm.multimeshRid] = mm;
            });
            _groups.Add(group);
        }
    }

    /// <summary>
    /// Crea una instancia usando el textureId
    /// </summary>
    public (Rid multimeshRid, int instanceId, int layer) CreateInstance(int textureId)
    {
        var loc = _textureManager.GetLocation(textureId);

        if (!loc.IsValid())
        {
            GD.PrintErr($"❌ TextureId inválido: {textureId}");
            return (default, -1, -1);
        }

        if (loc.ArrayIndex < 0 || loc.ArrayIndex >= _groups.Count)
        {
            GD.PrintErr($"❌ ArrayIndex fuera de rango: {loc.ArrayIndex}");
            return (default, -1, -1);
        }

        var group = _groups[loc.ArrayIndex];

        var (mm, id) = group.CreateInstance();

        return (mm.multimeshRid, id, loc.LayerIndex);
    }

    /// <summary>
    /// Libera una instancia
    /// </summary>
    public void FreeInstance(Rid multimeshRid, int instanceId)
    {
        if (instanceId < 0)
            return;

        var mm = FindMultiMesh(multimeshRid);

        if (mm == null)
        {
            GD.PrintErr("❌ Multimesh no encontrado para liberar.");
            return;
        }

        mm.FreeInstance(instanceId);
    }
    private MultimeshData FindMultiMesh(Rid rid)
    {
        if (_ridToMultimesh.TryGetValue(rid, out var mm))
            return mm;

        return null;
    }
}