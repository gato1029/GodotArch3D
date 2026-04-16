using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Metrics;

namespace GodotEcsArch.sources.BlackyTiles.Tiles;
public class BlackyTileRenderSystem
{
    public event Action<int, Vector2I> OnRefreshDirtyTile;
    public event Action<BlackyChunk> OnRefreshDirtyChunk;

    private readonly BlackyChunkRenderData chunkRenderData;
    private readonly ChunkManagerBase chunkManager;
    private readonly FlecsManager flecsManager;
    // Chunk -> (layer,x,y) -> instance
    private readonly Dictionary<Vector2I,
        Dictionary<(int height,int layer, int x, int y), BlackyTileRenderInstance>>
        chunkRenderInstances = new();

    public BlackyTileRenderSystem(
        FlecsManager flecsManager,
        BlackyChunkRenderData renderData,
        ChunkManagerBase chunkManagerBase)
    {
        this.flecsManager = flecsManager;
        chunkRenderData = renderData;
        chunkManager = chunkManagerBase;

        chunkManager.OnChunkLoad += Instance_OnChunkLoad;
        chunkManager.OnChunkUnload += Instance_OnChunkUnload;
    }

    #region Chunk Events

    private void Instance_OnChunkUnload(Vector2I chunkCoordFloat)
    {

        Vector2I chunkCoord = chunkCoordFloat;



        if (!chunkRenderInstances.TryGetValue(chunkCoord, out var tileDict))
            return;
        foreach (var kv in tileDict.ToList())
        {
            var instance = kv.Value;

            instance.MarkDestroyed();

            RenderCommandQueue.Enqueue(
                new RemoveInstanceCommand(instance)
            );

            if (instance.HasEntityReference)
            {
                RenderCommandQueue.Enqueue(
                    new DestroyTileEntityCommand(instance)
                );
            }
        }

        tileDict.Clear();
        chunkRenderInstances.Remove(chunkCoord);
    }

    private void Instance_OnChunkLoad(Vector2I chunkCoordFloat)
    {
        Vector2I chunkCoord = chunkCoordFloat;

        if (!chunkRenderData.TryGetChunk(chunkCoord.X, chunkCoord.Y, out var chunk))
            return;

        if (!chunkRenderInstances.ContainsKey(chunkCoord))
        {
            RefreshRenderForChunk(chunk);
        }

        if (chunk.HasDirtyTiles())
        {
            RebuildDirtyTiles(chunk);
        }

        if (chunk.IsDirty)
        {
            OnRefreshDirtyChunk?.Invoke(chunk);
            chunk.MarkSaved();
        }
    }

    #endregion

    #region Full Chunk Render

    private void RefreshRenderForChunk(BlackyChunk chunk)
    {
        Vector2I chunkCoord = new(chunk.Coord.X, chunk.Coord.Y);

        foreach (var height in chunk.GetHeights())
        {
            foreach (var layer in height.Layers)
            {
                foreach (var tile in layer.GetAllTiles())
                {
                    RemoveTileInstance(chunkCoord, height.Height, layer.LayerIndex, tile.x, tile.y);
                    RenderTile(chunkCoord,height.Height,  layer.LayerIndex, tile.x, tile.y, tile.renderId, tile.worldX, tile.worldY, tile.offsetX, tile.offsetY);
                }
            }
          
        }
    }

    #endregion

    #region Tile Render Core

    internal void ForceUpdateTile(BlackyChunk chunk,int height, int layerIndex, int localX, int localY)
    {

        if (!chunk.TryGetLayer(height,layerIndex, out var layer))
            return;
        Vector2I chunkCoord = new(chunk.Coord.X, chunk.Coord.Y);
        var tile = layer.GetRenderData(localX, localY);


        if (tile.renderId==0)
        {
            RemoveTileInstance(chunkCoord, height, layerIndex, localX, localY);         
            return;
        }
        // 🔥 Elimina instancia previa si existe
        RemoveTileInstance(chunkCoord,height, layerIndex, localX, localY);

        RenderTile(chunkCoord, height, layerIndex, localX, localY, tile.renderId, tile.worldX, tile.worldY,tile.offsetX,tile.offsetY);
    }
    internal void ForceRemoveTile(Vector2I chunkCoord,int height, int layerIndex, int localX, int localY)
    {
        RemoveTileInstance(chunkCoord,height, layerIndex, localX, localY);
    }

