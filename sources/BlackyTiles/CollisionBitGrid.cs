using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles;

public sealed class CollisionBitGrid
{
    private readonly ulong[] _data;

    private readonly int _tilesX;
    private readonly int _tilesY;

    private readonly int _originX;
    private readonly int _originY;

    private readonly float _invTileSize;

    public CollisionBitGrid(int width, int height, int tileSize)
    {
        _tilesX = width / tileSize;
        _tilesY = height / tileSize;

        // Centro → permite negativos
        _originX = _tilesX >> 1;
        _originY = _tilesY >> 1;

        int totalTiles = _tilesX * _tilesY;
        _data = new ulong[(totalTiles + 63) >> 6];

        // 🔥 evitar divisiones
        _invTileSize = 1f / tileSize;
    }

    // ---------------------------
    // INDEXING
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ToIndex(int x, int y)
    {
        int tx = x + _originX;
        int ty = y + _originY;
        return ty * _tilesX + tx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Word(int index) => index >> 6;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Bit(int index) => index & 63;

    // ---------------------------
    // BASIC OPS
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y)
    {
        int i = ToIndex(x, y);
        _data[Word(i)] |= 1UL << Bit(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear(int x, int y)
    {
        int i = ToIndex(x, y);
        _data[Word(i)] &= ~(1UL << Bit(i));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOccupied(int x, int y)
    {
        int i = ToIndex(x, y);
        return (_data[Word(i)] & (1UL << Bit(i))) != 0;
    }

    // ---------------------------
    // WORLD → TILE (OPTIMIZADO)
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WorldToTile(float x, float y, out int tx, out int ty)
    {
        // 🔥 más rápido que Floor + división
        tx = (int)MathF.Floor(x * _invTileSize);
        ty = (int)MathF.Floor(y * _invTileSize);
    }

    // ---------------------------
    // SET DESDE WORLD
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWorld(float x, float y)
    {
        WorldToTile(x, y, out int tx, out int ty);
        GD.Print("posTile:"+ tx, ty);
        Set(tx, ty);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWorldRectUltra(float x, float y, float width, float height)
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        float minX = x - halfW;
        float maxX = x + halfW;
        float minY = y - halfH;
        float maxY = y + halfH;

        int tMinX = (int)MathF.Floor(minX * _invTileSize);
        int tMaxX = (int)MathF.Floor(maxX * _invTileSize);
        int tMinY = (int)MathF.Floor(minY * _invTileSize);
        int tMaxY = (int)MathF.Floor(maxY * _invTileSize);

        int sizeX = tMaxX - tMinX;
        int sizeY = tMaxY - tMinY;

        // 🔥 1x1
        if (sizeX == 0 && sizeY == 0)
        {
            Set(tMinX, tMinY);
            return;
        }

        // 🔥 2x1
        if (sizeX == 1 && sizeY == 0)
        {
            Set(tMinX, tMinY);
            Set(tMinX + 1, tMinY);
            return;
        }

        // 🔥 1x2
        if (sizeX == 0 && sizeY == 1)
        {
            Set(tMinX, tMinY);
            Set(tMinX, tMinY + 1);
            return;
        }

        // 🔥 2x2
        if (sizeX == 1 && sizeY == 1)
        {
            Set2x2(tMinX, tMinY);
            return;
        }

        // fallback (casos grandes)
        for (int ty = tMinY; ty <= tMaxY; ty++)
        {
            int rowBase = (ty + _originY) * _tilesX;
            int index = rowBase + (tMinX + _originX);

            for (int tx = tMinX; tx <= tMaxX; tx++, index++)
            {
                _data[index >> 6] |= 1UL << (index & 63);
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Set2x2(int tx, int ty)
    {
        Set(tx, ty);
        Set(tx + 1, ty);
        Set(tx, ty + 1);
        Set(tx + 1, ty + 1);
    }
    // ---------------------------
    // CLEAR ALL
    // ---------------------------

    public void ClearAll()
    {
        Array.Clear(_data, 0, _data.Length);
    }
}
