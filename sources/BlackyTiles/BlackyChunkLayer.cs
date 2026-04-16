using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles;
using System.Collections.Generic;

public class BlackyChunkLayer
{
    private readonly long[,] _renderIds;
    private readonly int[,] _groupIds;
    private readonly float[,] _offsetX;
    private readonly float[,] _offsetY;

    private readonly HashSet<int> _dirtyTiles = new();

    private readonly int _size;

    private readonly int _chunkWorldX;
    private readonly int _chunkWorldY;

    public int LayerIndex { get; }

    public bool HasDirtyTiles => _dirtyTiles.Count > 0;

    public int Size => _size;

    public BlackyChunkLayer(
        int layerIndex,
        int size,
        int chunkWorldX,
        int chunkWorldY)
    {
        LayerIndex = layerIndex;
        _size = size;

        _chunkWorldX = chunkWorldX;
        _chunkWorldY = chunkWorldY;

        _renderIds = new long[size, size];
        _groupIds = new int[size, size];
        _offsetX = new float[size, size];
        _offsetY = new float[size, size];
    }

    private int GetIndex(int x, int y)
        => y * _size + x;

    #region Render Data

    public void SetRender(
        int x,
        int y,
        int groupId,
        long renderId,
        float offsetX = 0f,
        float offsetY = 0f,
        bool isDirty = false)
    {
        _renderIds[x, y] = renderId;
        _offsetX[x, y] = offsetX;
        _offsetY[x, y] = offsetY;
        _groupIds[x,y] = groupId;
        if (isDirty)
            MarkTileDirty(x, y);
    }
    public bool isEmpty(int x, int y)
    {
        return _renderIds[x, y] == 0;
    }
    public void ClearRender(int x, int y)
    {
        _renderIds[x, y] = 0;
        _offsetX[x, y] = 0f;
        _offsetY[x, y] = 0f;
        _groupIds[x,y] = 0;

        MarkTileDirty(x, y);
    }

    public (int x, int y,int groupId,
                        long renderId,
                        float offsetX,
                        float offsetY,
                        float worldX,
                        float worldY)
        GetRenderData(int x, int y)
    {
        long id = _renderIds[x, y];
        int groupId = _groupIds[x, y];


        float offX = _offsetX[x, y];
        float offY = _offsetY[x, y];

        float worldX = _chunkWorldX + x ;
        float worldY = _chunkWorldY + y ;

         return (x, y, groupId, id, offX, offY, worldX, worldY);
    }

    #endregion

    #region Enumerators

    // 🔥 Enumerar TODO el layer (ideal para build inicial)
    public IEnumerable<(int x, int y,
                        long groupId,
                        long renderId,
                        float offsetX,
                        float offsetY,
                        float worldX,
                        float worldY)> GetAllTiles()
    {
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                long id = _renderIds[x, y];
                long groupId = _groupIds[x, y];

                if (id == 0)
                    continue;

                float offX = _offsetX[x, y];
                float offY = _offsetY[x, y];

                float worldX = _chunkWorldX + x ;
                float worldY = _chunkWorldY + y ;

                yield return (x, y,groupId, id, offX, offY, worldX, worldY);
            }
        }
    }

    // 🔥 Enumerar solo dirty tiles (para updates incrementales)
    public IEnumerable<(int x, int y, long groupId,
                        long renderId,
                        float offsetX,
                        float offsetY,
                        float worldX,
                        float worldY)> ConsumeDirtyTilesWithData()
    {
        foreach (var index in _dirtyTiles)
        {
            int y = index / _size;
            int x = index % _size;

            long id = _renderIds[x, y];
            long groupId = _groupIds[x, y];
            float offX = _offsetX[x, y];
            float offY = _offsetY[x, y];

            float worldX = _chunkWorldX + x ;
            float worldY = _chunkWorldY + y ;

            yield return (x, y,groupId,  id, offX, offY, worldX, worldY);
        }

        _dirtyTiles.Clear();
    }

    #endregion

    #region Dirty

    public void MarkTileDirty(int x, int y)
    {
        _dirtyTiles.Add(GetIndex(x, y));
    }

    #endregion
}