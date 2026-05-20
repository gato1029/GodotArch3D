using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Mods;

public class SaveModTable
{
    // =========================
    // PERSISTENTE
    // =========================

    private readonly List<string> _mods = new();

    // =========================
    // RUNTIME CACHE
    // =========================

    private readonly Dictionary<byte, ushort> _saveToRuntime = new();

    private readonly Dictionary<ushort, byte> _runtimeToSave = new();

    // =========================
    // SAVE -> RUNTIME
    // =========================

    public ushort ToRuntime(byte saveIndex)
    {
        return _saveToRuntime[saveIndex];
    }

    // =========================
    // RUNTIME -> SAVE
    // =========================

    public byte ToSave(ushort runtimeId)
    {
        // ya existe
        if (_runtimeToSave.TryGetValue(runtimeId, out byte existing))
            return existing;

        // obtener nombre persistente
        string modName =
            TableMods.Instance.ObtenerNombre(runtimeId);

        // crear nuevo save index
        byte saveIndex = (byte)_mods.Count;

        _mods.Add(modName);

        _runtimeToSave[runtimeId] = saveIndex;
        _saveToRuntime[saveIndex] = runtimeId;

        return saveIndex;
    }

    // =========================
    // LOAD
    // =========================

    public void BuildRuntimeCache()
    {
        _saveToRuntime.Clear();
        _runtimeToSave.Clear();

        for (byte i = 0; i < _mods.Count; i++)
        {
            string modName = _mods[i];

            ushort runtimeId =
                TableMods.Instance.ObtenerId(modName).Value;

            _saveToRuntime[i] = runtimeId;
            _runtimeToSave[runtimeId] = i;
        }
    }
}
