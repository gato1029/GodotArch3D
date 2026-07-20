using Godot;
using GodotEcsArch.sources.BlackyTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Procedural;

/// <summary>
/// Precalcula, para cada chunk, la lista de nodos cuyo InfluenceRadius
/// lo alcanza. Evita comparar cada tile contra TODOS los nodos del mundo:
/// solo compara contra los pocos candidatos relevantes de su chunk.
///
/// Un chunk con 1 solo candidato = totalmente dentro de un bioma, ni
/// siquiera hace falta rasterizar tile por tile (se puede rellenar entero
/// de una).
/// Un chunk con 2+ candidatos = chunk de borde: se rasteriza tile por
/// tile con nearest-neighbor entre esos candidatos, y la división entre
/// biomas sale sola, siguiendo la línea real de Voronoi.
/// </summary>
public class BlackyWorldChunkNode
{
    private readonly int _chunkSize;
    private readonly Dictionary<BlackyChunkCoord, List<BlackyWorldNode>> _candidatesByChunk = new();

    public List<BlackyChunkCoord> GetAllChunksWithCandidates()
    {
        return _candidatesByChunk.Keys.ToList();
    }
    public BlackyWorldChunkNode(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public void Build(IEnumerable<BlackyWorldNode> nodes)
    {
        _candidatesByChunk.Clear();

        foreach (var node in nodes)
        {
            float r = node.InfluenceRadius;

            int minChunkX = (int)MathF.Floor((node.Position.X - r) / _chunkSize);
            int maxChunkX = (int)MathF.Floor((node.Position.X + r) / _chunkSize);
            int minChunkY = (int)MathF.Floor((node.Position.Y - r) / _chunkSize);
            int maxChunkY = (int)MathF.Floor((node.Position.Y + r) / _chunkSize);

            for (int cx = minChunkX; cx <= maxChunkX; cx++)
                for (int cy = minChunkY; cy <= maxChunkY; cy++)
                {
                    if (!CircleOverlapsChunk(node.Position, r, cx, cy))
                        continue;

                    var coord = new BlackyChunkCoord(cx, cy);
                    if (!_candidatesByChunk.TryGetValue(coord, out var list))
                        _candidatesByChunk[coord] = list = new List<BlackyWorldNode>();

                    list.Add(node);
                }
        }
    }

    /// <summary>Nodos candidatos para un chunk. Vacío si ningún radio lo alcanza (raro, ver fallback en el rasterizador).</summary>
    public IReadOnlyList<BlackyWorldNode> GetCandidates(BlackyChunkCoord chunk)
    {
        return _candidatesByChunk.TryGetValue(chunk, out var list)
            ? list
            : Array.Empty<BlackyWorldNode>();
    }

    /// <summary>Test circle-vs-square: ¿el círculo de influencia del nodo toca este chunk?</summary>
    private bool CircleOverlapsChunk(Vector2 center, float radius, int chunkX, int chunkY)
    {
        float chunkMinX = chunkX * _chunkSize;
        float chunkMinY = chunkY * _chunkSize;

        float closestX = Math.Clamp(center.X, chunkMinX, chunkMinX + _chunkSize);
        float closestY = Math.Clamp(center.Y, chunkMinY, chunkMinY + _chunkSize);

        float dx = center.X - closestX;
        float dy = center.Y - closestY;
        return (dx * dx + dy * dy) <= radius * radius;
    }
}
