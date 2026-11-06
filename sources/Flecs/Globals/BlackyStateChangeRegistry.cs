using GodotEcsArch.sources.managers.Characters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Globals;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class BlackyStateChangeRegistry
{
    // Ahora el diccionario guarda una tupla con el estado y el índice de la GPU
    private readonly ConcurrentDictionary<int, (StateType State, int GpuIndex)> _pending = new();

    // Se actualizó la firma para recibir el gpuIndex
    public void Register(int gpuIndex,int shaderId, StateType state)
    {
        _pending[shaderId] = (state, gpuIndex);
    }

    public void ApplyChanges()
    {
        // ToArray() es seguro para hilos y extrae instantáneamente el estado actual
        var changes = _pending.ToArray();
        _pending.Clear();

        Parallel.ForEach(changes, change =>
        {
            int shaderId = change.Key;
            // Desestructuramos la tupla para obtener ambos valores
            var (state, gpuIndex) = change.Value;

            UpdateAnimation(shaderId, state, gpuIndex);
        });
    }

    public void Remove(int shaderId)
    {
        _pending.TryRemove(shaderId, out _);
    }

    // Se agregó el parámetro gpuIndex al método de actualización
    private void UpdateAnimation(int shaderId, StateType state, int gpuIndex)
    {
        // Actualizar animación del shader usando el state y gpuIndex.
    }
}