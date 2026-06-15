using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Mods;

public class MultiIndex<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, List<TValue>> _index = new();

    public void Add(TKey key, TValue value)
    {
        if (!_index.TryGetValue(key, out var list))
        {
            list = new List<TValue>();
            _index[key] = list;
        }

        list.Add(value);
    }

    public IReadOnlyList<TValue> Get(TKey key)
    {
        return _index.TryGetValue(key, out var list)
            ? list
            : Array.Empty<TValue>();
    }

    public bool TryGet(TKey key, out List<TValue> list)
    {
        return _index.TryGetValue(key, out list);
    }
    public bool TryGetFirst(TKey key, out TValue value)
    {
        if (_index.TryGetValue(key, out var list) && list.Count > 0)
        {
            value = list[0];
            return true;
        }

        value = default;
        return false;
    }
    public void Clear()
    {
        foreach (var kv in _index)
        {
            kv.Value.Clear();
        }

        _index.Clear();
    }
}
