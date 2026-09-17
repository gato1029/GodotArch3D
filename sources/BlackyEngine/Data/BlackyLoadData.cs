using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyTiles;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.Data;

internal class BlackyLoadData
{
    private enum LayerType { Ramp, Surface, Decoration, Path }

    public static void LoadAll(BlackyWorld blackyWorld)
    {

        
        // 1. Cargamos todas las regiones una sola vez desde el disco
        List<RegionSaveData> regions = blackyWorld.Services.persistenceData.LoadAllRegions();
        if (regions == null || regions.Count == 0) return;

        // 2. Cargamos el terreno por separado (tiene estructura distinta con isBorder e IDs)
        CargarTerreno(blackyWorld, regions);

        // 3. Cargamos las capas genéricas usando el selector (Func) para elegir la propiedad de la región
        CargarCapaGenerica(blackyWorld, regions, r => r.RampChunks, LayerType.Ramp);
        CargarCapaGenerica(blackyWorld, regions, r => r.SurfaceChunks, LayerType.Surface);
        CargarCapaGenerica(blackyWorld, regions, r => r.DecorationChunks, LayerType.Decoration);
        CargarCapaGenerica(blackyWorld, regions, r => r.PathChunks, LayerType.Path);
    }
    

    private static void CargarTerreno(BlackyWorld blackyWorld, List<RegionSaveData> regions)
    {
        int chunkSize = blackyWorld.Config.ChunkSize;

        foreach (var item in regions)
        {
            List<TerrainChunkSave> terrain = item.TerrainChunks;
            if (terrain == null) continue;

            foreach (var itemChunk in terrain)
            {
                BlackyChunkCoord chunkCoord = new BlackyChunkCoord(itemChunk.ChunkX, itemChunk.ChunkY);

                foreach (var itemHeight in itemChunk.Heights)
                {
                    int height = itemHeight.Height;
                    ushort[] terrainIds = itemHeight.TerrainIds;
                    bool[] terrainBorders = itemHeight.TerrainBorders;

                    if (terrainIds == null) continue;

                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int x = 0; x < chunkSize; x++)
                        {
                            int index = y * chunkSize + x;
                            if (index >= terrainIds.Length) continue;

                            ushort idTerrain = terrainIds[index];
                            bool isborder = terrainBorders != null && index < terrainBorders.Length ? terrainBorders[index] : false;

                            if (idTerrain == 0 && !isborder) continue;

                            blackyWorld.Services.TerrainDataLienzo.SetTerrainDataNoRenderLocal(chunkCoord, x, y, height, isborder, idTerrain);
                        }
                    }
                }
            }
        }
    }

    // Método genérico que usa el Func para extraer los chunks y un switch interno ultra rápido para las celdas
    private static void CargarCapaGenerica(
        BlackyWorld blackyWorld,
        List<RegionSaveData> regions,
        Func<RegionSaveData, List<GenericChunkSave>> chunkSelector, // <-- Aquí usas el Func
        LayerType layerType)
    {
        int chunkSize = blackyWorld.Config.ChunkSize;

        foreach (var item in regions)
        {
            // El Func se invoca aquí (fuera del bucle pesado de celdas)
            List<GenericChunkSave> chunks = chunkSelector(item);
            if (chunks == null) continue;

            foreach (var itemChunk in chunks)
            {
                BlackyChunkCoord chunkCoord = new BlackyChunkCoord(itemChunk.ChunkX, itemChunk.ChunkY);

                foreach (var itemHeight in itemChunk.Heights)
                {
                    int height = itemHeight.Height;
                    ushort[] idData = itemHeight.idData;

                    if (idData == null) continue;

                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int x = 0; x < chunkSize; x++)
                        {
                            int index = y * chunkSize + x;
                            if (index >= idData.Length) continue;

                            ushort id = idData[index];
                            if (id == 0) continue;

                            // Asignación directa según el tipo de capa
                            switch (layerType)
                            {
                                case LayerType.Ramp:
                                     blackyWorld.Services.RampasDataLienzo.SetDataDirectNoRenderLocal(chunkCoord, x, y, height, id);
                                    break;
                                case LayerType.Surface:
                                     blackyWorld.Services.SuperficiesDataLienzo.SetDataDirectNoRenderLocal(chunkCoord, x, y, height, id);
                                    break;
                                case LayerType.Decoration:
                                     blackyWorld.Services.AdornosDataLienzo.SetDataDirectNoRenderLocal(chunkCoord, x, y, height, id);
                                    break;
                                case LayerType.Path:
                                     blackyWorld.Services.CaminosDataLienzo.SetDataDirectNoRenderLocal(chunkCoord, x, y, height, id);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}