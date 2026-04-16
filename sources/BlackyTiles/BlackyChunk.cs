using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles;

public class BlackyHeightLevel
{
    public int Height { get; }

    public readonly BlackyChunkLayer[] Layers;

    public BlackyHeightLevel(
        int height,
        int size,
        int worldX,
        int worldY)
    {
        Height = height;

        Layers = new BlackyChunkLayer[BlackyChunk.MaxLayers];

        for (int l = 0; l < BlackyChunk.MaxLayers; l++)
        {
            Layers[l] = new BlackyChunkLayer(
                l,
                size,
                worldX,
                worldY);
        }
    }

    public bool HasDirtyTiles()
    {
        for (int l = 0; l < Layers.Length; l++)
        {
            if (Layers[l].HasDirtyTiles)
                return true;
        }

        return false;
    }
}

public class BlackyChunk
{
    public BlackyChunkCoord Coord { get; }

    private readonly BlackyHeightLevel[] _heights;

    private readonly int _size;

    public int MaxHeights = 3;
    public const int MaxLayers = 4;

    public bool IsDirty { get; private set; }
    public bool IsSaved { get; private set; }

    public int WorldBaseX { get; }
    public int WorldBaseY { get; }

    public BlackyChunk(BlackyChunkCoord coord, int size, int height)
    {
        Coord = coord;
        _size = size;
        MaxHeights = height;

        WorldBaseX = coord.X * size;
        WorldBaseY = coord.Y * size;

        _heights = new BlackyHeightLevel[MaxHeights];

        for (int h = 0; h < MaxHeights; h++)
        {
            _heights[h] = new BlackyHeightLevel(       
                h,
                size,
                WorldBaseX,
                WorldBaseY);
        }
    }
    public bool TryGetLayer(int height, int layer, out BlackyChunkLayer chunkLayer)
    {
        chunkLayer = null;

        if ((uint)height >= MaxHeights)
            return false;

        if ((uint)layer >= MaxLayers)
            return false;

        chunkLayer = _heights[height].Layers[layer];
        return true;
    }
    public bool TryGetHeight(int height, out BlackyHeightLevel level)
    {
        level = null;

        if ((uint)height >= MaxHeights)
            return false;

        level = _heights[height];
        return true;
    }
    public BlackyChunkLayer GetLayer(int height, int layer)
    {
        return _heights[height].Layers[layer];
    }

    public BlackyHeightLevel GetHeight(int height)
    {
        return _heights[height];
    }

    public IEnumerable<BlackyHeightLevel> GetHeights()
    {
        for (int i = 0; i < MaxHeights; i++)
            yield return _heights[i];
    }

    public void MarkDirty()
    {
        IsDirty = true;
        IsSaved = false;
    }

    public void MarkSaved()
    {
        IsDirty = false;
        IsSaved = true;
    }

    public bool HasDirtyTiles()
    {
        for (int h = 0; h < MaxHeights; h++)
        {
            if (_heights[h].HasDirtyTiles())
                return true;
        }

        return false;
    }
}