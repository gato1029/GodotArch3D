using Godot;
using GodotEcsArch.sources.BlackyTiles.Procedural.Terrain;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using System;
using System.Collections.Generic;
using System.Linq;
using static Flecs.NET.Core.Ecs;

namespace GodotEcsArch.sources.BlackyTiles.Procedural.Terrain;


public class CachedTerrainData
{
    public bool HasHeight;
    public int BorderPaddingBottom;

    public TerrainTileEntry Min;
    public TerrainTileEntry Max;

    // 🔥 capas ya resueltas (sin foreach en runtime)
    public LayerData MinLayers;
    public LayerData MaxLayers;
}

public struct LayerData
{
    public long IdBase;
    public long IdSuperficie;
    public long IdDecoracionBase;
    public long IdDecoracionSuperficie;

    public int LayerBase;
    public int LayerSuperficie;
    public int LayerDecoracionBase;
    public int LayerDecoracionSuperficie;
}


public class BlackyWorldTileMapper
    {
        private readonly int chunkSize;
        private readonly BlackyWorldBiomeMap biomeMap;
        private readonly BlackyWorldTerrainGenerator terrainGenerator;
        private readonly BlackyTerrainSystem blackyTerrainSystem;

    private Dictionary<ushort, CachedTerrainData> terrainCache = new();

    FastNoiseLite noise;
    
    public BlackyWorldTileMapper(
        int chunkSize,
        int seed,
        BlackyWorldBiomeMap biomeMap,
        BlackyWorldTerrainGenerator terrainGenerator,
        BlackyTerrainSystem blackyTerrainSystem)
    {
        this.chunkSize = chunkSize;
        this.biomeMap = biomeMap;
        this.terrainGenerator = terrainGenerator;
        this.blackyTerrainSystem = blackyTerrainSystem;
        noise = new FastNoiseLite(seed + 3450);        
        noise.SetFrequency(0.09f);

        BuildTerrainCache();
    }
    public void BuildTerrainCache()
    {
        MasterDataManager.RegisterAllData<long, TerrainData>();

        var all = MasterDataManager.GetAllData<long, TerrainData>();

        foreach (var data in all)
        {
            var terrains = data.terrains;

            int min = int.MaxValue;
            int max = int.MinValue;

            foreach (var k in terrains.Keys)
            {
                if (k < min) min = k;
                if (k > max) max = k;
            }

            var terrainMin = terrains[min];
            var terrainMax = terrains[max];

            terrainCache[(ushort)data.idSave] = new CachedTerrainData
            {
                HasHeight = terrains.Count > 1,

                Min = terrainMin,
                Max = terrainMax,

                MinLayers = ExtractLayers(terrainMin),
                MaxLayers = ExtractLayers(terrainMax),
                BorderPaddingBottom = data.paddingBorder
            };
        }
    }
    private LayerData ExtractLayers(TerrainTileEntry terrain)
    {
        LayerData result = new();

        foreach (var item in terrain.layersRelative)
        {
            switch (item.layerType)
            {
                case TerrinLayerType.Base:
                    result.IdBase = item.idAutoTile;
                    result.LayerBase = item.layer;
                    break;

                case TerrinLayerType.Superficie:
                    result.IdSuperficie = item.idAutoTile;
                    result.LayerSuperficie = item.layer;
                    break;

                case TerrinLayerType.DecoracionBase:
                    result.IdDecoracionBase = item.idAutoTile;
                    result.LayerDecoracionBase = item.layer;
                    break;

                case TerrinLayerType.DecoracionSuperficie:
                    result.IdDecoracionSuperficie = item.idAutoTile;
                    result.LayerDecoracionSuperficie = item.layer;
                    break;
            }
        }

        return result;
    }
    // =========================================
    // 🌍 GENERAR DATA DE TILE POR CHUNK
    // =========================================
    public void GenerateChunkTileData(Vector2I chunkCoord)
    {
      

        // 🔹 offset mundo real del chunk
        int startX = chunkCoord.X * chunkSize;
        int startY = chunkCoord.Y * chunkSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = startX + x;
                int worldY = startY + y;

                Vector2I worldPosition = new Vector2I(worldX, worldY);

                // =====================================
                // 🔥 ACCESO GLOBAL (CLAVE)
                // =====================================
                ushort biomeId = biomeMap.GetBiomeAt(worldX, worldY);
                bool isBiomeBorder = biomeMap.GetBorderAt(worldX, worldY) != 0;

                ushort height = terrainGenerator.GetHeightAt(worldX, worldY);
                bool isHeightBorder = terrainGenerator.GetHeightBorderAt(worldX, worldY) != 0;

                // =====================================
                // 🔹 pintar tile
                // =====================================
                PlotTile(
                    worldPosition,
                    biomeId,
                    isBiomeBorder,
                    height,
                    isHeightBorder
                );
                PlotTileOverlay(worldPosition,
                    biomeId,
                    isBiomeBorder,
                    height,
                    isHeightBorder);

                float n = noise.GetNoise(worldPosition.X, worldPosition.Y);
                if (n > 0.3f)
                {
                    PlotTileDecoration(worldPosition,
                 biomeId,
                 isBiomeBorder,
                 height,
                 isHeightBorder);
                }
             
            }
        }

    
    }
    public void PlotTileDecoration(Vector2I worldPosition, ushort biomeId, bool isBiomeBorder, ushort height, bool isHeightBorder)
    {
        var data = terrainCache[biomeId];
        bool hasHeightBiome = data.HasHeight;
        bool isHeightTop = hasHeightBiome && data.Max.heightReal == height;
        bool isHeightBottom = data.Min.heightReal == height;

        if (isHeightBottom && data.MinLayers.IdDecoracionSuperficie != 0 && isBiomeBorder == false )
        {
            blackyTerrainSystem.SetTerrain(
                  worldPosition.X,
                  worldPosition.Y,
                  biomeId,
                  data.MinLayers.IdDecoracionSuperficie,
                  height,
                  data.MinLayers.LayerDecoracionSuperficie,
                  true,
                  true,
                  false);
        }
        if (isHeightTop && data.MaxLayers.IdDecoracionSuperficie != 0 && isHeightBorder ==false)
        {
            // borde de altura máxima
            blackyTerrainSystem.SetTerrain(
            worldPosition.X,
            worldPosition.Y,
            biomeId,
            data.MaxLayers.IdDecoracionSuperficie,
            height,
            data.MaxLayers.LayerDecoracionSuperficie,
            true,
            true,
            false);
        }


    }
    public void PlotTileOverlay(Vector2I worldPosition, ushort biomeId, bool isBiomeBorder, ushort height, bool isHeightBorder)
    {
       var data = terrainCache[biomeId];
        bool hasHeightBiome = data.HasHeight;
        bool isHeightTop = hasHeightBiome && data.Max.heightReal == height;
        bool isHeightBottom = data.Min.heightReal == height;

        if (isHeightBottom &&data.MinLayers.IdSuperficie!=0)
        {
            blackyTerrainSystem.SetTerrain(
                  worldPosition.X,
                  worldPosition.Y,
                  biomeId,
                  data.MinLayers.IdSuperficie,
                  height,
                  data.MinLayers.LayerSuperficie,
                  true,
                  true,
                  false);
        }
        if (isHeightTop && data.MaxLayers.IdSuperficie != 0)
        {
                // borde de altura máxima
                blackyTerrainSystem.SetTerrain(
                worldPosition.X,
                worldPosition.Y,
                biomeId,
                data.MaxLayers.IdSuperficie,
                height,
                data.MaxLayers.LayerSuperficie,
                true,
                true,
                false);
        }
    
  
    }
    public void PlotTile(Vector2I worldPosition, ushort biomeId, bool isBiomeBorder, ushort height, bool isHeightBorder)
    {
        var data = terrainCache[biomeId];

        bool hasHeightBiome = data.HasHeight;
        bool isHeightTop = hasHeightBiome && data.Max.heightReal == height;
        bool isHeightBottom = data.Min.heightReal == height;
        // 🔥 1. SIEMPRE pintar base primero
        if (!isBiomeBorder && isHeightBottom)
        { 
            blackyTerrainSystem.SetTerrain(
                worldPosition.X,
                worldPosition.Y,
                biomeId,
                data.MinLayers.IdBase,
                height,
                data.MinLayers.LayerBase,
                true,
                true,
                false);
        }

        // 🔥 2. biome border encima (si aplica)
        if (isBiomeBorder && isHeightBottom)
        {
   
            PaintWithThicknessSquare(worldPosition, data.BorderPaddingBottom, pos =>
            {
                blackyTerrainSystem.SetTerrain(
                    pos.X,
                    pos.Y,
                    biomeId,
                    data.MinLayers.IdBase,
                    height,
                    data.MinLayers.LayerBase,
                    true,
                    true,
                    false);
            });

        }

        // 🔥 3. height border encima (máxima prioridad)
        if (isHeightTop && isHeightBorder)
        {


            PaintWithThicknessSquare(worldPosition, 1, pos =>
            {
                // debajo
                blackyTerrainSystem.SetTerrain(
                    pos.X,
                    pos.Y,
                    biomeId,
                    data.MinLayers.IdBase,
                    height - 1,
                    data.MinLayers.LayerBase,
                    false,
                    true,
                    false);
            });

            blackyTerrainSystem.SetTerrain(
                   worldPosition.X,
                   worldPosition.Y,
                   biomeId,
                   data.MaxLayers.IdBase,
                   height,
                   data.MaxLayers.LayerBase,
                   true,
                   true,
                   false);
        }
        else if (isHeightTop)
        {
            // 🔥 altura normal superior

            blackyTerrainSystem.SetTerrain(
                worldPosition.X,
                worldPosition.Y,
                biomeId,
                data.MaxLayers.IdBase,
                height,
                data.MaxLayers.LayerBase,
                true,
                true,
                false);

        }
    }

   



    private void PaintWithThicknessSquare(
    Vector2I center,
    int thickness,
    Action<Vector2I> paintAction)
    {
        for (int dx = -thickness; dx <= thickness; dx++)
        {
            for (int dy = -thickness; dy <= thickness; dy++)
            {
                Vector2I pos = new Vector2I(
                    center.X + dx,
                    center.Y + dy
                );
                //if (pos.X == center.X && pos.Y == center.Y)
                //    continue;

                paintAction(pos);
            }
        }
    }
}
