using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;


public class BlackyChunkTilemapTextureByte : BlackyChunkTilemapTextureBase
{
    private readonly byte[] _tiles;

    public BlackyChunkTilemapTextureByte(int layer, int size, int wx, int wy)
        : base(layer, size, wx, wy)
    {
        _tiles = new byte[size * size];
    }

    public override void SetTile(int x, int y, int tileId, bool isDirty = true)
    {
        int i = GetIndex(x, y);
        _tiles[i] = (byte)tileId;

        if (isDirty) MarkDirty(x, y);
    }

    protected override void SetTileUnsafe(int index, int tileId)
    {
        _tiles[index] = (byte)tileId;
    }

    public override int GetTile(int x, int y)
        => _tiles[GetIndex(x, y)];

    public override bool IsEmpty(int x, int y)
        => _tiles[GetIndex(x, y)] == 0;

    public override void ClearTile(int x, int y)
    {
        int i = GetIndex(x, y);
        _tiles[i] = 0;
        MarkDirty(x, y);
    }
}