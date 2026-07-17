using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;

using GodotEcsArch.sources.managers.Chunks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.BlackyEngine.Streaming;

public sealed class BlackyWorldStreaming
{
    public ChunkManagerBase chunkManagerLocal { get; }    

    public event Action<Vector2I> OnChunkPreloadRequested;

    private readonly BlackyWorld world;
    private readonly BlackyWorldConfig config;

    public BlackyWorldStreaming(BlackyWorld world)
    {
        this.world = world;
        this.config = world.Config;

        chunkManagerLocal = ChunkManager.Instance.tiles16X16;
        chunkManagerLocal.SetBounds(config.MinChunk, config.MaxChunk);

        //chunkManagerLocal.OnChunkPreLoadGenerator += HandleChunkPreloadRequest;
    }

    //private void HandleChunkPreloadRequest(Vector2I chunk)
    //{
    //    if (!config.IsChunkInsideBounds(chunk))
    //        return;

    //    OnChunkPreloadRequested?.Invoke(chunk);
    //}

    public bool IsChunkInsideBounds(Vector2I chunk)
    {
        return config.IsChunkInsideBounds(chunk);
    }

    public List<Vector2I> GetRequiredChunks(int widthTiles, int heightTiles)
    {
        List<Vector2I> result = new();

        int chunksX = Mathf.CeilToInt((float)widthTiles / config.ChunkSize);
        int chunksY = Mathf.CeilToInt((float)heightTiles / config.ChunkSize);

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
}