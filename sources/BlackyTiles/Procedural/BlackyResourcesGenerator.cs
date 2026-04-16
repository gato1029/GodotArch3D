using Godot;
using GodotEcsArch.sources.BlackyTiles.Procedural.Resources;
using GodotEcsArch.sources.BlackyTiles.Procedural.Terrain;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.BlackyTiles.Procedural;

public sealed class BlackyResourcesGenerator : IDisposable
{
    private readonly BlackyHeightSystem heightSystem;
    private readonly BlackyResourcesSourceSystem resourcesSourceSystem;
    private readonly BlackyResourcesPostProcessor postProcessor;  
    private readonly BlackyWorldTerrainGenerator terrainGenerator;
    private readonly ChunkGenerationState chunkGenerationState;

    private readonly Dictionary<(int height, ushort biome), BlackyHeightResourceTable> tables = new();
    private readonly Dictionary<int, BlackyHeightResourceTable> baseTablesByHeight = new();

    private readonly Dictionary<ResourceSourceType, FastNoiseLite> noiseByType = new();

    private readonly FastNoiseLite noise;

    private readonly int seed;
    private bool disposed;
    public int MinDistanceFromHeightBorder { get; set; } = 3;
    public BlackyResourcesGenerator(
        BlackyHeightSystem heightSystem,        
        BlackyWorldTerrainGenerator terrainGenerator,
        BlackyResourcesSourceSystem resourcesSourceSystem,
        BlackyResourcesPostProcessor postProcessor,
        int seed = 12345)
    {
        
        this.terrainGenerator = terrainGenerator ?? throw new ArgumentNullException(nameof(terrainGenerator));
        this.heightSystem = heightSystem ?? throw new ArgumentNullException(nameof(heightSystem));
        this.resourcesSourceSystem = resourcesSourceSystem ?? throw new ArgumentNullException(nameof(resourcesSourceSystem));
        this.postProcessor = postProcessor ?? throw new ArgumentNullException(nameof(postProcessor));
        this.chunkGenerationState = new ChunkGenerationState();
        this.seed = seed;

        noise = new FastNoiseLite(seed);

        // tipo de ruido
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        // 🔥 escala global (IMPORTANTE)
        noise.SetFrequency(0.03f);
    }

    public int Seed => seed;

    /// <summary>
    /// Padding alrededor del chunk central necesario para que el post-proceso
    /// pueda evaluar conflictos con vecinos de chunks adyacentes.
    /// </summary>
    public int BorderPadding => postProcessor.MaxRequiredDistance;

    public void ConfigureNoiseForType(ResourceSourceType type, int seedOffset, float frequency)
    {
        noiseByType[type] = CreateNoise(seedOffset, frequency);
    }
    private FastNoiseLite CreateNoise(int seedOffset, float frequency)
    {
        var n = new FastNoiseLite(seed + seedOffset);
        n.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        n.SetFrequency(frequency);
        return n;
    }

