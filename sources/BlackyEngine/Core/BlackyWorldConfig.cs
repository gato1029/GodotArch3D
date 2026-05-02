using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Core;


public sealed class BlackyWorldConfig
{
    public string Name { get; }
    public BlackyWorldTypeDetail WorldTypeDetail { get; }
    public int ChunkSize { get; }
    public int HeightCount { get; }
    public int WorldSeed { get; }

    public Vector2I MapSize { get; }

    public Vector2I MinChunk { get; }
    public Vector2I MaxChunk { get; }

    public int ChunksX { get; }
    public int ChunksY { get; }

    public BlackyWorldConfig(
        string name,
        BlackyWorldTypeDetail worldTypeDetail,
        int chunkSize,
        int heightCount,
        int worldSeed,
        Vector2I requestedMapSize)
    {
        Name = name;
        WorldTypeDetail = worldTypeDetail;
        ChunkSize = chunkSize;
        HeightCount = heightCount;
        WorldSeed = worldSeed;

        MapSize = CorrectMapSize(requestedMapSize);

        ChunksX = Mathf.CeilToInt((float)MapSize.X / ChunkSize);
        ChunksY = Mathf.CeilToInt((float)MapSize.Y / ChunkSize);

        int halfX = ChunksX / 2;
        int halfY = ChunksY / 2;

        MinChunk = new Vector2I(-halfX, -halfY);
        MaxChunk = new Vector2I(halfX - 1, halfY - 1);
    }

    private static Vector2I CorrectMapSize(Vector2I input)
    {
        const int MIN = 64;
        const int MAX = 4096;
        const int STEP = 32;

        int Fix(int value)
        {
            if (value < MIN)
                value = MIN;

            int corrected = ((value + STEP - 1) / STEP) * STEP;

            if (corrected > MAX)
                corrected = MAX;

            return corrected;
        }

        return new Vector2I(
            Fix(input.X),
            Fix(input.Y));
    }

    public bool IsChunkInsideBounds(Vector2I chunk)
    {
        return chunk.X >= MinChunk.X &&
               chunk.X <= MaxChunk.X &&
               chunk.Y >= MinChunk.Y &&
               chunk.Y <= MaxChunk.Y;
    }
}