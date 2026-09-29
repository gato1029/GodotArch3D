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
        float radiusTiles)
    {
        if (!clearanceMap.IsWalkableForRadius(goalTile.X, goalTile.Y, radiusTiles))
            return null;

        var field = new FlowField(regionMin, regionMax, goalTile);

        var frontier = new PriorityQueue<Vector2I, float>();
        frontier.Enqueue(goalTile, 0f);
        field.SetDistance(goalTile, 0f);

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

                float bestDist = myDist;
                byte bestDir = NoDirection;

                for (byte i = 0; i < Dirs.Length; i++)
                {
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

    public Vector2 SampleTile(Vector2I tile)
    {
        if (!InRegion(tile)) return Vector2.Zero;
        byte dirIdx = GetDirectionAt(tile);
        return dirIdx == NoDirection ? Vector2.Zero : DirVectors[dirIdx];
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