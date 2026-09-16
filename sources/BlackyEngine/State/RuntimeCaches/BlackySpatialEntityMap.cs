using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Entities;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;

public class BlackySpatialEntityMap
{
    private readonly Dictionary<Vector2I, BlackyChunkEntityBucket> buckets = new();

    // Mapeo auxiliar: Region -> Chunks con entidades activas
    private readonly Dictionary<Vector2I, HashSet<Vector2I>> regionToChunks = new();

    // Registro de regiones que han sufrido cambios (sucias)
    private readonly HashSet<Vector2I> dirtyRegions = new();

    // Tamaño de la región en chunks
    private const int ChunksPerRegion = 4;

    private Vector2I GetRegionFromChunk(Vector2I chunk)
    {
        return new Vector2I(
            (int)MathF.Floor((float)chunk.X / ChunksPerRegion),
            (int)MathF.Floor((float)chunk.Y / ChunksPerRegion)
        );
    }

    public IReadOnlyCollection<BlackyChunkEntityBucket> GetAllBuckets()
    {
        return buckets.Values;
    }

    // Métodos para gestionar el estado sucio (Dirty)
    public IEnumerable<Vector2I> GetDirtyRegions() => dirtyRegions;

    public void ClearDirtyRegion(Vector2I region)
    {
        dirtyRegions.Remove(region);
    }

    public void Add(Entity entity, Vector2I chunk, bool isBuilding)
    {
        var region = GetRegionFromChunk(chunk);
        dirtyRegions.Add(region); // 🔴 Marcar región como sucia automáticamente

        bool isNewChunk = !buckets.ContainsKey(chunk);

        if (!buckets.TryGetValue(chunk, out var bucket))
        {
            bucket = new BlackyChunkEntityBucket();
            buckets[chunk] = bucket;
        }

        if (isNewChunk)
        {
            if (!regionToChunks.TryGetValue(region, out var chunkSet))
            {
                chunkSet = new HashSet<Vector2I>();
                regionToChunks[region] = chunkSet;
            }
            chunkSet.Add(chunk);
        }

        ref var spatial = ref entity.Ensure<SpatialComponent>();

        spatial.Chunk = chunk;
        spatial.IndexInBucket = bucket.Count;

        bucket.Add(entity, ref spatial, isBuilding);
    }

    public void Remove(Entity entity)
    {
        ref var spatial = ref entity.Ensure<SpatialComponent>();

        var chunk = spatial.Chunk;
        var region = GetRegionFromChunk(chunk);
        dirtyRegions.Add(region); // 🔴 Marcar región como sucia automáticamente

        if (!buckets.TryGetValue(chunk, out var bucket))
            return;

        bucket.Remove(entity, ref spatial);

        if (bucket.Count == 0)
        {
            buckets.Remove(chunk);

            // Limpiar del índice de regiones si el chunk se quedó completamente vacío
            if (regionToChunks.TryGetValue(region, out var chunkSet))
            {
                chunkSet.Remove(chunk);
                if (chunkSet.Count == 0)
                {
                    regionToChunks.Remove(region);
                }
            }
        }
    }

    public void Move(Entity entity, Vector2I newChunk, bool isBuilding)
    {
        ref var spatial = ref entity.Ensure<SpatialComponent>();

        if (spatial.Chunk == newChunk)
            return;

        var oldChunk = spatial.Chunk;
        var oldRegion = GetRegionFromChunk(oldChunk);
        var newRegion = GetRegionFromChunk(newChunk);

        // 🔴 Marcar ambas regiones involucradas como sucias
        dirtyRegions.Add(oldRegion);
        dirtyRegions.Add(newRegion);

        // 🔴 1. Remover del bucket viejo
        if (buckets.TryGetValue(oldChunk, out var oldBucket))
        {
            oldBucket.Remove(entity, ref spatial);

            if (oldBucket.Count == 0)
            {
                buckets.Remove(oldChunk);

                if (regionToChunks.TryGetValue(oldRegion, out var oldChunkSet))
                {
                    oldChunkSet.Remove(oldChunk);
                    if (oldChunkSet.Count == 0)
                        regionToChunks.Remove(oldRegion);
                }
            }
        }

        // 🟢 2. Agregar al nuevo bucket
        bool isNewChunk = !buckets.ContainsKey(newChunk);

        if (!buckets.TryGetValue(newChunk, out var newBucket))
        {
            newBucket = new BlackyChunkEntityBucket();
            buckets[newChunk] = newBucket;
        }

        if (isNewChunk)
        {
            if (!regionToChunks.TryGetValue(newRegion, out var newChunkSet))
            {
                newChunkSet = new HashSet<Vector2I>();
            newRegionMap: // (Label no necesaria)
                regionToChunks[newRegion] = newChunkSet;
            }
            newChunkSet.Add(newChunk);
        }

        spatial.Chunk = newChunk;
        newBucket.Add(entity, ref spatial, isBuilding);
    }

    public BlackyChunkEntityBucket GetBucket(Vector2I chunk)
    {
        buckets.TryGetValue(chunk, out var bucket);
        return bucket;
    }

    /// <summary>
    /// Devuelve los chunks activos de una región específica para consultar solo lo necesario al guardar.
    /// </summary>
    public IEnumerable<Vector2I> GetChunksForRegion(Vector2I region)
    {
        if (regionToChunks.TryGetValue(region, out var chunkSet))
        {
            return chunkSet;
        }
        return Array.Empty<Vector2I>();
    }
}