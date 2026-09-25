using Godot;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.utils;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding
{
    public class BlackyPathfinder
    {
        private readonly BlackyMacroGraphCache _macroGraphCache;
        private readonly BlackyChunkOccupancyMap _chunkOccupancyMap;
        private readonly BlackyMacroPathfinder _macroPathfinder;
        private readonly BlackyMicroPathfinder _microPathfinder;
        private readonly int _chunkSize;

        public BlackyPathfinder(
            BlackyMacroGraphCache macroGraphCache,
            BlackyChunkOccupancyMap chunkOccupancyMap,
            int chunkSize = 32)
        {
            _macroGraphCache = macroGraphCache;
            _chunkOccupancyMap = chunkOccupancyMap;
            _chunkSize = chunkSize;

            _macroPathfinder = new BlackyMacroPathfinder(_macroGraphCache);
            _microPathfinder = new BlackyMicroPathfinder(_chunkOccupancyMap);
        }

        public List<Vector2> FindPathWorld(Vector2I startTile, Vector2I goalTile)
        {
            List< Vector2> points = new ();
            var result = FindSimplifiedPath(startTile, goalTile);
            foreach (var item in result)
            {
                var point = TilesHelper.TilePositionToWorldPosition(item);
                points.Add(point);
            }
            return points;
        }
        public List<Vector2I> FindSimplifiedPath(Vector2I startTile, Vector2I goalTile)
        {
            List<Vector2I> fullPath = FindPath(startTile, goalTile);
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

        private List<Vector2I> FindPath(Vector2I startTile, Vector2I goalTile)
        {
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
                    subGoalTile = GetOptimalBorderTile(currentTile, currentChunk, nextChunk, goalTile);
                }

                List<Vector2I> segmentPath = _microPathfinder.FindTilePath(currentTile, subGoalTile);
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
        private Vector2I GetOptimalBorderTile(Vector2I currentTile, Vector2I fromChunk, Vector2I toChunk, Vector2I finalGoal)
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
            bool foundWalkable = false;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    Vector2I tile = new(x, y);

                    if (!_chunkOccupancyMap.IsOccupied(0,x,y))
                    {
                        float distFromCurrent = currentTile.DistanceSquaredTo(tile);
                        float distToGoal = tile.DistanceSquaredTo(finalGoal);
                        float totalCost = distFromCurrent + distToGoal;

                        if (totalCost < minTotalDistance)
                        {
                            minTotalDistance = totalCost;
                            bestTile = tile;
                            foundWalkable = true;
                        }
                    }
                }
            }

            return bestTile;
        }

        /// <summary>
        /// Obtiene todos los tiles transitables en la línea de frontera que une dos chunks adyacentes.
        /// </summary>
        private List<Vector2I> GetWalkableBorderTiles(Vector2I fromChunk, Vector2I toChunk)
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

                    // Asegura que el tile del borde sea transitable antes de considerarlo un portal
                    if (!_chunkOccupancyMap.IsOccupied(0,x,y))
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