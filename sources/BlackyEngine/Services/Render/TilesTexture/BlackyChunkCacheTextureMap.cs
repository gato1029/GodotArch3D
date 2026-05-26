using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;


public struct TileChange
{
    public int WorldX;
    public int WorldY;
    public int Height;
    public int Layer;
    public ushort TileId;
    public BlackyRegion region;
    public bool remove;
    public bool dual; 
}

public class BlackyChunkCacheTextureMap
{
    public event Action<TileChange> OnTileChanged;
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

    public BlackyChunkCacheTextureMap(int chunkSize, int heightCount, int maxLayers)
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

        if (tileLayer.GetDualMask(localX, localY) != 0)
        {
            tileLayer.SetSolid(localX,localY,false);
            
        }
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

    public void RemoveTileDualInternal(int worldX, int worldY, int height, int layer, bool dual = false)
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

        if (tileLayer.GetDualMask(localX, localY) != 0)
        {
            tileLayer.SetSolid(localX, localY, false);
            
        }

        OnTileChanged?.Invoke(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            TileId = tileId,
            region = chunk.ParentRegion,
            remove = false,
            dual = false
        });
        
        chunk.MarkDirty();

    }
    public void SetTileDualInternal(int worldX, int worldY, int height, int layer, string modName, ushort textureIndex, bool dual = false)
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
            remove = false,
            dual = dual
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
    public void RemoveTileDual(
    int worldX,
    int worldY,
    int height,
    int layer,
    DualTileTemplate dualTileTemplate)
    {
        var (chunk, lx, ly) =
            ResolveOrCreate(worldX, worldY);

        var tileLayer =
            chunk.GetOrCreateLayer(height, layer);


        tileLayer.SetSolid(lx, ly, false);   
    }
    public void ApplyBrushRemoveDual(
    int baseX,
    int baseY,
    int altura,
    int capa,
    Brush brush,
    DualTileTemplate dualTileTemplate)
    {
        HashSet<(int x, int y)> affected = new();

        foreach (var offset in brush.Cells)
        {
            int x = baseX + offset.x;
            int y = baseY + offset.y;

            RemoveTileDual(x, y, altura, capa, dualTileTemplate);

            for (int oy = -1; oy <= 1; oy++)
                for (int ox = -1; ox <= 1; ox++)
                    affected.Add((x + ox, y + oy));
        }

        foreach (var p in affected)
        {
            RebuildDualCell(p.x, p.y, altura, capa, dualTileTemplate);
        }
    }

    public void ApplyBrushCreateDual(
    int baseX,
    int baseY,
    int altura,
    int capa,
    Brush brush,
    DualTileTemplate dualTileTemplate)
    {
        HashSet<(int x, int y)> affected = new();

        foreach (var offset in brush.Cells)
        {
            int x = baseX + offset.x;
            int y = baseY + offset.y;

            SetTileDual(x, y, altura, capa, dualTileTemplate,true);

            for (int oy = -1; oy <= 1; oy++)
                for (int ox = -1; ox <= 1; ox++)
                    affected.Add((x + ox, y + oy));
        }

        foreach (var p in affected)
        {
            RebuildDualCell(p.x, p.y, altura, capa, dualTileTemplate);
        }
    }
    public void SetTileDual(
    int worldX,
    int worldY,
    int height,
    int layer,
    DualTileTemplate dualTileTemplate, bool isInternal=false)
    {
        var (chunk, lx, ly) =
            ResolveOrCreate(worldX, worldY);

        var tileLayer =
            chunk.GetOrCreateLayer(height, layer);

        tileLayer.SetSolid(lx, ly, true);
        if (!isInternal)
        {
            RebuildDualNeighborhood(
            worldX,
            worldY,
            height,
            layer,
            dualTileTemplate);
        }
        
    }

    public static class DualMask
    {
        // Orden visual:
        //
        // TL TR
        // BL BR
        //
        // Bits:
        //
        // 8 4
        // 2 1

        public const byte BottomRight = 1;
        public const byte BottomLeft = 2;
        public const byte TopRight = 4;
        public const byte TopLeft = 8;
    }
    public void RebuildDualNeighborhood(
    int x,
    int y,
    int height,
    int layer,
    DualTileTemplate dualTileTemplate)
    {
        //RebuildDualCell(x, y, height, layer, dualTileTemplate);
        //RebuildDualCell(x - 1, y, height, layer, dualTileTemplate);       
        //RebuildDualCell(x, y - 1, height, layer, dualTileTemplate);        
        //RebuildDualCell(x - 1, y - 1, height, layer, dualTileTemplate);

        for (int oy = -1; oy <= 1; oy++)
            for (int ox = -1; ox <= 1; ox++)
            {
                RebuildDualCell(x + ox, y + oy, height, layer, dualTileTemplate);
            }

    }
    public void RebuildDualCell(
    int vx,
    int vy,
    int height,
    int layer,
    DualTileTemplate dualTileTemplate)
    {   // El tile visual NO existe
        byte mask = 0;

        // TL
        if (IsSolidGlobal(vx, vy + 1, height, layer))
            mask |= DualMask.TopLeft;

        // TR
        if (IsSolidGlobal(vx + 1, vy + 1, height, layer))
            mask |= DualMask.TopRight;

        // BL
        if (IsSolidGlobal(vx, vy, height, layer))
            mask |= DualMask.BottomLeft;

        // BR
        if (IsSolidGlobal(vx + 1, vy, height, layer))
            mask |= DualMask.BottomRight;
        
        var (chunk, lx, ly) =
            ResolveOrCreate(vx, vy);

        var tileLayer =
            chunk.GetOrCreateLayer(height, layer);

        int lastMask = tileLayer.GetDualMask(lx, ly);
        tileLayer.SetDualMask(lx, ly, (byte)mask);

        bool markDelete = false;
        if (mask == 0)
        {
            //RemoveTile(vx, vy, height, layer);            
            //return;
            markDelete = true;
        }

        var slot = dualTileTemplate.GetSlot(mask);
        if (markDelete)
        {
            slot = dualTileTemplate.GetSlot(lastMask);
        }
        
        var slotHeight =
            slot.GetGeneric();

        if (slot.HasData(height))
        {
            slotHeight =
                slot.GetData(height);
        }

        for (int i = 0; i < slotHeight.Parts.Count; i++)
        {         
            var item = slotHeight.Parts[i];

            if (markDelete)
            {
                RemoveTileDualInternal(vx, vy-i, height, layer,true);
            }
            else
            {
                SetTileDualInternal(
                     vx,
                     vy - i,
                     height,
                     layer,
                     item.IdMod,
                     (ushort)item.TileIndex, true
                 );
            }
         
        }
    }
    public bool IsSolidGlobal(
    int worldX,
    int worldY,
    int height,
    int layer)
    {
        var (chunk, lx, ly) = Resolve(worldX, worldY);

        if (chunk == null)
            return false;

        if (!chunk.TryGetHeight(height, out var h))
            return false;

        if (!h.TryGetLayer(layer, out var l))
            return false;

        return l.IsSolid(lx, ly);
    }
}
