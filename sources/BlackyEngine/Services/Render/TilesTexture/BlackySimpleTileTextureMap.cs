
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

/// <summary>
/// Pintado y borrado de tiles simples (rampas, adornos, etc.).
///
/// No posee estado propio: todas las escrituras van a la caché de chunks
/// compartida con <see cref="BlackyDualTileTextureMap"/>.
/// </summary>
public sealed class BlackySimpleTileTextureMap
{
    private readonly BlackyChunkTextureCache _cache;

    public BlackySimpleTileTextureMap(BlackyChunkTextureCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public BlackyChunkTextureCache Cache => _cache;

    // ===============================
    // TILE ACCESS
    // ===============================

    public void RemoveTileSprite(int worldX, int worldY, int height, int layer)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = _cache.ResolveOrCreate(worldX, worldY);

        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.ClearTile(localX, localY);

        _cache.RaiseTileChanged(new TileChange
        {
            WorldX = worldX,
            WorldY = worldY,
            Height = height,
            Layer = layer,
            remove = true
        });
    }

    public int SetTileSprite(int worldX, int worldY, int height, int layer, long idTileSprite)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = _cache.ResolveOrCreate(worldX, worldY);

        int tileId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);

        // 2. Guardamos el ID en el chunk
        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.SetTile(localX, localY, tileId, 0);
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
            dual = false,
            isPersistent = false
        });

        chunk.MarkDirty();
        return tileId;
    }

    public ushort SetTile(int worldX, int worldY, int height, int layer, string modName, ushort textureIndex, bool DualOffset)
    {
        // 1. Resolvemos el chunk y las coordenadas locales
        var (chunk, localX, localY) = _cache.ResolveOrCreate(worldX, worldY);

        // 2. Le pedimos a la región del chunk que nos dé un ID de su paleta
        ushort tileId = chunk.ParentRegion.GetOrCreateTile(modName, textureIndex);

        // 3. Guardamos el ID en el chunk
        var tileLayer = chunk.GetOrCreateLayer(height, layer);
        tileLayer.SetTile(localX, localY, tileId, 0);

        // Si en esa celda existe un visual dual, el tile simple pasa a mandar:
        // la celda deja de ser sólida para que el dual no la tape.
        if (tileLayer.GetDualMask(localX, localY) != 0)
        {
            tileLayer.SetSolid(localX, localY, false);
        }

        _cache.RaiseTileChanged(new TileChange
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

    public int GetTile(int worldX, int worldY, int height, int layer)
    {
        var (chunk, localX, localY) = _cache.Resolve(worldX, worldY);

        if (chunk == null)
            return 0;

        if (!chunk.TryGetHeight(height, out var h))
            return 0;

        if (!h.TryGetLayer(layer, out var l))
            return 0;

        return l.GetTile(localX, localY);
    }

    // ===============================
    // MULTI-CHUNK BLOCK WRITE
    // ===============================

    public void SetTilesBlock(
        int startX,
        int startY,
        int width,
        int height,
        int heightLevel,
        int layer,
        string modName,
        ushort textureIndex)
    {
        int endX = startX + width;
        int endY = startY + height;

        int startChunkX = FloorDiv(startX, _cache.ChunkSize);
        int endChunkX = FloorDiv(endX - 1, _cache.ChunkSize);

        int startChunkY = FloorDiv(startY, _cache.ChunkSize);
        int endChunkY = FloorDiv(endY - 1, _cache.ChunkSize);

        for (int cy = startChunkY; cy <= endChunkY; cy++)
        {
            for (int cx = startChunkX; cx <= endChunkX; cx++)
            {
                var chunk = _cache.GetOrCreateChunk(cx, cy);

                // El ID se resuelve por región para que un bloque que cruza
                // fronteras regionales use el ID correcto en cada una.
                ushort localId = chunk.ParentRegion.GetOrCreateTile(modName, textureIndex);

                var tilemap = chunk.GetOrCreateLayer(heightLevel, layer);

                int chunkWorldX = cx * _cache.ChunkSize;
                int chunkWorldY = cy * _cache.ChunkSize;

                int localStartX = Math.Max(startX - chunkWorldX, 0);
                int localStartY = Math.Max(startY - chunkWorldY, 0);

                int localEndX = Math.Min(endX - chunkWorldX, _cache.ChunkSize);
                int localEndY = Math.Min(endY - chunkWorldY, _cache.ChunkSize);

                tilemap.FillRectLocal(
                    localStartX,
                    localStartY,
                    localEndX,
                    localEndY,
                    localId);

                chunk.MarkDirty();
            }
        }
    }

    /// <summary>
    /// Pinta todos los tiles simples del mapa de una sola pasada.
    /// Es el equivalente de <see cref="BlackyDualTileTextureMap.ApplyWorldBatchDualTiles"/>
    /// para tiles simples: recorre los bloques por chunk/altura y no emite
    /// <see cref="BlackyChunkTextureCache.OnTileChanged"/> por celda.
    /// </summary>
    public void ApplyWorldBatchSimpleTiles(
        IEnumerable<(BlackyChunkCoord Coord, int Height, ReadOnlyMemory<SerializerCellGeneric> Cells)> blocks,
        int layer)
    {
        if (blocks == null) throw new ArgumentNullException(nameof(blocks));

        foreach (var block in blocks)
        {
            var chunk = _cache.GetOrCreateChunk(block.Coord.X, block.Coord.Y);
            var cells = block.Cells.Span;            
            var tileLayer = chunk.GetOrCreateLayer(block.Height, layer);

            for (int i = 0; i < cells.Length; i++)
            {
                ref readonly var cell = ref cells[i];
                if (cell.id == 0) continue;

                int lx = i % _cache.ChunkSize;
                int ly = i / _cache.ChunkSize;

                int tileId = GetIdTileSprite((BlackyRenderLayer)layer, cell.id);
                tileLayer.SetTile(lx, ly, tileId, 0);
                tileLayer.SetRender(lx, ly, true);
            }            
            chunk.MarkDirty();
        }
    }

    public void AplyChunkBatchSingleSprite(int chunkX, int chunkY, int height, int layer, SerializerCellGeneric[] cells)
    {
        var chunk = _cache.GetOrCreateChunk(chunkX, chunkY);

        lock (chunk.SyncRoot)
        {
            var tileLayer = chunk.GetOrCreateLayer(height, layer);
            for (int i = 0; i < cells.Length; i++)
            {
                ref readonly var cell = ref cells[i];
                if (cell.id == 0) continue;

                int x = i % _cache.ChunkSize;
                int y = i / _cache.ChunkSize;

                int tileId = GetIdTileSprite((BlackyRenderLayer)layer, cell.id);
                tileLayer.SetTile(x, y, tileId, 0);
                tileLayer.SetRender(x, y, true);
            }
        }
    }

    private int GetIdTileSprite(BlackyRenderLayer blackyRenderLayer, ushort id)
    {
        switch (blackyRenderLayer)
        {
            case BlackyRenderLayer.Rampas:
                var data2 = BlackyPalletesPersistence.rampsPalette.GetData(id);
                return AtlasModsManager.GetSpriteUniqueId(data2.idTileSprite);

            case BlackyRenderLayer.Adornos:
                var data5 = BlackyPalletesPersistence.decorationsPalette.GetData(id);
                return AtlasModsManager.GetSpriteUniqueId(data5.idTileSprite);

            default:
                break;
        }

        return 0;
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
}
