using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Territory;

public class BlackyWorldProceduralTerritory
{
    private readonly BlackyWorld _world;
    private BlackyWorldSeed _worldSeed;    
    private BlackyWorldConfig _config;
    private BlackyWorldGeography _geography;
    private TemplateWorldProfileData _worldProfile;
    private int _heightMax;
    private BlackyWorldBiomes _biomes;
    public BlackyWorldProceduralTerritory(BlackyWorld world, BlackyWorldConfig worldConfig)
    {
        _worldProfile = AtlasModsManager.GetDirect<TemplateWorldProfileData>(worldConfig.IdPerfiladorMundo);
        ConfiguracionGeograficaData configuracionGeografica = new ConfiguracionGeograficaData(_worldProfile.perfilGeografico);
        _heightMax = configuracionGeografica.AlturaMaxima;
        _world = world;
        _config = worldConfig;
        _worldSeed = new BlackyWorldSeed(_config.WorldSeed);
        _geography = new BlackyWorldGeography(worldConfig, _worldSeed, _world.Services.HeightMapWorld, configuracionGeografica);

        _biomes = new BlackyWorldBiomes(worldConfig, _worldSeed, _world.Services.HeightMapWorld, _worldProfile);


        string timeT = "Territorio";
        string timePlot = "plot";
        GenerateGeography();

        Test();
        //PerformanceTimer.Instance.Start(timeT);
        //GenerateTerritory();
        //PerformanceTimer.Instance.Stop(timeT);
        //PerformanceTimer.Instance.Print(timeT);

        //PerformanceTimer.Instance.Start(timePlot);
        //_world.Services.TerrainDataLienzo.PlotAllMap();
        //PerformanceTimer.Instance.Stop(timePlot);
        //PerformanceTimer.Instance.Print(timePlot);
        world.Streaming.chunkManagerLocal.Teleport(new Vector2(0, 0)); // esto hace que se renderice el chunk central y se carguen los chunks alrededor
    }
    private void GenerateGeography()
    {
        _geography.Generate();

        // debug: exporta el mapa de terreno generado a un archivo txt y una imagen png en el escritorio
        //string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".txt");
        //string imgPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".png");
        //string filePath2 = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoOri" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".txt");
        //_world.Services.HeightMapWorld.ExportHeightsToTxt(filePath2);
        //_geography.ExportDebugTxt(filePath); // debug: genera un txt con el mapa de terreno generado
        //_geography.ExportDebugPng(imgPath); // debug: genera una imagen con el mapa de terreno generado
    }
    public void Test()
    {
        //_biomes.MeasureGenerateChunks(32);
        //const int chunkSize = 32;
        //var chunks = _biomes.EnumerateChunks(chunkSize).GetEnumerator();
        //long biomeTicks = 0;
        //long otherTicks = 0;

        //while (true)
        //{
        //    long start = Stopwatch.GetTimestamp();
        //    bool hasChunk = chunks.MoveNext();
        //    biomeTicks += Stopwatch.GetTimestamp() - start;

        //    if (!hasChunk)
        //        break;

        //    var (chunkX, chunkY, region) = chunks.Current;

        //    start = Stopwatch.GetTimestamp();

        //    // Aquí va el cuerpo actual del foreach para procesar `region`.

        //    otherTicks += Stopwatch.GetTimestamp() - start;
        //}

        //GD.Print($"Generar BiomeRegion: {biomeTicks * 1000.0 / Stopwatch.Frequency:F0} ms");
        //GD.Print($"Preparar/escribir chunks: {otherTicks * 1000.0 / Stopwatch.Frequency:F0} ms");
    }
    public void GenerateTerritory()
    {
        const int chunkSize = 32;
        const int chunksPerBatch = 64;
        int heightBufferCount = Math.Max(_heightMax, 3) + 1;
        var terrainPaletteIds = new Dictionary<long, ushort>();
        var terrainData = _world.Services.TerrainDataLienzo;
        var workBatch = new List<(BlackyChunkCoord coord, BiomeRegion region, BlackyChunkData<SerializerCellGeneric> chunkData, BlackyChunkHeightData<SerializerCellGeneric>[] heightBuffers)>(chunksPerBatch);

        void CachePaletteId(long terrainId)
        {
            if (terrainPaletteIds.ContainsKey(terrainId)) return;

            TerrainBaseData data = AtlasModsManager.GetDirect<TerrainBaseData>(terrainId);
            terrainPaletteIds.Add(terrainId, terrainData.GetTerrainPalettePersistenceId(data));
        }

        void ProcessBatch()
        {
            if (workBatch.Count == 0) return;

            // La generación de BiomeRegion y la preparación de chunks ya terminó en el hilo actual.
            // Cada worker escribe posiciones distintas en arrays ya creados y solo lee el cache de paleta.
            Parallel.For(0, workBatch.Count, i =>
            {
                var item = workBatch[i];
                BiomeRegion region = item.region;
                var heightBuffers = item.heightBuffers;

                for (int y = 0; y < region.Height; y++)
                {
                    int rowStart = y * region.Width;

                    for (int x = 0; x < region.Width; x++)
                    {
                        ref readonly BiomeCell cell = ref region.Cells[rowStart + x];
                        if (cell.Bioma == null) continue;
                        bool isBorder = cell.IsHeightBorder || cell.IsMapBorder || cell.IsHeightTransition;
                        int height = cell.HeightLevel;

                        ref var storedCell = ref heightBuffers[height].GetCell(x, y);
                        storedCell.id = terrainPaletteIds[cell.IdTerreno];
                        storedCell.isBorder = isBorder;

                        if (cell.IsHeightTransition && height > 0)
                        {
                            int lowerHeight = height - 1;
                            ref var lowerCell = ref heightBuffers[lowerHeight].GetCell(x, y);
                            lowerCell.id = terrainPaletteIds[cell.HeightLowerTerrainId];
                            lowerCell.isBorder = false;
                        }
                    }
                }
            });

            // Operaciones sobre los diccionarios/regiones compartidos: una vez por chunk y en serie.
            foreach (var item in workBatch)
                terrainData.CompleteGenerationChunk(item.coord, item.chunkData);

            workBatch.Clear();
        }

        foreach (var (chunkX, chunkY, region) in _biomes.EnumerateChunks(chunkSize))
        {
            var coord = new BlackyChunkCoord(chunkX, chunkY);
            var chunkData = terrainData.GetOrCreateChunkDataForGeneration(coord);
            int usedHeightMask = 0;

            // Prepara paletas y capas en serie antes de iniciar los workers.
            for (int i = 0; i < region.Cells.Length; i++)
            {
                ref readonly BiomeCell cell = ref region.Cells[i];
                if (cell.Bioma == null) continue;

                CachePaletteId(cell.IdTerreno);
                int height = cell.HeightLevel;
                usedHeightMask |= 1 << height;

                if (cell.IsHeightTransition && height > 0)
                {
                    CachePaletteId(cell.HeightLowerTerrainId);
                    usedHeightMask |= 1 << (height - 1);
                }
            }

            var heightBuffers = new BlackyChunkHeightData<SerializerCellGeneric>[heightBufferCount];
            for (int height = 0; height < heightBufferCount; height++)
            {
                if ((usedHeightMask & (1 << height)) != 0)
                    heightBuffers[height] = chunkData.GetOrCreateHeight(height);
            }

            workBatch.Add((coord, region, chunkData, heightBuffers));
            if (workBatch.Count >= chunksPerBatch)
                ProcessBatch();
        }

        ProcessBatch();
    }

}
