using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.BlackyTiles.Entities;
using GodotEcsArch.sources.BlackyTiles.Procedural;
using GodotEcsArch.sources.BlackyTiles.Procedural.Resources;
using GodotEcsArch.sources.BlackyTiles.Procedural.Terrain;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.BlackyTiles.Tiles;
using GodotEcsArch.sources.Flecs;
using GodotEcsArch.sources.Flecs.Systems.Generic;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;

namespace GodotEcsArch.sources.BlackyTiles;
public class BlackyWorld
{
    public bool isActive { get; private set; } = true;
    // --- EL MOTOR ---
    public FlecsManager flecsManager { get; private set; }

    // --- LAS COLISIONES (Viven aquí) ---
    public FastSpatialHash DynamicHash { get; private set; }
    public StaticSpatialGridOptimized StaticSpatial { get; private set; }

    public BlackyChunkedBitGrid GridMove { get; private set; }
    // Sistema de Ocupacion

    // sistemas Renderizado y datos
    public BlackyChunkRenderData RenderData { get; }
    public BlackyTileRenderSystem TileRenderSystem { get; }
    public BlackyEntityRenderSystem EntityRenderSystem { get; }
    public BlackySpatialEntityMap SpatialEntityMap { get; }
    public BlackyOccupancyRendererSystem blackyOccupancyRendererSystem { get; }

    // Sistemas del mundo
    public BlackyCharacterCreator characterCreator { get; }
    public BlackyTerrainSystem Terrain { get; }
    public BlackyResourcesSourceSystem Resources { get; }
    public BlackyBuildingSystem Building { get; }
    public BlackyHeightSystem Heights { get; }
    public BlackyWorldBiomeMap biomeMap { get; }

    // Generadores procedurales
    public BlackyWorldTerrainGenerator Generator { get; }
    public BlackyResourcesGenerator GeneratorResources { get; }
    public BlackyResourcesPostProcessor postProcessorResource { get; }

    public BlackyWorldTileMapper tileMapper { get; }

    private readonly ChunkManagerBase chunkManager;
    public int WorldSeed { get; }
    public int ChunkSize;
    public Vector2I MapSize { get; set; }
    public Vector2I MinChunk { get; private set; }
    public Vector2I MaxChunk { get; private set; }    
    public SimulationTick simulationTick { get; private set; }
    public BlackyWorld(
        int chunkSize,
        int heightCount,
        int worldSeed,
        Vector2I mapSize,
        BlackyChunkOccupancyMap occupancyMap,
        ChunkManagerBase chunkManager)
    {

        AtlasTexturesModsManager.Instance.FirstLoad();
        AtlasModsManager.Instance.FirstLoad();

        DataBaseManager.Instance.LoadCurrentDataBase();

        simulationTick = new SimulationTick{ 
            FixedDelta = 0.1f, 
            Accumulator = 0, 
            TickCount = 0 };
        
        var correccionMap = CorrectMapSize(mapSize); // corrige el tamaño del mapa para que sea múltiplo de chunkSize y esté dentro de los límites
        
        this.chunkManager = chunkManager;
        ChunkSize = chunkSize;
        WorldSeed = worldSeed;
        MapSize = correccionMap;

        WireShape.Instance.DrawGrid(MapSize.X, MapSize.Y, 16, new Vector2(0, 0), -50, Colors.DarkCyan);

        int chunksX = Mathf.CeilToInt((float)MapSize.X / ChunkSize);
        int chunksY = Mathf.CeilToInt((float)MapSize.Y / ChunkSize);

        int halfX = chunksX / 2;
        int halfY = chunksY / 2;

        MinChunk = new Vector2I(-halfX, -halfY);
        MaxChunk = new Vector2I(halfX - 1, halfY - 1);
        this.chunkManager.SetBounds(MinChunk, MaxChunk);

        DynamicHash = new FastSpatialHash(MapSize.X, MapSize.Y,  11000);

        StaticSpatial = new StaticSpatialGridOptimized(MapSize.X, MapSize.Y, 32, 65536);

        GridMove = new BlackyChunkedBitGrid(MapSize.X, MapSize.Y, 16);
        flecsManager = new FlecsManager(NodeMainHelper.node3DMain);
        flecsManager.WorldFlecs.SetCtx(this);
        
        RenderData = new BlackyChunkRenderData(chunkSize, heightCount);    
        TileRenderSystem = new BlackyTileRenderSystem(flecsManager,RenderData, chunkManager);
                
        SpatialEntityMap = new BlackySpatialEntityMap();
        EntityRenderSystem = new BlackyEntityRenderSystem( SpatialEntityMap, chunkManager);

        blackyOccupancyRendererSystem = new BlackyOccupancyRendererSystem(flecsManager,chunkManager, occupancyMap);

        characterCreator = new BlackyCharacterCreator(flecsManager, DynamicHash);
        biomeMap = new BlackyWorldBiomeMap(worldSeed, chunkSize, WorldType.Continents);

        Terrain = new BlackyTerrainSystem( RenderData, occupancyMap, TileRenderSystem);

        Resources = new BlackyResourcesSourceSystem(StaticSpatial,flecsManager, occupancyMap, SpatialEntityMap, EntityRenderSystem,Terrain);

        Building = new BlackyBuildingSystem(flecsManager,occupancyMap, SpatialEntityMap, EntityRenderSystem, Terrain);
        
        Generator = new BlackyWorldTerrainGenerator(worldSeed, chunkSize, biomeMap);

        Heights = new BlackyHeightSystem(Terrain);

        tileMapper = new BlackyWorldTileMapper(chunkSize, worldSeed, biomeMap, Generator, Terrain);

        postProcessorResource  = new BlackyResourcesPostProcessor(worldSeed);


        ReglasProcesador();
        GeneratorResources = new BlackyResourcesGenerator(Heights, Generator, Resources, postProcessorResource, worldSeed);
        GeneradoresProcedurales();
        GeneradorRecursos();
        chunkManager.OnChunkPreLoadGenerator += ChunkManager_OnChunkPreLoadGenerator;
        RenderCommandQueue.Enqueue(new ForceUpdateChunksCommand(chunkManager, new Vector2I(0, 0)));

        CurrentWorlds.Instance.AddWorld("mundo", this);

        Test();
        //Test2();
    }
    private void Test()
    {
        var e = characterCreator.Create(1, new Vector2(10, 0));    // principal

        var ee = characterCreator.Create(2, new Vector2(5, 0));

        ee.Set(new MoveTargetComponent(new Vector2(20, 0)));
    }

