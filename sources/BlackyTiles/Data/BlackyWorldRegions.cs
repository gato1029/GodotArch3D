using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Data;

public class BlackyWorldRegions
{
    private readonly ConcurrentDictionary<(int x, int y), BlackyRegion>
        _regions = new();

    public const int RegionShift = 4;
    // =====================================================
    // DIRTY REGIONS
    // =====================================================

    private readonly HashSet<(int x, int y)>
        _dirtyRegions = new();

    // =====================================================
    // MARK DIRTY
    // =====================================================

    public void MarkRegionDirty(
        int regionX,
        int regionY)
    {
        _dirtyRegions.Add(
            (regionX, regionY));
    }

    public void MarkRegionDirtyByChunk(
        int chunkX,
        int chunkY)
    {
        int regionX =
            chunkX >> RegionShift;

        int regionY =
            chunkY >> RegionShift;

        MarkRegionDirty(
            regionX,
            regionY);
    }

    // =====================================================
    // GET DIRTY REGIONS
    // =====================================================

    public IEnumerable<BlackyRegion>
        GetDirtyRegions()
    {
        foreach (var key in _dirtyRegions)
        {
            if (_regions.TryGetValue(
                key,
                out var region))
            {
                yield return region;
            }
        }
    }

    // =====================================================
    // CLEAR DIRTY
    // =====================================================

    public void ClearDirtyRegion(
        int regionX,
        int regionY)
    {
        _dirtyRegions.Remove(
            (regionX, regionY));
    }

    public void ClearAllDirtyRegions()
    {
        _dirtyRegions.Clear();
    }
    public BlackyRegion GetOrCreateRegionByChunk(int chunkX, int chunkY)
    {
        int regX = chunkX >> RegionShift;
        int regY = chunkY >> RegionShift;
        return _regions.GetOrAdd((regX, regY), key => new BlackyRegion(key.Item1, key.Item2));
    }
    public BlackyRegion GetOrCreateRegion(
       int regionX,
       int regionY)
    {
        var key = (regionX, regionY);

        if (!_regions.TryGetValue(
            key,
            out var region))
        {
            region =
                new BlackyRegion(
                    regionX,
                    regionY);

            _regions[key] = region;
        }

        return region;
    }

    // =====================================================
    // TRY GET
    // =====================================================

    public bool TryGetRegion(
        int regionX,
        int regionY,
        out BlackyRegion region)
    {
        return _regions.TryGetValue(
            (regionX, regionY),
            out region);
    }
    public BlackyRegion GetOrCreateRegionByWorld(
        int worldX,
        int worldY,
        int chunkSize)
    {
        int chunkX =
            FloorDiv(worldX, chunkSize);

        int chunkY =
            FloorDiv(worldY, chunkSize);

        return GetOrCreateRegionByChunk(
            chunkX,
            chunkY);
    }

    private static int FloorDiv(int a, int b)
    {
        int result = a / b;

        if ((a ^ b) < 0 &&
            (result * b != a))
        {
            result--;
        }

        return result;
    }
}
