using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Mods;

// podemos tener hasta 65535 mods activos, pero el ID 0 se reserva para el "mod base" del juego, que es el que contiene los datos originales sin modificaciones, por eso empezamos en 1
public class TableMods : SingletonBase<TableMods>
{
    private ushort _currentId = 0;

    private Queue<ushort> _freeIds = new Queue<ushort>();

    // ID -> Nombre
    private Dictionary<ushort, string> _mods = new();

    // Nombre -> ID
    private Dictionary<string, ushort> _nameToId = new();

    // Nombre -> Info
    private Dictionary<string, ModInfo> _modsInfo = new();

    private bool _initialized = false;

    // 🔹 Registrar mod (con info)
    public ushort Registrar(ModInfo info)
    {
        if (_initialized)
            throw new InvalidOperationException("No se puede registrar después de inicializar");

        if (string.IsNullOrWhiteSpace(info.Name))
            throw new ArgumentException("ModInfo inválido");

        if (_nameToId.TryGetValue(info.Name, out var existingId))
            return existingId;

        ushort id;

        if (_freeIds.Count > 0)
        {
            id = _freeIds.Dequeue();
        }
        else
        {
            if (_currentId == ushort.MaxValue)
                throw new InvalidOperationException("Se alcanzó el límite de mods activos (255)");

            _currentId++;
            id = _currentId;
        }

        _mods[id] = info.Name;
        _nameToId[info.Name] = id;
        _modsInfo[info.Name] = info;
        info.Id = id;
        return id;
    }

    // 🔹 Obtener nombre por ID
    public string ObtenerNombre(ushort id)
    {
        return _mods.TryGetValue(id, out var nombre) ? nombre : null;
    }

    // 🔹 Obtener info por ID
    public ModInfo ObtenerInfo(ushort id)
    {
        if (_mods.TryGetValue(id, out var nombre) &&
            _modsInfo.TryGetValue(nombre, out var info))
        {
            return info;
        }

        return default;
    }

    // 🔹 Obtener info por nombre
    public ModInfo ObtenerInfo(string nombre)
    {
        return _modsInfo.TryGetValue(nombre, out var info) ? info : default;
    }

    // 🔹 Obtener ID por nombre
    public ushort? ObtenerId(string nombre)
    {
        return _nameToId.TryGetValue(nombre, out var id) ? id : default;
    }

    // 🔹 Eliminar
    public bool Eliminar(ushort id)
    {
        if (!_mods.TryGetValue(id, out var nombre))
            return false;

        _mods.Remove(id);
        _nameToId.Remove(nombre);
        _modsInfo.Remove(nombre);

        _freeIds.Enqueue(id);

        return true;
    }

    // 🔹 Iterar (solo lectura)
    public IEnumerable<KeyValuePair<ushort, ModInfo>> ObtenerTodos()
    {
        foreach (var kv in _mods)
        {
            if (_modsInfo.TryGetValue(kv.Value, out var info))
            {
                yield return new KeyValuePair<ushort, ModInfo>(kv.Key, info);
            }
        }
    }
    public void Clear()
    {
        _currentId = 0;
        _freeIds.Clear();

        _mods.Clear();
        _nameToId.Clear();
        _modsInfo.Clear();

        _initialized = false;
    }

    // 🔹 Finalizar carga
    public void FinalizarCarga()
    {
        _initialized = true;
    }
}