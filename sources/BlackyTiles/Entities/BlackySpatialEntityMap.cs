using Flecs.NET.Core;
using Godot;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Entities;

public class BlackySpatialEntityMap
{
    private readonly Dictionary<Vector2I, BlackyChunkEntityBucket> buckets = new();

    public void Add(Entity entity, Vector2I chunk)
    {
        if (!buckets.TryGetValue(chunk, out var bucket))
        {
            bucket = new BlackyChunkEntityBucket();
            buckets[chunk] = bucket;
        }

        ref var spatial = ref entity.Ensure<SpatialComponent>();

        spatial.Chunk = chunk;
        spatial.IndexInBucket = bucket.Count;

        bucket.Add(entity, ref spatial);
    }

    public void Remove(Entity entity)
    {
        ref var spatial = ref entity.Ensure<SpatialComponent>();

        var chunk = spatial.Chunk;

        if (!buckets.TryGetValue(chunk, out var bucket))
            return;

        bucket.Remove(entity, ref spatial);

        if (bucket.Count == 0)
        {
            buckets.Remove(chunk);
        }
    }

    public void Move(Entity entity, Vector2I newChunk)
    {
        ref var spatial = ref entity.Ensure<SpatialComponent>();

        if (spatial.Chunk == newChunk)
            return;

        // 🔴 1. guardar chunk actual
        var oldChunk = spatial.Chunk;

        // 🔴 2. remover del bucket viejo
        if (buckets.TryGetValue(oldChunk, out var oldBucket))
        {
            oldBucket.Remove(entity, ref spatial);

            if (oldBucket.Count == 0)
                buckets.Remove(oldChunk);
        }

        // 🟢 3. agregar al nuevo bucket
        if (!buckets.TryGetValue(newChunk, out var newBucket))
        {
            newBucket = new BlackyChunkEntityBucket();
            buckets[newChunk] = newBucket;
        }

        spatial.Chunk = newChunk;
        newBucket.Add(entity, ref spatial);
    }

    public BlackyChunkEntityBucket GetBucket(Vector2I chunk)
    {
        buckets.TryGetValue(chunk, out var bucket);
        return bucket;
    }
}
