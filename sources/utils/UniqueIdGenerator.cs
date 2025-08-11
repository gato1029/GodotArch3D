using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public static class UniqueIdGenerator
{
    private class IdPool
    {
        public int NextId = 1;
        public Stack<int> ReusableIds = new();
    }

    // Diccionario por tipo que almacena su contador y pila de IDs reciclados
    private static readonly ConcurrentDictionary<Type, IdPool> _idPools = new();

    /// <summary>
    /// Obtiene el siguiente ID único disponible para el tipo T.
    /// Si hay IDs reciclados, reutiliza uno de ellos.
    /// </summary>
    public static int GetNextId<T>()
    {
        var pool = _idPools.GetOrAdd(typeof(T), _ => new IdPool());

        lock (pool)
        {
            if (pool.ReusableIds.Count > 0)
                return pool.ReusableIds.Pop();

            return pool.NextId++;
        }
    }

    /// <summary>
    /// Libera un ID para que pueda ser reutilizado por el tipo T.
    /// </summary>
    public static void ReleaseId<T>(int id)
    {
        var pool = _idPools.GetOrAdd(typeof(T), _ => new IdPool());

        lock (pool)
        {
            if (!pool.ReusableIds.Contains(id)) // Evita duplicados
                pool.ReusableIds.Push(id);
        }
    }

    /// <summary>
    /// Obtiene un ID como string con prefijo del tipo.
    /// </summary>
    public static string GetNextIdString<T>()
    {
        return $"{typeof(T).Name}_{GetNextId<T>()}";
    }
}
