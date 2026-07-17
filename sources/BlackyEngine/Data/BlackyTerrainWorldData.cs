
using Arch.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using static Flecs.NET.Core.Ecs.Units;


namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyTerrainWorldData : BlackyWorldDataMap<SerializerCellGeneric>
{
    
    private ChunkManagerBase _chunkManager;
    private DualTileTemplate _dualTemplate;
    public BlackyTerrainWorldData(int chunkSize, BlackyChunkCacheTextureMap textureMap, BlackyWorldRegions regions, ChunkManagerBase chunkManager) : base(chunkSize, BlackyRenderLayer.TerrenoBase, textureMap, true, regions)
    {
        _chunkManager = chunkManager;
        _chunkManager.OnChunkPreLoadGenerator += _chunkManager_OnChunkPreLoadGenerator;
    }
    private void _chunkManager_OnChunkPreLoadGenerator(Godot.Vector2I obj)
    {
        BlackyChunkCoord coord = new BlackyChunkCoord(obj.X, obj.Y);
        if (!_chunks.TryGetValue(coord, out BlackyChunkData<SerializerCellGeneric> chunk))
            return; 

        foreach (var (height, heightData) in chunk.GetHeights())
        {
            ReadOnlySpan<SerializerCellGeneric> cells = heightData.GetCells().Span;

            var allcells = heightData.GetCells().ToArray();
            _textureMap.ApplyChunkBatch(coord.X, coord.Y, height, (int)RenderLayer, allcells);
            //for (int y = 0; y < ChunkSize; y++)
            //{
            //    int row = y * ChunkSize;
            //    for (int x = 0; x < ChunkSize; x++)
            //    {
            //        ref readonly SerializerCellGeneric cell = ref cells[row + x];
            //        if (cell.id == 0) continue; 

            //        int worldX = coord.X * ChunkSize + x;
            //        int worldY = coord.Y * ChunkSize + y;

            //        var data = terrainPalette.GetData(cell.id);
            //        var dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate); // LOCAL                    
            //        _textureMap.SetTileDualConcurrent(worldX, worldY, height, (int)RenderLayer, dualTemplate,cell.isBorder);
            //    }
            //}
        }
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
        if(DiferentPosition(x,y,altura,(int)RenderLayer,brush,terrainBaseDataSelected.id))
        {
            SetTerrain(x, y, altura, terrainBaseDataSelected.nameMod, terrainBaseDataSelected.id, brush);
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

    public void SetTerrain(int worldX, int worldY, int height,string modName, long terrainId, Brush brush)
    {
        // 1. GUARDAR LÓGICA
        TerrainBaseData data = null;
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell = ref ResolveOrCreateCell(x, y, height);

            cell.id = BlackyPalletesPersistence.terrainPalette.GetIdPersistence(modName, terrainId, out  data);
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

        var data = BlackyPalletesPersistence.terrainPalette.GetData(lastId);

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
