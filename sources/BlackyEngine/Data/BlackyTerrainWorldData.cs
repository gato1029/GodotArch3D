using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
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
    public BlackyTerrainWorldData(int chunkSize, BlackyChunkCacheTextureMap textureMap) : base(chunkSize, BlackyRenderLayer.TerrenoBase, textureMap, true)
    {

    }
    // =====================================================
    // SET TERRAIN
    // =====================================================

    public void SetTerrain(
        int worldX,
        int worldY,
        int height,
        ushort terrainId, Brush brush)
    {
        // 1. GUARDAR LÓGICA

        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.id = terrainId;


        _textureMap.ApplyBrushCreateDual(
            worldX,
            worldY,
            height,
            (int)RenderLayer,
            brush,
            _dualTemplate);

    }

    // =====================================================
    // REMOVE
    // =====================================================

    public void RemoveTerrain(int worldX,int worldY,int height, Brush brush)
    {
        ref var cell =
            ref ResolveOrCreateCell(
                worldX,
                worldY,
                height);

        cell.id = 0;

        _textureMap.ApplyBrushRemoveDual(
          worldX,
          worldY,
          height,
          (int)RenderLayer,
          brush,
          _dualTemplate);

    }

   
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
