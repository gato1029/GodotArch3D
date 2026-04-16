using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Procedural.Resources;

public sealed class BlackyHeightResourceTable
{
    public int Height { get; }
    public ushort BiomeId { get; }

    public float DensityThreshold { get; set; } = 0f;

    public List<ResourceEntry> Entries { get; } = new();

    private readonly Dictionary<int, int> minDistanceToHeight = new();
    private readonly Dictionary<ResourceSourceType, float> typeWeights = new();
    private readonly Dictionary<ResourceSourceType, List<ResourceEntry>> entriesByType = new();

    public BlackyHeightResourceTable(int height, ushort biomeId)
    {
        Height = height;
        BiomeId = biomeId;
    }
    public void SetTypeWeight(ResourceSourceType type, float weight)
    {
        typeWeights[type] = weight;
    }

    public float GetTypeWeight(ResourceSourceType type)
    {
        if (typeWeights.TryGetValue(type, out var w))
            return w;

        return 0f; // default
    }
    public void AddEntry(ResourceEntry entry)
    {
        if (entry == null)
            throw new ArgumentNullException(nameof(entry));

        if (entry.Probability <= 0f)
            return;

        // lista global (compatibilidad)
        Entries.Add(entry);

        // 🔥 agrupado por tipo
        if (!entriesByType.TryGetValue(entry.ResourceType, out var list))
        {
            list = new List<ResourceEntry>();
            entriesByType.Add(entry.ResourceType, list);
        }

        list.Add(entry);
    }
    public bool TryGetEntriesByType(ResourceSourceType type, out List<ResourceEntry> list)
    {
        return entriesByType.TryGetValue(type, out list);
    }
    public IEnumerable<ResourceSourceType> GetAvailableTypes()
    {
        return entriesByType.Keys;
    }
    public IEnumerable<ResourceSourceType> GetAvailableTypesWeights()
    {
        return typeWeights.Keys;
    }
    // 🔥 CONFIG
    public void SetMinDistanceToHeight(int neighborHeight, int distance)
    {
        minDistanceToHeight[neighborHeight] = distance;
    }

    public int GetMinDistanceToHeight(int neighborHeight)
    {
        if (minDistanceToHeight.TryGetValue(neighborHeight, out int dist))
            return dist;

        return 0; // default: sin restricción
    }
}
