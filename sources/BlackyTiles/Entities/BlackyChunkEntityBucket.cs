using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Entities
{
    public class BlackyChunkEntityBucket
    {
        public Entity[] Entities;
        public int Count;

        public BlackyChunkEntityBucket(int capacity = 32)
        {
            Entities = new Entity[capacity];
            Count = 0;
        }

        public void Add(Entity entity, ref SpatialComponent spatial)
        {
            if (Count >= Entities.Length)
            {
                Array.Resize(ref Entities, Entities.Length * 2);
            }

            spatial.IndexInBucket = Count;
            Entities[Count++] = entity;
        }

        public void Remove(Entity entity, ref SpatialComponent spatial)
        {
            int index = spatial.IndexInBucket;
            int lastIndex = Count - 1;

            if (index != lastIndex)
            {
                var swapped = Entities[lastIndex];
                Entities[index] = swapped;

                // 🔥 actualizar índice del que movimos
                ref var swappedSpatial = ref swapped.Ensure<SpatialComponent>();
                swappedSpatial.IndexInBucket = index;
            }

            Count--;
        }
    }
}
