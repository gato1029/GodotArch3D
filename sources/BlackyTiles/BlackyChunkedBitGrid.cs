using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using Godot;


namespace GodotEcsArch.sources.BlackyTiles;


public  class BlackyChunkedBitGrid
{

    private const int CHUNK_SIZE = 32;
    private const int CHUNK_MASK = CHUNK_SIZE - 1;
    private const int CHUNK_AREA = CHUNK_SIZE * CHUNK_SIZE;
    private const int CHUNK_WORDS = CHUNK_AREA / 64; // 16

    public int MinTileX => -_originTileX;
    public int MaxTileX => (_chunksX * CHUNK_SIZE) - _originTileX - 1;

    public int MinTileY => -_originTileY;
    public int MaxTileY => (_chunksY * CHUNK_SIZE) - _originTileY - 1;

    private readonly ulong[][] _chunks;   // datos reales
    private readonly ulong[] _chunkMask; // bitmask global

    private readonly int _chunksX;
    private readonly int _chunksY;

    private readonly int _originChunkX;
    private readonly int _originChunkY;

    private readonly int _originTileX;
    private readonly int _originTileY;

    private readonly float _invTileSize;

    // ---------------------------
    // CONSTRUCTOR
    // ---------------------------

    public BlackyChunkedBitGrid(int width, int height, int tileSize)
    {
        int tilesX = width / tileSize;
        int tilesY = height / tileSize;

        _originTileX = tilesX >> 1;
        _originTileY = tilesY >> 1;

        _chunksX = (tilesX + CHUNK_SIZE - 1) / CHUNK_SIZE;
        _chunksY = (tilesY + CHUNK_SIZE - 1) / CHUNK_SIZE;

        _originChunkX = _chunksX >> 1;
        _originChunkY = _chunksY >> 1;

        int totalChunks = _chunksX * _chunksY;

        _chunks = new ulong[totalChunks][];
        _chunkMask = new ulong[(totalChunks + 63) >> 6];

        float tileSizeUnits = MeshCreator.PixelsToUnits(16); // 16 px → units
        _invTileSize =1f/ tileSizeUnits;
    }

    // ---------------------------
    // CHUNK INDEX
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ChunkIndex(int cx, int cy)
    {
        return cy * _chunksX + cx;
    }

