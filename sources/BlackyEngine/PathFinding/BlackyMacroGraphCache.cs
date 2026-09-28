using Godot;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

public readonly struct BlackyMacroNodeInfo
{
    public readonly Vector2I ChunkCoord;
    public readonly bool IsWalkable;

    public BlackyMacroNodeInfo(Vector2I chunkCoord, bool isWalkable)
    {
        ChunkCoord = chunkCoord;
        IsWalkable = isWalkable;
    }
}

public class BlackyMacroGraphCache
{
    private readonly Dictionary<Vector2I, BlackyMacroNodeInfo> _nodeCache = new();
    private readonly Dictionary<Vector2I, List<Vector2I>> _neighborCache = new();

    // Cuenta de tiles bloqueados por chunk, mantenida incrementalmente.
    private readonly Dictionary<Vector2I, int> _blockedTileCount = new();

    private readonly int _chunkSize;

    // Umbrales con histéresis: bloquea al pasar de 80%, pero solo se
    // vuelve a marcar transitable al bajar de 65%. Sin esto, un chunk
    // justo en el borde del 80% podría "parpadear" entre walkable/no
    // walkable con cada tile que se coloca o destruye cerca del límite.
    private const float BlockRatioThreshold = 0.8f;
    private const float UnblockRatioThreshold = 0.65f;

    private static readonly Vector2I[] Directions = new[]
    {
        new Vector2I(0, 1),  new Vector2I(0, -1),
        new Vector2I(1, 0),  new Vector2I(-1, 0),
        new Vector2I(1, 1),  new Vector2I(-1, 1),
        new Vector2I(1, -1), new Vector2I(-1, -1)
    };

    public BlackyMacroGraphCache(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    /// <summary>
    /// Inicializa la caché del mapa macro una sola vez al cargar el mundo.
    /// </summary>
    public void BuildCache(Vector2I minChunk, Vector2I maxChunk)
    {
        _nodeCache.Clear();
        _neighborCache.Clear();
        _blockedTileCount.Clear();

        for (int x = minChunk.X; x <= maxChunk.X; x++)
        {
            for (int y = minChunk.Y; y <= maxChunk.Y; y++)
            {
                Vector2I coord = new Vector2I(x, y);
                bool walkable = true; // por default nacen chunks transitables todos

                _nodeCache[coord] = new BlackyMacroNodeInfo(coord, walkable);

                List<Vector2I> validNeighbors = new();
                foreach (var dir in Directions)
                {
                    Vector2I neighborCoord = coord + dir;
                    if (neighborCoord.X >= minChunk.X && neighborCoord.X <= maxChunk.X &&
                        neighborCoord.Y >= minChunk.Y && neighborCoord.Y <= maxChunk.Y)
                    {
                        validNeighbors.Add(neighborCoord);
                    }
                }
                _neighborCache[coord] = validNeighbors;
            }
        }
    }

    public List<Vector2I> GetCachedNeighbors(Vector2I coord)
    {
        return _neighborCache.TryGetValue(coord, out var neighbors) ? neighbors : null;
    }

    public bool TryGetNode(Vector2I coord, out BlackyMacroNodeInfo nodeInfo)
    {
        return _nodeCache.TryGetValue(coord, out nodeInfo);
    }

    /// <summary>
    /// Permite actualizar en tiempo real si un chunk se abre o se bloquea
    /// manualmente (ej. forzado desde otro sistema).
    /// </summary>
    public void UpdateNodeWalkability(Vector2I coord, bool isWalkable)
    {
        if (_nodeCache.ContainsKey(coord))
        {
            _nodeCache[coord] = new BlackyMacroNodeInfo(coord, isWalkable);
        }
    }

    // ---------------------------------------------------------
    // Conexión con la ocupación real: cada vez que un tile pasa
    // a bloqueado/libre, actualizamos el contador de SU chunk y
    // reevaluamos si ese chunk debe marcarse walkable o no.
    //
    // Costo: O(1) por llamada — un par de lookups de diccionario,
    // nada de recorrer el chunk completo. Seguro de llamar en
    // cada cambio de ocupación, por más frecuente que sea.
    // ---------------------------------------------------------
    public void NotifyTileOccupancyChanged(int worldX, int worldY, bool isBlocked)
    {
        Vector2I chunkCoord = WorldTileToChunk(worldX, worldY);

        if (!_nodeCache.ContainsKey(chunkCoord))
            return; // chunk fuera del rango construido por BuildCache

        _blockedTileCount.TryGetValue(chunkCoord, out int count);

        count += isBlocked ? 1 : -1;
        count = Math.Max(0, count); // por seguridad ante desincronizaciones

        _blockedTileCount[chunkCoord] = count;

        int totalTiles = _chunkSize * _chunkSize;
        float ratio = (float)count / totalTiles;

        var info = _nodeCache[chunkCoord];

        if (info.IsWalkable && ratio >= BlockRatioThreshold)
        {
            UpdateNodeWalkability(chunkCoord, false);
        }
        else if (!info.IsWalkable && ratio <= UnblockRatioThreshold)
        {
            UpdateNodeWalkability(chunkCoord, true);
        }
    }

    private Vector2I WorldTileToChunk(int worldX, int worldY)
    {
        int cx = (int)MathF.Floor((float)worldX / _chunkSize);
        int cy = (int)MathF.Floor((float)worldY / _chunkSize);
        return new Vector2I(cx, cy);
    }
}