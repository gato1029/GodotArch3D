using System;
using System.Collections.Generic;
using System.Linq;
using GodotEcsArch.sources.BlackyTiles.Commands;

using GodotEcsArch.sources.managers.Chunks;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Services.Render.Tiles;
using Flecs.NET.Core;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;


public class TileRenderTextureInstance
{
    public Rid Rid;
    public int InstanceId;
    public Entity Entity;

    // esta para comparacion 
    public int SubTextureId;
    public ushort Index;

    public TileRenderTextureInstance() { }
    public TileRenderTextureInstance(Rid rid, int instanceId)
    {
        Rid = rid;
        InstanceId = instanceId;
        IsDestroyed = false;
    }

    public TileRenderTextureInstance(Rid rid, int instanceId, Entity entity)
    {
        Rid = rid;
        InstanceId = instanceId;
        Entity = entity;
        IsDestroyed = false;
    }

    public bool HasEntity => Entity.Id != 0;
    public bool IsDestroyed { get; private set; }

    public void MarkDestroyed()
    {
        IsDestroyed = true;
    }
}

public class BlackyChunkRenderTiles
{
    public Vector2I ChunkPosition;
    public bool IsDestroyed { get; private set; }

    public void MarkDestroyed()
    {
        IsDestroyed = true;
    }
    private readonly Dictionary<(int h, int l, int x, int y), TileRenderTextureInstance> instances = new();

    public void AddOrReplace((int h, int l, int x, int y) key, TileRenderTextureInstance instance)
    {
        instances[key] = instance;
    }
    
    public bool TryGet((int h, int l, int x, int y) key, out TileRenderTextureInstance instance)
    {
        return instances.TryGetValue(key, out instance);
    }

    public bool Remove((int h, int l, int x, int y) key)
    {
        return instances.Remove(key);
    }

    public IEnumerable<TileRenderTextureInstance> GetAll()
    {
        return instances.Values;
    }

    public void Clear()
    {
        instances.Clear();
    }
}
public class BlackyTileTextureRenderSystem
{


    private readonly BlackyChunkTextureMap chunkMap;
    private readonly ChunkManagerBase chunkManager;

    // chunk → instances
    //private readonly Dictionary<Vector2I,
    //    Dictionary<(int height, int layer, int x, int y), BlackyTileRenderInstance>>
    //    chunkRenderInstances = new();

    private readonly Dictionary<Vector2I, BlackyChunkRenderTiles> chunkRenderInstances = new Dictionary<Vector2I, BlackyChunkRenderTiles>();

    public BlackyTileTextureRenderSystem(
        BlackyChunkTextureMap chunkMap,
        ChunkManagerBase chunkManager)
    {
        this.chunkMap = chunkMap;
        this.chunkManager = chunkManager;
        chunkMap.OnTileChanged += HandleTileChanged;
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

        foreach (var instance in dict.GetAll())
        {
            

            instance.MarkDestroyed();
            //mejorar

            //RenderCommandQueue.Enqueue(new RemoveInstanceCommand(instance));

            //if (instance.HasEntityReference)
            //    RenderCommandQueue.Enqueue(new DestroyTileEntityCommand(instance));
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

        //if (chunk.HasDirtyTiles())
        //{
        //    RebuildDirty(chunk);
        //}

        if (chunk.IsDirty)
        {
            //OnRefreshDirtyChunk?.Invoke(chunk);
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
                        bool isDual = layer.GetDualMask(x, y) != 0;
                        if (tileId == 0)
                            continue;

                        //RemoveTile(coord, height.Height, layer.LayerIndex, x, y);

                        //RenderTile(coord, height.Height, layer.LayerIndex, x, y, tileId, chunk);
                    }
                }
            }
        }
    }

    // ===============================
    // DIRTY
    // ===============================

    //private void RebuildDirty(BlackyChunkTexture chunk)
    //{
    //    Vector2I coord = new(chunk.Coord.X, chunk.Coord.Y);

    //    foreach (var height in chunk.GetHeights())
    //    {
    //        foreach (var layer in height.GetAllLayers())
    //        {
    //            if (layer == null || !layer.HasDirtyTiles)
    //                continue;

    //            foreach (var tile in layer.ConsumeDirtyTiles())
    //            {
    //                int x = tile.x;
    //                int y = tile.y;
    //                int tileId = tile.tileId;

    //                if (tileId == 0)
    //                {
    //                    RemoveTile(coord, height.Height, layer.LayerIndex, x, y);
    //                    continue;
    //                }

    //                RemoveTile(coord, height.Height, layer.LayerIndex, x, y);

    //                //RenderTile(coord, height.Height, layer.LayerIndex, x, y, tileId, chunk);

    //                //OnRefreshDirtyTile?.Invoke(
    //                //    layer.LayerIndex,
    //                //    new Vector2I(tile.worldX, tile.worldY)
    //                //);
    //            }
    //        }
    //    }
    //}

    // ===============================
    // RENDER CORE
    // ===============================
    private void HandleTileChanged(TileChange change)
    {
        // aqui notifica cuando un tile cambio y debemos repintarlo
        var chunkCoord = chunkMap.WorldToChunkCoord(change.WorldX, change.WorldY);
        Vector2I pos = new(chunkCoord.X, chunkCoord.Y);
        RenderTile(pos, change.Height, change.Layer, change.WorldX, change.WorldY, change.TileId, change.region,change.remove,change.dual);
    }

    private void RenderTile(
        Vector2I chunkCoord,
        int height,
        int layer,
        int worldX,
        int worldY,
        ushort tileId,
        BlackyRegion region, bool remove, bool dual)
    {
        if (!chunkRenderInstances.TryGetValue(chunkCoord, out var chunkRender))
        {
            chunkRender = new();
            chunkRenderInstances[chunkCoord] = chunkRender;
        }

        chunkRender.TryGet((height, layer, worldX, worldY), out TileRenderTextureInstance instance);

        // =====================================================
        // SOLO ELIMINAR
        // =====================================================
        if (remove)
        {
            if (instance != null)
            {
                instance.MarkDestroyed();
                RenderCommandQueue.Enqueue(
                    new DestroyTileInstanceTextureCommand(height, layer, worldX, worldY, dual, instance, chunkRender));
            }

            return;
        }

        // =====================================================
        // OBTENER NUEVA DATA
        // =====================================================
        region.TryGetTileDataMod(tileId, out TileDataMod tileDataMod);

        // =====================================================
        // SI YA EXISTE Y ES LA MISMA TEXTURA -> IGNORAR
        // =====================================================
        if (instance != null)
        {
            if (instance.SubTextureId == tileDataMod.SubTextureId &&
                instance.Index == tileDataMod.Index)
            {
                return;
            }

            // destruir anterior porque cambia
            instance.MarkDestroyed();
            RenderCommandQueue.Enqueue(
                new DestroyTileInstanceTextureCommand(height, layer, worldX, worldY,dual, instance, chunkRender));
        }

        // =====================================================
        // CREAR NUEVA
        // =====================================================
        RenderCommandQueue.Enqueue(
            new CreateTileInstanceTextureCommand(height, layer, worldX, worldY, dual, tileDataMod, chunkRender));
    }
   


}