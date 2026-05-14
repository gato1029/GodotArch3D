using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTiles.Data;

// =========================================================
// CHUNK DATA
// =========================================================
public class BlackyChunkData<T>
       where T : struct
{
    private readonly Dictionary<int, BlackyChunkHeightData<T>>
        _heights = new();

    private readonly int _chunkSize;

    public BlackyChunkData(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public BlackyChunkHeightData<T>
        GetOrCreateHeight(int height)
    {
        if (!_heights.TryGetValue(height, out var h))
        {
            h =
                new BlackyChunkHeightData<T>(
                    _chunkSize);

            _heights[height] = h;
        }

        return h;
    }

    public bool TryGetHeight(
        int height,
        out BlackyChunkHeightData<T> h)
    {
        return _heights.TryGetValue(height, out h);
    }
}