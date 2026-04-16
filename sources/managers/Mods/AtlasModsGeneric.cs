using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Mods;

public class AtlasModsGeneric<T>
{
    // ModID -> (DataID -> Data)
    private readonly Dictionary<byte, Dictionary<int, T>> _data = new();

    // 🔹 Agregar dato
    public void Registrar(byte modId, int dataId, T value)
    {
        if (!_data.TryGetValue(modId, out var modDict))
        {
            modDict = new Dictionary<int, T>();
            _data[modId] = modDict;
        }

        modDict[dataId] = value;
    }

    // 🔹 Obtener dato
    public T Obtener(byte modId, int dataId)
    {
        if (_data.TryGetValue(modId, out var modDict) &&
            modDict.TryGetValue(dataId, out var value))
        {
            return value;
        }

        return default;
    }

    // 🔹 Verificar existencia
    public bool Existe(byte modId, int dataId)
    {
        return _data.TryGetValue(modId, out var modDict) &&
               modDict.ContainsKey(dataId);
    }

    // 🔹 Eliminar dato
    public bool Eliminar(byte modId, int dataId)
    {
        if (!_data.TryGetValue(modId, out var modDict))
            return false;

        var removed = modDict.Remove(dataId);

        // limpiar mod vacío (opcional pero limpio)
        if (modDict.Count == 0)
            _data.Remove(modId);

        return removed;
    }

    // 🔹 Obtener todos los datos de un mod
    public IEnumerable<KeyValuePair<int, T>> ObtenerPorMod(byte modId)
    {
        if (_data.TryGetValue(modId, out var modDict))
            return modDict;

        return Array.Empty<KeyValuePair<int, T>>();
    }

    // 🔹 Recorrer todo
    public IEnumerable<(byte modId, int dataId, T value)> ObtenerTodo()
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