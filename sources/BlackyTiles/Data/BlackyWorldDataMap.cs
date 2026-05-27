using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Data;

public enum BlackyRenderLayer
{
    TerrenoBase = 0, // para tiles que no tienen entidad, como el suelo este no cambiara cuando se diseñe el mapa
    Rampas = 1, // para rampas Y similares, esto es para que se renderice por encima de la superficie pero debajo de las entidades
    Superficie = 2, // esto si puede cambiar poner hierba, tierra de cultivo, etc
    Caminos = 3, // para caminos, esto es para que se renderice por encima de la superficie pero debajo de las entidades    
    Adornos = 4, // para adornos sobre la superficie, como flores, piedritas, partes de edificios, etc, pero no tienen entidad, esto es para que se renderice por encima de la superficie pero debajo de las entidades
                 // los recursos edificios, personajes, entidades se renderizaran por encima de todo esto, en su propia capa de renderizado, para que se rendericen por encima de todo esto, y asi no se vean tapados por nada del terreno
}

public abstract class BlackyWorldDataMap<T>
       where T : struct
{
    // =====================================================
    // CHUNKS
    // =====================================================

    protected readonly Dictionary<BlackyChunkCoord,BlackyChunkData<T>> _chunks = new();
    protected readonly HashSet<BlackyChunkCoord>
    _dirtyChunks = new();
    // =====================================================
    // VISUAL CACHE
    // =====================================================

    protected readonly BlackyChunkCacheTextureMap
        _textureMap;

    protected readonly BlackyWorldRegions
        _regions;

    // =====================================================
    // CONFIG
    // =====================================================

    public bool isDual { get; }
    public int ChunkSize { get; }

    public BlackyRenderLayer  RenderLayer { get; }

    // =====================================================
    // CACHE
    // =====================================================

    private BlackyChunkCoord
        _lastCoord;

    private BlackyChunkData<T>
        _lastChunk;

    // =====================================================
    // CTOR
    // =====================================================

    protected BlackyWorldDataMap(
        int chunkSize,
        BlackyRenderLayer renderLayer,
        BlackyChunkCacheTextureMap textureMap, bool isDual, BlackyWorldRegions regions)
    {
        ChunkSize = chunkSize;

        RenderLayer = renderLayer;
        _regions = regions;
        this.isDual = isDual;
        _textureMap = textureMap;
    }
    public void ClearDirtyRegion(
    BlackyRegion region)
    {
        List<BlackyChunkCoord> toRemove =
            new();

        foreach (var coord in _dirtyChunks)
        {
            int regionX =
                coord.X >> BlackyWorldRegions.RegionShift;

            int regionY =
                coord.Y >> BlackyWorldRegions.RegionShift;
            if (regionX == region.X &&
                regionY == region.Y)
            {
                toRemove.Add(coord);
            }
        }

        foreach (var coord in toRemove)
        {
            _dirtyChunks.Remove(coord);

            if (_chunks.TryGetValue(
                coord,
                out var chunk))
            {
                chunk.ClearDirty();
            }
        }
    }

    public IEnumerable<BlackyChunkCoord>
    GetDirtyChunksForRegion(
        BlackyRegion region)
    {
        foreach (var coord in _dirtyChunks)
        {
            int regionX =
                coord.X >> BlackyWorldRegions.RegionShift;

            int regionY =
                coord.Y >> BlackyWorldRegions.RegionShift;

            if (regionX == region.X &&
                regionY == region.Y)
            {
                yield return coord;
            }
        }
    }

    // =====================================================
    // CELL ACCESS
    // =====================================================

    public ref T ResolveOrCreateCell(
        int worldX,
        int worldY,
        int height)
    {
        var (chunk, lx, ly) =
            ResolveOrCreate(
                worldX,
                worldY);

        var h =
            chunk.GetOrCreateHeight(
                height);

        var coord = WorldToChunkCoord(worldX, worldY);

        MarkChunkDirty(      coord,         chunk);
        _regions.MarkRegionDirtyByChunk( coord.X, coord.Y);
        return ref h.GetCell(
            lx,
            ly);
    }

    public bool TryGetCell(
        int worldX,
        int worldY,
        int height,
        out T cell)
    {
        cell = default;

        var (chunk, lx, ly) =
            Resolve(
                worldX,
                worldY);

        if (chunk == null)
            return false;

        if (!chunk.TryGetHeight(
            height,
            out var h))
        {
            return false;
        }

        cell =
            h.GetCell(
                lx,
                ly);

        return true;
    }
    public bool TryGetChunk(
    int chunkX,
    int chunkY,
    out BlackyChunkData<T> chunk)
    {
        return _chunks.TryGetValue(
            new BlackyChunkCoord(
                chunkX,
                chunkY),
            out chunk);
    }

    // =====================================================
    // RESOLVE
    // =====================================================

    public (
        BlackyChunkData<T> chunk,
        int lx,
        int ly)
        ResolveOrCreate(
            int worldX,
            int worldY)
    {
        var coord =
            WorldToChunkCoord(
                worldX,
                worldY);



        if (_lastChunk != null &&
            coord.Equals(_lastCoord))
        {
            var (lx2, ly2) =
                WorldToLocal(
                    worldX,
                    worldY);

            return (
                _lastChunk,
                lx2,
                ly2);
        }

        if (!_chunks.TryGetValue(
            coord,
            out var chunk))
        {
            chunk =
                new BlackyChunkData<T>(
                    ChunkSize);

            _chunks[coord] = chunk;
        }



        _lastCoord = coord;
        _lastChunk = chunk;

        var (lx, ly) =
            WorldToLocal(
                worldX,
                worldY);

        return (
            chunk,
            lx,
            ly);
    }
    protected void MarkChunkDirty(
    BlackyChunkCoord coord,
    BlackyChunkData<T> chunk)
    {
        chunk.MarkDirty();
        _dirtyChunks.Add(coord);
        var region =
            _regions.GetOrCreateRegionByChunk(
                coord.X,
                coord.Y);

        region.MarkChunkDirty(coord);
    }
    public (
        BlackyChunkData<T> chunk,
        int lx,
        int ly)
        Resolve(
            int worldX,
            int worldY)
    {
        var coord =
            WorldToChunkCoord(
                worldX,
                worldY);

        if (_lastChunk != null &&
            coord.Equals(_lastCoord))
        {
            var (lx2, ly2) =
                WorldToLocal(
                    worldX,
                    worldY);

            return (
                _lastChunk,
                lx2,
                ly2);
        }

        _chunks.TryGetValue(
            coord,
            out var chunk);

        _lastCoord = coord;
        _lastChunk = chunk;

        var (lx, ly) =
            WorldToLocal(
                worldX,
                worldY);

        return (
            chunk,
            lx,
            ly);
    }

    // =====================================================
    // COORDS
    // =====================================================

    public BlackyChunkCoord
        WorldToChunkCoord(
            int worldX,
            int worldY)
    {
        return new BlackyChunkCoord(
            FloorDiv(
                worldX,
                ChunkSize),

            FloorDiv(
                worldY,
                ChunkSize)
        );
    }

    public (int x, int y)
        WorldToLocal(
            int worldX,
            int worldY)
    {
        return (
            Mod(worldX, ChunkSize),
            Mod(worldY, ChunkSize)
        );
    }

    // =====================================================
    // SAFE MATH
    // =====================================================

    private static int FloorDiv(
        int a,
        int b)
    {
        int result = a / b;

        if ((a ^ b) < 0 &&
            (result * b != a))
        {
            result--;
        }

        return result;
    }

    private static int Mod(
        int a,
        int b)
    {
        int result = a % b;

        if (result < 0)
            result += b;

        return result;
    }
}