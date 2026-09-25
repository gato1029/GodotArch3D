

using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Generation;
using GodotEcsArch.sources.BlackyEngine.Generation.Biomes;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
using GodotEcsArch.sources.BlackyEngine.Generation.Resources;
using GodotEcsArch.sources.BlackyEngine.Generation.Terrain;
using GodotEcsArch.sources.BlackyEngine.Services;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
using GodotEcsArch.sources.BlackyEngine.Services.Spawn;
using GodotEcsArch.sources.BlackyEngine.Simulation;
using GodotEcsArch.sources.BlackyEngine.State;

using GodotEcsArch.sources.BlackyEngine.Streaming;
using GodotEcsArch.sources.BlackyTiles.Entities;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.Core;

public enum BlackyWorldTypeDetail
{
    MUNDO,
    HABITACION,
    MAZMORRA
}
public sealed class BlackyWorld : IDisposable
{
    public BlackyWorldTypeDetail WorldTypeDetail { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; } = true;

    // =========================================
    // Root Modules
    // =========================================

    public BlackyWorldConfig Config { get; }
    public BlackyWorldState State { get; }
    public BlackyWorldSimulation Simulation { get; }
    public BlackyWorldServices Services { get; }
    public BlackyWorldGeneration Generation { get; } // esto ya no se usa
    public BlackyWorldStreaming Streaming { get; }
    public BlackyWorldProceduralGeneration Procedural { get; }
    // =========================================
    // Public Facade Shortcuts
    // =========================================

    public FlecsManager Flecs => Simulation.Flecs;
    public SimulationTick Tick => Simulation.Tick;

    public BlackyCharacterCreator Characters => Services.Characters;
    
    public BlackyResourcesCreator Resources => Services.ResourcePainter;
    public BlackyBuildingCreator Buildings => Services.BuildingPainter;
    public BlackyHeightSystem Heights => Services.HeightMapWorld;

    public BlackyTileRenderSystem TileRenderer => Services.TileRenderer;
    public BlackyEntityRenderSystem EntityRenderer => Services.EntityRenderer;
    public BlackyOccupancyRendererSystem OccupancyRenderer => Services.OccupancyRenderer;

    public BlackyWorldBiomeMap Biomes => Generation.BiomeMap;
    public BlackyWorldTerrainGenerator TerrainGenerator => Generation.TerrainGenerator;
    public BlackyResourcesGenerator ResourceGenerator => Generation.ResourceGenerator;
    public BlackyResourcesPostProcessor ResourcePostProcessor => Generation.ResourcePostProcessor;
    public BlackyWorldTileMapper TileMapper => Generation.TileMapper;

    public ChunkManagerBase ChunkManager => Streaming.chunkManagerLocal;


    // =========================================
    // Constructor
    // =========================================

    public BlackyWorld(
        string name,
        BlackyWorldTypeDetail worldTypeDetail,
        int chunkSize,
        int heightCount,
        int worldSeed,
        Vector2I mapSize,bool isLoad = false)
    {
        

        Name = name;
        WorldTypeDetail = worldTypeDetail;        

        Config = new BlackyWorldConfig(
            name,
            worldTypeDetail,
            chunkSize,
            heightCount,
            worldSeed,
            mapSize);


        State = new BlackyWorldState(this);
        Simulation = new BlackyWorldSimulation(this);
        Streaming = new BlackyWorldStreaming(this);
        Services = new BlackyWorldServices(this);
        BlackyWorldRegistry.Instance.AddWorld(name, this, true);
        RTSSelectionManager.Instance.SetWorld(this);

        //Generation = new BlackyWorldGeneration(this, Services); esto ya no funcionara luego quitar         

        if (!isLoad)
        {
            // si es nuevo hacemos procedural etc
            Procedural = new BlackyWorldProceduralGeneration(this, Config);
            DebugBoot();
        }
        else
        {
            BlackyLoadData.LoadAll(this);
            Streaming.chunkManagerLocal.Teleport(new Vector2(0, 0)); // esto hace que se renderice el chunk central y se carguen los chunks alrededor
        }
      //  DebugBootStressTest(10000);
    }
 
    // =========================================
    // Runtime
    // =========================================

    public void Update(float delta)
    {
        if (!IsActive)
            return;

        Simulation.Update(delta);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
    }

    // =========================================
    // Dispose
    // =========================================

