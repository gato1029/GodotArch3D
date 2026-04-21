using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;



public abstract class BlackyChunkTilemapTextureBase : IBlackyChunkTilemapTexture
{
    protected readonly int _size;
    protected readonly int _worldX;
    protected readonly int _worldY;

    // ===============================
    // HYBRID DIRTY
    // ===============================

    private HashSet<int> _dirtySet = new();
    private bool _useRegion = false;

    private int _dirtyMinX, _dirtyMinY;
    private int _dirtyMaxX, _dirtyMaxY;

    private const int DIRTY_THRESHOLD = 64; // 🔥 ajustable

    public int LayerIndex { get; }
    public int Size => _size;

    public bool HasDirtyTiles => _useRegion || _dirtySet.Count > 0;

    protected BlackyChunkTilemapTextureBase(int layerIndex, int size, int worldX, int worldY)
    {
        LayerIndex = layerIndex;
        _size = size;
        _worldX = worldX;
        _worldY = worldY;
    }

    protected int GetIndex(int x, int y)
        => y * _size + x;

    // ===============================
    // DIRTY LOGIC
    // ===============================

    protected void MarkDirty(int x, int y)
    {
        if (_useRegion)
        {
            ExpandRegion(x, y);
            return;
        }

        int index = GetIndex(x, y);
        _dirtySet.Add(index);

        if (_dirtySet.Count > DIRTY_THRESHOLD)
        {
            ConvertToRegion();
        }
    }

    private void ConvertToRegion()
    {
        _useRegion = true;

        bool first = true;

        foreach (var index in _dirtySet)
        {
            int y = index / _size;
            int x = index % _size;

            if (first)
            {
                _dirtyMinX = _dirtyMaxX = x;
                _dirtyMinY = _dirtyMaxY = y;
                first = false;
            }
            else
            {
                ExpandRegion(x, y);
            }
        }

        _dirtySet.Clear();
    }

    private void ExpandRegion(int x, int y)
    {
        if (x < _dirtyMinX) _dirtyMinX = x;
        if (y < _dirtyMinY) _dirtyMinY = y;
        if (x > _dirtyMaxX) _dirtyMaxX = x;
        if (y > _dirtyMaxY) _dirtyMaxY = y;
    }

    private void ResetDirty()
    {
        _dirtySet.Clear();
        _useRegion = false;
    }

    // ===============================
    // API
    // ===============================

    public abstract void SetTile(int x, int y, int tileId, bool isDirty = true);
    public abstract int GetTile(int x, int y);
    public abstract bool IsEmpty(int x, int y);
    public abstract void ClearTile(int x, int y);

    protected abstract void SetTileUnsafe(int index, int tileId);

    // ===============================
    // BULK
    // ===============================

    public void FillRectLocal(
        int startX,
        int startY,
        int endX,
        int endY,
        int tileId)
    {
        for (int y = startY; y < endY; y++)
        {
            int rowStart = y * _size;

            for (int x = startX; x < endX; x++)
            {
                int index = rowStart + x;
                SetTileUnsafe(index, tileId);
            }
        }

        // 🔥 fuerza a region directamente
        _useRegion = true;

        _dirtyMinX = startX;
        _dirtyMinY = startY;
        _dirtyMaxX = endX - 1;
        _dirtyMaxY = endY - 1;
    }

    // ===============================
    // CONSUME
    // ===============================

    public IEnumerable<(int x, int y, int tileId, float worldX, float worldY)> ConsumeDirtyTiles()
    {
        if (_useRegion)
        {
            for (int y = _dirtyMinY; y <= _dirtyMaxY; y++)
            {
                int rowStart = y * _size;

                for (int x = _dirtyMinX; x <= _dirtyMaxX; x++)
                {
                    yield return (
                        x,
                        y,
                        GetTile(x, y),
                        _worldX + x,
                        _worldY + y
                    );
                }
            }
        }
        else
        {
            foreach (var index in _dirtySet)
            {
                int y = index / _size;
                int x = index % _size;

                yield return (
                    x,
                    y,
                    GetTile(x, y),
                    _worldX + x,
                    _worldY + y
                );
            }
        }

        ResetDirty();
    }
}