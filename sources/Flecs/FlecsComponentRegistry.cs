using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Reflection;
namespace GodotFlecs.sources.Flecs;

public static class FlecsComponentRegistry
{
    private static List<Type> _cachedTypes;
    private static bool _initialized = false;

    /// <summary>
    /// Se llama desde cualquier FlecsManager para registrar componentes en SU world.
    /// </summary>
    public static void RegisterAll(FlecsManager manager)
    {
        if (!_initialized)
        {
            CacheTypes();
            _initialized = true;
        }

        foreach (var type in _cachedTypes)
        {
            var method = typeof(FlecsManager)
                .GetMethod("RegisterComponentWithMembersSimple", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.MakeGenericMethod(type);

            method?.Invoke(manager, null);

            GameLog.LogCat($"[FLECS] Registrado componente: {type.Name}");
        }
    }

    /// <summary>
    /// SOLO UNA VEZ en toda la app (reflection pesado)
    /// </summary>
    private static void CacheTypes()
    {
        _cachedTypes = new List<Type>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsValueType && type.IsPublic &&
                    type.GetCustomAttribute<RegisterComponentFlecsAttribute>() != null)
                {
                    _cachedTypes.Add(type);
                }
            }
        }

        GameLog.LogCat($"[FLECS] Componentes cacheados: {_cachedTypes.Count}");
    }
}