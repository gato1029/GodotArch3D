using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyTerrainWorldData : BlackyWorldDataMap<SerializerCellTerrain>
{
    private readonly DualTileTemplate _dualTemplate;
    public BlackyTerrainWorldData(int chunkSize, BlackyRenderLayer renderLayer, BlackyChunkCacheTextureMap textureMap, bool isDual) : base(chunkSize, renderLayer, textureMap, isDual)
    {

    }
    // =====================================================
    // SET TERRAIN
    // =====================================================

    public void SetTerrain(
        int worldX,
        int worldY,
        int height,

        ushort terrainId)
    {
        // 1. GUARDAR LÓGICA

        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.id = terrainId;

        // 2. REBUILD VISUAL

        RebuildNeighborhood(
            worldX,
            worldY,
            height);
    }

    // =====================================================
    // REMOVE
    // =====================================================

    public void RemoveSurface(
        int worldX,
        int worldY,
        int height)
    {
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.id = 0;

        RebuildNeighborhood(
            worldX,
            worldY,
            height);
    }

    // =====================================================
    // REBUILD
    // =====================================================

    private void RebuildNeighborhood(
        int x,
        int y,
        int height)
    {
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                RebuildCell(
                    x + ox,
                    y + oy,
                    height);
            }
        }
    }

    // =====================================================
    // REBUILD CELL
    // =====================================================

    private void RebuildCell(
        int worldX,
        int worldY,
        int height)
    {
        byte mask = 0;

        if (HasTerrain(worldX, worldY + 1, height))
            mask |= 8;

        if (HasTerrain(worldX + 1, worldY + 1, height))
            mask |= 4;

        if (HasTerrain(worldX, worldY, height))
            mask |= 2;

        if (HasTerrain(worldX + 1, worldY, height))
            mask |= 1;

        // NO HAY NADA

        if (mask == 0)
        {
            _textureMap.RemoveTile(
                worldX,
                worldY,
                height,
                (int)RenderLayer);

            return;
        }

        // OBTENER TILE VISUAL

        var slot =
            _dualTemplate.GetSlot(mask);

        // ESCRIBIR VISUAL CACHE

        _textureMap.SetTile(
            worldX,
            worldY,
            height,
            (int)RenderLayer,
            slot.ModId,
            slot.TileIndex);
    }

    // =====================================================
    // CHECK
    // =====================================================

    public bool HasTerrain(
        int worldX,
        int worldY,
        int height)
    {
        if (!TryGetCell(
            worldX,
            worldY,
            height,
            out var cell))
        {
            return false;
        }

        return cell.id != 0;
    }
}
