using Godot;

using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;

using GodotEcsArch.sources.managers.Chunks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.State;

public sealed class BlackyWorldState : IDisposable
{
    public FastSpatialHash DynamicHash { get; }
    public StaticSpatialGridOptimized StaticSpatial { get; }
    public BlackyChunkedBitGrid GridMove { get; }

    public BlackyChunkRenderData RenderData { get; }
    public BlackySpatialEntityMap SpatialEntityMap { get; }
    public BlackyTilePalette TilePalette { get; }

    public BlackyChunkOccupancyMap OccupancyMap { get; }

    private readonly BlackyWorld world;
    private readonly BlackyWorldConfig config;

    public BlackyWorldState(BlackyWorld world)
    {
        this.world = world;
        this.config = world.Config;

        DynamicHash = new FastSpatialHash(config.MapSize.X, config.MapSize.Y, 11000);
        StaticSpatial = new StaticSpatialGridOptimized(config.MapSize.X, config.MapSize.Y, 32, 65536);
        GridMove = new BlackyChunkedBitGrid(config.MapSize.X, config.MapSize.Y, 16);

        RenderData = new BlackyChunkRenderData(config.ChunkSize, config.HeightCount);
        SpatialEntityMap = new BlackySpatialEntityMap();
        TilePalette = new BlackyTilePalette();

        OccupancyMap = new BlackyChunkOccupancyMap(config.HeightCount, config.ChunkSize);

        WireShape.Instance.DrawGrid(
            config.MapSize.X,
            config.MapSize.Y,
            16,
            new Vector2(0, 0),
            -50,
            Colors.DarkCyan);
    }

    public void Dispose()
    {
        DynamicHash.Clear();
        StaticSpatial.Clear();
    }
}