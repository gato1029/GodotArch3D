using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles;
public class BlackyChunkRenderData
{
    private readonly Dictionary<BlackyChunkCoord, BlackyChunk> _chunks
        = new();

    public int ChunkSize { get; }
    public int HeightCount { get; }

    public BlackyChunkRenderData(int chunkSize, int heightCount)
    {
        ChunkSize = chunkSize;
        HeightCount = heightCount;
    }

    // ===============================
    // CHUNK MANAGEMENT
    // ===============================

    public BlackyChunk GetOrCreateChunk(int chunkX, int chunkY)
    {
        var coord = new BlackyChunkCoord(chunkX, chunkY);

        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            chunk = new BlackyChunk(coord, ChunkSize, HeightCount);
            _chunks[coord] = chunk;
        }

        return chunk;
    }

    public bool TryGetChunk(int chunkX, int chunkY, out BlackyChunk chunk)
    {
        return _chunks.TryGetValue(
            new BlackyChunkCoord(chunkX, chunkY),
            out chunk
        );
    }

    public IEnumerable<BlackyChunk> GetLoadedChunks()
    {
        return _chunks.Values;
    }

    // ===============================
    // COORDINATE RESOLUTION
    // ===============================

    public BlackyChunkCoord WorldToChunkCoord(int worldX, int worldY)
    {
        return new BlackyChunkCoord(
            FloorDiv(worldX, ChunkSize),
            FloorDiv(worldY, ChunkSize)
        );
    }

    public (int localX, int localY) WorldToLocal(int worldX, int worldY)
    {
        int localX = Mod(worldX, ChunkSize);
        int localY = Mod(worldY, ChunkSize);

        return (localX, localY);
    }

    public (BlackyChunk chunk, int localX, int localY)
        ResolveOrCreate(int worldX, int worldY)
    {
        var coord = WorldToChunkCoord(worldX, worldY);
        var (localX, localY) = WorldToLocal(worldX, worldY);

        var chunk = GetOrCreateChunk(coord.X, coord.Y);

        return (chunk, localX, localY);
    }

    public (BlackyChunk chunk, int localX, int localY)
        Resolve(int worldX, int worldY)
    {
        var coord = WorldToChunkCoord(worldX, worldY);
        var (localX, localY) = WorldToLocal(worldX, worldY);

        _chunks.TryGetValue(coord, out var chunk);

        return (chunk, localX, localY);
    }

    // ===============================
    // SAFE MATH FOR NEGATIVE VALUES
    // ===============================

    private static int FloorDiv(int a, int b)
    {
        int result = a / b;
        if ((a ^ b) < 0 && (result * b != a))
            result--;
        return result;
    }

    private static int Mod(int a, int b)
    {
        int result = a % b;
        if (result < 0)
            result += b;
        return result;
    }


}