    public void Dispose()
    {
        IsActive = false;
        ClearRenders();
        Services.Dispose();
        //Simulation.Dispose();
        State.Dispose();
        WireShape.Clear();
        Flecs.Destroy();        
    }
    private void ClearRenders()
    {
        // para limpiar entidades que quedaron al aire        
        var query = Flecs.WorldFlecs.QueryBuilder<RenderGPUComponent>()
            .With<PersistEntityTag>()
            .Build();

        // 2. Iteramos de forma ultra-performante por cada entidad encontrada
        query.Each((Entity ent, ref RenderGPUComponent gpu) =>
        {
            if (gpu.instance != -1) // quedo sucia debemos limpiarla
            {
                AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
                gpu.rid = default;
                gpu.instance = -1;
            }
        });

    }

    // =========================================
    // Debug only
    // =========================================

    private void DebugBoot()
    {
        var e = Characters.InternalExecuteCreation(1787768744605000,1, new Vector2(0, 0)); // principal

        SpawnEnemiesAroundPlayer(5000,20);
        
        //var ee = Characters.InternalExecuteCreation(1788369074799000,1, new Vector2(2, 0)); // enemigos
        //ee.Set(new MoveTargetComponent(new Vector2(60, 0)));
        
    }
    private void SpawnEnemiesAroundPlayer(int targetEnemies, int baseRadius = 100, int minSpacing = 3)
    {
        Vector2 playerWorldPos = Vector2.Zero; // O la posición real de tu jugador
        int playerTileX = 0;
        int playerTileY = 0;

        int spawned = 0;
        int currentRadius = baseRadius;
        int maxRadius = baseRadius + 400; // Límite máximo de expansión si faltan enemigos
        int ringStep = 6;                 // Grosor de cada anillo de expansión

        HashSet<Vector2> occupiedTiles = new HashSet<Vector2>();

        // Generar desde el radio base hacia afuera en anillos concéntricos
        for (int r = currentRadius; r <= maxRadius && spawned < targetEnemies; r += ringStep)
        {
            int pointsOnRing = Math.Max(12, r * 3); // Densidad proporcional a la circunferencia del anillo

            for (int i = 0; i < pointsOnRing && spawned < targetEnemies; i++)
            {
                float angle = i * (2f * MathF.PI / pointsOnRing);
                int tx = playerTileX + Mathf.RoundToInt(r * MathF.Cos(angle));
                int ty = playerTileY + Mathf.RoundToInt(r * MathF.Sin(angle));

                Vector2 tileCoord = new Vector2(tx, ty);
                if (occupiedTiles.Contains(tileCoord)) continue;

                // Validación estricta anti-solapamiento basada en la distancia mínima en tiles
                bool tooClose = false;
                foreach (var occupied in occupiedTiles)
                {
                    if (MathF.Abs(occupied.X - tx) < minSpacing && MathF.Abs(occupied.Y - ty) < minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                occupiedTiles.Add(tileCoord);
                Vector2 worldPos = TilesHelper.TilePositionToWorldPosition(tx, ty);

                var enemy = Characters.InternalExecuteCreation(1788369074799000,1, worldPos);
                //enemy.Set(new MoveTargetComponent(playerWorldPos));

                spawned++;
            }
        }
    }
    private void DebugBootStressTest(int targetEnemies = 1000)
    {
        var player = Characters.InternalExecuteCreation(1787768744605000,1, new Vector2(0, 0)); // principal
        Vector2 playerWorldPos = new Vector2(0, 0);

        int playerTileX = 0;
        int playerTileY = 0;

        int spawned = 0;
        int maxRadius = 490; // Radio inicial del anillo más grande
        int minRadius = 40; // Radio límite del anillo más pequeño
        int ringStep =10;   // Separación de 2 tiles entre anillos

        HashSet<Vector2> occupiedTiles = new HashSet<Vector2>();

        // Generar desde el anillo más grande hacia los más pequeños
        for (int r = maxRadius; r >= minRadius && spawned < targetEnemies; r -= ringStep)
        {
            int pointsOnRing = r * 4; // Estimación de la circunferencia en tiles para el radio actual
            for (int i = 0; i < pointsOnRing && spawned < targetEnemies; i++)
            {
                float angle = i * (2f * MathF.PI / pointsOnRing);
                int tx = playerTileX + Mathf.RoundToInt(r * MathF.Cos(angle));
                int ty = playerTileY + Mathf.RoundToInt(r * MathF.Sin(angle));

                Vector2 tileCoord = new Vector2(tx, ty);
                if (occupiedTiles.Contains(tileCoord)) continue;

                // Validación estricta para garantizar una separación mínima de 2 tiles entre cualquier unidad
                bool tooClose = false;
                foreach (var occupied in occupiedTiles)
                {
                    if (MathF.Abs(occupied.X - tx) < 3 && MathF.Abs(occupied.Y - ty) < 3)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                occupiedTiles.Add(tileCoord);
                Vector2 worldPos = TilesHelper.TilePositionToWorldPosition(tx, ty);

                var enemy = Characters.InternalExecuteCreation(1788369074799000,1, worldPos);
                enemy.Set(new MoveTargetComponent(playerWorldPos));

                spawned++;
            }
        }
    }
}


//using Godot;
//using GodotEcsArch.sources.BlackyEngine.Generation.Biomes;
//using GodotEcsArch.sources.BlackyEngine.Generation.Resources;
//using GodotEcsArch.sources.BlackyEngine.Generation.Terrain;
//using GodotEcsArch.sources.BlackyEngine.Services.Paint;
//using GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
//using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
//using GodotEcsArch.sources.BlackyEngine.Services.Spawn;
//using GodotEcsArch.sources.BlackyEngine.Simulation;
//using GodotEcsArch.sources.BlackyEngine.Spatial;
//using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
//using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;
//using GodotEcsArch.sources.BlackyTiles.Commands;
//using GodotEcsArch.sources.BlackyTiles.Entities;
//using GodotEcsArch.sources.BlackyTiles.Systems;
//using GodotEcsArch.sources.Flecs;
//using GodotEcsArch.sources.managers.Chunks;
//using GodotEcsArch.sources.managers.Mods;
//using GodotEcsArch.sources.managers.Profiler;
//using GodotEcsArch.sources.utils;
//using GodotEcsArch.sources.WindowsDataBase;
//using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
//using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
//using GodotFlecs.sources.Flecs;
//using GodotFlecs.sources.Flecs.Components;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.CompilerServices;


//namespace GodotEcsArch.sources.BlackyEngine.Core;
//public class BlackyWorld
//{
//    public bool isActive { get; private set; } = true;
//    // --- EL MOTOR ---
//    public FlecsManager flecsManager { get; private set; }
//    public BlackyTilePalette tilesPalette { get; private set; }

//    // --- LAS COLISIONES (Viven aquí) ---
//    public FastSpatialHash DynamicHash { get; private set; }
//    public StaticSpatialGridOptimized StaticSpatial { get; private set; }

//    public BlackyChunkedBitGrid GridMove { get; private set; }
//    // Sistema de Ocupacion

//    // sistemas Renderizado y datos
//    public BlackyChunkRenderData RenderData { get; }
//    public BlackyTileRenderSystem TileRenderSystem { get; }
//    public BlackyEntityRenderSystem EntityRenderSystem { get; }
//    public BlackySpatialEntityMap SpatialEntityMap { get; }
//    public BlackyOccupancyRendererSystem blackyOccupancyRendererSystem { get; }

//    // Sistemas del mundo
//    public BlackyCharacterCreator characterCreator { get; }
//    public BlackyTerrainSystem Terrain { get; }
//    public BlackyResourcesSourceSystem Resources { get; }
//    public BlackyBuildingSystem Building { get; }
//    public BlackyHeightSystem Heights { get; }
//    public BlackyWorldBiomeMap biomeMap { get; }

//    // Generadores procedurales
//    public BlackyWorldTerrainGenerator Generator { get; }
//    public BlackyResourcesGenerator GeneratorResources { get; }
//    public BlackyResourcesPostProcessor postProcessorResource { get; }

//    public BlackyWorldTileMapper tileMapper { get; }

//    private readonly ChunkManagerBase chunkManager;
//    public int WorldSeed { get; }
//    public int ChunkSize;
//    public Vector2I MapSize { get; set; }
//    public Vector2I MinChunk { get; private set; }
//    public Vector2I MaxChunk { get; private set; }    
//    public SimulationTick simulationTick { get; private set; }
//    public BlackyWorld(
//        int chunkSize,
//        int heightCount,
//        int worldSeed,
//        Vector2I mapSize,
//        BlackyChunkOccupancyMap occupancyMap,
//        ChunkManagerBase chunkManager)
//    {

//        AtlasTexturesModsManager.Instance.FirstLoad();

//        DataBaseManager.Instance.LoadCurrentDataBase(); // esto luego tiene que salir

//        simulationTick = new SimulationTick{ 
//            FixedDelta = 0.1f, 
//            Accumulator = 0, 
//            TickCount = 0 };

//        var correccionMap = CorrectMapSize(mapSize); // corrige el tamaño del mapa para que sea múltiplo de chunkSize y esté dentro de los límites

//        this.chunkManager = chunkManager;
//        ChunkSize = chunkSize;
//        WorldSeed = worldSeed;
//        MapSize = correccionMap;

//        WireShape.Instance.DrawGrid(MapSize.X, MapSize.Y, 16, new Vector2(0, 0), -50, Colors.DarkCyan);

//        int chunksX = Mathf.CeilToInt((float)MapSize.X / ChunkSize);
//        int chunksY = Mathf.CeilToInt((float)MapSize.Y / ChunkSize);

//        int halfX = chunksX / 2;
//        int halfY = chunksY / 2;

//        MinChunk = new Vector2I(-halfX, -halfY);
//        MaxChunk = new Vector2I(halfX - 1, halfY - 1);
//        this.chunkManager.SetBounds(MinChunk, MaxChunk);

//        DynamicHash = new FastSpatialHash(MapSize.X, MapSize.Y,  11000);

//        StaticSpatial = new StaticSpatialGridOptimized(MapSize.X, MapSize.Y, 32, 65536);

//        GridMove = new BlackyChunkedBitGrid(MapSize.X, MapSize.Y, 16);
//        flecsManager = new FlecsManager(NodeMainHelper.node3DMain);

//        tilesPalette = new BlackyTilePalette();
//        flecsManager.WorldFlecs.SetCtx(this);

//        RenderData = new BlackyChunkRenderData(chunkSize, heightCount);    
//        TileRenderSystem = new BlackyTileRenderSystem(flecsManager,RenderData, chunkManager);

//        SpatialEntityMap = new BlackySpatialEntityMap();
//        EntityRenderSystem = new BlackyEntityRenderSystem( SpatialEntityMap, chunkManager);

//        blackyOccupancyRendererSystem = new BlackyOccupancyRendererSystem(flecsManager,chunkManager, occupancyMap);

//        characterCreator = new BlackyCharacterCreator(flecsManager, DynamicHash);
//        biomeMap = new BlackyWorldBiomeMap(worldSeed, chunkSize, WorldType.Continents);

//        Terrain = new BlackyTerrainSystem( RenderData, occupancyMap, TileRenderSystem);

//        Resources = new BlackyResourcesSourceSystem(StaticSpatial,flecsManager, occupancyMap, SpatialEntityMap, EntityRenderSystem,Terrain);

//        Building = new BlackyBuildingSystem(flecsManager,occupancyMap, SpatialEntityMap, EntityRenderSystem, Terrain);

//        Generator = new BlackyWorldTerrainGenerator(worldSeed, chunkSize, biomeMap);

//        Heights = new BlackyHeightSystem(Terrain);

//        tileMapper = new BlackyWorldTileMapper(chunkSize, worldSeed, biomeMap, Generator, Terrain);

//        postProcessorResource  = new BlackyResourcesPostProcessor(worldSeed);


//        //ReglasProcesador();
//        //GeneratorResources = new BlackyResourcesGenerator(Heights, Generator, Resources, postProcessorResource, worldSeed);
//        //GeneradoresProcedurales();
//        //GeneradorRecursos();
//        //chunkManager.OnChunkPreLoadGenerator += ChunkManager_OnChunkPreLoadGenerator;
//        //RenderCommandQueue.Enqueue(new ForceUpdateChunksCommand(chunkManager, new Vector2I(0, 0)));
//        BlackyWorldRegistry.Instance.AddWorld("mundo_principal", this, true);

//        Test();
//        //Test2();
//    }
//    private void Test()
//    {
//        var e = characterCreator.Create(1, new Vector2(10, 0));    // principal

//        var ee = characterCreator.Create(2, new Vector2(5, 0));

//        ee.Set(new MoveTargetComponent(new Vector2(20, 0)));
//    }

//    private void Test2()
//    {
//        int count = 10000;

//        Vector2 target = new Vector2(0, 0);

//        int size = (int)MathF.Sqrt(count); // ~141 x 141

//        float spacing = 1f; // distancia entre unidades
//        float jitter = 6f;   // ruido para naturalidad

//        Random rng = new Random();

//        for (int i = 0; i < count; i++)
//        {
//            Vector2 pos = new Vector2(
//                    spacing + i,
//                    spacing + 10
//               );
//            var e = characterCreator.Create(2, pos);

//            e.Set(new MoveTargetComponent(target));
//        }


//    }

//    public static Vector2I CorrectMapSize(Vector2I input)
//    {
//        const int MIN = 64;
//        const int MAX = 4096;
//        const int STEP = 32;

//        int Fix(int value)
//        {
//            // mínimo absoluto
//            if (value < MIN)
//                value = MIN;

//            // redondear hacia arriba al múltiplo de STEP
//            int corrected = ((value + STEP - 1) / STEP) * STEP;

//            // clamp máximo
//            if (corrected > MAX)
//                corrected = MAX;

//            return corrected;
//        }

//        return new Vector2I(
//            Fix(input.X),
//            Fix(input.Y)
//        );
//    }

//    public void SetActive(bool active)
//    {
//        isActive = active;
//    }
//    public void Update(float delta)
//    {
//        if (!isActive) return;
//        flecsManager.Update(delta);
//    }
//    public void Dispose()
//    {
//        isActive = false;
//        flecsManager.Destroy();
//        DynamicHash.Clear();
//        StaticSpatial.Clear();
//    }
//    private void ReglasProcesador()
//    {
//        var treeRule = new BlackyResourcePostRule(ResourceSourceType.Arbol, minDistanceSameType: 2, priority: 1);
//        var stoneRule = new BlackyResourcePostRule(ResourceSourceType.Piedras, 2, 2);
//        var goldRule = new BlackyResourcePostRule(ResourceSourceType.MinaOro, minDistanceSameType: 3, priority: 10);

//        goldRule.SetMinDistanceTo(ResourceSourceType.Arbol, 2);
//        goldRule.SetMinDistanceTo(ResourceSourceType.Piedras, 2);

//        //treeRule.SetMinDistanceTo(ResourceSourceType.Piedras, 5);

//        postProcessorResource.AddRule(treeRule);
//        postProcessorResource.AddRule(goldRule);
//        postProcessorResource.AddRule(stoneRule);
//    }

//    private void GeneradorRecursos()
//    {


//        GeneratorResources.ConfigureNoiseForType(ResourceSourceType.Arbol, WorldSeed+1000, 0.03f); // frecuente
//        GeneratorResources.ConfigureNoiseForType(ResourceSourceType.Piedras, WorldSeed + 2000, 0.05f); //pequeño
//        GeneratorResources.ConfigureNoiseForType(ResourceSourceType.MinaOro, WorldSeed + 3000, 0.02f); // muy raro

//        GeneratorResources.ConfigureHeightGlobal(0).SetMinDistanceToHeight(2, 1);

//        GeneratorResources.ConfigureHeightGlobal(2).SetMinDistanceToHeight(0, 2);
//        GeneratorResources.ConfigureHeightGlobal(2).SetMinDistanceToHeight(3, 5);
//        GeneratorResources.ConfigureHeightGlobal(3).SetMinDistanceToHeight(2, 2);

//        GeneratorResources.ConfigureHeightGlobal(2).DensityThreshold = 0.4f;
//        GeneratorResources.ConfigureHeightGlobal(3).DensityThreshold = 0.5f;

//        GeneratorResources.ConfigureHeightGlobal(2).SetTypeWeight(ResourceSourceType.Arbol, 1);
//        GeneratorResources.ConfigureHeightGlobal(2).SetTypeWeight(ResourceSourceType.Piedras, 0.1f);
//        GeneratorResources.ConfigureHeightGlobal(2).SetTypeWeight(ResourceSourceType.MinaOro, 0.1f);

//        GeneratorResources.ConfigureHeightGlobal(3).SetTypeWeight(ResourceSourceType.Arbol, 1);
//        GeneratorResources.ConfigureHeightGlobal(3).SetTypeWeight(ResourceSourceType.Piedras, 0.1f);

//        MasterDataManager.RegisterAllData<long, TerrainData>();

//        var collection = MasterDataManager.GetAllData<long, TerrainData>();
//        foreach (var item in collection)
//        {
//            foreach (var id in item.idsElevacionResources)
//            {
//                List<ResourceEntry> li = id.Value;
//                foreach (var item2 in li)
//                {
//                    GeneratorResources.AddEntry(id.Key, item.idSave, item2);
//                    GeneratorResources.ConfigureHeight(id.Key, item.idSave);
//                }
//            }
//        }

//        //var listchunks = GetRequiredChunks(1024, 1024);
//        var listchunks = chunkManager.GetExtendedChunks(new Vector2(0, 0)).ToList();
//        foreach (var item in listchunks)
//        {
//            GeneratorResources.GenerateChunk(item);
//        }

//    }


//    public List<Vector2I> GetRequiredChunks(int widthTiles, int heightTiles)
//    {
//        List<Vector2I> result = new();

//        int chunksX = Mathf.CeilToInt((float)widthTiles / ChunkSize);
//        int chunksY = Mathf.CeilToInt((float)heightTiles / ChunkSize);

//        // 🔥 offsets para centrar en (0,0)
//        int startX = -chunksX / 2;
//        int startY = -chunksY / 2;

//        int endX = startX + chunksX;
//        int endY = startY + chunksY;

//        for (int x = startX; x < endX; x++)
//        {
//            for (int y = startY; y < endY; y++)
//            {
//                result.Add(new Vector2I(x, y));
//            }
//        }

//        return result;
//    }
//    private void GeneradoresProcedurales()
//    {


//        string tit = "Tiempo Generacion:";
//        using (new ProfileScope(tit))
//        {
//            //var listchunks = GetRequiredChunks(1024, 1024);
//            var listchunks = chunkManager.GetExtendedChunks(new Vector2(0, 0)).ToList();
//            GD.Print("Cantidad de chunks:"+listchunks.Count);
//            //Dictionary<Vector2I, ushort[,]> borderChunks = new();
//            //Dictionary<Vector2I, ushort[,]> worldChunks = new();
//            //Dictionary<Vector2I, ushort[,]> heightChunks = new();
//            //Dictionary<Vector2I, ushort[,]> borderHeightChunk = new();
//            foreach (var item in listchunks)
//            {



//                //var biomeMapChunk = biomeMap.GetChunkBiomes(item);
//                //var borderMapChunk = biomeMap.GetChunkBorders(item);
//                //var heightMapChunk = Generator.GetChunkHeights(item);
//                //var borderHeightMapChunk = Generator.GetChunkHeightBorders(item);
//                tileMapper.GenerateChunkTileData(item);

//                //worldChunks[item] = biomeMapChunk;
//                //borderChunks[item] = borderMapChunk;
//                //heightChunks[item] = heightMapChunk;
//                //borderHeightChunk[item] = borderHeightMapChunk;


//                // TODO: Esto es solo para debug, luego se debe eliminar
//                //float sizeRealWorld = MeshCreator.PixelsToUnits(16 * chunkManager.chunkDimencion.X);
//                //Vector2 plot = (sizeRealWorld * (Vector2)item) + new Vector2(sizeRealWorld / 2, sizeRealWorld / 2);
//                //WireShape.Instance.DrawSquare(new Vector2(16 * 32, 16 * 32), plot, 25, Colors.Red);


//            }
//        }
//        PerformanceTimer.Instance.Print(tit);
//        //biomeMap.ExportWorldStitched(worldChunks, "D:\\GitKraken\\AssetExternals\\debugs\\world_full.txt");
//        //biomeMap.ExportWorldBordersStitched(borderChunks, "D:\\GitKraken\\AssetExternals\\debugs\\world_full_borders.txt");
//        //Generator.ExportWorldHeightsStitched(heightChunks, "D:\\GitKraken\\AssetExternals\\debugs\\world_full_heights.txt");
//        //Generator.ExportWorldHeightBordersStitched(borderHeightChunk, "D:\\GitKraken\\AssetExternals\\debugs\\world_full_height_borders.txt");
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public bool IsChunkInsideBounds(Vector2I chunk)
//    {
//        return chunk.X >= MinChunk.X &&
//               chunk.X <= MaxChunk.X &&
//               chunk.Y >= MinChunk.Y &&
//               chunk.Y <= MaxChunk.Y;
//    }

//    private void ChunkManager_OnChunkPreLoadGenerator(Vector2I chunk)
//    {
//        if (!IsChunkInsideBounds(chunk))
//            return;

//        tileMapper.GenerateChunkTileData(chunk);
//        GeneratorResources.GenerateChunk(chunk);
//    }




//}