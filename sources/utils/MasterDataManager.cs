using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;

public sealed class MasterDataManager
{
    // 🔒 Instancia única (thread-safe, inicialización perezosa)
    private static readonly Lazy<MasterDataManager> _instance = new(() => new MasterDataManager());

    // 🧩 Diccionario interno: (TipoClave, TipoValor) → DataManager genérico
    private readonly Dictionary<(Type keyType, Type valueType), object> _managers = new();

    // 🚪 Propiedad interna de acceso si se necesita
    public static MasterDataManager Instance => _instance.Value;

    // 🧱 Constructor privado: evita instanciación externa
    private MasterDataManager() { }

    public static void RegisterAllData<TKey, TValue>()
    where TValue : class
    {
        Instance.GetManager<TKey, TValue>().RegisterAllData();
    }

    // 🔧 Obtiene o crea el DataManager correcto
    private DataManager<TKey, TValue> GetManager<TKey, TValue>() where TValue : class
    {
        var key = (typeof(TKey), typeof(TValue));

        if (!_managers.TryGetValue(key, out var manager))
        {
            manager = new DataManager<TKey, TValue>();
            _managers.Add(key, manager);
        }

        return (DataManager<TKey, TValue>)manager;
    }

    // 🧠 API pública simplificada (no requiere Instance)
    public static TValue GetData<TValue>(object id) where TValue : class
    {
        if (id is int intId)
            return Instance.GetManager<int, TValue>().GetData(intId);
        else if (id is long longId)
            return Instance.GetManager<long, TValue>().GetData(longId);
        else
            throw new ArgumentException($"Tipo de clave no soportado: {id.GetType()}");
    }

    public static void RegisterData<TValue>(object id, TValue data) where TValue : class
    {
        if (id is int intId)
            Instance.GetManager<int, TValue>().RegisterData(intId, data);
        else if (id is long longId)
            Instance.GetManager<long, TValue>().RegisterData(longId, data);
        else
            throw new ArgumentException($"Tipo de clave no soportado: {id.GetType()}");
    }
    public static void UpdateRegisterData<TValue>(object id, TValue data) where TValue : class
    {
        if (id is int intId)
            Instance.GetManager<int, TValue>().UpdateRegisterData(intId, data);
        else if (id is long longId)
            Instance.GetManager<long, TValue>().UpdateRegisterData(longId, data);
        else
            throw new ArgumentException($"Tipo de clave no soportado: {id.GetType()}");
    }

    public static TValue GetBySaveIds<TValue>( ushort idSave, ushort groupingSave = 0)
    where TValue : class
    {
        return Instance
            .GetManager<long, TValue>()
            .GetBySaveIds(groupingSave, idSave);
    }
    public static DataManager<TKey, TValue> GetDataManager<TKey, TValue>()
    where TValue : class
    {
        return Instance.GetManager<TKey, TValue>();
    }

    public static IEnumerable<TKey> GetAllIds<TKey, TValue>()
        where TValue : class
    {
        return Instance.GetManager<TKey, TValue>().GetAllIds();
    }
    public static IEnumerable<TValue> GetAllData<TKey, TValue>()
    where TValue : class
    {
        return Instance.GetManager<TKey, TValue>().GetAllData();
    }

    public static void ClearAll() => Instance._managers.Clear();
}