    public BlackyHeightResourceTable ConfigureHeightGlobal(int height)
    {
        if (!baseTablesByHeight.TryGetValue(height, out var table))
        {
            table = new BlackyHeightResourceTable(height, biomeId: 0); // 0 = global
            baseTablesByHeight.Add(height, table);
        }

        return table;
    }
    public BlackyHeightResourceTable ConfigureHeight(int height, ushort biome)
    {
        var key = (height, biome);

        if (tables.TryGetValue(key, out var table))
        {        
            // 🔥 CLONAR CONFIG GLOBAL SI EXISTE
            if (baseTablesByHeight.TryGetValue(height, out var baseTable))
            {
                CopyFromBase(table, baseTable);
            }        
        }

        return table;
    }
    private void CopyFromBase(
    BlackyHeightResourceTable target,
    BlackyHeightResourceTable source)
    {
        // density
        target.DensityThreshold = source.DensityThreshold;

        // type weights
        foreach (var type in source.GetAvailableTypesWeights())
        {
            target.SetTypeWeight(type, source.GetTypeWeight(type));            
        }

        // min distance heights
        var field = typeof(BlackyHeightResourceTable)
            .GetField("minDistanceToHeight", BindingFlags.NonPublic | BindingFlags.Instance);

        var dict = (Dictionary<int, int>)field.GetValue(source);

        foreach (var kv in dict)
        {
            target.SetMinDistanceToHeight(kv.Key, kv.Value);
        }
    }
    public bool TryGetHeightTable(int height, ushort biome, out BlackyHeightResourceTable table)
    {
        return tables.TryGetValue((height, biome), out table);
    }
    private bool IsNearHeightBorderAdvanced(
       int worldX,
       int worldY,
       int currentHeight,
       BlackyHeightResourceTable table)
    {
        int maxCheckRadius = GetMaxDistance(table);

        for (int dx = -maxCheckRadius; dx <= maxCheckRadius; dx++)
        {
            for (int dy = -maxCheckRadius; dy <= maxCheckRadius; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = worldX + dx;
                int ny = worldY + dy;

                if (!heightSystem.TryGetTopHeight(nx, ny, out int neighborHeight))
                    continue;

                int requiredDistance = table.GetMinDistanceToHeight(neighborHeight);

                if (requiredDistance <= 0)
                    continue;

                int dist = Math.Max(Math.Abs(dx), Math.Abs(dy));

                if (dist <= requiredDistance)
                    return true;
            }
        }

        return false;
    }
    private int GetMaxDistance(BlackyHeightResourceTable table)
    {
        int max = 0;

        foreach (var pair in table.GetType()
                                  .GetField("minDistanceToHeight", BindingFlags.NonPublic | BindingFlags.Instance)
                                  .GetValue(table) as Dictionary<int, int>)
        {
            if (pair.Value > max)
                max = pair.Value;
        }

        return max;
    }
    #region ===== CONFIG =====

    //public void AddHeightTable(BlackyHeightResourceTable table)
    //{
    //    ThrowIfDisposed();

    //    if (table == null)
    //        throw new ArgumentNullException(nameof(table));

    //    tablesByHeight[table.Height] = table;
    //}

    public void AddEntry(int height, ushort biome, ResourceEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        var key = (height, biome);

        if (!tables.TryGetValue(key, out var table))
        {
            table = new BlackyHeightResourceTable(height, biome);
            tables.Add(key, table);
        }

        table.AddEntry(entry);
    }

    public bool HasHeightTable(int height, ushort idBiome)
    {
        ThrowIfDisposed();
        return tables.ContainsKey((height,idBiome));
    }



    public void RemoveHeightTable(int height, ushort idBiome)
    {
        ThrowIfDisposed();
        tables.Remove((height,idBiome));
    }

    public void ClearTables()
    {
        ThrowIfDisposed();
        tables.Clear();
    }

    #endregion

    #region ===== CHUNK GENERATION STATE =====

    public bool IsChunkGenerated(Vector2I chunkCoord)
    {
        ThrowIfDisposed();
        return chunkGenerationState.IsGenerated(chunkCoord);
    }

    public void MarkChunkGenerated(Vector2I chunkCoord)
    {
        ThrowIfDisposed();
        chunkGenerationState.MarkGenerated(chunkCoord);
    }

    public void ClearGeneratedChunks()
    {
        ThrowIfDisposed();
        chunkGenerationState.Clear();
    }

    #endregion

