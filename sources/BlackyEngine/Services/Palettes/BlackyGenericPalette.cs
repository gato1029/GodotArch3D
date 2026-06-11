using GodotEcsArch.sources.managers.Mods;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Services.Palettes;

[MessagePackObject]
public struct GenericPersistenceData
{
    [Key(0)]
    public string ModName { get; set; }

    [Key(1)]
    public ushort id { get; set; }
}

public readonly record struct PairCacheData(
    string ModName,
    ushort Id
);

public class BlackyGenericPalette<T> where T : class
{
    private ushort _nextId = 1;

    private readonly Dictionary<PairCacheData, ushort> _cache = new();
    private Dictionary<ushort, GenericPersistenceData> _persistent = new();

    public T GetData(ushort id)
    {
        if (_persistent.TryGetValue(id, out var persistenceData))
        {
            return AtlasModsManager.Get<T>(persistenceData.ModName, persistenceData.id);
        }
        return null;
    }
    public ushort GetIdPersistence(string modName, ushort originalId, out T data)
    {
        data = AtlasModsManager.Get<T>(modName, originalId);

        var key = new PairCacheData(modName, originalId);

        if (_cache.TryGetValue(key, out ushort paletteId))
            return paletteId;

        if (_nextId == ushort.MaxValue)
            throw new InvalidOperationException("Palette limit reached.");

        paletteId = _nextId++;

        _cache[key] = paletteId;

        _persistent[paletteId] = new GenericPersistenceData
        {
            ModName = modName,
            id = originalId
        };

        return paletteId;
    }

    public IReadOnlyDictionary<ushort, GenericPersistenceData> GetPersistentPalette()
        => _persistent;

    public void LoadPersistentPalette(
        Dictionary<ushort, GenericPersistenceData> persistentData)
    {
        _persistent = new Dictionary<ushort, GenericPersistenceData>(persistentData);

        _cache.Clear();

        _nextId = 1;

        foreach (var kv in persistentData)
        {
            var pair = new PairCacheData(
                kv.Value.ModName,
                kv.Value.id);

            _cache[pair] = kv.Key;

            if (kv.Key >= _nextId)
                _nextId = (ushort)(kv.Key + 1);
        }
    }
}