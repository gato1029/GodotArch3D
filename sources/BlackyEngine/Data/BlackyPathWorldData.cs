using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyPathWorldData: BlackyWorldDataMap<ushort>
{
    public BlackyGenericPalette<CaminosData> pallete { get; } = new("Caminos");

    private DualTileTemplate _dualTemplate;
    public BlackyPathWorldData(int chunkSize, BlackyChunkCacheTextureMap textureMap, BlackyWorldRegions regions) : base(chunkSize, BlackyRenderLayer.Caminos, textureMap, true, regions)
    {

    }
    // =====================================================
    // SET PATH
    // =====================================================

    public void SetDualTemplate(DualTileTemplate dualTemplate)
    {
        _dualTemplate = dualTemplate;
    }
    internal void SetPath(int x, int y, int altura, CaminosData pathData, Brush brush)
    {
        if (DiferentPosition(x, y, altura, (int)RenderLayer, brush, pathData.id))
        {
            SetPath(x, y, altura, pathData.nameMod, pathData.id, brush);
        }

    }


    int lastX = 0;
    int lastY = 0;
    int lastAltura = -1;
    int lastCapa = -1;
    long lastTerrain = -1;
    private bool DiferentPosition(int baseX, int baseY,
    int altura,
    int capa,
    Brush brush,
    long terrainBaseDataId)
    {
        if (baseX != lastX || baseY != lastY || altura != lastAltura || capa != lastCapa || terrainBaseDataId != lastTerrain)
        {
            return true;
        }
        return false;
    }

    public void SetPath(int worldX, int worldY, int height, string modName, long id, Brush brush)
    {
        // 1. GUARDAR LÓGICA
        CaminosData data = null;
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell = ref ResolveOrCreateCell(x, y, height);

            cell = pallete.GetIdPersistence(modName, id, out data);
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

    public void RemovePath(int worldX, int worldY, int height, Brush brush)
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
            if (cell != 0)
            {
                lastId = cell;
            }
            cell = 0;
        }
        if (lastId == 0)
        {
            return; // es vacio 
        }

        var data = pallete.GetData(lastId);

        _dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);

        _textureMap.ApplyBrushRemoveDual(
          worldX,
          worldY,
          height,
          (int)RenderLayer,
          brush,
          _dualTemplate);

    }


    public bool HasPath(
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
}
