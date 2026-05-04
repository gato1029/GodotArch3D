using System;
using System.Collections;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Mods;

public class AtlasMods<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<byte, Dictionary<TKey, TValue>> _data = new();

    public void Register(byte modId, TKey id, TValue value)
    {
        if (!_data.TryGetValue(modId, out var dict))
        {
            dict = new Dictionary<TKey, TValue>();
            _data[modId] = dict;
        }

        dict[id] = value;
    }

    public TValue Get(byte modId, TKey id)
    {
        return _data.TryGetValue(modId, out var dict) &&
               dict.TryGetValue(id, out var value)
            ? value
            : default;
    }

    public bool TryGet(byte modId, TKey id, out TValue value)
    {
        if (_data.TryGetValue(modId, out var dict))
            return dict.TryGetValue(id, out value);

        value = default;
        return false;
    }

    public IReadOnlyDictionary<TKey, TValue> GetByMod(byte modId)
    {
        return _data.TryGetValue(modId, out var dict)
            ? dict
            : new Dictionary<TKey, TValue>();
    }

    public IEnumerable<TValue> GetAll(byte modId)
    {
        if (_data.TryGetValue(modId, out var dict))
            return dict.Values;

        return Array.Empty<TValue>();
    }
    public Dictionary<byte, Dictionary<TKey, TValue>> GetRawData()
    {
        return _data;
    }
    public void Clear()
    {
        foreach (var kv in _data)
        {
            kv.Value.Clear();
        }

        _data.Clear();
    }
}
