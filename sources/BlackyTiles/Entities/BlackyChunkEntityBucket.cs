using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Entities;

public class BlackyChunkEntityBucket
{
    public Entity[] Entities;
    public bool[] Exist;
    public int Count;

    public BlackyChunkEntityBucket(int capacity = 32)
    {
        Entities = new Entity[capacity];
        Exist = new bool[capacity];
        Count = 0;
    }

    public void Add(Entity entity, ref SpatialComponent spatial)
    {
        if (Count >= Entities.Length)
        {
            int newSize = Entities.Length * 2;
            Array.Resize(ref Entities, newSize);
            Array.Resize(ref Exist, newSize);
        }

        spatial.IndexInBucket = Count;
        Entities[Count] = entity;
        Exist[Count] = true;
        Count++;
    }

    public void Remove(Entity entity, ref SpatialComponent spatial)
    {
        int index = spatial.IndexInBucket;
        int lastIndex = Count - 1;

        if (index != lastIndex)
        {
            var swapped = Entities[lastIndex];
            Entities[index] = swapped;
            Exist[index] = Exist[lastIndex];

            // Actualizar índice del que movimos mediante swap-back
            ref var swappedSpatial = ref swapped.Ensure<SpatialComponent>();
            swappedSpatial.IndexInBucket = index;
        }

        // Limpiar referencias y marcar como inexistente
        Entities[lastIndex] = default;
        Exist[lastIndex] = false;
        Count--;
    }
}
