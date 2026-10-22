
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;

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

/// <summary>
/// Construcción y pintado de tiles duales (terreno, superficie, caminos).
///
/// No posee estado propio de chunks: escribe sobre la caché compartida con
/// <see cref="BlackySimpleTileTextureMap"/> y es el único dueño de los
/// colliders y de los visuales derivados de la máscara dual.
/// </summary>
public sealed class BlackyDualTileTextureMap
{
    private const bool DEBUG_COLLIDER = true;
    private readonly Dictionary<int, List<int>> _colliderDebugMap = new();

    private readonly BlackyChunkTextureCache _cache;
    private readonly BlackyWorldHeight _heightMapWorld;
    private readonly StaticSpatialChunkGrid<ColliderSpriteInstanceData> _staticSpatialTerrain;

    public BlackyDualTileTextureMap(
        BlackyChunkTextureCache cache,
        BlackyWorldHeight heightMapWorld,
        StaticSpatialChunkGrid<ColliderSpriteInstanceData> staticSpatialTerrain)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _heightMapWorld = heightMapWorld ?? throw new ArgumentNullException(nameof(heightMapWorld));
        _staticSpatialTerrain = staticSpatialTerrain ?? throw new ArgumentNullException(nameof(staticSpatialTerrain));
    }

    public BlackyChunkTextureCache Cache => _cache;

    // ===============================
    // TILE ACCESS
    // ===============================

    public void RemoveTileDualInternal(int worldX, int worldY, int height, int layer)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = _cache.ResolveOrCreate(worldX, worldY);

        _heightMapWorld.SetTopHeight(worldX, worldY, height - 1);

        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.ClearTile(localX, localY);

        RemoverCollider(tileLayer, localX, localY);

        _cache.RaiseTileChanged(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            remove = true
        });
    }

    private void RemoverCollider(IBlackyChunkTilemapTexture tileLayer, int localX, int localY)
    {
        int idCollider = tileLayer.GetIdCollider(localX, localY);
        if (idCollider != 0)
        {
            if (DEBUG_COLLIDER)
            {
                foreach (var idDebugBody in _colliderDebugMap[idCollider])
                {
                    CollisionShapeDraw.Instance.FreeDraw(idDebugBody);
                }

                _colliderDebugMap.Remove(idCollider);
            }

            tileLayer.SetIdCollider(localX, localY, 0);
            _staticSpatialTerrain.FreeCollider(idCollider);
        }
    }

    public void SetTileDualInternalSprite(int worldX, int worldY, int height, int layer, long idTileSprite, bool isBorder)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = _cache.ResolveOrCreate(worldX, worldY);

        int tileId = AtlasModsManager.GetSpriteUniqueId(idTileSprite, out TileSpriteData tileSpriteData);

        _heightMapWorld.SetTopHeight(worldX, worldY, height);

        var tileLayer = chunk.GetOrCreateLayer(height, layer);

        RemoverCollider(tileLayer, localX, localY);

        int idCollider = AsignarCollider(worldX, worldY, tileId, tileSpriteData);
        tileLayer.SetTile(localX, localY, tileId, idCollider);
        tileLayer.SetRender(localX, localY, true);

        _cache.RaiseTileChanged(new TileChange
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
        var (chunk, localX, localY) = _cache.ResolveOrCreate(worldX, worldY);

        // 2. Le pedimos a la región del chunk que nos dé un ID de su paleta
        ushort tileId = chunk.ParentRegion.GetOrCreateTile(modName, textureIndex, false);

        // 3. Guardamos el ID en el chunk
        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.SetTile(localX, localY, tileId, 0);

        _cache.RaiseTileChanged(new TileChange
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
    // DUAL
    // ===============================

    public void RemoveTileDual(int worldX, int worldY, int height, int layer)
    {
        var (chunk, lx, ly) = _cache.ResolveOrCreate(worldX, worldY);

        var tileLayer = chunk.GetOrCreateLayer(height, layer);

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
            //RefreshTileUnder(p.x, p.y, altura, capa);
        }
    }

    private void RefreshTileUnder(int worldX, int worldY, int height, int layer)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = _cache.ResolveOrCreate(worldX, worldY);
        if (height - 1 >= 0)
        {
            var LayerDown = chunk.GetOrCreateLayer(height - 1, layer);
            var tileDown = LayerDown.GetTile(localX, localY);
            if (tileDown != 0 && LayerDown.IsRender(localX, localY) == false)
            {
                LayerDown.SetRender(localX, localY, true);
                _cache.RaiseTileChanged(new TileChange
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

    public void ApplyBrushCreateDual(int baseX, int baseY, int altura, int capa, Brush brush, DualTileTemplate dualTileTemplate)
    {
        HashSet<(int x, int y)> affected = new();

        foreach (var offset in brush.Cells)
        {
            int x = baseX + offset.x;
            int y = baseY + offset.y;

            SetTileDual(x, y, altura, capa, dualTileTemplate, true);

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
    private static void DecodeKey(long key, out int x, out int y)
    {
        // Recuperamos x desplazando los 32 bits de vuelta a la derecha
        x = (int)(key >> 32);

        // Recuperamos y convirtiendo a uint para limpiar los bits de x y luego a int
        y = (int)(uint)(key & 0xFFFFFFFF);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long MakeKey(int x, int y)
    {
        return ((long)x << 32) | (uint)y;
    }

    /// <summary>
    /// Construye el mapa dual completo en tres fases: primero registra la solidez de todos los chunks;
    /// después reúne la vecindad de los bordes; finalmente calcula y escribe cada tile visual una sola vez.
    /// </summary>
    public void ApplyWorldBatchDualTiles(
        IEnumerable<(BlackyChunkCoord Coord, int Height, ReadOnlyMemory<SerializerCellGeneric> Cells)> blocks,
        int layer)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));

        var borderVisuals = new Dictionary<int, Dictionary<long, ushort>>();

        // Fase 1: solidez e interiores. El camino sólido usa máscara 15 y no consulta vecinos.
        foreach (var block in blocks)
        {
            var chunk = _cache.GetOrCreateChunk(block.Coord.X, block.Coord.Y);
            var tileLayer = chunk.GetOrCreateLayer(block.Height, layer);
            var cells = block.Cells.Span;

            for (int i = 0; i < cells.Length; i++)
            {
                ref readonly var cell = ref cells[i];
                int lx = i % _cache.ChunkSize;
                int ly = i / _cache.ChunkSize;
                tileLayer.SetSolid(lx, ly, cell.id != 0);

                if (cell.id == 0 || cell.isBorder) continue;

                int worldX = block.Coord.X * _cache.ChunkSize + lx;
                int worldY = block.Coord.Y * _cache.ChunkSize + ly;
                var template = GetTemplate((BlackyRenderLayer)layer, cell.id);
                UpdateDualVisualInternal(worldX, worldY, lx, ly, block.Height, layer, template, tileLayer);
            }

            chunk.MarkDirty();
        }

        // Fase 2: reunir la vecindad de las celdas de borde, deduplicada globalmente por altura.
        foreach (var block in blocks)
        {
            if (!borderVisuals.TryGetValue(block.Height, out var borders))
                borderVisuals[block.Height] = borders = new Dictionary<long, ushort>();

            var cells = block.Cells.Span;
            for (int i = 0; i < cells.Length; i++)
            {
                ref readonly var cell = ref cells[i];
                if (cell.id == 0 || !cell.isBorder) continue;

                int lx = i % _cache.ChunkSize;
                int ly = i / _cache.ChunkSize;
                int worldX = block.Coord.X * _cache.ChunkSize + lx;
                int worldY = block.Coord.Y * _cache.ChunkSize + ly;

                for (int oy = -1; oy <= 1; oy++)
                {
                    for (int ox = -1; ox <= 1; ox++)
                        borders.TryAdd(MakeKey(worldX + ox, worldY + oy), cell.id);
                }
            }
        }

        // Fase 3: dibujar bordes una vez. Los interiores ya renderizados conservan prioridad.
        foreach (var heightEntry in borderVisuals)
        {
            int height = heightEntry.Key;
            foreach (var entry in heightEntry.Value)
            {
                DecodeKey(entry.Key, out int worldX, out int worldY);
                var (targetChunk, lx, ly) = _cache.ResolveOrCreate(worldX, worldY);
                var targetLayer = targetChunk.GetOrCreateLayer(height, layer);
                if (targetLayer.IsRender(lx, ly)) continue;

                var template = GetTemplate((BlackyRenderLayer)layer, entry.Value);
                UpdateDualVisualInternal(worldX, worldY, lx, ly, height, layer, template, targetLayer, false);
                targetChunk.MarkDirty();
            }
        }
    }

    public void ApplyChunkBatchDualTiles(int chunkX, int chunkY, int height, int layer, SerializerCellGeneric[] cells)
    {
        var chunk = _cache.GetOrCreateChunk(chunkX, chunkY);

        lock (chunk.SyncRoot)
        {
            // Key: coordenada (long), Value: el ID del tile que debe dictar la apariencia
            Dictionary<long, ushort> tilesToUpdateSolid = new();
            Dictionary<long, ushort> tilesToUpdateBorder = new();

            // Optimizacion 1: Lista para no volver a recorrer toda la grilla
            List<int> borderIndices = new List<int>();

            var tileLayer = chunk.GetOrCreateLayer(height, layer);

            // 1. PRIMERA PASADA: Identificar sólidos y recolectar bordes
            for (int i = 0; i < cells.Length; i++)
            {
                ref readonly var cell = ref cells[i];
                if (cell.id == 0) continue;

                int x = i % _cache.ChunkSize;
                int y = i / _cache.ChunkSize;
                tileLayer.SetSolid(x, y, true);

                long centerKey = MakeKey(chunkX * _cache.ChunkSize + x, chunkY * _cache.ChunkSize + y);
                bool isBorderLogic = cell.isBorder;

                if (isBorderLogic)
                {
                    borderIndices.Add(i);
                }
                else
                {
                    tilesToUpdateSolid[centerKey] = cell.id;
                }
            }

            // 2. SEGUNDA PASADA (Optimizada): Solo iteramos los bordes encontrados
            foreach (int i in borderIndices)
            {
                int x = i % _cache.ChunkSize;
                int y = i / _cache.ChunkSize;
                int worldX = chunkX * _cache.ChunkSize + x;
                int worldY = chunkY * _cache.ChunkSize + y;

                ushort borderId = cells[i].id;

                for (int oy = -1; oy <= 1; oy++)
                {
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        long neighborKey = MakeKey(worldX + ox, worldY + oy);

                        // Solo se agrega si la clave no existe en tilesToUpdateSolid
                        if (!tilesToUpdateSolid.ContainsKey(neighborKey))
                        {
                            tilesToUpdateBorder.TryAdd(neighborKey, borderId);
                        }
                    }
                }
            }

            // 3. TERCERA PASADA: Aplicar las actualizaciones visuales
            foreach (var entry in tilesToUpdateSolid)
            {
                long key = entry.Key;
                ushort id = entry.Value;
                DecodeKey(key, out int nx, out int ny);
                var template = GetTemplate((BlackyRenderLayer)layer, id);
                var (targetChunk, lx, ly) = _cache.ResolveOrCreate(nx, ny);
                var targetLayer = targetChunk.GetOrCreateLayer(height, layer);

                UpdateDualVisualInternal(nx, ny, lx, ly, height, layer, template, targetLayer);
            }

            foreach (var entry in tilesToUpdateBorder)
            {
                long key = entry.Key;
                ushort id = entry.Value;
                DecodeKey(key, out int nx, out int ny);

                var template = GetTemplate((BlackyRenderLayer)layer, id);

                var (targetChunk, lx, ly) = _cache.ResolveOrCreate(nx, ny);
                var targetLayer = targetChunk.GetOrCreateLayer(height, layer);

                UpdateDualVisualInternal(nx, ny, lx, ly, height, layer, template, targetLayer, false);
            }

            chunk.MarkDirty();
        }
    }

    private DualTileTemplate GetTemplate(BlackyRenderLayer blackyRenderLayer, ushort id)
    {
        switch (blackyRenderLayer)
        {
            case BlackyRenderLayer.TerrenoBase:
                var data = BlackyPalletesPersistence.terrainPalette.GetData(id);
                return AtlasModsManager.Get<DualTileTemplate>(data.nameMod, data.idDualTemplate);

            case BlackyRenderLayer.Superficie:
                var data3 = BlackyPalletesPersistence.surfacesPalette.GetData(id);
                return AtlasModsManager.Get<DualTileTemplate>(data3.nameMod, data3.idDualTemplate);

            case BlackyRenderLayer.Caminos:
                var data4 = BlackyPalletesPersistence.pathsPalette.GetData(id);
                return AtlasModsManager.Get<DualTileTemplate>(data4.nameMod, data4.idDualTemplate);

            default:
                break;
        }

        return null;
    }

    private void UpdateDualVisualInternal(int vx, int vy, int lx, int ly, int height, int layer, DualTileTemplate template, IBlackyChunkTilemapTexture tileLayer, bool esSolido = true)
    {
        byte mask = 0;
        if (esSolido) // si es solido no verifico vecindad
        {
            mask = 15;
        }
        else
        {
            if (IsSolidGlobal(vx, vy + 1, height, layer)) mask |= DualMask.TopLeft;
            if (IsSolidGlobal(vx + 1, vy + 1, height, layer)) mask |= DualMask.TopRight;
            if (IsSolidGlobal(vx, vy, height, layer)) mask |= DualMask.BottomLeft;
            if (IsSolidGlobal(vx + 1, vy, height, layer)) mask |= DualMask.BottomRight;
        }

        if (mask == 0)
        {
            //_heightMapWorld.SetTopHeight(vx, vy, height-1);
            return; // no hacer nada
        }

        tileLayer.SetDualMask(lx, ly, mask);

        var slot = template.GetSlot(mask);
        var item = slot.GetGeneric().Parts[0];

        int idCollider = 0;
        int tileId = AtlasModsManager.GetSpriteUniqueId(item.IdTileSpriteData, out TileSpriteData tileSpriteData);
        RemoverCollider(tileLayer, lx, ly);

        if (!esSolido)
        {
            idCollider = AsignarCollider(vx, vy, tileId, tileSpriteData);
        }

        // _heightMapWorld.SetTopHeight(vx, vy, height);

        tileLayer.SetTile(lx, ly, tileId, idCollider);
        tileLayer.SetRender(lx, ly, true);
    }

    private int AsignarCollider(int Mundo_x, int Mundo_y, int idTileSpriteData, TileSpriteData tileSpriteData)
    {
        if (tileSpriteData.fastCollidersBody.Count == 0)
        {
            return 0;
        }

        var debugBodies = new List<int>();
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(Mundo_x, Mundo_y);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var collider in tileSpriteData.fastCollidersBody)
        {
            FastCollider fast = collider;

            // Nota: Si el offset es individual por collider, usa fast.Offset.X y fast.Offset.Y en su lugar
            float actualX = positionCenter.X + fast.Offset.X;
            float actualY = positionCenter.Y + fast.Offset.Y;

            float width = fast.Width;
            float height = fast.Height;

            if (fast.Shape == ShapeType.Circle) // esto por que es el radio, y queremos diametro para el collider
            {
                width = width * 2;
                height = height * 2;
            }

            // Se usa height para el eje Y (en tu código original tenías width por error en el eje Y)
            float currentMinX = actualX - (width * 0.5f) - 0.01f;
            float currentMinY = actualY - (height * 0.5f) - 0.01f;
            float currentMaxX = actualX + (width * 0.5f) + 0.01f;
            float currentMaxY = actualY + (height * 0.5f) + 0.01f;

            // Acumular mínimos y máximos globales
            if (currentMinX < minX) minX = currentMinX;
            if (currentMinY < minY) minY = currentMinY;
            if (currentMaxX > maxX) maxX = currentMaxX;
            if (currentMaxY > maxY) maxY = currentMaxY;

            if (DEBUG_COLLIDER)
            {
                int idDebugBody = CollisionShapeDraw.Instance.DrawCollisionShapes(fast, positionCenter, Godot.Colors.OrangeRed);
                debugBodies.Add(idDebugBody);
            }
        }

        var tilePositionMin = new Vector2(minX, minY);
        var tilePositionMax = new Vector2(maxX, maxY);

        int idCollider = _staticSpatialTerrain.RegisterStatic(
            new ColliderSpriteInstanceData(idTileSpriteData, positionCenter),
            tilePositionMin.X, tilePositionMin.Y, tilePositionMax.X, tilePositionMax.Y);

        _colliderDebugMap.Add(idCollider, debugBodies);

        return idCollider;
    }

    public void SetTileDual(
        int worldX,
        int worldY,
        int height,
        int layer,
        DualTileTemplate dualTileTemplate,
        bool isInternal = false)
    {
        var (chunk, lx, ly) = _cache.ResolveOrCreate(worldX, worldY);

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
    {
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

        var (chunk, lx, ly) = _cache.ResolveOrCreate(vx, vy);

        var tileLayer = chunk.GetOrCreateLayer(height, layer);

        byte lastMask = tileLayer.GetDualMask(lx, ly);
        tileLayer.SetDualMask(lx, ly, (byte)mask);

        bool markDelete = false;
        if (mask == 0)
        {
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
                RemoveTileDualInternal(vx, vy - i, height, layer);
            }
            else
            {
                SetTileDualInternalSprite(vx, vy - i, height, layer, item.IdTileSpriteData, isBorder);
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
        var (chunk, lx, ly) = _cache.Resolve(worldX, worldY);

        // 2. Si el chunk vecino no existe, devolvemos 'false'
        if (chunk == null)
            return false;

        // 3. Acceso a datos de lectura (esto es seguro)
        if (!chunk.TryGetHeight(height, out var h))
            return false;

        if (!h.TryGetLayer(layer, out var l))
            return false;

        // Nota: como estamos en un entorno multihilo,
        // l.IsSolid(lx, ly) debe ser una lectura atómica de un campo simple.
        return l.IsSolid(lx, ly);
    }

    // ===============================
    // CLEAR
    // ===============================

    /// <summary>
    /// Libera los colliders y los cuerpos de depuración creados por este mapa.
    /// La caché de chunks la vacía su dueño (<see cref="BlackyChunkTextureCache.Clear"/>).
    /// </summary>
    public void Clear()
    {
        foreach (var kvp in _colliderDebugMap)
        {
            int idCollider = kvp.Key;
            if (DEBUG_COLLIDER)
            {
                foreach (var idDebugBody in kvp.Value)
                {
                    CollisionShapeDraw.Instance?.FreeDraw(idDebugBody);
                }
            }

            _staticSpatialTerrain.FreeCollider(idCollider);
        }

        _colliderDebugMap.Clear();
    }
}
