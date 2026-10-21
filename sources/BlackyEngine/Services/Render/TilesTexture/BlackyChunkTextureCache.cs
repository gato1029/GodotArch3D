
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;

public struct DirtyTile
{
    public int X;
    public int Y;
    public ushort Id;

    public DirtyTile(int x, int y, ushort id)
    {
        X = x;
        Y = y;
        Id = id;
    }
}

public struct TileChange
{
    public int WorldX;
    public int WorldY;
    public int Height;
    public int Layer;
    public int SpriteId;
    public BlackyRegion region;
    public bool remove;
    public bool dual;
    public bool isPersistent;
}

/// <summary>
/// Caché compartida de chunks de textura y resolución de coordenadas.
///
/// Esta clase NO sabe pintar tiles: solo es el almacén común que comparten
/// <see cref="BlackySimpleTileTextureMap"/> (tiles simples) y
/// <see cref="BlackyDualTileTextureMap"/> (tiles duales).
/// Ambos reciben esta misma instancia, por lo que escriben y leen exactamente
/// los mismos <see cref="BlackyChunkTexture"/>.
/// </summary>
public sealed class BlackyChunkTextureCache
{
    private readonly ConcurrentDictionary<BlackyChunkCoord, BlackyChunkTexture> _chunks = new();
    private readonly BlackyWorldRegions _regions;
    private readonly ChunkManagerBase _chunkManager;

    public int ChunkSize { get; }
    public int HeightCount { get; }
    public int MaxLayers { get; }

    public object SyncRoot { get; } = new();

    /// <summary>
    /// Se dispara cuando cambia un tile, venga del mapa simple o del dual.
    /// El render system se suscribe aquí una sola vez.
    /// </summary>
    public event Action<TileChange> OnTileChanged;

    [ThreadStatic]
    private static BlackyChunkCoord _lastCoord;

    [ThreadStatic]
    private static BlackyChunkTexture _lastChunk;

    public BlackyChunkTextureCache(
        int chunkSize,
        int heightCount,
        int maxLayers,
        BlackyWorldRegions regions,
        ChunkManagerBase chunkManager)
    {
        ChunkSize = chunkSize;
        HeightCount = heightCount;
        MaxLayers = maxLayers;
        _regions = regions ?? throw new ArgumentNullException(nameof(regions));
        _chunkManager = chunkManager ?? throw new ArgumentNullException(nameof(chunkManager));
        _chunkManager.OnChunkDataUnload += ChunkManager_OnChunkDataUnload;
    }

    private void ChunkManager_OnChunkDataUnload(Vector2I obj)
    {
        //BlackyChunkCoord coord = new BlackyChunkCoord(obj.X, obj.Y);
        //if (_chunks.TryGetValue(coord, out var chunkCurrent))
        //{
        //    //GD.Print($"[BlackyChunkTextureCache] Unloading chunk at {coord.X}, {coord.Y}");
        //    _chunks.TryRemove(coord, out _);
        //}
    }

    // ===============================
    // GESTIÓN DE CHUNKS
    // ===============================

    public BlackyChunkTexture GetOrCreateChunk(int chunkX, int chunkY)
    {
        var coord = new BlackyChunkCoord(chunkX, chunkY);

        return _chunks.GetOrAdd(coord, c =>
        {
            var region = _regions.GetOrCreateRegionByChunk(c.X, c.Y);

            var chunk = new BlackyChunkTexture(
                c,
                region,
                ChunkSize,
                HeightCount,
                MaxLayers);

            region.RegisterChunk(c);

            return chunk;
        });
    }

    public bool TryGetChunk(int chunkX, int chunkY, out BlackyChunkTexture chunk)
    {
        return _chunks.TryGetValue(
            new BlackyChunkCoord(chunkX, chunkY),
            out chunk
        );
    }

    public IEnumerable<BlackyChunkTexture> GetLoadedChunks()
    {
        return _chunks.Values;
    }

    public IEnumerable<BlackyChunkTexture> GetDirtyChunks()
    {
        foreach (var chunk in _chunks.Values)
        {
            if (chunk.HasDirtyTiles())
                yield return chunk;
        }
    }

    // ===============================
    // COORDENADAS Y RESOLUCIÓN
    // ===============================

    public (BlackyChunkTexture chunk, int localX, int localY) ResolveOrCreate(int worldX, int worldY)
    {
        var coord = WorldToChunkCoord(worldX, worldY);

        if (_lastChunk != null && coord.Equals(_lastCoord))
        {
            var (lx, ly) = WorldToLocal(worldX, worldY);
            return (_lastChunk, lx, ly);
        }

        var chunk = GetOrCreateChunk(coord.X, coord.Y);

        _lastCoord = coord;
        _lastChunk = chunk;

        var (localX, localY) = WorldToLocal(worldX, worldY);
        return (chunk, localX, localY);
    }

    public (BlackyChunkTexture chunk, int localX, int localY) Resolve(int worldX, int worldY)
    {
        var coord = WorldToChunkCoord(worldX, worldY);

        if (_lastChunk != null && coord.Equals(_lastCoord))
        {
            var (lx, ly) = WorldToLocal(worldX, worldY);
            return (_lastChunk, lx, ly);
        }

        _chunks.TryGetValue(coord, out var chunk);

        _lastCoord = coord;
        _lastChunk = chunk;

        var (localX, localY) = WorldToLocal(worldX, worldY);

        return (chunk, localX, localY);
    }

    public BlackyChunkCoord WorldToChunkCoord(int worldX, int worldY)
    {
        return new BlackyChunkCoord(
            FloorDiv(worldX, ChunkSize),
            FloorDiv(worldY, ChunkSize)
        );
    }

    public (int localX, int localY) WorldToLocal(int worldX, int worldY)
    {
        int localX = Mod(worldX, ChunkSize);
        int localY = Mod(worldY, ChunkSize);

        return (localX, localY);
    }

    // ===============================
    // SAFE MATH
    // ===============================

    private static int FloorDiv(int a, int b)
    {
        int result = a / b;
        if ((a ^ b) < 0 && (result * b != a))
            result--;
        return result;
    }

    private static int Mod(int a, int b)
    {
        int result = a % b;
        if (result < 0)
            result += b;
        return result;
    }

    // ===============================
    // NOTIFICACIÓN
    // ===============================

    internal void RaiseTileChanged(TileChange change)
    {
        OnTileChanged?.Invoke(change);
    }

    // ===============================
    // CLEAR
    // ===============================

    /// <summary>
    /// Vacía la caché de chunks compartida. No libera colliders:
    /// de eso se encarga <see cref="BlackyDualTileTextureMap.Clear"/>.
    /// </summary>
    public void Clear()
    {
        _chunks.Clear();

        // Invalidar las cachés locales por hilo
        _lastChunk = null;
        _lastCoord = default;
    }
}
