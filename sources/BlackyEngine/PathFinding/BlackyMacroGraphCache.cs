using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    private static readonly Vector2I[] Directions = new[]
    {
            new Vector2I(0, 1),  new Vector2I(0, -1),
            new Vector2I(1, 0),  new Vector2I(-1, 0),
            new Vector2I(1, 1),  new Vector2I(-1, 1),
            new Vector2I(1, -1), new Vector2I(-1, -1)
        };

    /// <summary>
    /// Inicializa la caché del mapa macro una sola vez al cargar el mundo.
    /// </summary>
    public void BuildCache(Vector2I minChunk, Vector2I maxChunk)
    {
        _nodeCache.Clear();
        _neighborCache.Clear();

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
    /// Permite actualizar en tiempo real si un chunk se abre o se bloquea (ej. se destruye un edificio).
    /// </summary>
    public void UpdateNodeWalkability(Vector2I coord, bool isWalkable)
    {
        if (_nodeCache.ContainsKey(coord))
        {
            _nodeCache[coord] = new BlackyMacroNodeInfo(coord, isWalkable);
        }
    }
}
