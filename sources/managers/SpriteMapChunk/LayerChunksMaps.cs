using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;
public class LayerInfo
{
    public string Name { get; set; }
    public string InternalPath { get; set; }
}
public class LayerChunksMaps<T> : IEnumerable<KeyValuePair<string, SpriteMapChunk<T>>> where T : DataItem, new()
{
    // Capa real: nombre → instancia
    [ProtoIgnore, JsonIgnore]
    private readonly Dictionary<string, SpriteMapChunk<T>> layers;

    // Alias: alias → nombre de capa original
    [ProtoIgnore, JsonIgnore]
    private readonly Dictionary<string, string> aliases;
    [ProtoIgnore, JsonIgnore]
    private FormatSave formatSave;
    private string name = "LayersInfo";
    private string pathBase;
    public LayerChunksMaps(string pathBase, FormatSave formatSave = FormatSave.BINARIO)
    {
        this.formatSave = formatSave;
        this.pathBase = pathBase;
        layers = new Dictionary<string, SpriteMapChunk<T>>();
        aliases = new Dictionary<string, string>();
    }
    public IEnumerator<KeyValuePair<string, SpriteMapChunk<T>>> GetEnumerator()
    {
        return layers.GetEnumerator(); // Solo iteramos capas reales
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    // Agrega o reemplaza una capa real
    public void AddLayer(string name, int layer, bool render = true,bool SerializeOnUnload = false)
    {
        layers[name] = new SpriteMapChunk<T>(name, pathBase, layer, ChunkManager.Instance.tiles16X16, SerializeOnUnload,render, formatSave);
    }

    public void SaveAll()
    {
        var layerInfos = layers.Select(kvp => new LayerInfo
        {
            Name = kvp.Key,
            InternalPath = kvp.Value.CarpetSave 
        }).ToList();
        SerializerManager.SaveToFileJson(layerInfos, pathBase, name);
        foreach (var item in layers)
        {
            item.Value.SaveAll();
        }
    }
    public void SetRenderEnableAllLayers(bool render)
    {
        foreach (var item in layers)
        {
            item.Value.SetRenderEnabled(render);
        }
    }
    public void LoadAll()
    {
        
        foreach (var item in layers)
        {
            item.Value.LoadAll();
        }        
    }
 
    // Agrega un alias a una capa ya existente
    public bool AddAlias(string existingLayerName, string aliasName)
    {
        if (!layers.ContainsKey(existingLayerName))
            return false;

        aliases[aliasName] = existingLayerName;
        return true;
    }

    // Obtiene una capa por nombre o alias
    public SpriteMapChunk<T> GetLayer(string name)
    {
        if (layers.TryGetValue(name, out var direct))
            return direct;

        if (aliases.TryGetValue(name, out var targetName) && layers.TryGetValue(targetName, out var aliased))
            return aliased;

        return null;
    }

    // Verifica si existe una capa real o alias
    public bool HasLayer(string name)
    {
        return layers.ContainsKey(name) || aliases.ContainsKey(name);
    }

    // Devuelve nombre de capa real si es un alias
    public string GetCanonicalName(string name)
    {
        if (layers.ContainsKey(name))
            return name;

        return aliases.TryGetValue(name, out var real) ? real : null;
    }

    public void ClearAll()
    {
        foreach (var item in layers)
        {
            item.Value.ClearAllChunks();
        }
    }

    public void ClearAllFiles()
    {
        foreach (var item in layers)
        {
            item.Value.ClearAllFiles();
        }
    }



    // Acceso de solo lectura
    public IReadOnlyDictionary<string, SpriteMapChunk<T>> Layers => layers;
    public IReadOnlyDictionary<string, string> Aliases => aliases;
}

