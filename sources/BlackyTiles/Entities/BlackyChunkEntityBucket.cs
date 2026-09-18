using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using System;

namespace GodotEcsArch.sources.BlackyTiles.Entities;

public class BlackyChunkEntityBucket
{
    public Entity[] Entities;
    public bool[] Exist;
    public bool[] IsBuilding; // Nuevo array para marcar si es edificio (true) o recurso (false)
    public int Count;

    public BlackyChunkEntityBucket(int capacity = 32)
    {
        Entities = new Entity[capacity];
        Exist = new bool[capacity];
        IsBuilding = new bool[capacity]; // Inicializar el array
        Count = 0;
    }

    public void Add(Entity entity, ref SpatialComponent spatial, bool isBuilding)
    {
        if (Count >= Entities.Length)
        {
            int newSize = Entities.Length * 2;
            Array.Resize(ref Entities, newSize);
            Array.Resize(ref Exist, newSize);
            Array.Resize(ref IsBuilding, newSize); // Redimensionar el nuevo array
        }

        spatial.IndexInBucket = Count;
        Entities[Count] = entity;
        Exist[Count] = true;
        IsBuilding[Count] = isBuilding; // Asignar el valor
        Count++;
    }

    public void Remove(Entity entity, ref SpatialComponent spatial)
    {
        int index = spatial.IndexInBucket;
        int lastIndex = Count - 1;

        if (index != lastIndex)
        {
            var swapped = Entities[lastIndex];

            // Verificamos si la entidad del final sigue viva
            if (swapped.IsAlive()) // O swapped.Id != 0 según tu versión de Flecs
            {
                // Si está viva, hacemos el swap normalmente
                Entities[index] = swapped;
                Exist[index] = Exist[lastIndex];
                IsBuilding[index] = IsBuilding[lastIndex];

                ref var swappedSpatial = ref swapped.Ensure<SpatialComponent>();
                swappedSpatial.IndexInBucket = index;
            }
            else
            {
                // Si la entidad del final ya estaba muerta, NO la movemos.
                // Limpiamos la posición del elemento que estamos borrando.
                Entities[index] = default;
                Exist[index] = false;
                IsBuilding[index] = false;
            }
        }

        // Limpiar referencias y marcar como inexistente
        Entities[lastIndex] = default;
        Exist[lastIndex] = false;
        IsBuilding[lastIndex] = false; // Limpiar valor por defecto
        Count--;
    }

    internal void Clear()
    {
        if (Count > 0)
        {
            // Limpiamos los elementos actuales en los arreglos para liberar referencias de Flecs
            Array.Clear(Entities, 0, Count);
            Array.Clear(Exist, 0, Count);
            Array.Clear(IsBuilding, 0, Count);
        }

        // Reiniciamos el contador
        Count = 0;
    }
}