    #region ===== EVALUATION =====
    private bool TryPickType(
    BlackyHeightResourceTable table,
    int worldX,
    int worldY,
    out ResourceSourceType selectedType)
    {
        selectedType = default;

        float bestScore = float.MinValue;
        bool found = false;

        foreach (var type in table.GetAvailableTypes())
        {
            if (!noiseByType.TryGetValue(type, out var noise))
                continue;

            float n = (noise.GetNoise(worldX, worldY) + 1f) * 0.5f;

            // 🔥 AQUI VA EL PESO
            float weight = table.GetTypeWeight(type);

            float score = n * weight;

            if (score > bestScore)
            {
                bestScore = score;
                selectedType = type;
                found = true;
            }
        }

        return found;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEvaluateCell(int worldX, int worldY, out BlackyResourceGenerationData generationData)
    {
        ThrowIfDisposed();

        generationData = default;

        ushort biome = terrainGenerator.GetBiomeAt(worldX, worldY);

        int height = terrainGenerator.GetHeightAt(worldX, worldY);

        //if (!heightSystem.TryGetTopHeight(worldX, worldY, out int height))
        //    return false;

        if (height < 0)
            return false;
        // 🔥 NUEVO: si es borde de altura → descartamos directo
        if (terrainGenerator.GetHeightBorderAt(worldX, worldY) != 0)
            return false;

        ushort biomeBorder = terrainGenerator.GetBorderAt(worldX, worldY);
        if (biomeBorder != 0) return false;

        if (!tables.TryGetValue((height, biome), out var table))
            return false;



        //// 🔥 NUEVO SISTEMA
        if (IsNearHeightBorderAdvanced(worldX, worldY, height, table))
            return false;

        if (!TryPickType(table, worldX, worldY, out var type))
            return false;

        if (!table.TryGetEntriesByType(type, out var entries))
            return false;
        // 🔥 density opcional
        float density = (noiseByType[type].GetNoise(worldX, worldY) + 1f) * 0.5f;

        if (density < table.DensityThreshold)
            return false;

        if (entries.Count == 0)
            return false;

        if (!TryPickEntry(entries, worldX, worldY, out var selectedEntry))
            return false;

        var templateResource = MasterDataManager.GetBySaveIds<ResourceSourceData>(selectedEntry.idResourceSave);

        generationData = new BlackyResourceGenerationData(
            templateResource.idSave,
            templateResource.resourceSourceType,
            new Vector2I(worldX, worldY)
        );

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEvaluateCell(Vector2I worldPos, out BlackyResourceGenerationData generationData)
    {
        return TryEvaluateCell(worldPos.X, worldPos.Y, out generationData);
    }

    public List<BlackyResourceGenerationData> EvaluateChunk(Vector2I chunkCoord)
    {
        ThrowIfDisposed();

        List<BlackyResourceGenerationData> results = new();
        EvaluateChunk(chunkCoord, results);
        return results;
    }

    public void EvaluateChunk(Vector2I chunkCoord, List<BlackyResourceGenerationData> results)
    {
        ThrowIfDisposed();

        if (results == null)
            throw new ArgumentNullException(nameof(results));

        results.Clear();

        int baseWorldX = chunkCoord.X * ChunkHelper.ChunkSize;
        int baseWorldY = chunkCoord.Y * ChunkHelper.ChunkSize;

        for (int localX = 0; localX < ChunkHelper.ChunkSize; localX++)
        {
            for (int localY = 0; localY < ChunkHelper.ChunkSize; localY++)
            {
                int worldX = baseWorldX + localX;
                int worldY = baseWorldY + localY;

                if (TryEvaluateCell(worldX, worldY, out var generationData))
                {
                    results.Add(generationData);
                }
            }
        }
    }

    public List<BlackyResourceGenerationData> EvaluateExpandedChunk(Vector2I chunkCoord)
    {
        ThrowIfDisposed();

        List<BlackyResourceGenerationData> results = new();
        EvaluateExpandedChunk(chunkCoord, results);
        return results;
    }

    public void EvaluateExpandedChunk(Vector2I chunkCoord, List<BlackyResourceGenerationData> results)
    {
        ThrowIfDisposed();

        if (results == null)
            throw new ArgumentNullException(nameof(results));

        results.Clear();

        int padding = BorderPadding;

        int minWorldX = chunkCoord.X * ChunkHelper.ChunkSize - padding;
        int minWorldY = chunkCoord.Y * ChunkHelper.ChunkSize - padding;
        int maxWorldX = ((chunkCoord.X + 1) * ChunkHelper.ChunkSize - 1) + padding;
        int maxWorldY = ((chunkCoord.Y + 1) * ChunkHelper.ChunkSize - 1) + padding;

        for (int worldX = minWorldX; worldX <= maxWorldX; worldX++)
        {
            for (int worldY = minWorldY; worldY <= maxWorldY; worldY++)
            {
                if (TryEvaluateCell(worldX, worldY, out var generationData))
                {
                    results.Add(generationData);
                }
            }
        }
    }

    #endregion

    #region ===== GENERATION =====

    public bool GenerateCell(int worldX, int worldY, bool renderForce = false)
    {
        ThrowIfDisposed();

        if (!TryEvaluateCell(worldX, worldY, out var generationData))
            return false;

        List<BlackyResourceGenerationData> single = new(1) { generationData };
        postProcessor.Process(single);

        if (single.Count == 0)
            return false;

        var data = single[0];

        resourcesSourceSystem.Create(
            data.ResourceId,
            data.PositionTileWorld,
            renderForce
        );

        return true;
    }

    public bool GenerateCell(Vector2I worldPos, bool renderForce = false)
    {
        return GenerateCell(worldPos.X, worldPos.Y, renderForce);
    }

    public int GenerateChunk(Vector2I chunkCoord, bool renderForce = false)
    {
        ThrowIfDisposed();

        if (chunkGenerationState.IsGenerated(chunkCoord))
            return 0;

        List<BlackyResourceGenerationData> buffer = new();
        int created = GenerateChunkInternal(chunkCoord, buffer, renderForce);

        chunkGenerationState.MarkGenerated(chunkCoord);
        return created;
    }

    public int GenerateChunk(
        Vector2I chunkCoord,
        List<BlackyResourceGenerationData> reusableBuffer,
        bool renderForce = false)
    {
        ThrowIfDisposed();

        if (reusableBuffer == null)
            throw new ArgumentNullException(nameof(reusableBuffer));

        if (chunkGenerationState.IsGenerated(chunkCoord))
            return 0;

        int created = GenerateChunkInternal(chunkCoord, reusableBuffer, renderForce);

        chunkGenerationState.MarkGenerated(chunkCoord);
        return created;
    }

    private int GenerateChunkInternal(
        Vector2I chunkCoord,
        List<BlackyResourceGenerationData> buffer,
        bool renderForce)
    {
        EvaluateExpandedChunk(chunkCoord, buffer);

        postProcessor.Process(buffer);

        int createdCount = 0;

        int minWorldX = chunkCoord.X * ChunkHelper.ChunkSize;
        int minWorldY = chunkCoord.Y * ChunkHelper.ChunkSize;
        int maxWorldX = minWorldX + ChunkHelper.ChunkSize - 1;
        int maxWorldY = minWorldY + ChunkHelper.ChunkSize - 1;

        for (int i = 0; i < buffer.Count; i++)
        {
            var data = buffer[i];
            var pos = data.PositionTileWorld;

            if (pos.X < minWorldX || pos.X > maxWorldX || pos.Y < minWorldY || pos.Y > maxWorldY)
                continue;

            resourcesSourceSystem.EnqueueCreate(
                data.ResourceId,
                data.PositionTileWorld,
                renderForce
            );

            createdCount++;
        }

        return createdCount;
    }

    #endregion

    #region ===== INTERNAL =====

    private bool TryPickEntry(
    List<ResourceEntry> entries,
    int worldX,
    int worldY,
    out ResourceEntry selectedEntry)
    {
        selectedEntry = null;

        float totalWeight = 0f;

        // sumar pesos
        for (int i = 0; i < entries.Count; i++)
        {
            float w = entries[i].Probability;
            if (w > 0f)
                totalWeight += w;
        }

        if (totalWeight <= 0f)
            return false;

        // random determinista [0,1]
        float rnd = GetDeterministic01(worldX, worldY);

        float roll = rnd * totalWeight;

        float accumulated = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (entry.Probability <= 0f)
                continue;

            accumulated += entry.Probability;

            if (roll < accumulated)
            {
                selectedEntry = entry;
                return true;
            }
        }

        return false;
    }
  
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetDeterministic01(int worldX, int worldY)
    {
        unchecked
        {
            int hash = seed;
            hash ^= worldX * 374761393;
            hash ^= worldY * 668265263;

            hash = (hash ^ (hash >> 13)) * 1274126177;
            hash ^= (hash >> 16);

            return (hash & 0xFFFFFF) / (float)0x1000000;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetDeterministicPercent(int worldX, int worldY)
    {
        unchecked
        {
            int hash = seed;
            hash = (hash * 397) ^ worldX;
            hash = (hash * 397) ^ worldY;

            hash ^= (hash >> 13);
            hash *= 1274126177;
            hash ^= (hash >> 16);

            uint positive = (uint)hash;
            return (positive / (float)uint.MaxValue) * 100f;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(BlackyResourcesGenerator));
    }

    #endregion

    #region ===== IDisposable =====

    public void Dispose()
    {
        if (disposed)
            return;

        tables.Clear();
        chunkGenerationState.Clear();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    ~BlackyResourcesGenerator()
    {
        Dispose();
    }

    #endregion
}