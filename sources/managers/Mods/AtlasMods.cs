using System;
using System.Collections;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Mods;

public class AtlasMods<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<ushort, Dictionary<TKey, TValue>> _data = new();
    private readonly Dictionary<TKey, TValue> _dataDirect = new();
    public void Register(ushort modId, TKey id, TValue value)
    {
        if (!_data.TryGetValue(modId, out var dict))
        {
            dict = new Dictionary<TKey, TValue>();
            _data[modId] = dict;
            
        }

        _dataDirect[id] = value;
        dict[id] = value;
    }
    public TValue GetDirect(TKey id)
    {
        if (_dataDirect.TryGetValue(id, out var value))
        {
            return value;
        }
        return default;
    }
    public TValue Get(ushort modId, TKey id)
    {
        return _data.TryGetValue(modId, out var dict) &&
               dict.TryGetValue(id, out var value)
            ? value
            : default;
    }

    public bool TryGet(ushort modId, TKey id, out TValue value)
    {
        if (_data.TryGetValue(modId, out var dict))
            return dict.TryGetValue(id, out value);

        value = default;
        return false;
    }

    public IReadOnlyDictionary<TKey, TValue> GetByMod(ushort modId)
    {
        return _data.TryGetValue(modId, out var dict)
            ? dict
            : new Dictionary<TKey, TValue>();
    }

    public IEnumerable<TValue> GetAll(ushort modId)
    {
        if (_data.TryGetValue(modId, out var dict))
            return dict.Values;

        return Array.Empty<TValue>();
    }
    public Dictionary<ushort, Dictionary<TKey, TValue>> GetRawData()
    {
        return _data;
    }
    public void Clear()
    {
        foreach (var kv in _data)
        {
            kv.Value.Clear();
        }
        _dataDirect.Clear();
        _data.Clear();
    }
}
