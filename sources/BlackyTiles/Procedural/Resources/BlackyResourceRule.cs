using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Procedural.Resources;

public sealed class BlackyResourceRule
{
    public ResourceSourceType ResourceType { get; }
    public int MinHeight { get; }
    public int MaxHeight { get; }
    public float Probability { get; }
    public ushort[] ResourceIds { get; }

    public BlackyResourceRule(
        ResourceSourceType resourceType,
        int minHeight,
        int maxHeight,
        float probability,
        params ushort[] resourceIds)
    {
        if (resourceIds == null || resourceIds.Length == 0)
            throw new ArgumentException("ResourceIds cannot be null or empty.", nameof(resourceIds));

        ResourceType = resourceType;
        MinHeight = minHeight;
        MaxHeight = maxHeight;
        Probability = probability;
        ResourceIds = resourceIds;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MatchesHeight(int height)
    {
        return height >= MinHeight && height <= MaxHeight;
    }
}
