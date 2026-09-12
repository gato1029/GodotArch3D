using Flecs.NET.Core;
using Godot;

using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;

using GodotEcsArch.sources.managers.Chunks;
using GodotFlecs.sources.Flecs.Services.Spawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.State;

public enum ModeGrid
{
    DUAL,
    NORMAL
}
public sealed class BlackyWorldState : IDisposable
{
    public FastSpatialHash DynamicHash { get; }
    public StaticSpatialGridOptimizedGeneric<Entity> StaticSpatial { get; }
    public StaticSpatialGridOptimizedGeneric<ColliderSpriteInstanceData> StaticSpatialTerrain { get; }
    public BlackyChunkedBitGrid GridMove { get; }

    public BlackyChunkRenderData RenderData { get; }
    public BlackySpatialEntityMap SpatialEntityMap { get; }
    public BlackyPersistentTilePalette TilePalette { get; }

    public BlackyChunkOccupancyMap OccupancyMap { get; }
 

    private int idGridDraw { get; set; }
    private ModeGrid modeGrid { get; set; } = ModeGrid.NORMAL;
    private readonly BlackyWorld world;
    private readonly BlackyWorldConfig config;

    public BlackyWorldState(BlackyWorld world)
    {
        this.world = world;
        this.config = world.Config;

        DynamicHash = new FastSpatialHash(config.MapSize.X, config.MapSize.Y, 11000);
        StaticSpatial = new StaticSpatialGridOptimizedGeneric<Entity>(config.MapSize.X, config.MapSize.Y, 32, 100_000, 50_000);
        StaticSpatialTerrain = new StaticSpatialGridOptimizedGeneric<ColliderSpriteInstanceData>(config.MapSize.X, config.MapSize.Y, 32, 100_000,50_000);
        GridMove = new BlackyChunkedBitGrid(config.MapSize.X, config.MapSize.Y, 16);

        RenderData = new BlackyChunkRenderData(config.ChunkSize, config.HeightCount);
        SpatialEntityMap = new BlackySpatialEntityMap();
        TilePalette = new BlackyPersistentTilePalette();

        OccupancyMap = new BlackyChunkOccupancyMap(config.HeightCount, config.ChunkSize);

        idGridDraw= WireShape.Instance.DrawGrid(config.MapSize.X,config.MapSize.Y,16,new Vector2(0, 0),-50,Colors.DarkCyan);
   
    }
    public void SetModeGrid(ModeGrid modeGrid)
    {
        this.modeGrid = modeGrid;
        switch (modeGrid)
        {
            case ModeGrid.DUAL:
                idGridDraw = WireShape.Instance.DrawGrid(config.MapSize.X, config.MapSize.Y, 16, new Vector2(0.25f, 0.25f), -50, Colors.DarkCyan);
                break;
            case ModeGrid.NORMAL:
                idGridDraw = WireShape.Instance.DrawGrid(config.MapSize.X, config.MapSize.Y, 16, new Vector2(0, 0), -50, Colors.DarkCyan);
                break;
            default:
                break;
        }
    }
    public void SetGridDrawVisible(bool visible)
    {
        if (visible)
        {
            SetModeGrid(modeGrid);
        }
        else
        {
            WireShape.Instance.FreeShape(idGridDraw);
        }                
    }


    public void Dispose()
    {
        DynamicHash.Clear();
        StaticSpatial.Clear();
        StaticSpatialTerrain.Clear();
    }
}