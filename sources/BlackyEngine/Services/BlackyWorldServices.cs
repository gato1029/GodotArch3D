using global::GodotEcsArch.sources.BlackyEngine.Core;
using global::GodotEcsArch.sources.BlackyEngine.Services.Paint;
using global::GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
using global::GodotEcsArch.sources.BlackyEngine.Services.Spawn;
using global::GodotEcsArch.sources.BlackyEngine.State;
using global::GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using global::GodotEcsArch.sources.BlackyTiles.Entities;
using global::GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyTiles.Data;

namespace GodotEcsArch.sources.BlackyEngine.Services;

public sealed class BlackyWorldServices
{

    // ==============================
    // Terreno
    // ==============================

    public BlackyWorldDataMap<ushort> TerrainData { get; } // para pintar el terreno, el ushort es el id del terreno que se pintara en ese lugar
    public BlackyWorldDataMap<ushort> RampasData { get; } // para pintar rampas y similares, el ushort es el id del tile que se pintara en ese lugar
    public BlackyWorldDataMap<ushort> SuperficiesData { get; } // para pintar superficies, el ushort es el id del tile que se pintara en ese lugar
    public BlackyWorldDataMap<ushort> CaminosData { get; } // para pintar caminos, el ushort es el id del tile que se pintara en ese lugar
    public BlackyWorldDataMap<ushort> AdornosData { get; } // para pintar adornos sobre la superficie, como flores, piedritas, partes de edificios, etc, pero no tienen entidad, esto es para que se renderice por encima de la superficie pero debajo de las entidades, el ushort es el id del tile que se pintara en ese lugar


    // ============================
    // Spawn / Author tools
    // ============================

    public BlackyCharacterCreator Characters { get; }

    public BlackyTerrainSystem TerrainPainter { get; }

    public BlackyResourcesSourceSystem ResourcePainter { get; }

    public BlackyBuildingSystem BuildingPainter { get; }

    public BlackyHeightSystem HeightTool { get; }

    // ============================
    // Cache para renderizado
    // ============================

    public BlackyChunkCacheTextureMap TerrainTexturePainter { get; } // para pintar Terreno

    // ============================
    // Render helpers
    // ============================
    public BlackyTileTextureRenderSystem TerrainTextureRenderSystem { get; } // para renderizar pintar Terreno // esto luego se eliminara

    public BlackyTileRenderSystem TileRenderer { get; } // eliminar luego

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

        //TerrainData = new BlackyWorldDataMap<ushort>(inf.ChunkSize, BlackyRenderLayer.TerrenoBase, TerrainTexturePainter,true);

        TerrainTexturePainter = new BlackyChunkCacheTextureMap(inf.ChunkSize, inf.HeightCount,5);

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