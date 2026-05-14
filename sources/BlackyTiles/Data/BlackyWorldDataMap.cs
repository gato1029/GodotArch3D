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
    Superficie = 1, // esto si puede cambiar poner hierba, tierra de cultivo, etc
    Camino = 2, // para caminos, esto es para que se renderice por encima de la superficie pero debajo de las entidades
    Adornos = 3, // para adornos sobre la superficie, como flores, piedritas, partes de edificios, etc, pero no tienen entidad, esto es para que se renderice por encima de la superficie pero debajo de las entidades
                 // los recursos edificios, personajes, entidades se renderizaran por encima de todo esto, en su propia capa de renderizado, para que se rendericen por encima de todo esto, y asi no se vean tapados por nada del terreno
}
public struct BlackyWorldCellChange
{
    public int WorldX;
    public int WorldY;

    public int Height;

    public BlackyRenderLayer RenderLayer;
}
public abstract class BlackyWorldDataMap<T>
       where T : struct
{
    // =====================================================
    // CHUNKS
    // =====================================================

    protected readonly Dictionary<
        BlackyChunkCoord,
        BlackyChunkData<T>>
        _chunks = new();

    // =====================================================
    // VISUAL CACHE
    // =====================================================

    protected readonly BlackyChunkTextureMap
        _textureMap;

    // =====================================================
    // CONFIG
    // =====================================================

    public int ChunkSize { get; }

    public BlackyRenderLayer
        RenderLayer
    { get; }

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
        BlackyChunkTextureMap textureMap)
    {
        ChunkSize = chunkSize;

        RenderLayer = renderLayer;

        _textureMap = textureMap;
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