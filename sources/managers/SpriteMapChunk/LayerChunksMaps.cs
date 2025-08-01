using GodotEcsArch.sources.managers.Tilemap;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;
public class LayerChunksMaps<T> : IEnumerable<KeyValuePair<string, SpriteMapChunk<T>>> where T : DataItem, new()
{
    // Capa real: nombre → instancia
    [ProtoIgnore, JsonIgnore]
    private readonly Dictionary<string, SpriteMapChunk<T>> layers;

    // Alias: alias → nombre de capa original
    [ProtoIgnore, JsonIgnore]
    private readonly Dictionary<string, string> aliases;

    public LayerChunksMaps()
    {
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
    public void AddLayer(string name, SpriteMapChunk<T> layer)
    {
        layers[name] = layer;
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

    // Acceso de solo lectura
    public IReadOnlyDictionary<string, SpriteMapChunk<T>> Layers => layers;
    public IReadOnlyDictionary<string, string> Aliases => aliases;
}

