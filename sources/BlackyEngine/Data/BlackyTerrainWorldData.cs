using Arch.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyTerrainWorldData : BlackyWorldDataMap<SerializerCellTerrain>
{
    public BlackyGenericPalette<TerrainBaseData> terrainPalette { get; } = new();

    private DualTileTemplate _dualTemplate;
    public BlackyTerrainWorldData(int chunkSize, BlackyChunkCacheTextureMap textureMap, BlackyWorldRegions regions) : base(chunkSize, BlackyRenderLayer.TerrenoBase, textureMap, true, regions)
    {

    }
    // =====================================================
    // SET TERRAIN
    // =====================================================

    public void SetDualTemplate(DualTileTemplate dualTemplate)
    {
        _dualTemplate = dualTemplate;
    }
    internal void SetTerrain(int x, int y, int altura, TerrainBaseData terrainBaseDataSelected, Brush brush)
    {
        if(DiferentPosition(x,y,altura,(int)RenderLayer,brush,terrainBaseDataSelected.idSave))
        {
            SetTerrain(x, y, altura, terrainBaseDataSelected.nameMod, terrainBaseDataSelected.idSave, brush);
        }
        
    }


    int lastX = 0;
    int lastY = 0;
    int lastAltura = -1;
    int lastCapa = -1;
    int lastTerrain = -1;
    private bool DiferentPosition(int baseX, int baseY,
    int altura,
    int capa,
    Brush brush,
    int terrainBaseDataId)
    {
        if (baseX != lastX || baseY != lastY || altura != lastAltura || capa != lastCapa || terrainBaseDataId != lastTerrain)
        {
            return true;
        }
        return false;
    }

    public void SetTerrain(int worldX, int worldY, int height,string modName, ushort terrainId, Brush brush)
    {
        // 1. GUARDAR LÓGICA
        TerrainBaseData data = null;
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell =
                ref ResolveOrCreateCell(
                    x,
                    y,
                    height);

            cell.id = terrainPalette.GetIdPersistence(modName, terrainId, out  data);
        }
        _dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);

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
        if (!DiferentPosition(worldX, worldY, height, (int)RenderLayer, brush, 0))
        {
            return;
        }
            ushort lastId = 0;
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell =
                ref ResolveOrCreateCell(
                    x,
                    y,
                    height);
            if (cell.id!=0)
            {
                lastId = cell.id;
            }            
            cell.id = 0;
        }
        if (lastId==0)
        {
            return; // es vacio 
        }

        var data = terrainPalette.GetData(lastId);

        _dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod,data.idDualTemplate);
        
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
