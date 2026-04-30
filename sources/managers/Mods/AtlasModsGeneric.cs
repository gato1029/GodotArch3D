using System;
using System.Collections;
using System.Collections.Generic;

namespace GodotEcsArch.sources.managers.Mods;

public interface IAtlasModsGeneric
{
    object ObtenerRaw(byte modId, object dataId);
    bool ExisteRaw(byte modId, object dataId);

    IEnumerable ObtenerValoresRaw(byte modId);
    IEnumerable ObtenerValoresTodosRaw();

    object ObtenerDictionaryRaw(byte modId);
    IEnumerable ObtenerTodoRaw();
}


public class AtlasModsGeneric<TKey, TValue> : IAtlasModsGeneric where TKey : notnull
{
    // ModID -> (DataID -> Data)
    private readonly Dictionary<byte, Dictionary<TKey, TValue>> _data = new();

    // =========================================================
    // REGISTRO
    // =========================================================

    public void Registrar(byte modId, TKey dataId, TValue value)
    {
        if (!_data.TryGetValue(modId, out var modDict))
        {
            modDict = new Dictionary<TKey, TValue>();
            _data[modId] = modDict;
        }

        modDict[dataId] = value;
    }

    // =========================================================
    // GET TIPADO
    // =========================================================

    public TValue Obtener(byte modId, TKey dataId)
    {
        if (_data.TryGetValue(modId, out var modDict) &&
            modDict.TryGetValue(dataId, out var value))
        {
            return value;
        }

        return default;
    }

    public bool TryObtener(byte modId, TKey dataId, out TValue value)
    {
        value = default;

        if (_data.TryGetValue(modId, out var modDict) &&
            modDict.TryGetValue(dataId, out var result))
        {
            value = result;
            return true;
        }

        return false;
    }

    public bool Existe(byte modId, TKey dataId)
    {
        return _data.TryGetValue(modId, out var modDict) &&
               modDict.ContainsKey(dataId);
    }

    public bool Eliminar(byte modId, TKey dataId)
    {
        if (!_data.TryGetValue(modId, out var modDict))
            return false;

        bool removed = modDict.Remove(dataId);

        if (modDict.Count == 0)
            _data.Remove(modId);

        return removed;
    }

    public void Clear()
    {
        _data.Clear();
    }

    // =========================================================
    // ITERADORES TIPADOS
    // =========================================================

    public Dictionary<TKey, TValue> ObtenerDictionary(byte modId)
    {
        if (_data.TryGetValue(modId, out var modDict))
            return modDict;

        return new Dictionary<TKey, TValue>();
    }

    public IEnumerable<TValue> ObtenerValores(byte modId)
    {
        if (_data.TryGetValue(modId, out var modDict))
        {
            foreach (var item in modDict.Values)
                yield return item;
        }
    }

    public IEnumerable<TValue> ObtenerValoresTodos()
    {
        foreach (var mod in _data.Values)
        {
            foreach (var item in mod.Values)
                yield return item;
        }
    }

    public IEnumerable<(byte modId, TKey key, TValue value)> ObtenerTodo()
    {
        foreach (var mod in _data)
        {
            foreach (var item in mod.Value)
            {
                yield return (mod.Key, item.Key, item.Value);
            }
        }
    }

    // =========================================================
    // INTERFAZ RAW (POLIMORFISMO)
    // =========================================================

    public object ObtenerRaw(byte modId, object dataId)
    {
        if (dataId is not TKey key)
            return default;

        return Obtener(modId, key);
    }

    public bool ExisteRaw(byte modId, object dataId)
    {
        if (dataId is not TKey key)
            return false;

        return Existe(modId, key);
    }

    public IEnumerable ObtenerValoresRaw(byte modId)
    {
        if (_data.TryGetValue(modId, out var modDict))
        {
            foreach (var item in modDict.Values)
                yield return item;
        }
    }

    public IEnumerable ObtenerValoresTodosRaw()
    {
        foreach (var mod in _data.Values)
        {
            foreach (var item in mod.Values)
                yield return item;
        }
    }

    public object ObtenerDictionaryRaw(byte modId)
    {
        if (_data.TryGetValue(modId, out var modDict))
            return modDict;

        return null;
    }

    public IEnumerable ObtenerTodoRaw()
    {
        foreach (var mod in _data)
        {
            foreach (var item in mod.Value)
            {
                yield return (mod.Key, item.Key, item.Value);
            }
        }
    }
}