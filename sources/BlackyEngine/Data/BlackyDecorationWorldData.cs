using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyDecorationWorldData: BlackyWorldDataMap<ushort>
{
    public BlackyGenericPalette<DecorationData> palette { get; } = new("Decoracion");
    public BlackyDecorationWorldData(
        int chunkSize,
        BlackyChunkCacheTextureMap textureMap,
        BlackyWorldRegions regions)
        : base(
            chunkSize,
            BlackyRenderLayer.Adornos,
            textureMap,
            false,
            regions)
    {

    }

    // =====================================================
    // SET TILE
    // =====================================================
    public void SetDecoration(int worldX, int worldY, int height, DecorationData decorationData, Brush brush)
    {
        ushort id = palette.GetIdPersistence(decorationData.nameMod, decorationData.id, out var decorationDataPersist);
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell = ref ResolveOrCreateCell(x, y, height);

            cell = id;
        }

        AtlasModsManager.GetSpriteUniqueId(decorationDataPersist.idTileSprite, out TileSpriteData tileSpriteData);
                
        _textureMap.SetTileSprite(worldX, worldY, height, (int)RenderLayer, decorationDataPersist.idTileSprite, true);

    }



    // =====================================================
    // REMOVE TILE
    // =====================================================

    public void RemoveDecoration(
        int worldX,
        int worldY,
        int height)
    {
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell= 0;

        _textureMap.RemoveTileSprite(
            worldX,
            worldY,
            height,
            (int)RenderLayer);
    }

    // =====================================================
    // GET
    // =====================================================

    public bool HasDecoration(
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

        return cell != 0;
    }

    public ushort GetDecoration(
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
            return 0;
        }

        return cell;
    }
}