    private void Test2()
    {
        int count = 10000;

        Vector2 target = new Vector2(0, 0);

        int size = (int)MathF.Sqrt(count); // ~141 x 141

        float spacing = 1f; // distancia entre unidades
        float jitter = 6f;   // ruido para naturalidad

        Random rng = new Random();

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = new Vector2(
                    spacing + i,
                    spacing + 10
               );
            var e = characterCreator.Create(2, pos);

            e.Set(new MoveTargetComponent(target));
        }

      
    }

    public static Vector2I CorrectMapSize(Vector2I input)
    {
        const int MIN = 64;
        const int MAX = 4096;
        const int STEP = 32;

        int Fix(int value)
        {
            // mínimo absoluto
            if (value < MIN)
                value = MIN;

            // redondear hacia arriba al múltiplo de STEP
            int corrected = ((value + STEP - 1) / STEP) * STEP;

            // clamp máximo
            if (corrected > MAX)
                corrected = MAX;

            return corrected;
        }

        return new Vector2I(
            Fix(input.X),
            Fix(input.Y)
        );
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }
    public void Update(float delta)
    {
        if (!isActive) return;
        flecsManager.Update(delta);
    }
    public void Dispose()
    {
        isActive = false;
        flecsManager.Destroy();
        DynamicHash.Clear();
        StaticSpatial.Clear();
    }
    private void ReglasProcesador()
    {
        var treeRule = new BlackyResourcePostRule(ResourceSourceType.Arbol, minDistanceSameType: 2, priority: 1);
        var stoneRule = new BlackyResourcePostRule(ResourceSourceType.Piedras, 2, 2);
        var goldRule = new BlackyResourcePostRule(ResourceSourceType.MinaOro, minDistanceSameType: 3, priority: 10);

        goldRule.SetMinDistanceTo(ResourceSourceType.Arbol, 2);
        goldRule.SetMinDistanceTo(ResourceSourceType.Piedras, 2);

        //treeRule.SetMinDistanceTo(ResourceSourceType.Piedras, 5);

        postProcessorResource.AddRule(treeRule);
        postProcessorResource.AddRule(goldRule);
        postProcessorResource.AddRule(stoneRule);
    }

    private void GeneradorRecursos()
    {
 

        GeneratorResources.ConfigureNoiseForType(ResourceSourceType.Arbol, WorldSeed+1000, 0.03f); // frecuente
        GeneratorResources.ConfigureNoiseForType(ResourceSourceType.Piedras, WorldSeed + 2000, 0.05f); //pequeño
        GeneratorResources.ConfigureNoiseForType(ResourceSourceType.MinaOro, WorldSeed + 3000, 0.02f); // muy raro

        GeneratorResources.ConfigureHeightGlobal(0).SetMinDistanceToHeight(2, 1);

        GeneratorResources.ConfigureHeightGlobal(2).SetMinDistanceToHeight(0, 2);
        GeneratorResources.ConfigureHeightGlobal(2).SetMinDistanceToHeight(3, 5);
        GeneratorResources.ConfigureHeightGlobal(3).SetMinDistanceToHeight(2, 2);

        GeneratorResources.ConfigureHeightGlobal(2).DensityThreshold = 0.4f;
        GeneratorResources.ConfigureHeightGlobal(3).DensityThreshold = 0.5f;
        
        GeneratorResources.ConfigureHeightGlobal(2).SetTypeWeight(ResourceSourceType.Arbol, 1);
        GeneratorResources.ConfigureHeightGlobal(2).SetTypeWeight(ResourceSourceType.Piedras, 0.1f);
        GeneratorResources.ConfigureHeightGlobal(2).SetTypeWeight(ResourceSourceType.MinaOro, 0.1f);

        GeneratorResources.ConfigureHeightGlobal(3).SetTypeWeight(ResourceSourceType.Arbol, 1);
        GeneratorResources.ConfigureHeightGlobal(3).SetTypeWeight(ResourceSourceType.Piedras, 0.1f);

        MasterDataManager.RegisterAllData<long, TerrainData>();

        var collection = MasterDataManager.GetAllData<long, TerrainData>();
        foreach (var item in collection)
        {
            foreach (var id in item.idsElevacionResources)
            {
                List<ResourceEntry> li = id.Value;
                foreach (var item2 in li)
                {
                    GeneratorResources.AddEntry(id.Key, item.idSave, item2);
                    GeneratorResources.ConfigureHeight(id.Key, item.idSave);
                }
            }
        }

        //var listchunks = GetRequiredChunks(1024, 1024);
        var listchunks = chunkManager.GetExtendedChunks(new Vector2(0, 0)).ToList();
        foreach (var item in listchunks)
        {
            GeneratorResources.GenerateChunk(item);
        }
   
    }


    public List<Vector2I> GetRequiredChunks(int widthTiles, int heightTiles)
    {
        List<Vector2I> result = new();

        int chunksX = Mathf.CeilToInt((float)widthTiles / ChunkSize);
        int chunksY = Mathf.CeilToInt((float)heightTiles / ChunkSize);

        // 🔥 offsets para centrar en (0,0)
        int startX = -chunksX / 2;
        int startY = -chunksY / 2;

        int endX = startX + chunksX;
        int endY = startY + chunksY;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                result.Add(new Vector2I(x, y));
            }
        }

        return result;
    }
    private void GeneradoresProcedurales()
    {
      

        string tit = "Tiempo Generacion:";
        using (new ProfileScope(tit))
        {
            //var listchunks = GetRequiredChunks(1024, 1024);
            var listchunks = chunkManager.GetExtendedChunks(new Vector2(0, 0)).ToList();
            GD.Print("Cantidad de chunks:"+listchunks.Count);
            //Dictionary<Vector2I, ushort[,]> borderChunks = new();
            //Dictionary<Vector2I, ushort[,]> worldChunks = new();
            //Dictionary<Vector2I, ushort[,]> heightChunks = new();
            //Dictionary<Vector2I, ushort[,]> borderHeightChunk = new();
            foreach (var item in listchunks)
            {

                

                //var biomeMapChunk = biomeMap.GetChunkBiomes(item);
                //var borderMapChunk = biomeMap.GetChunkBorders(item);
                //var heightMapChunk = Generator.GetChunkHeights(item);
                //var borderHeightMapChunk = Generator.GetChunkHeightBorders(item);
                tileMapper.GenerateChunkTileData(item);

                //worldChunks[item] = biomeMapChunk;
                //borderChunks[item] = borderMapChunk;
                //heightChunks[item] = heightMapChunk;
                //borderHeightChunk[item] = borderHeightMapChunk;


                // TODO: Esto es solo para debug, luego se debe eliminar
                //float sizeRealWorld = MeshCreator.PixelsToUnits(16 * chunkManager.chunkDimencion.X);
                //Vector2 plot = (sizeRealWorld * (Vector2)item) + new Vector2(sizeRealWorld / 2, sizeRealWorld / 2);
                //WireShape.Instance.DrawSquare(new Vector2(16 * 32, 16 * 32), plot, 25, Colors.Red);


            }
        }
        PerformanceTimer.Instance.Print(tit);
        //biomeMap.ExportWorldStitched(worldChunks, "D:\\GitKraken\\AssetExternals\\debugs\\world_full.txt");
        //biomeMap.ExportWorldBordersStitched(borderChunks, "D:\\GitKraken\\AssetExternals\\debugs\\world_full_borders.txt");
        //Generator.ExportWorldHeightsStitched(heightChunks, "D:\\GitKraken\\AssetExternals\\debugs\\world_full_heights.txt");
        //Generator.ExportWorldHeightBordersStitched(borderHeightChunk, "D:\\GitKraken\\AssetExternals\\debugs\\world_full_height_borders.txt");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsChunkInsideBounds(Vector2I chunk)
    {
        return chunk.X >= MinChunk.X &&
               chunk.X <= MaxChunk.X &&
               chunk.Y >= MinChunk.Y &&
               chunk.Y <= MaxChunk.Y;
    }

    private void ChunkManager_OnChunkPreLoadGenerator(Vector2I chunk)
    {
        if (!IsChunkInsideBounds(chunk))
            return;

        tileMapper.GenerateChunkTileData(chunk);
        GeneratorResources.GenerateChunk(chunk);
    }




}