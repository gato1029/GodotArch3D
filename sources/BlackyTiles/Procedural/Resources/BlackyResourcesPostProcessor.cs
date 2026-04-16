using Godot;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.BlackyTiles.Procedural.Resources;



public sealed class BlackyResourcesPostProcessor
{
    private readonly Dictionary<ResourceSourceType, BlackyResourcePostRule> rules = new();
    private readonly int seed;

    public BlackyResourcesPostProcessor(int seed = 12345)
    {
        this.seed = seed;
    }

    public int MaxRequiredDistance
    {
        get
        {
            int max = 0;

            foreach (var pair in rules)
            {
                var rule = pair.Value;

                if (rule.MinDistanceSameType > max)
                    max = rule.MinDistanceSameType;

                foreach (var other in rule.MinDistanceToOtherTypes)
                {
                    if (other.Value > max)
                        max = other.Value;
                }
            }

            return max;
        }
    }

    public void AddRule(BlackyResourcePostRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        rules[rule.ResourceType] = rule;
    }

    public bool HasRule(ResourceSourceType resourceType)
    {
        return rules.ContainsKey(resourceType);
    }

    public bool TryGetRule(ResourceSourceType resourceType, out BlackyResourcePostRule rule)
    {
        return rules.TryGetValue(resourceType, out rule);
    }

    public void ClearRules()
    {
        rules.Clear();
    }

    public void Process(List<BlackyResourceGenerationData> resources)
    {
        if (resources == null)
            throw new ArgumentNullException(nameof(resources));

        if (resources.Count <= 1)
            return;

        resources.Sort(CompareForOrdering);

        List<BlackyResourceGenerationData> accepted = new(resources.Count);

        int maxDistance = MaxRequiredDistance;
        int bucketSize = Math.Max(1, maxDistance + 1);

        Dictionary<Vector2I, List<BlackyResourceGenerationData>> spatialBuckets = new();

        for (int i = 0; i < resources.Count; i++)
        {
            var candidate = resources[i];

            if (ConflictsWithAccepted(candidate, spatialBuckets, bucketSize))
                continue;

            accepted.Add(candidate);
            AddToBucket(candidate, spatialBuckets, bucketSize);
        }

        resources.Clear();
        resources.AddRange(accepted);
    }

    private bool ConflictsWithAccepted(
        BlackyResourceGenerationData candidate,
        Dictionary<Vector2I, List<BlackyResourceGenerationData>> spatialBuckets,
        int bucketSize)
    {
        var candidateBucket = WorldToBucket(candidate.PositionTileWorld, bucketSize);

        for (int by = candidateBucket.Y - 1; by <= candidateBucket.Y + 1; by++)
        {
            for (int bx = candidateBucket.X - 1; bx <= candidateBucket.X + 1; bx++)
            {
                var bucketCoord = new Vector2I(bx, by);

                if (!spatialBuckets.TryGetValue(bucketCoord, out var bucket))
                    continue;

                for (int i = 0; i < bucket.Count; i++)
                {
                    var existing = bucket[i];

                    int requiredDistance = GetRequiredDistance(candidate.ResourceType, existing.ResourceType);
                    if (requiredDistance <= 0)
                        continue;

                    if (AreTooClose(candidate.PositionTileWorld, existing.PositionTileWorld, requiredDistance))
                        return true;
                }
            }
        }

        return false;
    }

    private void AddToBucket(
        BlackyResourceGenerationData data,
        Dictionary<Vector2I, List<BlackyResourceGenerationData>> spatialBuckets,
        int bucketSize)
    {
        var bucketCoord = WorldToBucket(data.PositionTileWorld, bucketSize);

        if (!spatialBuckets.TryGetValue(bucketCoord, out var bucket))
        {
            bucket = new List<BlackyResourceGenerationData>();
            spatialBuckets.Add(bucketCoord, bucket);
        }

        bucket.Add(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2I WorldToBucket(Vector2I worldPos, int bucketSize)
    {
        return new Vector2I(
            FloorDiv(worldPos.X, bucketSize),
            FloorDiv(worldPos.Y, bucketSize)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int FloorDiv(int value, int divisor)
    {
        int quotient = value / divisor;
        int remainder = value % divisor;

        if (remainder != 0 && ((remainder < 0) != (divisor < 0)))
            quotient--;

        return quotient;
    }

    private int CompareForOrdering(BlackyResourceGenerationData a, BlackyResourceGenerationData b)
    {
        int priorityA = GetPriority(a.ResourceType);
        int priorityB = GetPriority(b.ResourceType);

        if (priorityA != priorityB)
            return priorityB.CompareTo(priorityA);

        int scoreA = GetStableScore(a);
        int scoreB = GetStableScore(b);

        if (scoreA != scoreB)
            return scoreB.CompareTo(scoreA);

        if (a.PositionTileWorld.X != b.PositionTileWorld.X)
            return a.PositionTileWorld.X.CompareTo(b.PositionTileWorld.X);

        if (a.PositionTileWorld.Y != b.PositionTileWorld.Y)
            return a.PositionTileWorld.Y.CompareTo(b.PositionTileWorld.Y);

        if (a.ResourceId != b.ResourceId)
            return a.ResourceId.CompareTo(b.ResourceId);

        return ((int)a.ResourceType).CompareTo((int)b.ResourceType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetPriority(ResourceSourceType resourceType)
    {
        if (rules.TryGetValue(resourceType, out var rule))
            return rule.Priority;

        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetRequiredDistance(ResourceSourceType a, ResourceSourceType b)
    {
        int distanceAB = 0;
        int distanceBA = 0;

        if (rules.TryGetValue(a, out var ruleA))
            distanceAB = ruleA.GetMinDistanceTo(b);

        if (rules.TryGetValue(b, out var ruleB))
            distanceBA = ruleB.GetMinDistanceTo(a);

        return Math.Max(distanceAB, distanceBA);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AreTooClose(Vector2I a, Vector2I b, int minDistance)
    {
        int dx = Math.Abs(a.X - b.X);
        int dy = Math.Abs(a.Y - b.Y);

        return Math.Max(dx, dy) <= minDistance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetStableScore(BlackyResourceGenerationData data)
    {
        unchecked
        {
            int hash = seed;
            hash = (hash * 397) ^ data.PositionTileWorld.X;
            hash = (hash * 397) ^ data.PositionTileWorld.Y;
            hash = (hash * 397) ^ data.ResourceId;
            hash = (hash * 397) ^ (int)data.ResourceType;

            hash ^= (hash >> 13);
            hash *= 1274126177;
            hash ^= (hash >> 16);

            return hash;
        }
    }
}