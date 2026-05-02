using global::GodotEcsArch.sources.BlackyEngine.Core;
using global::GodotEcsArch.sources.BlackyEngine.Services.Paint;
using global::GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
using global::GodotEcsArch.sources.BlackyEngine.Services.Spawn;
using global::GodotEcsArch.sources.BlackyEngine.State;
using global::GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using global::GodotEcsArch.sources.BlackyTiles.Entities;
using global::GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;

namespace GodotEcsArch.sources.BlackyEngine.Services;

public sealed class BlackyWorldServices
{
    // ============================
    // Spawn / Author tools
    // ============================

    public BlackyCharacterCreator Characters { get; }

    public BlackyTerrainSystem TerrainPainter { get; }

    public BlackyResourcesSourceSystem ResourcePainter { get; }

    public BlackyBuildingSystem BuildingPainter { get; }

    public BlackyHeightSystem HeightTool { get; }

    public BlackyChunkTextureMap TerrainTexturePainter { get; }

    // ============================
    // Render helpers
    // ============================
    public BlackyTileTextureRenderSystem TerrainTextureRenderSystem { get; }

    public BlackyTileRenderSystem TileRenderer { get; }

    public BlackyEntityRenderSystem EntityRenderer { get; }

    public BlackyOccupancyRendererSystem OccupancyRenderer { get; }

    private readonly BlackyWorld world;

    public BlackyWorldServices(BlackyWorld world)
    {
        this.world = world;

        var state = world.State;
        var sim = world.Simulation;
        var inf = world.Config;
        var stream = world.Streaming;
        // ============================
        // Render infra first
        // ============================

        TerrainTexturePainter = new BlackyChunkTextureMap(inf.ChunkSize, inf.HeightCount,5);

        TileRenderer = new BlackyTileRenderSystem(
            sim.Flecs,
            state.RenderData,
            world.Streaming.chunkManagerLocal);

        EntityRenderer = new BlackyEntityRenderSystem(
            state.SpatialEntityMap,
            world.Streaming.chunkManagerLocal);

        OccupancyRenderer = new BlackyOccupancyRendererSystem(
            sim.Flecs,
            world.Streaming.chunkManagerLocal,
            state.OccupancyMap);

        // ============================
        // Runtime authoring services
        // ============================

        TerrainTextureRenderSystem = new BlackyTileTextureRenderSystem(TerrainTexturePainter, stream.chunkManagerLocal);

        Characters = new BlackyCharacterCreator(
            sim.Flecs,
            state.DynamicHash);

        TerrainPainter = new BlackyTerrainSystem(
            state.RenderData,
            state.OccupancyMap,
            TileRenderer);

        ResourcePainter = new BlackyResourcesSourceSystem(
            state.StaticSpatial,
            sim.Flecs,
            state.OccupancyMap,
            state.SpatialEntityMap,
            EntityRenderer,
            TerrainPainter);

        BuildingPainter = new BlackyBuildingSystem(
            sim.Flecs,
            state.OccupancyMap,
            state.SpatialEntityMap,
            EntityRenderer,
            TerrainPainter);

        HeightTool = new BlackyHeightSystem(TerrainPainter);
    }
}