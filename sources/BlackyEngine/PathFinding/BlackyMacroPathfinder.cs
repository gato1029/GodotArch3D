using Godot;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

public class BlackyMacroPathfinder
{
    private  BlackyMacroGraphCache macroGraphCache = null;
    public BlackyMacroPathfinder(BlackyMacroGraphCache blackyMacroGraphCache)
    {
        macroGraphCache = blackyMacroGraphCache;
    }

    public List<Vector2I> FindChunkPath(
        Vector2I startChunk,
        Vector2I goalChunk)
    {
        if (!macroGraphCache.TryGetNode(goalChunk, out var goalInfo) || !goalInfo.IsWalkable)
            return null;

        PriorityQueue<Vector2I, float> openQueue = new();
        Dictionary<Vector2I, Vector2I> cameFrom = new();
        Dictionary<Vector2I, float> gScore = new();
        HashSet<Vector2I> closedSet = new();

        openQueue.Enqueue(startChunk, 0f);
        gScore[startChunk] = 0f;

        while (openQueue.Count > 0)
        {
            var currentChunk = openQueue.Dequeue();

            if (currentChunk == goalChunk)
            {
                return ReconstructPath(cameFrom, currentChunk);
            }

            closedSet.Add(currentChunk);

            var neighbors = macroGraphCache.GetCachedNeighbors(currentChunk);
            if (neighbors == null) continue;

            foreach (var neighborChunk in neighbors)
            {
                if (closedSet.Contains(neighborChunk)) continue;

                if (!macroGraphCache.TryGetNode(neighborChunk, out var neighborInfo) || !neighborInfo.IsWalkable)
                    continue;

                // 1. Costo base calculado al vuelo (Recto = 1.0, Diagonal = 1.414)
                float baseCost = (currentChunk.X != neighborChunk.X && currentChunk.Y != neighborChunk.Y) ? 1.414f : 1.0f;
            
                float tentativeG = gScore[currentChunk] + baseCost ;

                if (!gScore.ContainsKey(neighborChunk) || tentativeG < gScore[neighborChunk])
                {
                    cameFrom[neighborChunk] = currentChunk;
                    gScore[neighborChunk] = tentativeG;

                    float h = neighborChunk.DistanceTo(goalChunk);
                    openQueue.Enqueue(neighborChunk, tentativeG + h);
                }
            }
        }
        return null; // No hay ruta macro posible
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
