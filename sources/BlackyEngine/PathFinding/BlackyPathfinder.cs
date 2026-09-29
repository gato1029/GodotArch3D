using Godot;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding
{
    public class BlackyPathfinder
    {
        private const float DefaultUnitRadius = 0.6f; // o el valor que ya usas como default
        private readonly BlackyMacroGraphCache _macroGraphCache;
        private readonly BlackyClearanceMap _clearanceMap;
        private readonly BlackyFlowFieldManager _flowFieldManager;
        private readonly BlackyMacroPathfinder _macroPathfinder;
        private readonly BlackyMicroPathfinder _microPathfinder;
        private readonly int _chunkSize;

        // Radio por defecto (en tiles, admite decimales) para la mayoría de tus unidades.
        // Se usa cuando quien llama no especifica uno propio.
        public const float DefaultRadiusTiles = 1f;

        public BlackyPathfinder(
            BlackyMacroGraphCache macroGraphCache,
            BlackyClearanceMap clearanceMap,
            BlackyFlowFieldManager flowFieldManager,
            int chunkSize = 32)
        {
            _macroGraphCache = macroGraphCache;
            _clearanceMap = clearanceMap;
            _chunkSize = chunkSize;
            _flowFieldManager = flowFieldManager;
            _macroPathfinder = new BlackyMacroPathfinder(_macroGraphCache);
            _microPathfinder = new BlackyMicroPathfinder(_clearanceMap);
        }

        // ---------------------------------------------------------
        // Punto único de notificación: llama esto cada vez que un
        // tile pasa a estar ocupado/libre (colocar o destruir un
        // edificio, árbol, recurso, etc.). Propaga el cambio tanto
        // al mapa de holgura (nivel tile) como al grafo macro
        // (nivel chunk, para marcar chunks saturados como no
        // transitables).
        // ---------------------------------------------------------
        public void NotifyTileOccupancyChanged(int worldX, int worldY, bool isBlocked)
        {
            _clearanceMap.OnTileChanged(worldX, worldY, isBlocked);
            _macroGraphCache.NotifyTileOccupancyChanged(worldX, worldY, isBlocked);
            _flowFieldManager.MarkTileChanged(worldX, worldY); // solo marca, no recalcul
        }

        public List<Vector2> FindPathWorld(Vector2I startTile, Vector2I goalTile, float radiusTiles = DefaultRadiusTiles)
        {
            List<Vector2> points = new();
            var result = FindSimplifiedPath(startTile, goalTile, radiusTiles);
            if (result == null)
            {
                return null; // no hubo camino
            }
            foreach (var item in result)
            {
                var point = TilesHelper.TilePositionToWorldPosition(item);
                points.Add(point);
            }
            return points;
        }

        public List<Vector2I> FindSimplifiedPath(Vector2I startTile, Vector2I goalTile, float radiusTiles = DefaultRadiusTiles)
        {
            List<Vector2I> fullPath = FindPath(startTile, goalTile, radiusTiles);
            if (fullPath == null || fullPath.Count <= 2) return fullPath;

            List<Vector2I> simplifiedPath = new();
            simplifiedPath.Add(fullPath[0]);

            Vector2I lastDirection = fullPath[1] - fullPath[0];

            for (int i = 1; i < fullPath.Count - 1; i++)
            {
                Vector2I currentDirection = fullPath[i + 1] - fullPath[i];

                if (currentDirection != lastDirection)
                {
                    simplifiedPath.Add(fullPath[i]);
                    lastDirection = currentDirection;
                }
            }

            simplifiedPath.Add(fullPath[^1]);
            return simplifiedPath;
        }

        private List<Vector2I> FindPath(Vector2I startTile, Vector2I goalTile, float radiusTiles)
        {
            
            _microPathfinder.ClearDraw(); // solo para debug
            _microPathfinder.DrawOriginTarget(startTile, goalTile);

            Vector2I startChunk = WorldToChunkCoord(startTile);
            Vector2I goalChunk = WorldToChunkCoord(goalTile);

            List<Vector2I> chunkPath = _macroPathfinder.FindChunkPath(startChunk, goalChunk);
            if (chunkPath == null || chunkPath.Count == 0) return null;

            List<Vector2I> fullTilePath = new();
            Vector2I currentTile = startTile;

            for (int i = 0; i < chunkPath.Count; i++)
            {
                Vector2I currentChunk = chunkPath[i];
                Vector2I subGoalTile;

                // Si es el último chunk, la meta es la meta final.
                if (i == chunkPath.Count - 1)
                {
                    subGoalTile = goalTile;
                }
                else
                {
                    // Calculamos dinámicamente el mejor punto de cruce en el borde hacia el siguiente chunk
                    Vector2I nextChunk = chunkPath[i + 1];
                    subGoalTile = GetOptimalBorderTile(currentTile, currentChunk, nextChunk, goalTile, radiusTiles);
                }

                List<Vector2I> segmentPath = _microPathfinder.FindTilePath(currentTile, subGoalTile, radiusTiles);
                if (segmentPath == null) continue;

                if (fullTilePath.Count > 0 && segmentPath.Count > 0)
                {
                    segmentPath.RemoveAt(0); // Evita duplicar el nodo de unión
                }

                fullTilePath.AddRange(segmentPath);

                if (segmentPath.Count > 0)
                {
                    currentTile = segmentPath[^1];
                }
            }

            return fullTilePath.Count > 0 ? fullTilePath : null;
        }

        /// <summary>
        /// Selecciona el tile libre en la frontera entre dos chunks que ofrece la ruta más directa hacia la meta.
        /// </summary>
        private Vector2I GetOptimalBorderTile(Vector2I currentTile, Vector2I fromChunk, Vector2I toChunk, Vector2I finalGoal, float radiusTiles)
        {
            Vector2I dir = toChunk - fromChunk;

            int minX = toChunk.X * _chunkSize;
            int maxX = minX + _chunkSize - 1;
            int minY = toChunk.Y * _chunkSize;
            int maxY = minY + _chunkSize - 1;

            if (dir.X > 0) { minX = maxX = toChunk.X * _chunkSize; }
            else if (dir.X < 0) { minX = maxX = (toChunk.X * _chunkSize) + _chunkSize - 1; }

            if (dir.Y > 0) { minY = maxY = toChunk.Y * _chunkSize; }
            else if (dir.Y < 0) { minY = maxY = (toChunk.Y * _chunkSize) + _chunkSize - 1; }

            Vector2I bestTile = GetChunkCenterTile(toChunk); // Fallback por defecto
            float minTotalDistance = float.MaxValue;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2I tile = new(x, y);

                    if (_clearanceMap.IsWalkableForRadius(x, y, radiusTiles))
                    {
                        float distFromCurrent = currentTile.DistanceSquaredTo(tile);
                        float distToGoal = tile.DistanceSquaredTo(finalGoal);
                        float totalCost = distFromCurrent + distToGoal;

                        if (totalCost < minTotalDistance)
                        {
                            minTotalDistance = totalCost;
                            bestTile = tile;
                        }
                    }
                }
            }

            return bestTile;
        }

        // En BlackyPathfinder

        public bool IsWalkableForRadius(Vector2I tile, float radiusTiles)
            => _clearanceMap.IsWalkableForRadius(tile.X, tile.Y, radiusTiles);

        /// <summary>
        /// Busca el tile transitable más cercano a 'center', expandiendo
        /// en anillos cuadrados crecientes. Útil cuando un punto calculado
        /// matemáticamente (no una posición real de entidad) puede caer
        /// sobre un obstáculo y necesitas "recuperarlo" al tile válido
        /// más próximo.
        /// </summary>
        public Vector2I? FindNearestWalkableTile(Vector2I center, float radiusTiles, int searchRadiusTiles = 5)
        {
            if (_clearanceMap.IsWalkableForRadius(center.X, center.Y, radiusTiles))
                return center;

            for (int ring = 1; ring <= searchRadiusTiles; ring++)
            {
                for (int x = -ring; x <= ring; x++)
                {
                    for (int y = -ring; y <= ring; y++)
                    {
                        // Solo el borde del anillo actual (evita re-chequear el interior ya probado)
                        if (MathF.Max(MathF.Abs(x), MathF.Abs(y)) != ring)
                            continue;

                        var candidate = new Vector2I(center.X + x, center.Y + y);

                        if (_clearanceMap.IsWalkableForRadius(candidate.X, candidate.Y, radiusTiles))
                            return candidate;
                    }
                }
            }

            return null; // nada transitable cerca, en el rango buscado
        }

        /// <summary>
        /// Obtiene todos los tiles transitables en la línea de frontera que une dos chunks adyacentes.
        /// </summary>
        private List<Vector2I> GetWalkableBorderTiles(Vector2I fromChunk, Vector2I toChunk, float radiusTiles)
        {
            List<Vector2I> walkableBorders = new();
            Vector2I dir = toChunk - fromChunk;

            int minX = toChunk.X * _chunkSize;
            int maxX = minX + _chunkSize - 1;
            int minY = toChunk.Y * _chunkSize;
            int maxY = minY + _chunkSize - 1;

            // Determina la línea de entrada según la dirección del movimiento entre Chunks
            if (dir.X > 0) { minX = maxX = toChunk.X * _chunkSize; }                     // Entra por el borde OESTE de toChunk
            else if (dir.X < 0) { minX = maxX = (toChunk.X * _chunkSize) + _chunkSize - 1; }  // Entra por el borde ESTE de toChunk

            if (dir.Y > 0) { minY = maxY = toChunk.Y * _chunkSize; }                     // Entra por el borde NORTE de toChunk
            else if (dir.Y < 0) { minY = maxY = (toChunk.Y * _chunkSize) + _chunkSize - 1; }  // Entra por el borde SUR de toChunk

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2I tile = new(x, y);

                    // Asegura que el tile del borde tenga holgura suficiente antes de considerarlo un portal
                    if (_clearanceMap.IsWalkableForRadius(x, y, radiusTiles))
                    {
                        walkableBorders.Add(tile);
                    }
                }
            }

            return walkableBorders;
        }

        private Vector2I GetChunkCenterTile(Vector2I chunkCoord)
        {
            int centerX = (chunkCoord.X * _chunkSize) + (_chunkSize / 2);
            int centerY = (chunkCoord.Y * _chunkSize) + (_chunkSize / 2);
            return new Vector2I(centerX, centerY);
        }

        private Vector2I WorldToChunkCoord(Vector2I tileCoord)
        {
            int cx = Mathf.FloorToInt((float)tileCoord.X / _chunkSize);
            int cy = Mathf.FloorToInt((float)tileCoord.Y / _chunkSize);
            return new Vector2I(cx, cy);
        }

        public void UpdateChunkWalkability(Vector2I chunkCoord, bool isWalkable)
        {
            _macroGraphCache.UpdateNodeWalkability(chunkCoord, isWalkable);
        }
    }
}