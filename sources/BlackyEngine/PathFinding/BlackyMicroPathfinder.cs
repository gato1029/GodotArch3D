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

    private readonly BlackyClearanceMap _clearanceMap;

    public BlackyMicroPathfinder(BlackyClearanceMap clearanceMap)
    {
        _clearanceMap = clearanceMap;
    }

    /// <summary>
    /// Calcula una ruta tile por tile entre dos puntos en coordenadas globales o locales de tiles.
    /// </summary>
    /// <param name="startTile">Tile de inicio</param>
    /// <param name="goalTile">Tile de destino</param>
    /// <param name="radiusTiles">
    /// Radio del collider (en tiles) para el que se calcula la ruta. Un tile
    /// es transitable solo si su distancia al obstáculo más cercano es mayor
    /// a este radio (ver BlackyClearanceMap).
    /// </param>
    /// <param name="maxIterations">Límite de seguridad para evitar congelamientos si no hay ruta</param>
    public List<Vector2I> FindTilePath(
        Vector2I startTile,
        Vector2I goalTile,
        int radiusTiles,
        int maxIterations = 2000)
    {
        // Si el destino no tiene holgura suficiente para este radio, no podemos ir ahí
        if (!_clearanceMap.IsWalkableForRadius(goalTile.X, goalTile.Y, radiusTiles)) return null;

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
                if (!_clearanceMap.IsWalkableForRadius(neighborTile.X, neighborTile.Y, radiusTiles)) continue;

                // Validación de esquinas en diagonal: no cortar la esquina de
                // un obstáculo aunque el tile diagonal en sí tenga holgura.
                if (dir.X != 0 && dir.Y != 0)
                {
                    if (!_clearanceMap.IsWalkableForRadius(currentTile.X + dir.X, currentTile.Y, radiusTiles) ||
                        !_clearanceMap.IsWalkableForRadius(currentTile.X, currentTile.Y + dir.Y, radiusTiles))
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