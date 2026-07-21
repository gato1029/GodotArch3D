using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyTiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Procedural
{
    public class BlackyWorldChunkNode
    {
        private readonly BlackyWorldConfig _config;
        private readonly Dictionary<BlackyChunkCoord, List<BlackyWorldNode>> _candidatesByChunk = new();
        private List<BlackyWorldNode> _allNodes = new();

        public BlackyWorldChunkNode(BlackyWorldConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Construye y rellena el índice de chunks cubriendo todo el rango global del mapa 
        /// definido en BlackyWorldConfig, evitando huecos o zonas vacías.
        /// </summary>
        public void Build(IEnumerable<BlackyWorldNode> nodes)
        {
            _candidatesByChunk.Clear();
            _allNodes = nodes.ToList();

            // 1. Inicializar todas las coordenadas de chunks dentro del rango oficial del mapa (MinChunk a MaxChunk)
            for (int cx = _config.MinChunk.X; cx <= _config.MaxChunk.X; cx++)
            {
                for (int cy = _config.MinChunk.Y; cy <= _config.MaxChunk.Y; cy++)
                {
                    var coord = new BlackyChunkCoord(cx, cy);
                    if (!_candidatesByChunk.ContainsKey(coord))
                    {
                        _candidatesByChunk[coord] = new List<BlackyWorldNode>();
                    }
                }
            }

            // 2. Asociar nodos a los chunks que intersectan con su radio de influencia
            foreach (var node in _allNodes)
            {
                float r = node.InfluenceRadius;

                int minChunkX = (int)MathF.Floor((node.Position.X - r) / _config.ChunkSize);
                int maxChunkX = (int)MathF.Floor((node.Position.X + r) / _config.ChunkSize);
                int minChunkY = (int)MathF.Floor((node.Position.Y - r) / _config.ChunkSize);
                int maxChunkY = (int)MathF.Floor((node.Position.Y + r) / _config.ChunkSize);

                for (int cx = minChunkX; cx <= maxChunkX; cx++)
                {
                    for (int cy = minChunkY; cy <= maxChunkY; cy++)
                    {
                        if (!CircleOverlapsChunk(node.Position, r, cx, cy))
                            continue;

                        var coord = new BlackyChunkCoord(cx, cy);

                        // Si el chunk está dentro del rango del mapa, aseguramos la lista y agregamos el nodo
                        if (!_candidatesByChunk.TryGetValue(coord, out var list))
                        {
                            list = new List<BlackyWorldNode>();
                            _candidatesByChunk[coord] = list;
                        }

                        if (!list.Contains(node))
                        {
                            list.Add(node);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene los nodos candidatos para un chunk específico. Si el chunk no tiene nodos directos 
        /// por su radio de influencia, aplica un fallback devolviendo el nodo más cercano del mundo.
        /// </summary>
        public IReadOnlyList<BlackyWorldNode> GetCandidates(BlackyChunkCoord chunk)
        {
            if (_candidatesByChunk.TryGetValue(chunk, out var list) && list.Count > 0)
            {
                return list;
            }

            // Fallback de seguridad para chunks aislados o sin cobertura directa de radio
            if (_allNodes.Count > 0)
            {
                Vector2 chunkCenter = new Vector2(
                    (chunk.X + 0.5f) * _config.ChunkSize,
                    (chunk.Y + 0.5f) * _config.ChunkSize
                );

                var closestNode = _allNodes.OrderBy(n => n.Position.DistanceSquaredTo(chunkCenter)).First();
                return new List<BlackyWorldNode> { closestNode };
            }

            return Array.Empty<BlackyWorldNode>();
        }

        public List<BlackyChunkCoord> GetAllChunksWithCandidates()
        {
            return _candidatesByChunk.Keys.ToList();
        }

        /// <summary>
        /// Test de colisión círculo-cuadrado para comprobar si el radio de influencia alcanza el chunk.
        /// </summary>
        private bool CircleOverlapsChunk(Vector2 center, float radius, int chunkX, int chunkY)
        {
            float chunkMinX = chunkX * _config.ChunkSize;
            float chunkMinY = chunkY * _config.ChunkSize;

            float closestX = Math.Clamp(center.X, chunkMinX, chunkMinX + _config.ChunkSize);
            float closestY = Math.Clamp(center.Y, chunkMinY, chunkMinY + _config.ChunkSize);

            float dx = center.X - closestX;
            float dy = center.Y - closestY;
            return (dx * dx + dy * dy) <= radius * radius;
        }
    }
}