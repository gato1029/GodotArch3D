using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public class BlackyChunkTexture
{
    public BlackyRegion ParentRegion { get; }
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
        BlackyRegion parentRegion,
        int size,
        int maxHeights,
        int maxLayers
        )
    {
        Coord = coord;
        _size = size;
        _maxHeights = maxHeights;
        _maxLayers = maxLayers;

        WorldBaseX = coord.X * size;
        WorldBaseY = coord.Y * size;

        _heights = new BlackyHeightLevelTexture[maxHeights];
        ParentRegion = parentRegion;
        parentRegion.RegisterChunk(coord);
    }

    // ==========================================
    // MÉTODO PARA EL RENDERIZADOR
    // ==========================================

    /// <summary>
    /// Devuelve los datos de textura (UVs y dimensiones) para un tile ID específico.
    /// Este ID debe ser el ushort que está guardado en las capas de este chunk.
    /// </summary>
    public Color GetTileUV(ushort tileId)
    {
        // El chunk no sabe qué es el "ID 5", pero su región sí.
        return ParentRegion.GetTileUV(tileId);
    }

    /// <summary>
    /// Devuelve la paleta completa de la región si el renderizador 
    /// necesita procesar múltiples tiles de forma masiva.
    /// </summary>
    public BlackyTilePalette GetPalette()
    {
        return ParentRegion.Palette;
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
        int layer
        )
    {
        var h = GetOrCreateHeight(height);

        return h.GetOrCreateLayer(
            layer,            
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
