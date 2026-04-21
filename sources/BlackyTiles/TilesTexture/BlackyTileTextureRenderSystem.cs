using System;
using System.Collections.Generic;
using System.Linq;
using global::GodotEcsArch.sources.BlackyTiles.Commands;
using global::GodotEcsArch.sources.BlackyTiles.Tiles;
using global::GodotEcsArch.sources.managers.Chunks;
using Godot;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;



public class BlackyTileTextureRenderSystem
{
    public event Action<int, Vector2I> OnRefreshDirtyTile;
    public event Action<BlackyChunkTexture> OnRefreshDirtyChunk;

    private readonly BlackyChunkTextureMap chunkMap;
    private readonly ChunkManagerBase chunkManager;

    // chunk → instances
    private readonly Dictionary<Vector2I,
        Dictionary<(int height, int layer, int x, int y), BlackyTileRenderInstance>>
        chunkRenderInstances = new();

    public BlackyTileTextureRenderSystem(
        BlackyChunkTextureMap chunkMap,
        ChunkManagerBase chunkManager)
    {
        this.chunkMap = chunkMap;
        this.chunkManager = chunkManager;

        chunkManager.OnChunkLoad += OnChunkLoad;
        chunkManager.OnChunkUnload += OnChunkUnload;
    }

    // ===============================
    // CHUNK EVENTS
    // ===============================

    private void OnChunkUnload(Vector2I coord)
    {
        if (!chunkRenderInstances.TryGetValue(coord, out var dict))
            return;

        foreach (var kv in dict.ToList())
        {
            var instance = kv.Value;

            instance.MarkDestroyed();

            RenderCommandQueue.Enqueue(new RemoveInstanceCommand(instance));

            if (instance.HasEntityReference)
                RenderCommandQueue.Enqueue(new DestroyTileEntityCommand(instance));
        }

        dict.Clear();
        chunkRenderInstances.Remove(coord);
    }

    private void OnChunkLoad(Vector2I coord)
    {
        if (!chunkMap.TryGetChunk(coord.X, coord.Y, out var chunk))
            return;

        if (!chunkRenderInstances.ContainsKey(coord))
        {
            BuildChunk(chunk);
        }

        if (chunk.HasDirtyTiles())
        {
            RebuildDirty(chunk);
        }

        if (chunk.IsDirty)
        {
            OnRefreshDirtyChunk?.Invoke(chunk);
            chunk.MarkSaved();
        }
    }

    // ===============================
    // FULL BUILD
    // ===============================

    private void BuildChunk(BlackyChunkTexture chunk)
    {
        Vector2I coord = new(chunk.Coord.X, chunk.Coord.Y);

        foreach (var height in chunk.GetHeights())
        {
            foreach (var layer in height.GetAllLayers())
            {
                if (layer == null)
                    continue;

                for (int y = 0; y < layer.Size; y++)
                {
                    for (int x = 0; x < layer.Size; x++)
                    {
                        int tileId = layer.GetTile(x, y);
                        if (tileId == 0)
                            continue;

                        RemoveTile(coord, height.Height, layer.LayerIndex, x, y);

                        //RenderTile(coord, height.Height, layer.LayerIndex, x, y, tileId, chunk);
                    }
                }
            }
        }
    }

    // ===============================
    // DIRTY
    // ===============================

    private void RebuildDirty(BlackyChunkTexture chunk)
    {
        Vector2I coord = new(chunk.Coord.X, chunk.Coord.Y);

        foreach (var height in chunk.GetHeights())
        {
            foreach (var layer in height.GetAllLayers())
            {
                if (layer == null || !layer.HasDirtyTiles)
                    continue;

                foreach (var tile in layer.ConsumeDirtyTiles())
                {
                    int x = tile.x;
                    int y = tile.y;
                    int tileId = tile.tileId;

                    if (tileId == 0)
                    {
                        RemoveTile(coord, height.Height, layer.LayerIndex, x, y);
                        continue;
                    }

                    RemoveTile(coord, height.Height, layer.LayerIndex, x, y);

                    //RenderTile(coord, height.Height, layer.LayerIndex, x, y, tileId, chunk);

                    //OnRefreshDirtyTile?.Invoke(
                    //    layer.LayerIndex,
                    //    new Vector2I(tile.worldX, tile.worldY)
                    //);
                }
            }
        }
    }

    // ===============================
    // RENDER CORE
    // ===============================

    //private void RenderTile(
    //    Vector2I chunkCoord,
    //    int height,
    //    int layer,
    //    int localX,
    //    int localY,
    //    int tileId,
    //    BlackyChunkTexture chunk)
    //{
    //    // 🔥 lookup desde palette
    //    var palette = chunk.GetPalette(height, layer);
    //    var renderData = palette.Get(tileId);

    //    if (renderData == null)
    //        return;

    //    float worldX = chunk.WorldBaseX + localX;
    //    float worldY = chunk.WorldBaseY + localY;

    //    // 🔥 aquí puedes usar tu lógica de depth igual que antes
    //    Vector3 position = new Vector3(worldX, worldY, 0);

    //    CreateInstance(chunkCoord, height, layer, localX, localY, position, renderData);
    //}

    //private void CreateInstance(
    //    Vector2I chunkCoord,
    //    int height,
    //    int layer,
    //    int x,
    //    int y,
    //    Vector3 position,
    //    TileRenderData data)
    //{
    //    if (!chunkRenderInstances.TryGetValue(chunkCoord, out var dict))
    //    {
    //        dict = new();
    //        chunkRenderInstances[chunkCoord] = dict;
    //    }

    //    var key = (height, layer, x, y);

    //    RenderCommandQueue.Enqueue(
    //        new CreateTileInstanceCommand(
    //            position,
    //            height,
    //            layer,
    //            x,
    //            y,
    //            data,
    //            (instance) => dict[key] = instance
    //        )
    //    );
    //}

    private void RemoveTile(Vector2I chunkCoord, int height, int layer, int x, int y)
    {
        if (!chunkRenderInstances.TryGetValue(chunkCoord, out var dict))
            return;

        var key = (height, layer, x, y);

        if (!dict.TryGetValue(key, out var instance))
            return;

        instance.MarkDestroyed();

        RenderCommandQueue.Enqueue(new RemoveInstanceCommand(instance));

        if (instance.HasEntityReference)
            RenderCommandQueue.Enqueue(new DestroyTileEntityCommand(instance));

        dict.Remove(key);
    }
}