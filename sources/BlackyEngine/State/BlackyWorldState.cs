using Flecs.NET.Core;
using Godot;

using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.PathFinding;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;
using System;

namespace GodotEcsArch.sources.BlackyEngine.State;

public enum ModeGrid
{
    DUAL,
    NORMAL
}
public sealed class BlackyWorldState : IDisposable
{
    public FastSpatialHash DynamicHash { get; }
    public StaticSpatialGridOptimizedGeneric<Entity> StaticSpatialBuildings { get; }
    public StaticSpatialGridOptimizedGeneric<Entity> StaticSpatialResources { get; }
    public StaticSpatialGridOptimizedGeneric<ColliderSpriteInstanceData> StaticSpatialTerrain { get; }
    public BlackyChunkedBitGrid GridMove { get; }

    public BlackyChunkRenderData RenderData { get; }
    public BlackySpatialEntityMap SpatialEntityMap { get; }
    public BlackyPersistentTilePalette TilePalette { get; }
    public BlackyPathfinder PathFinder { get; }
    public BlackyChunkOccupancyMap OccupancyMap { get; }
    public BlackyMacroGraphCache GraphCache { get; }

    private int idGridDraw { get; set; }
    private ModeGrid modeGrid { get; set; } = ModeGrid.NORMAL;
    private readonly BlackyWorld world;
    private readonly BlackyWorldConfig config;

    public BlackyWorldState(BlackyWorld world)
    {
        this.world = world;
        this.config = world.Config;

        DynamicHash = new FastSpatialHash(config.MapSize.X, config.MapSize.Y, 11000);
        StaticSpatialBuildings = new StaticSpatialGridOptimizedGeneric<Entity>(config.MapSize.X, config.MapSize.Y, 32, 100_000, 50_000);
        StaticSpatialResources = new StaticSpatialGridOptimizedGeneric<Entity>(config.MapSize.X, config.MapSize.Y, 32, 1000_000, 50_000);
        StaticSpatialTerrain = new StaticSpatialGridOptimizedGeneric<ColliderSpriteInstanceData>(config.MapSize.X, config.MapSize.Y, 32, 100_000,50_000);
        GridMove = new BlackyChunkedBitGrid(config.MapSize.X, config.MapSize.Y, 16);

        RenderData = new BlackyChunkRenderData(config.ChunkSize, config.HeightCount);
        SpatialEntityMap = new BlackySpatialEntityMap();
        TilePalette = new BlackyPersistentTilePalette();

        OccupancyMap = new BlackyChunkOccupancyMap(config.HeightCount, config.ChunkSize);

        idGridDraw= WireShape.Instance.DrawGrid(config.MapSize.X,config.MapSize.Y,16,new Vector2(0, 0),40,Colors.DarkCyan);
        GraphCache = new BlackyMacroGraphCache();
        GraphCache.BuildCache(config.MinChunk, config.MaxChunk);
        PathFinder = new BlackyPathfinder(GraphCache,OccupancyMap);
    }
    public void SetModeGrid(ModeGrid modeGrid)
    {
        this.modeGrid = modeGrid;
        switch (modeGrid)
        {
            case ModeGrid.DUAL:
                idGridDraw = WireShape.Instance.DrawGrid(config.MapSize.X, config.MapSize.Y, 16, new Vector2(0.25f, 0.25f), 40, Colors.DarkCyan);
                break;
            case ModeGrid.NORMAL:
                idGridDraw = WireShape.Instance.DrawGrid(config.MapSize.X, config.MapSize.Y, 16, new Vector2(0, 0), 40, Colors.DarkCyan);
                break;
            default:
                break;
        }
    }
    public void SetGridDrawVisible(bool visible)
    {
        if (idGridDraw!=-1)
        {
            WireShape.Instance.FreeShape(idGridDraw);
        }
        if (visible)
        {
            SetModeGrid(modeGrid);
        }
        else
        {
            WireShape.Instance.FreeShape(idGridDraw);
            idGridDraw = -1;
        }                
    }


    public void Dispose()
    {
        DynamicHash.Clear();
        StaticSpatialBuildings.Clear();
        StaticSpatialTerrain.Clear();
        StaticSpatialResources.Clear(); 
        WireShape.Instance.FreeShape(idGridDraw);
        OccupancyMap.Dispose();
        SpatialEntityMap.Clear();
    }
}