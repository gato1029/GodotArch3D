using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.utils;

public enum TipoContador
{
    EdificiosUnidadesRango, // sirve para hacer la particion de frames para procesamiento
    UnidadesMelle,
    _TotalTipos // Utilizado para dimensionar los arrays automáticamente
}

internal class ContadoresHelper
{
    // Arrays estáticos de tamaño fijo basados en el enum
    private static readonly Stack<int>[] _pools =
        new Stack<int>[(int)TipoContador._TotalTipos];

    private static readonly int[] _contadoresGlobales =
        new int[(int)TipoContador._TotalTipos];

    static ContadoresHelper()
    {
        // Inicializamos cada stack en el array estático
        for (int i = 0; i < _pools.Length; i++)
        {
            _pools[i] = new Stack<int>();
        }
    }

    public static int Obtener(TipoContador tipo)
    {
        int index = (int)tipo;
        var pool = _pools[index];

        if (pool.Count > 0)
        {
            return pool.Pop();
        }

        _contadoresGlobales[index]++;
        return _contadoresGlobales[index];
    }

    public static void Liberar(TipoContador tipo, int id)
    {
        int index = (int)tipo;
        var pool = _pools[index];

        // Opcional: si el volumen de elementos es muy bajo, Contains es seguro. 
        // En bucles críticos masivos, se suele omitir o usar un HashSet si hay miles de IDs concurrentes.
        if (!pool.Contains(id))
        {
            pool.Push(id);
        }
    }
    public static int ObtenerCantidadActivos(TipoContador tipo)
    {
        int index = (int)tipo;
        return _contadoresGlobales[index] - _pools[index].Count;
    }
    public static int ObtenerUltimo(TipoContador tipo)
    {
        int index = (int)tipo;
        return _contadoresGlobales[index];
    }
}