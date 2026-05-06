using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;

public class BlackyDualNodeTilemap
{
    private readonly byte[] _nodes;
    private readonly int _sizePlusOne;

    public int SizePlusOne => _sizePlusOne;

    public BlackyDualNodeTilemap(int chunkSize)
    {
        _sizePlusOne = chunkSize + 1;
        _nodes = new byte[_sizePlusOne * _sizePlusOne];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int x, int y)
        => y * _sizePlusOne + x;

    public byte GetNode(int x, int y)
        => _nodes[GetIndex(x, y)];

    public void SetNode(int x, int y, byte value)
        => _nodes[GetIndex(x, y)] = value;
}
public class BlackyHeightLevelTexture
{
    public int Height { get; }
    private readonly BlackyDualNodeTilemap[] _dualLayers;
    private readonly IBlackyChunkTilemapTexture[] _layers;
    private readonly int _maxLayers;

    public BlackyHeightLevelTexture(int height, int maxLayers)
    {
        Height = height;
        _maxLayers = maxLayers;
        _dualLayers = new BlackyDualNodeTilemap[maxLayers];
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

    public BlackyDualNodeTilemap GetOrCreateDualLayer(int layer, int chunkSize)
    {
        var d = _dualLayers[layer];

        if (d == null)
        {
            d = new BlackyDualNodeTilemap(chunkSize);
            _dualLayers[layer] = d;
        }

        return d;
    }

    public bool TryGetDualLayer(int layer, out BlackyDualNodeTilemap dual)
    {
        if ((uint)layer < _maxLayers)
        {
            dual = _dualLayers[layer];
            return dual != null;
        }

        dual = null;
        return false;
    }
}
