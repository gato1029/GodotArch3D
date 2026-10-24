using Godot;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

public class FlowField
{
    public const float Unreachable = float.MaxValue;
    public const byte NoDirection = 255;

    private static readonly Vector2I[] Dirs =
    {
        new(1,0), new(-1,0), new(0,1), new(0,-1),
        new(1,1), new(1,-1), new(-1,1), new(-1,-1)
    };

    private static readonly Vector2[] DirVectors;

    static FlowField()
    {
        DirVectors = new Vector2[Dirs.Length];
        for (int i = 0; i < Dirs.Length; i++)
            DirVectors[i] = new Vector2(Dirs[i].X, Dirs[i].Y).Normalized();
    }

    public readonly Vector2I RegionMin;
    public readonly Vector2I RegionMax;
    public readonly Vector2I GoalTile;

    private readonly int _width;
    private readonly int _height;
    private readonly float[] _distance;
    private readonly byte[] _directionIndex;

    private FlowField(Vector2I regionMin, Vector2I regionMax, Vector2I goalTile)
    {
        RegionMin = regionMin;
        RegionMax = regionMax;
        GoalTile = goalTile;

        _width = regionMax.X - regionMin.X + 1;
        _height = regionMax.Y - regionMin.Y + 1;

        _distance = new float[_width * _height];
        _directionIndex = new byte[_width * _height];

        Array.Fill(_distance, Unreachable);
        Array.Fill(_directionIndex, NoDirection);
    }

    public static FlowField Build(
      BlackyClearanceMap clearanceMap,
      Vector2I goalTile,
      Vector2I regionMin,
      Vector2I regionMax,
      float radiusTiles,
      int goalSpreadRadius = 0) // 0 = un solo tile semilla (comportamiento original)
    {
        if (!clearanceMap.IsWalkableForRadius(goalTile.X, goalTile.Y, radiusTiles))
            return null;

        var field = new FlowField(regionMin, regionMax, goalTile);
        var frontier = new PriorityQueue<Vector2I, float>();

        bool anySeed = false;

        // Sembramos el Dijkstra desde un disco alrededor del destino,
        // no solo un tile — así el campo converge hacia una zona en vez
        // de un punto exacto, repartiendo naturalmente a las unidades.
        for (int dx = -goalSpreadRadius; dx <= goalSpreadRadius; dx++)
        {
            for (int dy = -goalSpreadRadius; dy <= goalSpreadRadius; dy++)
            {
                var seedTile = goalTile + new Vector2I(dx, dy);

                if (!field.InRegion(seedTile)) continue;
                if (!clearanceMap.IsWalkableForRadius(seedTile.X, seedTile.Y, radiusTiles)) continue;

                field.SetDistance(seedTile, 0f);
                frontier.Enqueue(seedTile, 0f);
                anySeed = true;
            }
        }

        if (!anySeed)
            return null; // ni el propio goalTile ni nada de su disco es transitable

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            float currentDist = field.GetDistance(current);

            foreach (var dir in Dirs)
            {
                Vector2I neighbor = current + dir;

                if (!field.InRegion(neighbor)) continue;
                if (!clearanceMap.IsWalkableForRadius(neighbor.X, neighbor.Y, radiusTiles)) continue;

                bool isDiagonal = dir.X != 0 && dir.Y != 0;
                float stepCost = isDiagonal ? 1.41421356f : 1.0f;
                float newDist = currentDist + stepCost;
                if (isDiagonal)
                {
                    if (!clearanceMap.IsWalkableForRadius(current.X + dir.X, current.Y, radiusTiles) ||
                        !clearanceMap.IsWalkableForRadius(current.X, current.Y + dir.Y, radiusTiles))
                        continue;
                }
                if (newDist < field.GetDistance(neighbor))
                {
                    field.SetDistance(neighbor, newDist);
                    frontier.Enqueue(neighbor, newDist);
                }
            }
        }

