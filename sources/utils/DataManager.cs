using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
internal class DataManager<TKey, TValue> where TValue : class
{
    private readonly Dictionary<TKey, TValue> _dictionary = new();

    public void RegisterData(TKey id)
    {
        if (!_dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindByIdGlobal<TValue>(id);
            _dictionary.Add(id, data);
        }
    }

    public void RegisterData(TKey id, TValue data)
    {
        if (!_dictionary.ContainsKey(id))
            _dictionary.Add(id, data);
    }
    public void UpdateRegisterData(TKey id, TValue newData)
    {
        if (_dictionary.ContainsKey(id))
        {
            _dictionary[id] = newData;
        }
        else
        {
            _dictionary.Add(id, newData);
        }
    }
    public TValue GetData(TKey id)
    {
        if (_dictionary.TryGetValue(id, out var value))
            return value;

        RegisterData(id);
        return _dictionary[id];
    }
}