using GodotEcsArch.sources.BlackyTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;

public readonly struct BlackyDualNodeRef
{
    public readonly BlackyChunkTexture Chunk;
    public readonly BlackyDualNodeTilemap DualLayer;
    public readonly int LocalX;
    public readonly int LocalY;

    public BlackyDualNodeRef(
        BlackyChunkTexture chunk,
        BlackyDualNodeTilemap dualLayer,
        int localX,
        int localY)
    {
        Chunk = chunk;
        DualLayer = dualLayer;
        LocalX = localX;
        LocalY = localY;
    }
}
public struct TileChange
{
    public int WorldX;
    public int WorldY;
    public int Height;
    public int Layer;
    public ushort TileId;
    public BlackyRegion region;
    public bool remove;
}

public class BlackyChunkTextureMap
{
    public event Action<TileChange> OnTileChanged;
    private readonly HashSet<(int x, int y, int h, int l, string modName)> _dirtyDualVisuals = new();

    private readonly Dictionary<BlackyChunkCoord, BlackyChunkTexture> _chunks = new();
    private readonly Dictionary<(int, int), BlackyRegion> _regions = new();

    public int ChunkSize { get; }
    public int HeightCount { get; }
    public int MaxLayers { get; }

    // Tamaño de región: 16x16 chunks. 
    // Usamos bit shift (>> 4) porque 2^4 = 16. Es mucho más rápido que la división.
    private const int RegionShift = 4;
    private const int ChunksPerRegionSide = 16; // no se usa por el momento

    // 🔥 CACHE SECUENCIAL
    private BlackyChunkCoord _lastCoord;
    private BlackyChunkTexture _lastChunk;

    public BlackyChunkTextureMap(int chunkSize, int heightCount, int maxLayers)
    {
        ChunkSize = chunkSize;
        HeightCount = heightCount;
        MaxLayers = maxLayers;
    }

    // ===============================
    // GESTIÓN DE REGIONES
    // ===============================

    public BlackyRegion GetOrCreateRegion(int regX, int regY)
    {
        var key = (regX, regY);
        if (!_regions.TryGetValue(key, out var region))
        {
            region = new BlackyRegion(regX, regY);
            _regions[key] = region;
            // Aquí podrías disparar la carga desde disco si el archivo existe
        }
        return region;
    }

    // ===============================
    // GESTIÓN DE CHUNKS
    // ===============================

    public BlackyChunkTexture GetOrCreateChunk(int chunkX, int chunkY)
    {
        var coord = new BlackyChunkCoord(chunkX, chunkY);

        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            // 1. Calcular a qué región pertenece este chunk
            int regX = chunkX >> RegionShift;
            int regY = chunkY >> RegionShift;

            // 2. Obtener o crear la región
            var region = GetOrCreateRegion(regX, regY);

            // 3. Crear el chunk pasándole su región padre
            chunk = new BlackyChunkTexture(
                coord,
                region, // <--- El chunk ahora sabe a qué región pertenece
                ChunkSize,
                HeightCount,
                MaxLayers);

            _chunks[coord] = chunk;

            // 4. Registrar el chunk en la región (para saber qué guardar luego)
            region.RegisterChunk(coord);
        }

