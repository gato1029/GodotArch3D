using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;

public struct DirtyTile
{
    public int X;
    public int Y;
    public ushort Id;
    public DirtyTile(int x, int y, ushort id)
    {
        X = x;
        Y = y;
        Id = id;
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
public struct TileChange
{
    public int WorldX;
    public int WorldY;
    public int Height;
    public int Layer;
    public int SpriteId;
    public BlackyRegion region;
    public bool remove;
    public bool dual; 
    public bool isPersistent;
}

public class BlackyChunkCacheTextureMap
{
    public event Action<TileChange> OnTileChanged;
    private readonly ConcurrentDictionary<BlackyChunkCoord, BlackyChunkTexture> _chunks = new();
    private readonly BlackyWorldRegions _regions;
    private readonly ChunkManagerBase chunkManager;
    public int ChunkSize { get; }
    public int HeightCount { get; }
    public int MaxLayers { get; }

    // Tamaño de región: 16x16 chunks. 
    // Usamos bit shift (>> 4) porque 2^4 = 16. Es mucho más rápido que la división.
    private const int RegionShift = 4;
    private const int ChunksPerRegionSide = 16; // no se usa por el momento

    [ThreadStatic]
    private static BlackyChunkCoord _lastCoord;

    [ThreadStatic]
    private static BlackyChunkTexture _lastChunk;
    
    public object SyncRoot { get; } = new();

    public BlackyChunkCacheTextureMap(int chunkSize, int heightCount, int maxLayers, BlackyWorldRegions regions, ChunkManagerBase chunkManager)
    {
        ChunkSize = chunkSize;
        HeightCount = heightCount;
        MaxLayers = maxLayers;
        _regions = regions;
        this.chunkManager = chunkManager;
        chunkManager.OnChunkDataUnload += ChunkManager_OnChunkDataUnload;
    }

    private void ChunkManager_OnChunkDataUnload(Vector2I obj)
    {
        
        BlackyChunkCoord coord = new BlackyChunkCoord(obj.X, obj.Y);
        if (_chunks.TryGetValue(coord, out var chunkCurrent))
        {
            GD.Print($"[BlackyChunkCacheTextureMap] Unloading chunk at {coord.X}, {coord.Y}");
            _chunks.TryRemove(coord, out _);
        }
    }


    // ===============================
    // GESTIÓN DE CHUNKS
    // ===============================


    public BlackyChunkTexture GetOrCreateChunk(int chunkX, int chunkY)
    {
        var coord = new BlackyChunkCoord(chunkX, chunkY);

        return _chunks.GetOrAdd(coord, c =>
        {
            var region =
                _regions.GetOrCreateRegionByChunk(
                    c.X,
                    c.Y);

            var chunk =
                new BlackyChunkTexture(
                    c,
                    region,
                    ChunkSize,
                    HeightCount,
                    MaxLayers);

            region.RegisterChunk(c);

            return chunk;
        });
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

    public void RemoveTileSprite(int worldX, int worldY, int height, int layer)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = ResolveOrCreate(worldX, worldY);

        // (Asumiendo que tu BlackyChunkTexture tiene GetOrCreateLayer)
        var tileLayer = chunk.GetOrCreateLayer(height, layer);

        //if (tileLayer.GetDualMask(localX, localY) != 0)
        //{
        //    tileLayer.SetSolid(localX,localY,false);
            
        //}
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

    public void RemoveTileDualInternal(int worldX, int worldY, int height, int layer)
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
    public int SetTileSprite(int worldX, int worldY, int height, int layer, long idTileSprite, bool offsetDual)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = ResolveOrCreate(worldX, worldY);

        // 2. Le pedimos a la región del chunk que nos dé un ID de su paleta
        //ushort tileId = chunk.ParentRegion.GetOrCreateTile(modName, textureIndex);

        int tileId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);
        // (Asumiendo que tu BlackyChunkTexture tiene GetOrCreateLayer)
        


        // 3. Guardamos el ID en el chunk
        // (Asumiendo que tu BlackyChunkTexture tiene GetOrCreateLayer)
        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.SetTile(localX, localY, tileId);
        tileLayer.SetRender(localX, localY, true);

        //if (tileLayer.GetDualMask(localX, localY) != 0)
        //{
        //    tileLayer.SetSolid(localX, localY, false);

        //}

        OnTileChanged?.Invoke(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            SpriteId = tileId,
            region = chunk.ParentRegion,
            remove = false,
            dual = offsetDual,
            isPersistent = false
        });

        chunk.MarkDirty();
        return tileId;

    }

