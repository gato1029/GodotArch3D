using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Procedural.Resources;

using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;

public sealed class BlackyResourcePostRule
{
    public ResourceSourceType ResourceType { get; }
    public int MinDistanceSameType { get; set; }
    public int Priority { get; set; }

    // opcional para futuro: distancia frente a otros tipos
    public Dictionary<ResourceSourceType, int> MinDistanceToOtherTypes { get; } = new();

    public BlackyResourcePostRule(
        ResourceSourceType resourceType,
        int minDistanceSameType = 0,
        int priority = 0)
    {
        ResourceType = resourceType;
        MinDistanceSameType = minDistanceSameType;
        Priority = priority;
    }

    public void SetMinDistanceTo(ResourceSourceType otherType, int distance)
    {
        MinDistanceToOtherTypes[otherType] = distance;
    }

    public int GetMinDistanceTo(ResourceSourceType otherType)
    {
        if (otherType == ResourceType)
            return MinDistanceSameType;

        if (MinDistanceToOtherTypes.TryGetValue(otherType, out int distance))
            return distance;

        return 0;
    }
}