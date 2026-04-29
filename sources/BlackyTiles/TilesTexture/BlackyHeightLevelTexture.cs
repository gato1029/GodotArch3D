using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public class BlackyHeightLevelTexture
{
    public int Height { get; }

    private readonly IBlackyChunkTilemapTexture[] _layers;
    private readonly int _maxLayers;

    public BlackyHeightLevelTexture(int height, int maxLayers)
    {
        Height = height;
        _maxLayers = maxLayers;

        _layers = new IBlackyChunkTilemapTexture[maxLayers];
    }

    #region Layers

    public bool HasLayer(int layer)
    {
        return (uint)layer < _maxLayers && _layers[layer] != null;
    }

    public IBlackyChunkTilemapTexture GetLayer(int layer)
    {
        return _layers[layer];
    }

    public bool TryGetLayer(int layer, out IBlackyChunkTilemapTexture tilemap)
    {
        if ((uint)layer < _maxLayers)
        {
            tilemap = _layers[layer];
            return tilemap != null;
        }

        tilemap = null;
        return false;
    }

    public IBlackyChunkTilemapTexture GetOrCreateLayer(
        int layer,      
        int size,
        int worldX,
        int worldY)
    {
        var tilemap = _layers[layer];

        if (tilemap == null)
        {
            tilemap = BlackyChunkTilemapTextureFactory.Create(                
                layer,
                size,
                worldX,
                worldY);

            _layers[layer] = tilemap;
        }

        return tilemap;
    }

    public IEnumerable<IBlackyChunkTilemapTexture> GetAllLayers()
    {
        for (int i = 0; i < _maxLayers; i++)
        {
            var l = _layers[i];
            if (l != null)
                yield return l;
        }
    }

    #endregion

    public bool HasDirtyTiles()
    {
        for (int i = 0; i < _maxLayers; i++)
        {
            var l = _layers[i];
            if (l != null && l.HasDirtyTiles)
                return true;
        }

        return false;
    }
}
