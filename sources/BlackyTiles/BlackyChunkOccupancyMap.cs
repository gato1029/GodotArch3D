using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles;

public class BlackyOccupancySubChunk
{
    private readonly ulong[,,] _entityIds;

    public BlackyOccupancySubChunk(int layers, int size)
    {
        _entityIds = new ulong[layers, size, size];
    }

    public ulong Get(int layer, int x, int y)
        => _entityIds[layer, x, y];

    public bool IsOccupied(int layer, int x, int y)
        => _entityIds[layer, x, y] != 0;

    public void Set(int layer, int x, int y, ulong entityId)
        => _entityIds[layer, x, y] = entityId;

    public void Clear(int layer, int x, int y)
        => _entityIds[layer, x, y] = 0;
}
public class BlackyChunkOccupancyMap
{
    private readonly int _chunkSize;
    private readonly int _layers;

    private readonly Dictionary<Vector2I, BlackyOccupancySubChunk> _chunks
        = new();
    private readonly Dictionary<ulong, List<(int layer, int x, int y)>> _entityToTiles
    = new();
    public event Action<int, int, int, ulong>? OnTileUpdated;
    public BlackyChunkOccupancyMap(int layers, int chunkSize)
    {
        _layers = layers;
        _chunkSize = chunkSize;
    }

    #region Public API

    public ulong Get(int layer, int worldX, int worldY)
    {
        var (chunkCoord, localX, localY) = GetCoords(worldX, worldY);

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
            return 0;

        return chunk.Get(layer, localX, localY);
    }

    public bool IsOccupied(int layer, int worldX, int worldY)
        => Get(layer, worldX, worldY) != 0;

    public bool IsOccupiedTiles(int layer, int worldX, int worldY, List<KuroTile> tiles)
    {
        foreach (var tile in tiles)
        {
            int x = worldX + tile.x;
            int y = worldY + tile.y;
            if (IsOccupied(layer, x, y))
                return true;
        }
        return false;
    }
    public void SetTiles(int layer, int worldX, int worldY, List<KuroTile> tiles, ulong entityId)
    {
        foreach (var tile in tiles)
        {
            int x = worldX + tile.x;
            int y = worldY + tile.y;

            Set(layer, x, y, entityId);
        }
    }
    public void Set(int layer, int worldX, int worldY, ulong entityId)
    {
        var (chunkCoord, localX, localY) = GetCoords(worldX, worldY);

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
        {
            chunk = new BlackyOccupancySubChunk(_layers, _chunkSize);
            _chunks.Add(chunkCoord, chunk);
        }

        chunk.Set(layer, localX, localY, entityId);

        // 🔥 Index inverso
        if (!_entityToTiles.TryGetValue(entityId, out var list))
        {
            list = new List<(int, int, int)>();
            _entityToTiles[entityId] = list;
        }

        list.Add((layer, worldX, worldY));

        OnTileUpdated?.Invoke(layer, worldX, worldY, entityId);
    }

    public void Clear(int layer, int worldX, int worldY)
    {
        var (chunkCoord, localX, localY) = GetCoords(worldX, worldY);

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
            return;

        chunk.Clear(layer, localX, localY);
        // 🔔 Notificamos al renderer
        OnTileUpdated?.Invoke(layer, worldX, worldY, 0);
    }
    public void ClearByEntity(int layer, int worldX, int worldY)
    {
        ulong entityId = Get(layer, worldX, worldY);
        if (entityId == 0) return;

        if (!_entityToTiles.TryGetValue(entityId, out var tiles))
            return;

        foreach (var (l, x, y) in tiles)
        {
            var (chunkCoord, localX, localY) = GetCoords(x, y);

            if (_chunks.TryGetValue(chunkCoord, out var chunk))
            {
                chunk.Clear(l, localX, localY);
                OnTileUpdated?.Invoke(l, x, y, 0);
            }
        }

        _entityToTiles.Remove(entityId);
    }
    #endregion

    #region Tiles grandes (🔥 lo importante)

    public void SetArea(int layer, int startX, int startY, int width, int height, ulong entityId)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Set(layer, startX + x, startY + y, entityId);
            }
        }
    }

    public void ClearArea(int layer, int startX, int startY, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Clear(layer, startX + x, startY + y);
            }
        }
    }

    #endregion

    #region Coordinate Math

    private (Vector2I chunkCoord, int localX, int localY) GetCoords(int worldX, int worldY)
    {
        int chunkX = WorldToChunk(worldX);
        int chunkY = WorldToChunk(worldY);

        int localX = WorldToLocal(worldX);
        int localY = WorldToLocal(worldY);

        return (new Vector2I(chunkX, chunkY), localX, localY);
    }

    private int WorldToChunk(int coord)
        => (int)MathF.Floor((float)coord / _chunkSize);

    private int WorldToLocal(int coord)
    {
        int local = coord % _chunkSize;
        return local < 0 ? local + _chunkSize : local;
    }

    #endregion
}