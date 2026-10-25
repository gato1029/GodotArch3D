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
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Territory;

public class BlackyWorldProceduralTerritory
{

    private readonly struct ResourceSpawn
    {
        public readonly ushort PaletteId;
        public readonly Vector2I Position;
        public readonly int Height;

        public ResourceSpawn(ushort paletteId, Vector2I position, int height)
        {
            PaletteId = paletteId;
            Position = position;
            Height = height;
        }
    }

    private readonly BlackyWorld _world;
    private BlackyWorldSeed _worldSeed;    
    private BlackyWorldConfig _config;
    private BlackyWorldGeography _geography;
    private BlackyWorldGeographyVertical _geographyVertical;
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
        _geographyVertical = new BlackyWorldGeographyVertical(worldConfig, _worldSeed, _world.Services.HeightMapWorld, configuracionGeografica)
        {
            HighAtPositiveY = true,   // altura 3 arriba (Y+), altura 0 abajo (Y-)
            BorderMarginLeft = 0,
            BorderMarginRight = 0,
            BorderMarginTop = 0,
            BorderMarginBottom = 4,
            Height1To2Margin = 4
        };

        _biomes = new BlackyWorldBiomes(worldConfig, _worldSeed, _world.Services.HeightMapWorld, _worldProfile);
       
       

        string timeT = "Territorio";
        string timePlot = "plot";
        GenerateGeography();

        //Test();
        PerformanceTimer.Instance.Start(timeT);
        GenerateTerritory();
        PerformanceTimer.Instance.Stop(timeT);
        PerformanceTimer.Instance.Print(timeT);

        PerformanceTimer.Instance.Start(timePlot);
        _world.Services.TerrainDataLienzo.PlotAllMap();
        _world.Services.SuperficiesDataLienzo.PlotAllMap();
        _world.Services.AdornosDataLienzo.PlotAllMap();

        // Deja los chunks de colisión listos para las consultas paralelas del sistema de colisión.
        _world.State.StaticSpatialTerrain.Flush();