        field.ComputeDirections(clearanceMap, radiusTiles);
        return field;
    }
    private void ComputeDirections(BlackyClearanceMap clearanceMap, float radiusTiles)
    {
        for (int x = RegionMin.X; x <= RegionMax.X; x++)
        {
            for (int y = RegionMin.Y; y <= RegionMax.Y; y++)
            {
                var tile = new Vector2I(x, y);
                float myDist = GetDistance(tile);
                if (myDist >= Unreachable) continue;

                // Zona de meta: empujar hacia el tile objetivo para que la horda
                // se compacte hacia adentro en vez de detenerse en el borde.
                if (myDist == 0f)
                {
                    Vector2 toGoal = new Vector2(GoalTile.X - x, GoalTile.Y - y);
                    if (toGoal.LengthSquared() > 0.01f)
                        SetDirection(tile, NearestDirIndex(toGoal));
                    continue;   // el propio GoalTile queda sin dirección
                }

                float bestDist = myDist;
                byte bestDir = NoDirection;

                for (byte i = 0; i < Dirs.Length; i++)
                {
                    if (i >= 4)
                    {
                        var d = Dirs[i];
                        if (GetDistance(tile + new Vector2I(d.X, 0)) >= Unreachable ||
                            GetDistance(tile + new Vector2I(0, d.Y)) >= Unreachable)
                            continue;
                    }

                    Vector2I neighbor = tile + Dirs[i];
                    if (!InRegion(neighbor)) continue;

                    float neighborDist = GetDistance(neighbor);
                    if (neighborDist < bestDist)
                    {
                        bestDist = neighborDist;
                        bestDir = i;
                    }
                }

                SetDirection(tile, bestDir);
            }
        }
    }
  
    public bool IsGoalTile(Vector2I tile) => tile == GoalTile;
    public Vector2 SampleTile(Vector2I tile)
    {
        if (!InRegion(tile)) return Vector2.Zero;
        byte dirIdx = GetDirectionAt(tile);
        return dirIdx == NoDirection ? Vector2.Zero : DirVectors[dirIdx];
    }
    private static byte NearestDirIndex(Vector2 v)
    {
        v = v.Normalized();
        byte best = 0;
        float bestDot = float.MinValue;

        for (byte i = 0; i < DirVectors.Length; i++)
        {
            float d = DirVectors[i].Dot(v);
            if (d > bestDot)
            {
                bestDot = d;
                best = i;
            }
        }

        return best;
    }
    public Vector2 SampleInterpolated(Vector2 worldPos)
    {
        Vector2I baseTile = TilesHelper.WorldPositionToTile(worldPos);
        Vector2 tileOrigin = TilesHelper.TilePositionToWorldPosition(baseTile);
        Vector2 frac = worldPos - tileOrigin;

        Vector2 d00 = SampleTile(baseTile);
        Vector2 d10 = SampleTile(baseTile + new Vector2I(1, 0));
        Vector2 d01 = SampleTile(baseTile + new Vector2I(0, 1));
        Vector2 d11 = SampleTile(baseTile + new Vector2I(1, 1));

        Vector2 top = d00.Lerp(d10, frac.X);
        Vector2 bottom = d01.Lerp(d11, frac.X);
        Vector2 blended = top.Lerp(bottom, frac.Y);

        return blended.LengthSquared() > 0.0001f ? blended.Normalized() : Vector2.Zero;
    }
    public Vector2 SampleEscape(Vector2I tile, int maxRadius = 3)
    {
        for (int r = 1; r <= maxRadius; r++)
        {
            float bestDist = Unreachable;
            Vector2I best = tile;
            bool found = false;

            for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != r) continue; // solo el anillo
                    var n = tile + new Vector2I(dx, dy);
                    float d = GetDistance(n);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = n;
                        found = true;
                    }
                }

            if (found)
            {
                var toBest = new Vector2(best.X - tile.X, best.Y - tile.Y);
                return toBest.Normalized();
            }
        }
        return Vector2.Zero;
    }
    public bool IsGoalReached(Vector2I tile) => tile == GoalTile;

    private bool InRegion(Vector2I tile)
        => tile.X >= RegionMin.X && tile.X <= RegionMax.X &&
           tile.Y >= RegionMin.Y && tile.Y <= RegionMax.Y;

    private int Index(Vector2I tile)
        => (tile.Y - RegionMin.Y) * _width + (tile.X - RegionMin.X);

    private float GetDistance(Vector2I tile)
        => InRegion(tile) ? _distance[Index(tile)] : Unreachable;

    private void SetDistance(Vector2I tile, float value)
        => _distance[Index(tile)] = value;

    private byte GetDirectionAt(Vector2I tile)
        => InRegion(tile) ? _directionIndex[Index(tile)] : NoDirection;

    private void SetDirection(Vector2I tile, byte value)
        => _directionIndex[Index(tile)] = value;
}