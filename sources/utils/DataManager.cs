using Flecs.NET.Core;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
public class DataManager<TKey, TValue> where TValue : class
{
    private readonly Dictionary<TKey, TValue> _dictionary = new();

    // 🔥 índice compuesto real
    private readonly Dictionary<(ushort groupingSave, ushort idSave), TValue>
        _compositeIndex = new();

    // -----------------------------------------
    // 🔹 Registro automático desde BD
    // -----------------------------------------
    public void RegisterAllData()
    {
        var allData = DataBaseManager.Instance.FindAll<TValue>();

        if (allData == null)
            return;

        foreach (var data in allData)
        {
            if (data is IdDataLong entity)
            {
                var key = (TKey)(object)entity.id;

                if (!_dictionary.ContainsKey(key))
                    RegisterData(key, data);
            }
        }
    }
    public void RegisterData(TKey id)
    {
        if (_dictionary.ContainsKey(id))
            return;

        var data = DataBaseManager.Instance.FindByIdGlobal<TValue>(id);

        if (data == null)
            return;

        _dictionary.Add(id, data);
        IndexSecondaryKeys(data);
    }

    // -----------------------------------------
    // 🔹 Registro manual (id + data)
    // -----------------------------------------
    public void RegisterData(TKey id, TValue data)
    {
        if (_dictionary.ContainsKey(id))
            return;

        _dictionary.Add(id, data);
        IndexSecondaryKeys(data);
    }

    // -----------------------------------------
    // 🔹 Update (reindexa correctamente)
    // -----------------------------------------
    public void UpdateRegisterData(TKey id, TValue newData)
    {
        // 🔥 Si ya existía, remover índice viejo
        if (_dictionary.TryGetValue(id, out var oldData))
            RemoveSecondaryIndex(oldData);

        _dictionary[id] = newData;

        IndexSecondaryKeys(newData);
    }

    // -----------------------------------------
    // 🔹 Búsqueda primaria
    // -----------------------------------------
    public TValue GetData(TKey id)
    {
        if (_dictionary.TryGetValue(id, out var value))
            return value;

        RegisterData(id);
        return _dictionary.TryGetValue(id, out value) ? value : null;
    }

    // -----------------------------------------
    // 🔹 Búsqueda por clave compuesta
    // -----------------------------------------
    public TValue GetBySaveIds(ushort groupingSave, ushort idSave)
    {
        // 1️⃣ Intentar en cache
        if (_compositeIndex.TryGetValue((groupingSave, idSave), out var value))
            return value;

        // 2️⃣ No está en cache → ir a BD
        var data = DataBaseManager.Instance
            .FindBySaveIds<TValue>(groupingSave, idSave);

        if (data == null)
            return null;

        // 3️⃣ Registrar en cache (indexa automáticamente)
        if (data is IdDataLong entity)
        {
            var keyId = (TKey)(object)entity.id;
            RegisterData(keyId, data);
        }

        return data;
    }
    public IEnumerable<TKey> GetAllIds()
    {
        return _dictionary.Keys;
    }
    public IEnumerable<TValue> GetAllData()
    {
        return _dictionary.Values;
    }
    // -----------------------------------------
    // 🔹 Indexado secundario
    // -----------------------------------------
    private void IndexSecondaryKeys(TValue data)
    {
        if (data is IdDataLong entity)
        {
            var key = (entity.idGroupingSave, entity.idSave);
            _compositeIndex[key] = data;
        }
    }

    private void RemoveSecondaryIndex(TValue data)
    {
        if (data is IdDataLong entity)
        {
            var key = (entity.idGroupingSave, entity.idSave);
            _compositeIndex.Remove(key);
        }
    }
}