    public ushort SetTile(int worldX, int worldY, int height, int layer, string modName, ushort textureIndex, bool DualOffset)
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
            SpriteId = tileId,
            region = chunk.ParentRegion,
            remove = false,
            dual = DualOffset,
            isPersistent = true
        });
        
        chunk.MarkDirty();
        return tileId;

    }

    public void SetTileDualInternalSprite(int worldX, int worldY, int height, int layer, long idTileSprite, bool isBorder)
    {
        
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = ResolveOrCreate(worldX, worldY);
        
        int tileId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);         
        // (Asumiendo que tu BlackyChunkTexture tiene GetOrCreateLayer)
        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.SetTile(localX, localY, tileId);
        tileLayer.SetRender(localX, localY, true);
        
        if (height-1 >=0 && isBorder==false) // solo si no es borde y la altura inferior es mayor a 0 no se marca para renderizar
        {
            var layerDown = chunk.GetOrCreateLayer(height - 1, layer);
            var tileDown = layerDown.GetTile(localX, localY);
            if (tileDown!=0 && layerDown.IsRender(localX,localY))
            {
                layerDown.SetRender(localX, localY, false);
                OnTileChanged?.Invoke(new TileChange
                {
                    WorldX = worldX,
                    WorldY = worldY,
                    Height = height - 1,
                    Layer = layer,
                    remove = true
                });
            }            
        }
        


        OnTileChanged?.Invoke(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            SpriteId = tileId,
            region = chunk.ParentRegion,
            remove = false,
            dual = true,
            isPersistent = false,

        });

        chunk.MarkDirty();


    }

    public void SetTileDualInternal(int worldX, int worldY, int height, int layer, string modName, ushort textureIndex, bool dual = false)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = ResolveOrCreate(worldX, worldY);

        // 2. Le pedimos a la región del chunk que nos dé un ID de su paleta
        ushort tileId = chunk.ParentRegion.GetOrCreateTile(modName, textureIndex,false);

        // 3. Guardamos el ID en el chunk        
        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.SetTile(localX, localY, tileId);

        OnTileChanged?.Invoke(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            SpriteId = tileId,
            region = chunk.ParentRegion,
            remove = false,
            dual = dual,
            isPersistent = false

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
    int layer)
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

            RemoveTileDual(x, y, altura, capa);

            for (int oy = -1; oy <= 1; oy++)
                for (int ox = -1; ox <= 1; ox++)
                    affected.Add((x + ox, y + oy));
        }

        foreach (var p in affected)
        {
        
            RebuildDualCell(p.x, p.y, altura, capa, dualTileTemplate);
            RefreshTileUnder(p.x, p.y, altura, capa);
        }
    }

    private void RefreshTileUnder(int worldX, int worldY, int height, int layer)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = ResolveOrCreate(worldX, worldY);    
        if (height - 1 >= 0)
        {
            var LayerDown = chunk.GetOrCreateLayer(height - 1, layer);
            var tileDown = LayerDown.GetTile(localX, localY);
            if (tileDown != 0 && LayerDown.IsRender(localX, localY) == false)
            {
                LayerDown.SetRender(localX, localY, true);
                OnTileChanged?.Invoke(new TileChange
                {
                    WorldX = worldX,
                    WorldY = worldY,
                    Height = height - 1,
                    Layer = layer,
                    SpriteId = tileDown,
                    region = chunk.ParentRegion,
                    remove = false,
                    dual = true,
                    isPersistent = false,
                });
            }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long MakeKey(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }
    private int Index(int x, int y)
    {
        return x + y * ChunkSize;
    }
    public void ApplyChunkBatch(int chunkX, int chunkY, int height, int layer, SerializerCellGeneric[] cells)
    {
        var chunk = GetOrCreateChunk(chunkX, chunkY);
        DualTileTemplate ultimoTemplate = null;
        lock (chunk.SyncRoot)
        {
            HashSet<long> visited = new();
            List<DirtyTile> dirtyTiles = new();

            var tileLayer = chunk.GetOrCreateLayer(height, layer);

            
            for (int y = 0; y < ChunkSize; y++)
            {                
                for (int x = 0; x < ChunkSize; x++)
                {
                    int i = Index(x, y);
                    ref readonly var cell = ref cells[i];

                    if (cell.id == 0)
                        continue;

                    tileLayer.SetSolid(x, y, true);

                    int worldX = chunkX * ChunkSize + x;
                    int worldY = chunkY * ChunkSize + y;

                    for (int oy = -1; oy <= 1; oy++)
                    {
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            int nx = worldX + ox;
                            int ny = worldY + oy;

                            long key = MakeKey(nx, ny);

                            if (visited.Add(key))
                            {
                                dirtyTiles.Add(new DirtyTile(nx, ny, cell.id));
                                GD.Print($"Pos: {nx}, {ny}");
                            }
                        }
                    }
                }
            }
            // Optimización: Si ChunkSize es 16, el compilador puede optimizar esto muy bien.
            //for (int y = 0; y < ChunkSize; y++)
            //{
            //    int rowOffset = y * ChunkSize;
            //    for (int x = 0; x < ChunkSize; x++)
            //    {
            //        ref readonly var cell = ref cells[rowOffset + x];
            //        if (cell.id == 0) continue;

            //        // Marcamos sólido
            //        tileLayer.SetSolid(x, y, true);
            //    }
            //}

            //// Fase B: Dual
            //for (int y = 0; y < ChunkSize; y++)
            //{
            //    int rowOffset = y * ChunkSize;
            //    for (int x = 0; x < ChunkSize; x++)
            //    {
            //        ref readonly var cell = ref cells[rowOffset + x];
            //        if (cell.id == 0) continue;

            //        // OPTIMIZACIÓN: Cachea los datos de la paleta/template 
            //        // Si el mismo ID se repite mucho (ej. un suelo de pasto), 
            //        // no busques en el diccionario 256 veces.
            //        var data = BlackyPalletesPersistence.terrainPalette.GetData(cell.id);
            //        var dualTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);

            //        int worldX = (chunkX * ChunkSize) + x;
            //        int worldY = (chunkY * ChunkSize) + y;

            //        UpdateDualVisualInternal(worldX, worldY, x, y, height, layer, dualTemplate, tileLayer);
            //    }
            //}

            // Fase B: Dual (Incluyendo vecinos de borde)
            // Recorremos desde -1 hasta ChunkSize para abarcar el borde exterior
            // Fase B: Dual

            foreach (ref readonly var tile in CollectionsMarshal.AsSpan(dirtyTiles))
            {
                var data = BlackyPalletesPersistence.terrainPalette.GetData(tile.Id);
                ultimoTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);
                var (targetChunk, lx, ly) = ResolveOrCreate(tile.X, tile.Y);
                
                UpdateDualVisualInternal(tile.X, tile.Y, lx, ly, height, layer, ultimoTemplate, targetChunk.GetOrCreateLayer(height, layer), true);
            }
            //for (int y = 0; y < ChunkSize; y++)
            //{
            //    for (int x = 0; x < ChunkSize; x++)
            //    {
            //        int index = y * ChunkSize + x;
            //        var cell = cells[index];

            //        if (cell.id != 0)
            //        {
            //            // Es sólido: calculamos visual normal
            //            var data = BlackyPalletesPersistence.terrainPalette.GetData(cell.id);
            //            ultimoTemplate = AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);

            //            int worldX = (chunkX * ChunkSize) + x;
            //            int worldY = (chunkY * ChunkSize) + y;

            //            UpdateDualVisualInternal(worldX, worldY, x, y, height, layer, ultimoTemplate, tileLayer, false);
            //        }
            //        else if (ultimoTemplate != null)
            //        {
            //            // Es vacío pero tiene template previo: calculamos borde
            //            int worldX = (chunkX * ChunkSize) + x;
            //            int worldY = (chunkY * ChunkSize) + y;

            //            UpdateDualVisualInternal(worldX, worldY, x, y, height, layer, ultimoTemplate, tileLayer, true);
            //        }
            //    }
            //}
            //if (ultimoTemplate != null)
            //{
            //    ProcesarBordes(chunkX, chunkY, height, layer, ultimoTemplate);
            //}
            chunk.MarkDirty();
        }
    }
    private void ProcesarBordes(int chunkX, int chunkY, int height, int layer, DualTileTemplate template)
    {
        // Definimos los rangos de borde: -1 y ChunkSize
        int[] bordes = { -1, ChunkSize };

        foreach (int bx in bordes)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                ProcesarBordeTile(chunkX * ChunkSize + bx, chunkY * ChunkSize + y, height, layer, template);
            }
        }
        foreach (int by in bordes)
        {
            for (int x = 0; x < ChunkSize; x++)
            {
                ProcesarBordeTile(chunkX * ChunkSize + x, chunkY * ChunkSize + by, height, layer, template);
            }
        }
    }

    private void ProcesarBordeTile(int worldX, int worldY, int height, int layer, DualTileTemplate template)
    {
        var (targetChunk, lx, ly) = ResolveOrCreate(worldX, worldY);
        if (targetChunk == null) return;

        // Solo nos importa si es un tile vacío que necesita borde
        // Si ya es sólido (id != 0), ya se procesó o se procesará en su propio ApplyChunkBatch
        //if (!IsSolidGlobal(worldX, worldY, height, layer))
        {
            lock (targetChunk.SyncRoot)
            {
                var targetLayer = targetChunk.GetOrCreateLayer(height, layer);
                UpdateDualVisualInternal(worldX, worldY, lx, ly, height, layer, template, targetLayer, true);
            }
        }
    }
  
    private void UpdateDualVisualInternal(int vx, int vy, int lx, int ly, int height, int layer, DualTileTemplate template, IBlackyChunkTilemapTexture tileLayer, bool verificarVecindad = false)
    {
        byte mask = 0;
        // Usamos IsSolidGlobal (que es lectura pura, segura en multihilo)
        //if (verificarVecindad)
        //{
 
            if (IsSolidGlobal(vx, vy + 1, height, layer)) mask |= DualMask.TopLeft;
            if (IsSolidGlobal(vx + 1, vy + 1, height, layer)) mask |= DualMask.TopRight;
            if (IsSolidGlobal(vx, vy, height, layer)) mask |= DualMask.BottomLeft;
            if (IsSolidGlobal(vx + 1, vy, height, layer)) mask |= DualMask.BottomRight;

            
        //}
        //else
        //{
        //    mask = 15;
        //}
        if (mask==0)
        {
            return; // no hacer nada
        }
        tileLayer.SetDualMask(lx, ly, mask);
        // ... aquí tu lógica de obtener slot y SetTile ...
        var slot = template.GetSlot(mask);
        var item = slot.GetGeneric().Parts[0];
        int tileId = AtlasModsManager.GetSpriteUniqueId(item.IdTileSpriteData);

        tileLayer.SetTile(lx, ly, tileId);
        tileLayer.SetRender(lx, ly, true);
    }
    public void SetTileDualConcurrent(int worldX,int worldY, int height, int layer, DualTileTemplate dualTileTemplate, bool isborder=false)
    {
        
        
        if (isborder)
        {
            // revisar alrededores
            for (int oy = -1; oy <= 1; oy++)
            {
                for (int ox = -1; ox <= 1; ox++)
                {
                    var (chunk, lx, ly) = ResolveOrCreate(worldX+ox, worldY+oy);
                    IBlackyChunkTilemapTexture tileLayer = chunk.GetOrCreateLayer(height, layer);
                    if (ox==0 || oy==0)
                    {
                        tileLayer.SetSolid(lx, ly, true);
                    }                                        
                    RebuildDualCellConcurrent(worldX+ox, worldY+oy, lx, ly, height, layer, dualTileTemplate, tileLayer);
                }
            }
            
        }
        else
        {
            var (chunk, lx, ly) = ResolveOrCreate(worldX, worldY);
            IBlackyChunkTilemapTexture tileLayer = chunk.GetOrCreateLayer(height, layer);
            tileLayer.SetSolid(lx, ly, true);
            tileLayer.SetRender(lx, ly, true);

            var slot = dualTileTemplate.GetSlot(15); // el 15 es el que representa que todos los tiles alrededor son sólidos, es decir, no hay borde
            var slotHeight = slot.GetGeneric();
            var item = slotHeight.Parts[0];
            int tileId = AtlasModsManager.GetSpriteUniqueId(item.IdTileSpriteData); // esto existe y solo se lee en modo lectura, no se modifica      
            tileLayer.SetTile(lx, ly, tileId);
        }
        
    }

    public void RebuildDualCellConcurrent(int vx, int vy,int lx,int ly, int height,int layer, DualTileTemplate dualTileTemplate,  IBlackyChunkTilemapTexture tileLayer)
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
              
        byte lastMask = tileLayer.GetDualMask(lx, ly);
        tileLayer.SetDualMask(lx, ly, mask);

        bool markDelete = false;
        if (mask == 0)
        {            
            markDelete = true;
        }

        var slot = dualTileTemplate.GetSlot(mask);
        
        if (markDelete)
        {
            slot = dualTileTemplate.GetSlot(lastMask);            
        }

        var slotHeight = slot.GetGeneric();

        var item = slotHeight.Parts[0];

        if (markDelete)
        {                     
            tileLayer.ClearTile(lx, ly);
        }
        else
        {
            int tileId = AtlasModsManager.GetSpriteUniqueId(item.IdTileSpriteData); // esto existe y solo se lee en modo lectura, no se modifica      
            tileLayer.SetTile(lx, ly, tileId);
            tileLayer.SetRender(lx, ly, true);
        }
      
    }

    public void SetTileDual(
    int worldX,
    int worldY,
    int height,
    int layer,
    DualTileTemplate dualTileTemplate, bool isInternal=false)
    {
        var (chunk, lx, ly) =  ResolveOrCreate(worldX, worldY);

        var tileLayer = chunk.GetOrCreateLayer(height, layer);

        tileLayer.SetSolid(lx, ly, true);       
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

        byte lastMask = tileLayer.GetDualMask(lx, ly);
        tileLayer.SetDualMask(lx, ly, (byte)mask);

        bool markDelete = false;
        if (mask == 0)
        {
            //RemoveTile(vx, vy, height, layer);            
            //return;
            markDelete = true;
        }

        var slot = dualTileTemplate.GetSlot(mask);
        bool isBorder = IsBorderDual(mask);
        if (markDelete)
        {
            slot = dualTileTemplate.GetSlot(lastMask);
            isBorder = IsBorderDual(lastMask);
        }
        
        var slotHeight = slot.GetGeneric();

        if (slot.HasData(height))
        {
            slotHeight = slot.GetData(height);
        }

        for (int i = 0; i < slotHeight.Parts.Count; i++)
        {         
            var item = slotHeight.Parts[i];

            if (markDelete)
            {
                RemoveTileDualInternal(vx, vy-i, height, layer);
            }
            else
            {
                SetTileDualInternalSprite(vx,vy - i,height,layer,item.IdTileSpriteData,isBorder);
                //SetTileDualInternal(
                //     vx,
                //     vy - i,
                //     height,
                //     layer,
                //     item.IdMod,
                //     (ushort)item.TileIndex, true
                // );
            }
         
        }
    }

    private bool IsBorderDual(byte mask)
    {
        if (mask == 15 || mask == 0)
        {
            return false;
        }
        return true;
    }

    public bool IsSolidGlobal(int worldX, int worldY, int height, int layer)
    {
        // 1. Resolvemos el chunk (sin crear si no existe)
        var (chunk, lx, ly) = Resolve(worldX, worldY);

        // 2. Si el chunk vecino no existe, devolvemos 'false' (o lo que consideres 'vacío')
        if (chunk == null)
            return false;

        // 3. Acceso a datos de lectura (esto es seguro)
        if (!chunk.TryGetHeight(height, out var h))
            return false;

        if (!h.TryGetLayer(layer, out var l))
            return false;

        // Nota: Como estamos en un entorno multihilo, 
        // l.IsSolid(lx, ly) debe ser una lectura atómica de un campo simple.
        return l.IsSolid(lx, ly);
    }
}
