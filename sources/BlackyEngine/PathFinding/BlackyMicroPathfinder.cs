using Godot;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

public class BlackyMicroPathfinder
{
    private static readonly Vector2I[] TileDirections = new[]
    {
        new Vector2I(0, 1),   new Vector2I(0, -1),
        new Vector2I(1, 0),   new Vector2I(-1, 0),
        // Diagonales opcionales (puedes quitarlas si tu juego es estrictamente ortogonal de 4 direcciones)
        new Vector2I(1, 1),   new Vector2I(-1, 1),
        new Vector2I(1, -1),  new Vector2I(-1, -1)
    };
    private readonly BlackyChunkOccupancyMap blackyChunkOccupancy; 
    public BlackyMicroPathfinder(BlackyChunkOccupancyMap blackyChunkOccupancyMap)
    {
        blackyChunkOccupancy = blackyChunkOccupancyMap;
    }

    /// <summary>
    /// Calcula una ruta tile por tile entre dos puntos en coordenadas globales o locales de tiles.
    /// </summary>
    /// <param name="startTile">Tile de inicio</param>
    /// <param name="goalTile">Tile de destino</param>
    /// <param name="maxIterations">Límite de seguridad para evitar congelamientos si no hay ruta</param>
    public List<Vector2I> FindTilePath(
             Vector2I startTile,
             Vector2I goalTile,
             int maxIterations = 2000) // Aumentado para evitar falsos nulos
    {
        // Corregido: Si el destino está ocupado, no podemos ir ahí
        if (blackyChunkOccupancy.IsOccupied(0, goalTile.X, goalTile.Y)) return null;

        PriorityQueue<Vector2I, float> openQueue = new();
        Dictionary<Vector2I, Vector2I> cameFrom = new();
        Dictionary<Vector2I, float> gScore = new();
        HashSet<Vector2I> closedSet = new();

        openQueue.Enqueue(startTile, 0f);
        gScore[startTile] = 0f;

        int iterations = 0;

        while (openQueue.Count > 0)
        {
            if (++iterations > maxIterations) return null;

            var currentTile = openQueue.Dequeue();

            if (currentTile == goalTile)
            {
                return ReconstructPath(cameFrom, currentTile);
            }

            closedSet.Add(currentTile);

            foreach (var dir in TileDirections)
            {
                Vector2I neighborTile = currentTile + dir;

                if (closedSet.Contains(neighborTile)) continue;
                if (blackyChunkOccupancy.IsOccupied(0, neighborTile.X, neighborTile.Y)) continue;

                // Validación de esquinas en diagonal (corregido el doble ++)
                if (dir.X != 0 && dir.Y != 0)
                {
                    if (blackyChunkOccupancy.IsOccupied(0, currentTile.X + dir.X, currentTile.Y) ||
                        blackyChunkOccupancy.IsOccupied(0, currentTile.X, currentTile.Y + dir.Y))
                    {
                        continue;
                    }
                }

                float baseCost = (dir.X != 0 && dir.Y != 0) ? 1.414f : 1.0f;
                float tentativeG = gScore[currentTile] + baseCost;

                if (!gScore.ContainsKey(neighborTile) || tentativeG < gScore[neighborTile])
                {
                    cameFrom[neighborTile] = currentTile;
                    gScore[neighborTile] = tentativeG;

                    float h = neighborTile.DistanceTo(goalTile);
                    openQueue.Enqueue(neighborTile, tentativeG + h);
                }
            }
        }

        return null;
    }

    private static List<Vector2I> ReconstructPath(Dictionary<Vector2I, Vector2I> cameFrom, Vector2I current)
    {
        List<Vector2I> path = new() { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }
        return path;
    }
}