        PerformanceTimer.Instance.Stop(timePlot);
        PerformanceTimer.Instance.Print(timePlot);
        world.Streaming.chunkManagerLocal.Teleport(new Vector2(0, 0)); // esto hace que se renderice el chunk central y se carguen los chunks alrededor
    }

    /// <summary>
    /// Elige una decoración usando Probability (0..100) como porcentaje de aparición.
    /// Si la suma es menor que 100, el resto de las celdas queda sin decoración.
    /// Si la suma supera 100, se normaliza para que todas tengan oportunidad.
    /// </summary>
    private static bool TryPickDecoration(List<DecorationEntry> entries, Random rng, out long decorationId)
    {
        decorationId = 0;
        if (entries == null || entries.Count == 0) return false;

        float total = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry != null && entry.Probability > 0f)
                total += entry.Probability;
        }

        if (total <= 0f) return false;

        float roll = rng.NextSingle() * 100f;

        // Lo que no llega a 100 es probabilidad de que no aparezca nada.
        if (total < 100f && roll >= total) return false;

        // Si se pasa de 100, se escala para que todas las entradas tengan oportunidad.
        float scale = total > 100f ? 100f / total : 1f;
        float accumulated = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry == null || entry.Probability <= 0f)
                continue;

            accumulated += entry.Probability * scale;
            if (roll < accumulated)
            {
                decorationId = entry.idDecoracion;
                return decorationId > 0;
            }
        }

        return false;
    }
    // <summary>
    /// Elige una entrada por Probability (0..100 = porcentaje de aparición). Lo que falte para 100 es la probabilidad
    /// de que no aparezca nada; si la suma supera 100 se normaliza. Devuelve false si no sale ninguna.
    /// Si tu tipo de entrada es un struct, quita "where T : class" y los "if (entry == null) continue;".
    /// </summary>
    private static bool TryPickByProbability<T>(List<T> entries, Random rng,
        Func<T, float> getProbability, Func<T, long> getId, out long pickedId) where T : class
    {
        pickedId = 0;
        if (entries == null || entries.Count == 0) return false;

        float total = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry == null) continue;
            float p = getProbability(entry);
            if (p > 0f) total += p;
        }

        if (total <= 0f) return false;

        float roll = rng.NextSingle() * 100f;
        if (total < 100f && roll >= total) return false;   // cae en la parte de "nada"

        float scale = total > 100f ? 100f / total : 1f;
        float accumulated = 0f;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry == null) continue;
            float p = getProbability(entry);
            if (p <= 0f) continue;

            accumulated += p * scale;
            if (roll < accumulated)
            {
                pickedId = getId(entry);
                return pickedId > 0;
            }
        }

        return false;
    }

    private void GenerateDecorations()
    {
        // genera decoracion, como cesped, manchas, u otras cosas que no tienen interaccion con el mundo

    }
    private void GenerateGeography()
    {
        //_geography.Generate();
        _geographyVertical.Generate();
        // debug: exporta el mapa de terreno generado a un archivo txt y una imagen png en el escritorio
        //string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".txt");
        string imgPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".png");
        //string filePath2 = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoOri" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".txt");
        //_world.Services.HeightMapWorld.ExportHeightsToTxt(filePath2);
      //  _geography.ExportDebugTxt(filePath); // debug: genera un txt con el mapa de terreno generado
        _geographyVertical.ExportDebugPng(imgPath); // debug: genera una imagen con el mapa de terreno generado
    }

    public void GenerateTerritory()
    {
        const int chunkSize = 32;          // debe coincidir con el chunk del juego
        const int chunksPerBatch = 64;
        int heightBufferCount = Math.Max(_heightMax, 3) + 1;
        var terrainPaletteIds = new Dictionary<long, ushort>();
        var surfacePaletteIds = new Dictionary<long, ushort>();
        var decorationPaletteIds = new Dictionary<long, ushort>();
        var resourcePaletteIds = new Dictionary<long, ushort>();
        var resourceBiomesCached = new HashSet<BiomaData>(ReferenceEqualityComparer.Instance);   // biomas cuyos recursos ya están en paleta
        var terrainData = _world.Services.TerrainDataLienzo;
        var surfaceData = _world.Services.SuperficiesDataLienzo;
        var decorationData = _world.Services.AdornosDataLienzo;
        var painter = _world.Services.ResourcePainter;

        var workBatch = new List<(
            BlackyChunkCoord coord,
            BiomeRegion region,
            BlackyChunkData<SerializerCellGeneric> chunkData,
            BlackyChunkHeightData<SerializerCellGeneric>[] heightBuffers,
            BlackyChunkData<SerializerCellGeneric> surfaceChunkData,                  // null si el chunk no tiene transiciones
            BlackyChunkHeightData<SerializerCellGeneric>[] surfaceBuffers,
            BlackyChunkData<SerializerCellGeneric> decorationChunkData,               // null si el chunk no tiene decoraciones
            BlackyChunkHeightData<SerializerCellGeneric>[] decorationBuffers)>(chunksPerBatch);

        _biomes.HeightReadsAreThreadSafe = true;

        void CachePaletteId(long terrainId)
        {
            if (terrainPaletteIds.ContainsKey(terrainId)) return;

            TerrainBaseData data = AtlasModsManager.GetDirect<TerrainBaseData>(terrainId);
            terrainPaletteIds.Add(terrainId, terrainData.GetTerrainPalettePersistenceId(data));
        }

        void CacheSurfacePaletteId(long surfaceId)
        {
            if (surfacePaletteIds.ContainsKey(surfaceId)) return;

            SuperficieData data = AtlasModsManager.GetDirect<SuperficieData>(surfaceId);
            surfacePaletteIds.Add(surfaceId, surfaceData.GetSuperficiePalettePersistenceId(data));
        }

        void CacheDecorationPaletteId(long decorationId)
        {
            if (decorationPaletteIds.ContainsKey(decorationId)) return;

            DecorationData data = AtlasModsManager.GetDirect<DecorationData>(decorationId);
            decorationPaletteIds.Add(decorationId, decorationData.GetDecorationPalettePersistenceId(data));
        }

        void CacheResourcePaletteId(long resourceId)
        {
            if (resourcePaletteIds.ContainsKey(resourceId)) return;

            var res = AtlasModsManager.GetDirect<ResourceSourceData>(resourceId);
            ushort idInternal = BlackyPalletesPersistence.resourcesPalette.GetIdPersistence(res.nameMod, resourceId, out _);
            resourcePaletteIds.Add(resourceId, idInternal);
        }

        void ProcessBatch()
        {
            if (workBatch.Count == 0) return;

            // Recursos elegidos por cada chunk del lote (null = ninguno). Se crean en serie al final.
            var resourceSpawns = new List<ResourceSpawn>[workBatch.Count];

            // Cada worker escribe posiciones distintas en arrays ya creados y solo lee los caches de paleta.
            Parallel.For(0, workBatch.Count, i =>
            {
                var item = workBatch[i];
                BiomeRegion region = item.region;
                var heightBuffers = item.heightBuffers;
                var surfaceBuffers = item.surfaceBuffers;
                var decorationBuffers = item.decorationBuffers;
                int chunkHash = (item.coord.X * 73856093) ^ (item.coord.Y * 19349663);
                var decorationRng = _worldSeed.GetRng("DecSuperficie", chunkHash);
                Random resourceRng = null;                    // se crea solo si el chunk tiene recursos
                List<ResourceSpawn> spawns = null;

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
                        // Superficie: solo en transiciones, igual en todas las alturas.
                        bool hasSurface = cell.IdSuperficie != 0 && cell.IsTransition;

                        if (hasSurface)
                        {
                            ref var surfCell = ref surfaceBuffers[height].GetCell(x, y);
                            surfCell.id = surfacePaletteIds[cell.IdSuperficie];
                            surfCell.isBorder = cell.IsBiomeBorder;
                        }

                        if (cell.IsHeightTransition && height > 0)
                        {
                            int lowerHeight = height - 1;

                            ref var lowerCell = ref heightBuffers[lowerHeight].GetCell(x, y);
                            lowerCell.id = terrainPaletteIds[cell.HeightLowerTerrainId];
                            lowerCell.isBorder = false;
                        }

                        if (cell.HasDecoration && decorationBuffers != null)
                        {
                            // Elige la decoración según su probabilidad de aparición (0..100).
                            if (TryPickDecoration(cell.Bioma.idDecoraciones, decorationRng, out long decorationId)
                                && decorationBuffers[height] != null
                                && decorationPaletteIds.TryGetValue(decorationId, out ushort decorationPaletteId))
                            {
                                ref var decoCell = ref decorationBuffers[height].GetCell(x, y);
                                decoCell.id = decorationPaletteId;
                            }
                        }

                        if (cell.HasResource)
                        {
                            // Elige el recurso por Probability (0..100); lo que falte para 100 = no aparece nada.
                            // Solo se ANOTA: la creación real ocurre en serie al cerrar el lote.
                            resourceRng ??= _worldSeed.GetRng("RecursoSel", chunkHash);

                            if (TryPickByProbability(cell.Bioma.idRecursos, resourceRng,
                                    static e => e.Probability, static e => e.ResourceSourceId, out long resourceId)
                                && resourcePaletteIds.TryGetValue(resourceId, out ushort resourcePaletteId))
                            {
                                spawns ??= new List<ResourceSpawn>();
                                spawns.Add(new ResourceSpawn(
                                    resourcePaletteId,
                                    new Vector2I(region.MinX + x, region.MinY + y),   // coordenadas de MUNDO (la región ya viene en mundo)
                                    height));
                            }
                        }
                    }
                }

                resourceSpawns[i] = spawns;
            });

            // Operaciones sobre los diccionarios/regiones compartidos: una vez por chunk y en serie.
            foreach (var item in workBatch)
            {
                terrainData.CompleteGenerationChunk(item.coord, item.chunkData);
                if (item.surfaceChunkData != null)
                    surfaceData.CompleteGenerationChunk(item.coord, item.surfaceChunkData);
                if (item.decorationChunkData != null)
                    decorationData.CompleteGenerationChunk(item.coord, item.decorationChunkData);
            }

            // Creación de recursos en serie (el painter y la paleta no son seguros entre hilos).
            for (int n = 0; n < resourceSpawns.Length; n++)
            {
                var list = resourceSpawns[n];
                if (list == null) continue;

                for (int k = 0; k < list.Count; k++)
                {
                    var s = list[k];
                    painter.InternalExecuteCreation(s.PaletteId, s.Position, s.Height, out _, false);
                }
            }

            workBatch.Clear();
        }

        // Lista de chunks del mundo, recorrida por lotes
        _biomes.GetChunkRange(chunkSize, out int minCx, out int maxCx, out int minCy, out int maxCy);
        int chunksWide = maxCx - minCx + 1;
        long totalChunks = (long)chunksWide * (maxCy - minCy + 1);
        var regions = new BiomeRegion[chunksPerBatch];

        for (long batchStart = 0; batchStart < totalChunks; batchStart += chunksPerBatch)
        {
            int count = (int)Math.Min(chunksPerBatch, totalChunks - batchStart);
            long start = batchStart;

            // 1) Biomas del lote EN PARALELO
            Parallel.For(0, count, i =>
            {
                long index = start + i;
                int cx = minCx + (int)(index % chunksWide);
                int cy = minCy + (int)(index / chunksWide);
                regions[i] = _biomes.GenerateChunkSerial(cx, cy, chunkSize);
            });

            // 2) Preparación en serie (paletas, chunkData, capas)
            for (int n = 0; n < count; n++)
            {
                long index = start + n;
                var coord = new BlackyChunkCoord(minCx + (int)(index % chunksWide), minCy + (int)(index / chunksWide));
                BiomeRegion region = regions[n];
                var chunkData = terrainData.GetOrCreateChunkDataForGeneration(coord);
                int usedHeightMask = 0;
                int usedSurfaceMask = 0;
                int usedDecorationMask = 0;
                var decorationIdsToCache = new HashSet<long>();

                for (int i = 0; i < region.Cells.Length; i++)
                {
                    ref readonly BiomeCell cell = ref region.Cells[i];
                    if (cell.Bioma == null) continue;

                    CachePaletteId(cell.IdTerreno);
                    int height = cell.HeightLevel;
                    usedHeightMask |= 1 << height;

                    if (cell.IdSuperficie != 0 && cell.IsTransition)
                    {
                        CacheSurfacePaletteId(cell.IdSuperficie);
                        usedSurfaceMask |= 1 << height;
                    }

                    if (cell.IsHeightTransition && height > 0)
                    {
                        CachePaletteId(cell.HeightLowerTerrainId);
                        usedHeightMask |= 1 << (height - 1);
                    }

                    if (cell.HasDecoration)
                    {
                        usedDecorationMask |= 1 << height;

                        var entries = cell.Bioma.idDecoraciones;
                        if (entries != null)
                        {
                            for (int entryIndex = 0; entryIndex < entries.Count; entryIndex++)
                            {
                                var entry = entries[entryIndex];
                                if (entry == null || entry.idDecoracion <= 0 || entry.Probability <= 0f)
                                    continue;

                                decorationIdsToCache.Add(entry.idDecoracion);
                            }
                        }
                    }

                    // Recursos: se cachean los ids de paleta de TODAS las entradas del bioma, una sola vez por bioma
                    // (en el paralelo solo se lee el diccionario).
                    if (cell.HasResource && resourceBiomesCached.Add(cell.Bioma))
                    {
                        var resEntries = cell.Bioma.idRecursos;
                        if (resEntries != null)
                        {
                            for (int e = 0; e < resEntries.Count; e++)
                            {
                                var resEntry = resEntries[e];
                                if (resEntry == null || resEntry.ResourceSourceId <= 0 || resEntry.Probability <= 0f)
                                    continue;

                                CacheResourcePaletteId(resEntry.ResourceSourceId);
                            }
                        }
                    }
                }

                foreach (long decorationId in decorationIdsToCache)
                    CacheDecorationPaletteId(decorationId);

                var heightBuffers = new BlackyChunkHeightData<SerializerCellGeneric>[heightBufferCount];
                for (int height = 0; height < heightBufferCount; height++)
                {
                    if ((usedHeightMask & (1 << height)) != 0)
                        heightBuffers[height] = chunkData.GetOrCreateHeight(height);
                }

                // Capas de superficie solo si el chunk tiene alguna transición
                BlackyChunkData<SerializerCellGeneric> surfaceChunkData = null;
                BlackyChunkHeightData<SerializerCellGeneric>[] surfaceBuffers = null;
                if (usedSurfaceMask != 0)
                {
                    surfaceChunkData = surfaceData.GetOrCreateChunkDataForGeneration(coord);
                    surfaceBuffers = new BlackyChunkHeightData<SerializerCellGeneric>[heightBufferCount];
                    for (int height = 0; height < heightBufferCount; height++)
                    {
                        if ((usedSurfaceMask & (1 << height)) != 0)
                            surfaceBuffers[height] = surfaceChunkData.GetOrCreateHeight(height);
                    }
                }

                BlackyChunkData<SerializerCellGeneric> decorationChunkData = null;
                BlackyChunkHeightData<SerializerCellGeneric>[] decorationBuffers = null;
                if (usedDecorationMask != 0)
                {
                    decorationChunkData = decorationData.GetOrCreateChunkDataForGeneration(coord);
                    decorationBuffers = new BlackyChunkHeightData<SerializerCellGeneric>[heightBufferCount];
                    for (int height = 0; height < heightBufferCount; height++)
                    {
                        if ((usedDecorationMask & (1 << height)) != 0)
                            decorationBuffers[height] = decorationChunkData.GetOrCreateHeight(height);
                    }
                }

                workBatch.Add((coord, region, chunkData, heightBuffers, surfaceChunkData, surfaceBuffers, decorationChunkData, decorationBuffers));
            }

            // 3) Rellenado en paralelo + cierre en serie
            ProcessBatch();
        }
    }

}