    private void RenderTile(
        Vector2I chunkCoord,
        int height,
        int layerIndex,
        int localX,
        int localY,
        long renderId, float worldX, float worldY, float offsetX, float offsetY)
    {
        var dataTemplate = MasterDataManager.GetData<TileSpriteData>(renderId);


        float x = MeshCreator.PixelsToUnits(16) / 2f;
        float y = MeshCreator.PixelsToUnits(16) / 2f;

        Vector2 positionNormalize = new Vector2(worldX, worldY) * new Vector2(MeshCreator.PixelsToUnits(16), MeshCreator.PixelsToUnits(16)) + new Vector2(x, y);
        Vector2 positionCenter = positionNormalize  + new Vector2(offsetX, offsetY);

        Vector3 worldPosition;

        float depthOffset = 0;
        float z =0;
        float depthValue = 0;


        switch (dataTemplate.tileSpriteType)
        {
            case TileSpriteType.Static:
                depthOffset = MeshCreator.PixelsToUnits(dataTemplate.spriteData.yDepthRender);
                depthValue =
           positionNormalize.Y
          + depthOffset
          - height * CommonAtributes.HEIGHT_OFFSET
          ;

                z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layerIndex * CommonAtributes.LAYER_OFFSET;

                worldPosition = new(positionCenter.X, positionCenter.Y, z);
               
                CreateInstance(
                   chunkCoord,
                   height,
                   layerIndex,
                   localX,
                   localY,
                   worldPosition,
                   dataTemplate.spriteData
                );
                break;
            case TileSpriteType.Animated:
                depthOffset = MeshCreator.PixelsToUnits(dataTemplate.animationData.yDepthRender);
                 depthValue =
                  positionNormalize.Y
                + depthOffset
                - height * CommonAtributes.HEIGHT_OFFSET
               ;

                        z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layerIndex * CommonAtributes.LAYER_OFFSET;

                worldPosition = new(positionCenter.X, positionCenter.Y,z);
                CreateInstanceAnimate(
                          chunkCoord,
                          height,
                          layerIndex,
                          localX,
                          localY,
                          worldPosition,
                          dataTemplate.animationData, dataTemplate.id
                       );
                break;
         
        }
        
    }
    private void CreateInstanceAnimate(
    Vector2I chunkCoord,
    int height,
    int layer,
    int localX,
    int localY,
    Vector3 worldPosition,
    SpriteAnimationData data,
    long id)
    {
        if (!chunkRenderInstances.TryGetValue(chunkCoord, out var tileDict))
        {
            tileDict = new Dictionary<(int, int, int, int), BlackyTileRenderInstance>();
            chunkRenderInstances.Add(chunkCoord, tileDict);
        }

        var key = (height, layer, localX, localY);

        RenderCommandQueue.Enqueue(
            new CreateTileEntityCommand(
                worldPosition,
                height,
                localX,
                localY,
                layer,
                data.yDepthRender,
                data.scale,
                data.offsetInternal,
                id,
                data.frameDuration,
                data,
         
                (renderData) =>
                {
                    tileDict[key] = renderData;
                },flecsManager
            )
        );
    }

    private void CreateInstance(
      Vector2I chunkCoord,
      int height,
      int layer,
      int localX,
      int localY,
      Vector3 worldPosition,
      SpriteData data)
    {
        if (!chunkRenderInstances.TryGetValue(chunkCoord, out var tileDict))
        {
            tileDict = new Dictionary<(int, int, int, int), BlackyTileRenderInstance>();
            chunkRenderInstances.Add(chunkCoord, tileDict);
        }

        var key = (height, layer, localX, localY);

        RenderCommandQueue.Enqueue(
            new CreateTileInstanceCommand(
                worldPosition,
                height,
                layer,
                localX,
                localY,
                data,
                (renderData) =>
                {
                    tileDict[key] = renderData;
                }
            )
        );
    }

    private void RemoveTileInstance(Vector2I chunkCoord,int height ,int layer, int x, int y)
    {
        if (!chunkRenderInstances.TryGetValue(chunkCoord, out var tileDict))
        {
            return;
        }
        
        var key = (height, layer, x, y);

        if (!tileDict.TryGetValue(key, out var instance))
        {
            return;
        }
        instance.MarkDestroyed();

        // liberar instancia GPU
        RenderCommandQueue.Enqueue(
            new RemoveInstanceCommand(instance)
        );

        // destruir entity si existe
        if (instance.HasEntityReference)
        {
            RenderCommandQueue.Enqueue(
                new DestroyTileEntityCommand(instance)
            );
        }
        tileDict.Remove(key);
    }

    #endregion

    #region Dirty Tiles

    private void RebuildDirtyTiles(BlackyChunk chunk)
    {

        foreach (var height in chunk.GetHeights())
        {
            if (height.HasDirtyTiles())
            {
                foreach (var layer in height.Layers)
                {
                    if (!layer.HasDirtyTiles)
                        continue;

                    foreach (var dirty in layer.ConsumeDirtyTilesWithData())
                    {
                        RebuildTile(chunk, layer, dirty.x, dirty.y);
                    }
                }
            }
            
        }
        
    }

    private void RebuildTile(
        BlackyChunk chunk,
        BlackyChunkLayer layer,
        int x,
        int y)
    {
        OnRefreshDirtyTile?.Invoke(
            layer.LayerIndex,
            new Vector2I(
                chunk.Coord.X * chunkRenderData.ChunkSize + x,
                chunk.Coord.Y * chunkRenderData.ChunkSize + y
            )
        );
    }

    #endregion
}