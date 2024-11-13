using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class ServiceLocator
{
    public static ServiceLocator Instance => _instance ?? (_instance = new ServiceLocator());
    private static ServiceLocator _instance;

    private readonly Dictionary<Type, object> _services;

    public ServiceLocator()
    {
        _services = new Dictionary<Type, object>();
    }
    public bool ExistService<T>()
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            return true;
        }
        return false;
    }
    public void RegisterService<T>(T service)
    {
        var type = typeof(T);
        if (!_services.ContainsKey(type))
        {
            _services.Add(type, service);
        }
    }
    public void ReplaceService<T>(T service)
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services[type] = service;
        }
    }
    public void RemoveService<T>()
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
        }
    }
    public T GetService<T>()
    {
        var type = typeof(T);
        if (!_services.TryGetValue(type, out var service))
        {
            throw new Exception($"Service {type} not found");
        }
        return (T)service;
    }
}

