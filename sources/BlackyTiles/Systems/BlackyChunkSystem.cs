using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Systems;
public abstract class BlackyChunkSystem<TChunkData>
    where TChunkData : class
{
    protected readonly BlackyChunkRenderData RenderData;

    private readonly Dictionary<BlackyChunkCoord, TChunkData> _chunks
        = new();

    protected BlackyChunkSystem(BlackyChunkRenderData renderData)
    {
        RenderData = renderData;
    }

    // ================================
    // Acceso a datos por chunk
    // ================================

    protected bool TryGetChunkData(
        BlackyChunkCoord coord,
        out TChunkData chunkData)
    {
        return _chunks.TryGetValue(coord, out chunkData);
    }

    protected TChunkData GetOrCreateChunkData(
        BlackyChunkCoord coord,
        Func<TChunkData> factory)
    {
        if (!_chunks.TryGetValue(coord, out var chunkData))
        {
            chunkData = factory();
            _chunks[coord] = chunkData;
        }

        return chunkData;
    }

    protected void RemoveChunkData(BlackyChunkCoord coord)
    {
        _chunks.Remove(coord);
    }

    public IEnumerable<KeyValuePair<BlackyChunkCoord, TChunkData>>
        GetAllChunks()
    {
        return _chunks;
    }
}