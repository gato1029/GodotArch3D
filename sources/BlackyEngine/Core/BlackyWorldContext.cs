using GodotEcsArch.sources.BlackyEngine.Generation;
using GodotEcsArch.sources.BlackyEngine.Generation.Biomes;
using GodotEcsArch.sources.BlackyEngine.Generation.Resources;
using GodotEcsArch.sources.BlackyEngine.Generation.Terrain;
using GodotEcsArch.sources.BlackyEngine.Services;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Services.Spawn;
using GodotEcsArch.sources.BlackyEngine.Simulation;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyEngine.State;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;
using GodotEcsArch.sources.BlackyTiles.Entities;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.managers.Chunks;
using GodotFlecs.sources.Flecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Core;




public static class BlackyWorldContext
{
    // ================================
    // Active World Root
    // ================================

    public static BlackyWorld World => BlackyWorldRegistry.Instance.ActiveWorld;

    // ================================
    // Core Modules
    // ================================

    public static BlackyWorldConfig Config => World.Config;
    public static BlackyWorldState State => World.State;
    public static BlackyWorldSimulation Simulation => World.Simulation;
    public static BlackyWorldServices Services => World.Services;
    public static BlackyWorldGeneration Generation => World.Generation;

    // ================================
    // ECS
    // ================================

    public static FlecsManager Flecs => Simulation.Flecs;
    public static SimulationTick Tick => Simulation.Tick;

    // ================================
    // World Authoring Services
    // ================================

    public static BlackyCharacterCreator Characters => Services.Characters;
    public static BlackyTerrainSystem Terrain => Services.TerrainPainter;
    public static BlackyResourcesSourceSystem Resources => Services.ResourcePainter;
    public static BlackyBuildingSystem Buildings => Services.BuildingPainter;
    public static BlackyHeightSystem Heights => Services.HeightTool;
    public static BlackyChunkCacheTextureMap PintarTerreno => Services.TerrainTexturePainter;

    // ================================
    // Render Services
    // ================================

    public static BlackyTileRenderSystem TileRenderer => Services.TileRenderer;
    public static BlackyEntityRenderSystem EntityRenderer => Services.EntityRenderer;
    public static BlackyOccupancyRendererSystem OccupancyRenderer => Services.OccupancyRenderer;

    // ================================
    // Procedural Generation
    // ================================

    public static BlackyWorldBiomeMap Biomes => Generation.BiomeMap;
    public static BlackyWorldTerrainGenerator TerrainGenerator => Generation.TerrainGenerator;
    public static BlackyResourcesGenerator ResourceGenerator => Generation.ResourceGenerator;
    public static BlackyResourcesPostProcessor ResourcePostProcessor => Generation.ResourcePostProcessor;
    public static BlackyWorldTileMapper TileMapper => Generation.TileMapper;

    // ================================
    // Spatial Runtime State
    // ================================

    public static FastSpatialHash DynamicHash => State.DynamicHash;
    public static StaticSpatialGridOptimized StaticSpatial => State.StaticSpatial;
    public static BlackyChunkedBitGrid GridMove => State.GridMove;
    public static BlackyChunkRenderData RenderData => State.RenderData;
    public static BlackySpatialEntityMap SpatialEntityMap => State.SpatialEntityMap;
    public static BlackyPersistentTilePalette TilePalette => State.TilePalette;
    public static ChunkManagerBase ChunkManager => World.Streaming.chunkManagerLocal;
    public static BlackyChunkOccupancyMap OccupancyMap => State.OccupancyMap;

    // ================================
    // Helpers
    // ================================

    public static bool HasWorld => BlackyWorldRegistry.Instance.ActiveWorld != null;
}