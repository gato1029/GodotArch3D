using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTiles.Data;

// =========================================================
// CHUNK DATA
// =========================================================
public class BlackyChunkData<T>
       where T : struct
{
    private readonly Dictionary<int, BlackyChunkHeightData<T>>  _heights = new();

    private readonly int _chunkSize;
    private bool _dirty;

    public void MarkDirty()
    {
        _dirty = true;
    }

    public void ClearDirty()
    {
        _dirty = false;
    }
    public bool IsDirty()
    {
        return _dirty;
    }
    public BlackyChunkData(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public Dictionary<int, BlackyChunkHeightData<T>> Heights => _heights;

    public BlackyChunkHeightData<T>
        GetOrCreateHeight(int height)
    {
        if (!Heights.TryGetValue(height, out var h))
        {
            h =
                new BlackyChunkHeightData<T>(
                    _chunkSize);

            Heights[height] = h;
        }

        return h;
    }

    public bool TryGetHeight(
        int height,
        out BlackyChunkHeightData<T> h)
    {
        return Heights.TryGetValue(height, out h);
    }
}