using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyDecorationWorldData: BlackyWorldDataMap<SerializerCellGeneric>
{
    private readonly ChunkManagerBase _chunkManager;

    public BlackyDecorationWorldData(int chunkSize, BlackyChunkCacheTextureMap textureMap, BlackyWorldRegions regions, ChunkManagerBase chunkManager, BlackyWorld blackyWorld)
        : base(chunkSize, BlackyRenderLayer.Adornos, textureMap, true, regions, blackyWorld)
    {
        _chunkManager = chunkManager;
        _chunkManager.OnChunkDataPreload += OnChunkDataPreload;
    }

    private void OnChunkDataPreload(Vector2I pos)
    {
        var coord = new BlackyChunkCoord(pos.X, pos.Y);
        if (!_chunks.TryGetValue(coord, out var chunk)) return;

        foreach (var (height, heightData) in chunk.GetHeights())
        {
            _textureMap.AplyChunkBatchSingleSprite(coord.X, coord.Y, height, (int)RenderLayer, heightData.GetCells().ToArray());
        }
    }

    // =====================================================
    // SET TILE
    // =====================================================
    public void SetDecoration(int worldX, int worldY, int height, DecorationData data, Brush brush)
    {
        ushort id = BlackyPalletesPersistence.decorationsPalette.GetIdPersistence(data.nameMod, data.id, out var decorationPersist);
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell = ref ResolveOrCreateCell(x, y, height);

            cell.id = id;
        }

        AtlasModsManager.GetSpriteUniqueId(data.idTileSprite, out TileSpriteData tileSpriteData);

        foreach (var item in tileSpriteData.tilesOcupancy)
        {
            int x = worldX + item.x;
            int y = worldY + item.y;
            world.Services.TerrainDataLienzo.RemoveTerrain(x, y, height, brush);
        }

        //cell.TileId = rampsPalette.GetIdPersistence(,rampsData);


        _textureMap.SetTileSprite(worldX, worldY, height, (int)RenderLayer, decorationPersist.idTileSprite, true);

    }

    public void SetTile(int worldX, int worldY, int height, string modName, ushort textureIndex)
    {
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.id = _textureMap.SetTile(worldX, worldY, height, (int)RenderLayer, modName, textureIndex, true);

    }

    // =====================================================
    // REMOVE TILE
    // =====================================================

    public void RemoveAdorno(
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

        _textureMap.RemoveTileSprite(
            worldX,
            worldY,
            height,
            (int)RenderLayer);
    }

    // =====================================================
    // GET
    // =====================================================

    public bool HasTile(
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

    public ushort GetTile(
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
        return cell.id;
    }
}