    // ---------------------------
    // BITMASK OPS
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsChunkActive(int index)
    {
        return (_chunkMask[index >> 6] & (1UL << (index & 63))) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ActivateChunk(int index)
    {
        _chunkMask[index >> 6] |= 1UL << (index & 63);
    }

    // ---------------------------
    // GET OR CREATE CHUNK
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong[] GetOrCreateChunk(int index)
    {
        if (!IsChunkActive(index))
        {
            _chunks[index] = new ulong[CHUNK_WORDS];
            ActivateChunk(index);
        }

        return _chunks[index];
    }

    public void SetWorldFast(float x, float y)
    {
        int tx = (int)(x * _invTileSize);
        int ty = (int)(y * _invTileSize);

        tx += _originTileX;
        ty += _originTileY;

        int cx = tx >> 5;
        int cy = ty >> 5;

        if ((uint)cx >= _chunksX || (uint)cy >= _chunksY)
            return;

        int ci = cy * _chunksX + cx;

        ulong[] chunk = _chunks[ci];

        if (chunk == null)
        {
            chunk = new ulong[CHUNK_WORDS];
            _chunks[ci] = chunk;
            _chunkMask[ci >> 6] |= 1UL << (ci & 63);
        }

        int index = ((ty & 31) << 5) | (tx & 31);
        chunk[index >> 6] |= 1UL << (index & 63);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWorld(float x, float y)
    {
        int tx = (int)MathF.Floor(x * _invTileSize);
        int ty = (int)MathF.Floor(y * _invTileSize);

        Set(tx, ty);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWorldRect(float x, float y, float width, float height)
    {       
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        float minX = x - halfW;
        float maxX = x + halfW;
        float minY = y - halfH;
        float maxY = y + halfH;

        int tMinX = (int)MathF.Floor(minX * _invTileSize);
        int tMaxX = (int)MathF.Floor((maxX - 0.0001f) * _invTileSize);
        int tMinY = (int)MathF.Floor(minY * _invTileSize);
        int tMaxY = (int)MathF.Floor((maxY - 0.0001f) * _invTileSize);

        int sizeX = tMaxX - tMinX;
        int sizeY = tMaxY - tMinY;

        // ---------------------------
        // 🔥 FAST PATH (1 tile)
        // ---------------------------
        if (sizeX == 0 && sizeY == 0)
        {
            Set(tMinX, tMinY);
            return;
        }

        // ---------------------------
        // 🔥 CASOS PEQUEÑOS (SIN LOOPS)
        // ---------------------------
        if (sizeX == 1 && sizeY == 0)
        {
            Set(tMinX, tMinY);
            Set(tMinX + 1, tMinY);
            return;
        }

        if (sizeX == 0 && sizeY == 1)
        {
            Set(tMinX, tMinY);
            Set(tMinX, tMinY + 1);
            return;
        }

        if (sizeX == 1 && sizeY == 1)
        {
            Set(tMinX, tMinY);
            Set(tMinX + 1, tMinY);
            Set(tMinX, tMinY + 1);
            Set(tMinX + 1, tMinY + 1);
            return;
        }

        // ---------------------------
        // 🔥 FALLBACK GENERAL
        // ---------------------------
        for (int ty = tMinY; ty <= tMaxY; ty++)
        {
            int worldY = ty;

            for (int tx = tMinX; tx <= tMaxX; tx++)
            {
                Set(tx, worldY);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWorldRectFast(float x, float y, float width, float height)
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        int tMinX = (int)MathF.Floor((x - halfW) * _invTileSize);
        int tMaxX = (int)MathF.Floor((x + halfW) * _invTileSize);
        int tMinY = (int)MathF.Floor((y - halfH) * _invTileSize);
        int tMaxY = (int)MathF.Floor((y + halfH) * _invTileSize);

        for (int ty = tMinY; ty <= tMaxY; ty++)
        {
            int baseTy = ty + _originTileY;

            int cy = baseTy >> 5;
            int ly = baseTy & 31;

            for (int tx = tMinX; tx <= tMaxX; tx++)
            {
                int baseTx = tx + _originTileX;

                int cx = baseTx >> 5;
                int lx = baseTx & 31;

                int ci = ChunkIndex(cx, cy);
                ulong[] chunk = GetOrCreateChunk(ci);

                int index = (ly << 5) + lx;

                chunk[index >> 6] |= 1UL << (index & 63);
            }
        }
    }
    // ---------------------------
    // SET TILE
    // ---------------------------


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastDiv32(int x)
    {
        // división correcta para negativos
        return x >= 0 ? x >> 5 : ((x - 31) >> 5);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y)
    {
       
        int tx = x + _originTileX;
        int ty = y + _originTileY;

        int cx = FastDiv32(tx);
        int cy = FastDiv32(ty);

        int lx = tx & CHUNK_MASK;
        int ly = ty & CHUNK_MASK;

        int ci = ChunkIndex(cx, cy);
        ulong[] chunk = GetOrCreateChunk(ci);

        int index = (ly << 5) + lx;

        chunk[index >> 6] |= 1UL << (index & 63);
    }

    // ---------------------------
    // GET TILE
    // ---------------------------
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOccupiedWorld(float x, float y)
    {
        int tx = (int)MathF.Floor(x * _invTileSize);
        int ty = (int)MathF.Floor(y * _invTileSize);

        return IsOccupied(tx, ty);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOccupied(int x, int y)
    {
        int tx = x + _originTileX;
        int ty = y + _originTileY;

        int cx = tx >> 5;
        int cy = ty >> 5;

        int ci = ChunkIndex(cx, cy);

        if (!IsChunkActive(ci))
            return false;

        ulong[] chunk = _chunks[ci];

        int lx = tx & CHUNK_MASK;
        int ly = ty & CHUNK_MASK;

        int index = (ly << 5) + lx;

        return (chunk[index >> 6] & (1UL << (index & 63))) != 0;
    }

    // ---------------------------
    // WORLD → TILE
    // ---------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WorldToTile(float x, float y, out int tx, out int ty)
    {
        tx = (int)MathF.Floor(x * _invTileSize);
        ty = (int)MathF.Floor(y * _invTileSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2I WorldToTile(float x, float y)
    {
        int tx = (int)MathF.Floor(x * _invTileSize);
        int ty = (int)MathF.Floor(y * _invTileSize);

        return new Vector2I(tx, ty);
    }

    // ---------------------------
    // ITERAR CHUNKS ACTIVOS 🔥
    // ---------------------------

    public void ForEachActiveChunk(Action<int, ulong[]> action)
    {
        for (int i = 0; i < _chunkMask.Length; i++)
        {
            ulong mask = _chunkMask[i];

            while (mask != 0)
            {
                int bit = BitOperations.TrailingZeroCount(mask);
                int chunkIndex = (i << 6) + bit;

                action(chunkIndex, _chunks[chunkIndex]);

                mask &= mask - 1; // remove lowest bit
            }
        }
    }

    // ---------------------------
    // CLEAR
    // ---------------------------

    public void ClearAll()
    {
        Array.Clear(_chunkMask, 0, _chunkMask.Length);
        Array.Clear(_chunks, 0, _chunks.Length);
    }
}