using Godot;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.BlackyEngine.Services.Paint;



public sealed class BlackyWorldHeight : IDisposable
{
    
    private readonly Dictionary<Vector2I, BlackyHeightChunkData> chunks = new();

    private bool disposed;

    public BlackyWorldHeight()
    {    
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

        chunks.Clear();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    ~BlackyWorldHeight()
    {
        Dispose();
    }
    public KeyValuePair<Vector2I, BlackyHeightChunkData>[] GetChunksSnapshot()
    {
        return chunks.ToArray();
    }
    #endregion

    public void ExportHeightsToTxt(string filePath)
    {
        var snapshot = GetChunksSnapshot();

        if (snapshot.Length == 0)
        {
            File.WriteAllText(filePath, "No hay chunks en el mundo.");
            return;
        }

        int chunkSize = ChunkHelper.ChunkSize;

        // Calcular los límites globales del mundo.
        int minChunkX = int.MaxValue;
        int minChunkY = int.MaxValue;
        int maxChunkX = int.MinValue;
        int maxChunkY = int.MinValue;

        foreach (var entry in snapshot)
        {
            Vector2I coord = entry.Key;

            minChunkX = Math.Min(minChunkX, coord.X);
            minChunkY = Math.Min(minChunkY, coord.Y);
            maxChunkX = Math.Max(maxChunkX, coord.X);
            maxChunkY = Math.Max(maxChunkY, coord.Y);
        }

        int minX = minChunkX * chunkSize;
        int minY = minChunkY * chunkSize;
        int maxX = (maxChunkX + 1) * chunkSize - 1;
        int maxY = (maxChunkY + 1) * chunkSize - 1;

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        // Reconstruir el mapa completo.
        short[,] world = new short[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                world[y, x] = -1;
            }
        }

        foreach (var entry in snapshot)
        {
            Vector2I chunkCoord = entry.Key;
            BlackyHeightChunkData chunkData = entry.Value;

            int chunkWorldX = chunkCoord.X * chunkSize;
            int chunkWorldY = chunkCoord.Y * chunkSize;

            for (int localY = 0; localY < chunkData.Size; localY++)
            {
                int worldY = chunkWorldY + localY;
                int mapY = worldY - minY;

                for (int localX = 0; localX < chunkData.Size; localX++)
                {
                    int worldX = chunkWorldX + localX;
                    int mapX = worldX - minX;

                    world[mapY, mapX] = chunkData.Get(localX, localY);
                }
            }
        }

        // Escribir el mapa completo en un único archivo.
        using var writer = new StreamWriter(filePath);

        writer.WriteLine($"Dimensiones: {width} x {height}");
        writer.WriteLine($"Origen: ({minX}, {minY})");
        writer.WriteLine();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                writer.Write($"{world[y, x],4}");
            }

            writer.WriteLine();
        }
    }
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

