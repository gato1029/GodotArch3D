
using Arch.Core;
using Godot;
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
        _chunkManager.OnChunkDataPreload += _chunkManager_OnChunkDataPreload;
    }
    private void _chunkManager_OnChunkDataPreload(Godot.Vector2I obj)
    {
        BlackyChunkCoord coord = new BlackyChunkCoord(obj.X, obj.Y);
        if (!_chunks.TryGetValue(coord, out BlackyChunkData<SerializerCellGeneric> chunk))
            return; 
        GD.Print($"[BlackyTerrainWorldData] OnChunkDataPreload coord: {coord.X},{coord.Y} ");
        foreach (var (height, heightData) in chunk.GetHeights())
        {
            //ReadOnlySpan<SerializerCellGeneric> cells = heightData.GetCells().Span;

            var allcells = heightData.GetCells().ToArray();
            _textureMap.ApplyChunkBatch(coord.X, coord.Y, height, (int)RenderLayer, allcells);
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

    public void SetTerrain(int worldX, int worldY, int height, string modName, long terrainId, Brush brush)
    {
        // 1. PRIMERA PASADA: Guardar los IDs
        TerrainBaseData data = null;
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;
            ref var cell = ref ResolveOrCreateCell(x, y, height);
            cell.id = BlackyPalletesPersistence.terrainPalette.GetIdPersistence(modName, terrainId, out data);
        }

        // 2. SEGUNDA PASADA: Calcular si es borde
        // Debemos expandir la revisión un poco más allá del pincel (el área +1)
        // porque los vecinos de lo que pintaste también pueden haber cambiado su estado de borde
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                // Recorremos el área del pincel expandida en 1 para actualizar bordes vecinos
                foreach (var offset in brush.Cells)
                {
                    int x = worldX + offset.x + ox;
                    int y = worldY + offset.y + oy;

                    ref var cell = ref ResolveOrCreateCell(x, y, height);
                    {
                        // Solo calculamos si tiene ID
                        if (cell.id != 0)
                        {
                            cell.isBorder = CalculateIsBorder(x, y, height);
                        }
                    }
                }
            }
        }

        _dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);
        _textureMap.ApplyBrushCreateDual(worldX, worldY, height, (int)RenderLayer, brush, _dualTemplate);
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

    private bool CalculateIsBorder(int worldX, int worldY, int height)
    {
        // Revisar los 8 vecinos (o 4 si solo consideras ortogonales)
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                if (ox == 0 && oy == 0) continue;

                // Si el vecino está vacío, entonces el tile actual es un borde
                if (!HasTerrain(worldX + ox, worldY + oy, height))
                {
                    return true;
                }
            }
        }
        return false;
    }
    // =====================================================
    // REMOVE
    // =====================================================

    public void RemoveTerrain(int worldX, int worldY, int height, Brush brush)
    {
        if (!DiferentPosition(worldX, worldY, height, (int)RenderLayer, brush, 0))
        {
            return;
        }

        ushort lastId = 0;

        // 1. PRIMERA PASADA: Borrar los terrenos y capturar lastId para el renderizado
        foreach (var offset in brush.Cells)
        {
            int x = worldX + offset.x;
            int y = worldY + offset.y;

            ref var cell = ref ResolveOrCreateCell(x, y, height);
            if (cell.id != 0)
            {
                lastId = cell.id;
            }
            cell.id = 0;
            cell.isBorder = false; // Al estar vacío, ya no es un borde
        }

        if (lastId == 0) return;

        // 2. SEGUNDA PASADA: Actualizar bordes de los vecinos afectados
        // Cuando borras, los bloques alrededor que quedaron en pie ahora pueden haberse vuelto "borde"
        for (int oy = -1; oy <= 1; oy++)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                foreach (var offset in brush.Cells)
                {
                    int x = worldX + offset.x + ox;
                    int y = worldY + offset.y + oy;

                    ref var cell = ref ResolveOrCreateCell(x, y, height);
                    {
                        // Solo recalculamos los que tienen un ID válido
                        if (cell.id != 0)
                        {
                            cell.isBorder = CalculateIsBorder(x, y, height);
                        }
                    }
                }
            }
        }

        var data = BlackyPalletesPersistence.terrainPalette.GetData(lastId);
        _dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);

        _textureMap.ApplyBrushRemoveDual(worldX, worldY, height, (int)RenderLayer, brush, _dualTemplate);
    }




}
