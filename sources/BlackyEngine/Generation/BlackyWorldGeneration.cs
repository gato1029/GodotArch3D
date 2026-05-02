using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generation.Biomes;
using GodotEcsArch.sources.BlackyEngine.Generation.Resources;
using GodotEcsArch.sources.BlackyEngine.Generation.Terrain;
using GodotEcsArch.sources.BlackyEngine.Services;
using GodotEcsArch.sources.BlackyTiles.Systems;

using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Generation;


public  class BlackyWorldGeneration
{
    public BlackyWorldBiomeMap BiomeMap { get; }

    public BlackyWorldTerrainGenerator TerrainGenerator { get; }

    public BlackyResourcesGenerator ResourceGenerator { get; }

    public BlackyResourcesPostProcessor ResourcePostProcessor { get; }

    public BlackyWorldTileMapper TileMapper { get; }

    private readonly BlackyWorld world;
    private readonly BlackyWorldServices services;

    public BlackyWorldGeneration(BlackyWorld world, BlackyWorldServices services)
    {
        this.world = world;
        this.services = services;

        int seed = world.Config.WorldSeed;
        int chunkSize = world.Config.ChunkSize;

        // ================================
        // Procedural backend instancing
        // ================================

        BiomeMap = new BlackyWorldBiomeMap(seed, chunkSize, WorldType.Continents);

        TerrainGenerator = new BlackyWorldTerrainGenerator(seed, chunkSize, BiomeMap);

        ResourcePostProcessor = new BlackyResourcesPostProcessor(seed);

        TileMapper = new BlackyWorldTileMapper(
            chunkSize,
            seed,
            BiomeMap,
            TerrainGenerator,
            services.TerrainPainter);

        ResourceGenerator = new BlackyResourcesGenerator(
            services.HeightTool,
            TerrainGenerator,
            services.ResourcePainter,
            ResourcePostProcessor,
            seed);
        world.Streaming.OnChunkPreloadRequested += OnChunkPreloadRequested;

        ConfigureResourceRules();
        ConfigureResourceGenerator();
    }
    private void OnChunkPreloadRequested(Vector2I chunk)
    {
        GenerateChunk(chunk);
    }
    private void ConfigureResourceRules()
    {
        var treeRule = new BlackyResourcePostRule(ResourceSourceType.Arbol, 2, 1);
        var stoneRule = new BlackyResourcePostRule(ResourceSourceType.Piedras, 2, 2);
        var goldRule = new BlackyResourcePostRule(ResourceSourceType.MinaOro, 3, 10);

        goldRule.SetMinDistanceTo(ResourceSourceType.Arbol, 2);
        goldRule.SetMinDistanceTo(ResourceSourceType.Piedras, 2);

        ResourcePostProcessor.AddRule(treeRule);
        ResourcePostProcessor.AddRule(stoneRule);
        ResourcePostProcessor.AddRule(goldRule);
    }

    private void ConfigureResourceGenerator()
    {
        int seed = world.Config.WorldSeed;

        ResourceGenerator.ConfigureNoiseForType(ResourceSourceType.Arbol, seed + 1000, 0.03f);
        ResourceGenerator.ConfigureNoiseForType(ResourceSourceType.Piedras, seed + 2000, 0.05f);
        ResourceGenerator.ConfigureNoiseForType(ResourceSourceType.MinaOro, seed + 3000, 0.02f);

        ResourceGenerator.ConfigureHeightGlobal(0).SetMinDistanceToHeight(2, 1);

        ResourceGenerator.ConfigureHeightGlobal(2).SetMinDistanceToHeight(0, 2);
        ResourceGenerator.ConfigureHeightGlobal(2).SetMinDistanceToHeight(3, 5);

        ResourceGenerator.ConfigureHeightGlobal(3).SetMinDistanceToHeight(2, 2);

        ResourceGenerator.ConfigureHeightGlobal(2).DensityThreshold = 0.4f;
        ResourceGenerator.ConfigureHeightGlobal(3).DensityThreshold = 0.5f;
    }

    public void GenerateChunk(Vector2I chunk)
    {
        TileMapper.GenerateChunkTileData(chunk);
        ResourceGenerator.GenerateChunk(chunk);
    }

    public void GenerateChunks(IEnumerable<Vector2I> chunks)
    {
        foreach (var chunk in chunks)
            GenerateChunk(chunk);
    }
}