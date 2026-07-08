using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using System;

namespace GodotEcsArch.sources.BlackyEngine.Data;
/// <summary>
/// Mundo visual autoritativo para rampas.
/// 
/// A diferencia del terreno:
/// - NO usa dual tiles.
/// - NO genera visuales proceduralmente.
/// - Los tiles visuales SON persistentes.
/// - Guarda IDs locales regionales.
/// </summary>
public struct VisualTileCell
{
    /// <summary>
    /// ID local dentro de la palette regional.
    /// </summary>
    public ushort TileId;
}

public class BlackyRampVisualWorld
    : BlackyWorldDataMap<VisualTileCell>
{
    public BlackyGenericPalette<RampsData> rampsPalette { get; } = new();
    public BlackyRampVisualWorld(
        int chunkSize,
        BlackyChunkCacheTextureMap textureMap,
        BlackyWorldRegions regions)
        : base(
            chunkSize,
            BlackyRenderLayer.Rampas,
            textureMap,
            false,
            regions)
    {

    }

    // =====================================================
    // SET TILE
    // =====================================================
    public void SetRamp(int worldX, int worldY, int height, RampsData rampsData)
    {
        ref var cell = ref ResolveOrCreateCell(worldX,worldY, height);
        //cell.TileId = rampsPalette.GetIdPersistence(,rampsData);
        cell.TileId = rampsPalette.GetIdPersistence(rampsData.nameMod,rampsData.idSave, out var rampDataPersist);
        
        _textureMap.SetTileSprite(worldX, worldY, height, (int)RenderLayer, rampDataPersist.idTileSprite);

    }

    public void SetTile(int worldX,int worldY,int height,string modName, ushort textureIndex)
    {  
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.TileId = _textureMap.SetTile( worldX,  worldY,  height, (int)RenderLayer, modName, textureIndex,true);

    }

    // =====================================================
    // REMOVE TILE
    // =====================================================

    public void RemoveTile(
        int worldX,
        int worldY,
        int height)
    {
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.TileId = 0;

        _textureMap.RemoveTile(
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

        return cell.TileId != 0;
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

        return cell.TileId;
    }
}