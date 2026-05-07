using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;


public class BlackyChunkTilemapTextureByte : BlackyChunkTilemapTextureBase
{
    private readonly byte[] _tiles;
    // NUEVO
    private readonly byte[] _solid;
    private readonly byte[] _dualMask;
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
    // ==========================================
    // SOLID MAP
    // ==========================================

    public override void SetSolid(
        int x,
        int y,
        bool value)
    {
        int i = GetIndex(x, y);

        _solid[i] = (byte)(value ? 1 : 0);
    }

    public override bool IsSolid(int x, int y)
    {
        return _solid[GetIndex(x, y)] != 0;
    }

    // ==========================================
    // DUAL MASK
    // ==========================================

    public override void SetDualMask(
        int x,
        int y,
        byte mask)
    {
        _dualMask[GetIndex(x, y)] = mask;
    }

    public override byte GetDualMask(int x, int y)
    {
        return _dualMask[GetIndex(x, y)];
    }
}