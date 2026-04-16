using Godot;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.BlackyTiles.Systems;



public sealed class BlackyHeightSystem : IDisposable
{
    private readonly BlackyTerrainSystem terrainSystem;
    private readonly Dictionary<Vector2I, BlackyHeightChunkData> chunks = new();

    private bool disposed;

    public BlackyHeightSystem(BlackyTerrainSystem terrainSystem)
    {
        this.terrainSystem = terrainSystem ?? throw new ArgumentNullException(nameof(terrainSystem));
        this.terrainSystem.OnTopHeightChanged += OnTerrainTopHeightChanged;
    }

    #region ===== PUBLIC API =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetTopHeight(int worldX, int worldY, out int height)
    {
        var chunkCoord = ChunkHelper.WorldToChunkCoord(worldX, worldY);

        if (!chunks.TryGetValue(chunkCoord, out var chunkData))
        {
            height = -1;
            return false;
        }

        var (localX, localY) = ChunkHelper.WorldToLocal(worldX, worldY);
        height = chunkData.Get(localX, localY);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetTopHeight(Vector2I worldPos, out int height)
    {
        return TryGetTopHeight(worldPos.X, worldPos.Y, out height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTopHeight(int worldX, int worldY)
    {
        return TryGetTopHeight(worldX, worldY, out int height) ? height : -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetTopHeight(Vector2I worldPos)
    {
        return GetTopHeight(worldPos.X, worldPos.Y);
    }

    public void SetTopHeight(int worldX, int worldY, int height)
    {
        var chunkCoord = ChunkHelper.WorldToChunkCoord(worldX, worldY);
        var (localX, localY) = ChunkHelper.WorldToLocal(worldX, worldY);

        var chunkData = GetOrCreateChunkData(chunkCoord);
        chunkData.Set(localX, localY, (short)height);
    }

    public void SetTopHeight(Vector2I worldPos, int height)
    {
        SetTopHeight(worldPos.X, worldPos.Y, height);
    }

    public void UpdateCell(int worldX, int worldY)
    {
        int topHeight = terrainSystem.GetTopHeight(worldX, worldY);
        SetTopHeight(worldX, worldY, topHeight);
    }

    public void UpdateCell(Vector2I worldPos)
    {
        UpdateCell(worldPos.X, worldPos.Y);
    }

    public void RebuildChunk(Vector2I chunkCoord)
    {
        var chunkData = GetOrCreateChunkData(chunkCoord);

        int baseWorldX = chunkCoord.X * ChunkHelper.ChunkSize;
        int baseWorldY = chunkCoord.Y * ChunkHelper.ChunkSize;

        for (int localX = 0; localX < ChunkHelper.ChunkSize; localX++)
        {
            for (int localY = 0; localY < ChunkHelper.ChunkSize; localY++)
            {
                int worldX = baseWorldX + localX;
                int worldY = baseWorldY + localY;

                int topHeight = terrainSystem.GetTopHeight(worldX, worldY);
                chunkData.Set(localX, localY, (short)topHeight);
            }
        }
    }

    public void RebuildChunk(int chunkX, int chunkY)
    {
        RebuildChunk(new Vector2I(chunkX, chunkY));
    }

    public void RemoveChunk(Vector2I chunkCoord)
    {
        chunks.Remove(chunkCoord);
    }

    public void RemoveChunk(int chunkX, int chunkY)
    {
        RemoveChunk(new Vector2I(chunkX, chunkY));
    }

    public bool HasChunk(Vector2I chunkCoord)
    {
        return chunks.ContainsKey(chunkCoord);
    }

    public bool TryGetChunkData(Vector2I chunkCoord, out BlackyHeightChunkData chunkData)
    {
        return chunks.TryGetValue(chunkCoord, out chunkData);
    }

    public void Clear()
    {
        chunks.Clear();
    }

    #endregion

    #region ===== EVENTS =====

    private void OnTerrainTopHeightChanged(Vector2I worldPos, int oldTopHeight, int newTopHeight)
    {
        SetTopHeight(worldPos.X, worldPos.Y, newTopHeight);
    }

    #endregion

    #region ===== INTERNAL =====

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BlackyHeightChunkData GetOrCreateChunkData(Vector2I chunkCoord)
    {
        if (!chunks.TryGetValue(chunkCoord, out var chunkData))
        {
            chunkData = new BlackyHeightChunkData(ChunkHelper.ChunkSize);
            chunks.Add(chunkCoord, chunkData);
        }

        return chunkData;
    }

    #endregion

    #region ===== IDisposable =====

    public void Dispose()
    {
        if (disposed)
            return;

        terrainSystem.OnTopHeightChanged -= OnTerrainTopHeightChanged;
        chunks.Clear();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    ~BlackyHeightSystem()
    {
        Dispose();
    }

    #endregion
}

public sealed class BlackyHeightChunkData
{
    private readonly short[] heights;
    public int Size { get; }

    public BlackyHeightChunkData(int size)
    {
        Size = size;
        heights = new short[size * size];
        Array.Fill(heights, (short)-1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int localX, int localY)
    {
        return localY * Size + localX;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short Get(int localX, int localY)
    {
        return heights[GetIndex(localX, localY)];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int localX, int localY, short value)
    {
        heights[GetIndex(localX, localY)] = value;
    }

    public void Fill(short value)
    {
        Array.Fill(heights, value);
    }
}

