using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;



public interface IBlackyChunkTilemapTexture
{
    int LayerIndex { get; }
    int Size { get; }

    // ===============================
    // STATE
    // ===============================

    bool HasDirtyTiles { get; }

    // ===============================
    // SINGLE TILE
    // ===============================

    void SetTile(int x, int y, int tileId, bool isDirty = true);
    int GetTile(int x, int y);

    void ClearTile(int x, int y);
    bool IsEmpty(int x, int y);

    // ===============================
    // BULK (IMPORTANTE)
    // ===============================

    void FillRectLocal(
        int startX,
        int startY,
        int endX,
        int endY,
        int tileId);

    // ===============================
    // DIRTY CONSUME
    // ===============================

    IEnumerable<(int x, int y, int tileId, float worldX, float worldY)> ConsumeDirtyTiles();
}