        return chunk;
    }

    public bool TryGetChunk(int chunkX, int chunkY, out BlackyChunkTexture chunk)
    {
        return _chunks.TryGetValue(
            new BlackyChunkCoord(chunkX, chunkY),
            out chunk
        );
    }

    public IEnumerable<BlackyChunkTexture> GetLoadedChunks()
    {
        return _chunks.Values;
    }

    public IEnumerable<BlackyChunkTexture> GetDirtyChunks()
    {
        foreach (var chunk in _chunks.Values)
        {
            if (chunk.HasDirtyTiles())
                yield return chunk;
        }
    }

    // ===============================
    // TILE ACCESS
    // ===============================

    public void RemoveTile(int worldX, int worldY, int height, int layer)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = ResolveOrCreate(worldX, worldY);

        // (Asumiendo que tu BlackyChunkTexture tiene GetOrCreateLayer)
        var tileLayer = chunk.GetOrCreateLayer(height, layer);

        tileLayer.ClearTile(localX, localY);
        OnTileChanged?.Invoke(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            remove = true
        });
    }
    public void SetTile(int worldX, int worldY, int height, int layer, string modName, ushort textureIndex)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = ResolveOrCreate(worldX, worldY);

        // 2. Le pedimos a la región del chunk que nos dé un ID de su paleta
        ushort tileId = chunk.ParentRegion.GetOrCreateTile(modName, textureIndex);

        // 3. Guardamos el ID en el chunk
        // (Asumiendo que tu BlackyChunkTexture tiene GetOrCreateLayer)
        var tileLayer = chunk.GetOrCreateLayer(height, layer);


        tileLayer.SetTile(localX, localY, tileId);
        OnTileChanged?.Invoke(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            TileId = tileId,
            region = chunk.ParentRegion,
            remove = false
        });

        chunk.MarkDirty();

    }
    // ===============================
    // COORDINADAS Y RESOLUCIÓN
    // ===============================

    public (BlackyChunkTexture chunk, int localX, int localY) ResolveOrCreate(int worldX, int worldY)
    {
        var coord = WorldToChunkCoord(worldX, worldY);

        if (_lastChunk != null && coord.Equals(_lastCoord))
        {
            var (lx, ly) = WorldToLocal(worldX, worldY);
            return (_lastChunk, lx, ly);
        }

        var chunk = GetOrCreateChunk(coord.X, coord.Y);

        _lastCoord = coord;
        _lastChunk = chunk;

        var (localX, localY) = WorldToLocal(worldX, worldY);
        return (chunk, localX, localY);
    }
    public int GetTile(
        int worldX,
        int worldY,
        int height,
        int layer)
    {
        var (chunk, localX, localY) = Resolve(worldX, worldY);

        if (chunk == null)
            return 0;

        if (!chunk.TryGetHeight(height, out var h))
            return 0;

        if (!h.TryGetLayer(layer, out var l))
            return 0;

        return l.GetTile(localX, localY);
    }

    // ===============================
    // 🔥 MULTI-CHUNK BLOCK WRITE
    // ===============================

    public void SetTilesBlock(
      int startX,
      int startY,
      int width,
      int height,
      int heightLevel,
      int layer,
      string modName,      // 👈 Ahora recibimos la identidad del tile
      ushort textureIndex) // 👈 Y su índice en el atlas
    {
        int endX = startX + width;
        int endY = startY + height;

        int startChunkX = FloorDiv(startX, ChunkSize);
        int endChunkX = FloorDiv(endX - 1, ChunkSize);

        int startChunkY = FloorDiv(startY, ChunkSize);
        int endChunkY = FloorDiv(endY - 1, ChunkSize);

        for (int cy = startChunkY; cy <= endChunkY; cy++)
        {
            for (int cx = startChunkX; cx <= endChunkX; cx++)
            {
                var chunk = GetOrCreateChunk(cx, cy);

                // 🔥 CRUCIAL: Obtener el ID específico para la región de este chunk
                // Así nos aseguramos que si el bloque cruza fronteras de región,
                // el ID sea el correcto para cada una.
                ushort localId = chunk.ParentRegion.GetOrCreateTile(modName, textureIndex);

                var tilemap = chunk.GetOrCreateLayer(heightLevel, layer);

                int chunkWorldX = cx * ChunkSize;
                int chunkWorldY = cy * ChunkSize;

                int localStartX = Math.Max(startX - chunkWorldX, 0);
                int localStartY = Math.Max(startY - chunkWorldY, 0);

                int localEndX = Math.Min(endX - chunkWorldX, ChunkSize);
                int localEndY = Math.Min(endY - chunkWorldY, ChunkSize);

                // Pintamos usando el ID local resuelto para esta región
                tilemap.FillRectLocal(
                    localStartX,
                    localStartY,
                    localEndX,
                    localEndY,
                    localId); // 👈 ID dinámico por región

                chunk.MarkDirty();
            }
        }
    }



    public (BlackyChunkTexture chunk, int localX, int localY)
        Resolve(int worldX, int worldY)
    {
        var coord = WorldToChunkCoord(worldX, worldY);

        if (_lastChunk != null && coord.Equals(_lastCoord))
        {
            var (lx, ly) = WorldToLocal(worldX, worldY);
            return (_lastChunk, lx, ly);
        }

        _chunks.TryGetValue(coord, out var chunk);

        _lastCoord = coord;
        _lastChunk = chunk;

        var (localX, localY) = WorldToLocal(worldX, worldY);

        return (chunk, localX, localY);
    }

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

    // ===============================
    // SAFE MATH
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

    // dual

    public BlackyDualNodeRef ResolveDualNodeOrCreate(
        int worldNodeX,
        int worldNodeY,
        int height,
        int layer)
    {
        var coord = WorldToChunkCoord(worldNodeX, worldNodeY);

        var chunk = GetOrCreateChunk(coord.X, coord.Y);

        int localX = Mod(worldNodeX, ChunkSize);
        int localY = Mod(worldNodeY, ChunkSize);

        var dualLayer = chunk.GetOrCreateDualLayer(height, layer);

        return new BlackyDualNodeRef(chunk, dualLayer, localX, localY);
    }

    public bool ResolveDualNode(
    int worldNodeX,
    int worldNodeY,
    int height,
    int layer,
    out BlackyDualNodeRef nodeRef)
    {
        var coord = WorldToChunkCoord(worldNodeX, worldNodeY);

        if (!_chunks.TryGetValue(coord, out var chunk))
        {
            nodeRef = default;
            return false;
        }

        if (!chunk.TryGetHeight(height, out var h))
        {
            nodeRef = default;
            return false;
        }

        if (!h.TryGetDualLayer(layer, out var dualLayer))
        {
            nodeRef = default;
            return false;
        }

        int localX = Mod(worldNodeX, ChunkSize);
        int localY = Mod(worldNodeY, ChunkSize);

        nodeRef = new BlackyDualNodeRef(chunk, dualLayer, localX, localY);
        return true;
    }

    public byte GetGlobalDualNode(
    int worldNodeX,
    int worldNodeY,
    int height,
    int layer)
    {
        if (!ResolveDualNode(worldNodeX, worldNodeY, height, layer, out var nodeRef))
            return 0;

        return nodeRef.DualLayer.GetNode(nodeRef.LocalX, nodeRef.LocalY);
    }
    public void SetGlobalDualNode(
    int worldNodeX,
    int worldNodeY,
    int height,
    int layer,
    byte value,
    string modName)
    {
        var nodeRef = ResolveDualNodeOrCreate(worldNodeX, worldNodeY, height, layer);

        nodeRef.DualLayer.SetNode(nodeRef.LocalX, nodeRef.LocalY, value);
        nodeRef.Chunk.MarkDirty();

        for (int oy = -1; oy <= 0; oy++)
        {
            for (int ox = -1; ox <= 0; ox++)
            {
                _dirtyDualVisuals.Add((
                    worldNodeX + ox,
                    worldNodeY + oy,
                    height,
                    layer,
                    modName));
            }
        }
    }
    public int GetDualMaskGlobal(
    int visualX,
    int visualY,
    int height,
    int layer)
    {
        int mask = 0;

        if (GetGlobalDualNode(visualX, visualY, height, layer) != 0) mask |= 1;
        if (GetGlobalDualNode(visualX + 1, visualY, height, layer) != 0) mask |= 2;
        if (GetGlobalDualNode(visualX, visualY + 1, height, layer) != 0) mask |= 4;
        if (GetGlobalDualNode(visualX + 1, visualY + 1, height, layer) != 0) mask |= 8;

        return mask;
    }

    public void RebuildDualVisualTile(
    int visualX,
    int visualY,
    int height,
    int layer,
    string modName)
    {
        int spriteIndex = GetDualMaskGlobal(visualX, visualY, height, layer);

        var (chunk, localX, localY) = ResolveOrCreate(visualX, visualY);

        ushort tileId = chunk.ParentRegion.GetOrCreateTile(modName, (ushort)spriteIndex);

        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.SetTile(localX, localY, tileId);

        chunk.MarkDirty();

        OnTileChanged?.Invoke(new TileChange
        {
            WorldX = visualX,
            WorldY = visualY,
            Height = height,
            Layer = layer,
            TileId = tileId,
            region = chunk.ParentRegion,
            remove = false
        });
    }

    public void FlushDualVisualChanges()
    {
        foreach (var d in _dirtyDualVisuals)
        {
            RebuildDualVisualTile(d.x, d.y, d.h, d.l, d.modName);
        }

        _dirtyDualVisuals.Clear();
    }

    public void SetDualNodeSolid(
    int worldNodeX,
    int worldNodeY,
    int height,
    int layer,
    string modName)
    {
        SetGlobalDualNode(worldNodeX, worldNodeY, height, layer, 1, modName);
    }
    public void ClearDualNode(
    int worldNodeX,
    int worldNodeY,
    int height,
    int layer,
    string modName)
    {
        SetGlobalDualNode(worldNodeX, worldNodeY, height, layer, 0, modName);
    }

    public void PaintDualCell(
    int worldX,
    int worldY,
    int height,
    int layer,
    string modName)
    {
        SetGlobalDualNode(worldX, worldY, height, layer, 1, modName);
        SetGlobalDualNode(worldX + 1, worldY, height, layer, 1, modName);
        SetGlobalDualNode(worldX, worldY + 1, height, layer, 1, modName);
        SetGlobalDualNode(worldX + 1, worldY + 1, height, layer, 1, modName);
    }
    public void ClearDualCell(
    int worldX,
    int worldY,
    int height,
    int layer,
    string modName)
    {
        SetGlobalDualNode(worldX, worldY, height, layer, 0, modName);
        SetGlobalDualNode(worldX + 1, worldY, height, layer, 0, modName);
        SetGlobalDualNode(worldX, worldY + 1, height, layer, 0, modName);
        SetGlobalDualNode(worldX + 1, worldY + 1, height, layer, 0, modName);
    }

    //PaintDualCell(10,5,...);
    //FlushDualVisualChanges();

    public void PaintDualBlock(
    int startX,
    int startY,
    int width,
    int height,
    int heightLevel,
    int layer,
    string modName)
    {
        int endX = startX + width;
        int endY = startY + height;

        // =========================
        // 1. Pintar malla nodal envolvente
        // =========================
        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                SetGlobalDualNode(x, y, heightLevel, layer, 1, modName);
            }
        }

        // =========================
        // 2. Flush visual único
        // =========================
        FlushDualVisualChanges();
    }

    public void ClearDualBlock(
    int startX,
    int startY,
    int width,
    int height,
    int heightLevel,
    int layer,
    string modName)
    {
        int endX = startX + width;
        int endY = startY + height;

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                SetGlobalDualNode(x, y, heightLevel, layer, 0, modName);
            }
        }

        FlushDualVisualChanges();
    }
}
