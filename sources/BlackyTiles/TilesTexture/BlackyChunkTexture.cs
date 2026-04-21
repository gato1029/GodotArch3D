using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public class BlackyChunkTexture
{
    public BlackyChunkCoord Coord { get; }

    private readonly BlackyHeightLevelTexture[] _heights;
    private readonly int _maxHeights;
    private readonly int _maxLayers;
    private readonly int _size;

    public int WorldBaseX { get; }
    public int WorldBaseY { get; }

    public bool IsDirty { get; private set; }
    public bool IsSaved { get; private set; }

    public BlackyChunkTexture(
        BlackyChunkCoord coord,
        int size,
        int maxHeights,
        int maxLayers)
    {
        Coord = coord;
        _size = size;
        _maxHeights = maxHeights;
        _maxLayers = maxLayers;

        WorldBaseX = coord.X * size;
        WorldBaseY = coord.Y * size;

        _heights = new BlackyHeightLevelTexture[maxHeights];
    }

    #region Heights

    public bool HasHeight(int height)
    {
        return (uint)height < _maxHeights && _heights[height] != null;
    }

    public BlackyHeightLevelTexture GetHeight(int height)
    {
        return _heights[height];
    }

    public bool TryGetHeight(int height, out BlackyHeightLevelTexture level)
    {
        if ((uint)height < _maxHeights)
        {
            level = _heights[height];
            return level != null;
        }

        level = null;
        return false;
    }

    public BlackyHeightLevelTexture GetOrCreateHeight(int height)
    {
        var h = _heights[height];

        if (h == null)
        {
            h = new BlackyHeightLevelTexture(height, _maxLayers);
            _heights[height] = h;
        }

        return h;
    }

    public IEnumerable<BlackyHeightLevelTexture> GetHeights()
    {
        for (int i = 0; i < _maxHeights; i++)
        {
            var h = _heights[i];
            if (h != null)
                yield return h;
        }
    }

    #endregion

    #region Layers helper

    public IBlackyChunkTilemapTexture GetOrCreateLayer(
        int height,
        int layer,
        TilePalette palette)
    {
        var h = GetOrCreateHeight(height);

        return h.GetOrCreateLayer(
            layer,
            palette,
            _size,
            WorldBaseX,
            WorldBaseY);
    }

    #endregion

    #region State

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
        for (int i = 0; i < _maxHeights; i++)
        {
            var h = _heights[i];
            if (h != null && h.HasDirtyTiles())
                return true;
        }

        return false;
    }

    #endregion
}
