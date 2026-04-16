

using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Entities;

public class BlackyEntityRenderSystem
{
    private readonly BlackySpatialEntityMap spatialMap;
    private readonly ChunkManagerBase chunkManager;

    public BlackyEntityRenderSystem(
        BlackySpatialEntityMap spatialMap,
        ChunkManagerBase chunkManager)
    {
        this.spatialMap = spatialMap;
        this.chunkManager = chunkManager;

        chunkManager.OnChunkLoad += OnChunkLoad;
        chunkManager.OnChunkUnload += OnChunkUnload;
    }
    public void ForceDisposeEntity(Entity entity)
    {
        if (!entity.Has<SpatialComponent>())
            return;
        var spatial = entity.Get<SpatialComponent>();            
        // Si el chunk está cargado, resolver el dispose del entity
        // ❌ nunca tuvo render → ignorar
        if (!entity.Has<RenderInstanceComponent>())
            return;

        var render = entity.Get<RenderInstanceComponent>();

        // ❌ ya está desactivado → ignorar
        if (!render.isActive)
            return;

        // 🔥 desactivar render (GPU)
        RenderCommandQueue.Enqueue(
            new DisableEntityRenderCommand(
                entity,
                render.rid,
                render.instance,
                render.materialId
            )
        );
    }
    public void ForceRenderEntity(Entity entity)
    {
        if (!entity.Has<SpatialComponent>())
            return;
        var spatial = entity.Get<SpatialComponent>();
        var chunkCoord = spatial.Chunk;            
        // Si el chunk está cargado, resolver el render del entity
        ResolveEntityRender_FirstTime(entity, chunkCoord);            
    }
    private void OnChunkLoad(Vector2I chunkCoord)
    {
        var bucket = spatialMap.GetBucket(chunkCoord);
        if (bucket == null) return;

        for (int i = 0; i < bucket.Count; i++)
        {
            var entity = bucket.Entities[i];
            bool hasRender = entity.Has<RenderInstanceComponent>();

            if (!hasRender)
            {
                // 🔥 PRIMERA VEZ → CREATE
                ResolveEntityRender_FirstTime(entity, chunkCoord);
            }
            else
            {
                ref var render = ref entity.GetMut<RenderInstanceComponent>();

                if (!render.isActive)
                {
                    // 🔥 YA EXISTÍA → ENABLE
                    ResolveEntityRender_ReEnable(entity, chunkCoord);
                }
                // 🟢 ya activo → no hacer nada
            }
        }
    }
    private void ResolveEntityRender_ReEnable(Entity entity, Vector2I chunkCoord)
    {
        int layerIndex = 4;
        var tileSpriteComponent = entity.Get<TileSpriteComponent>();
        var positionComponent = entity.Get<PositionComponent>();

        var dataTemplate = MasterDataManager.GetData<TileSpriteData>(tileSpriteComponent.idTileSprite);


        float x = MeshCreator.PixelsToUnits(16) / 2f;
        float y = MeshCreator.PixelsToUnits(16) / 2f;

        float offsetX = 0;
        float offsetY = 0;

        switch (dataTemplate.tileSpriteType)
        {
            case TileSpriteType.Static:
                offsetX = dataTemplate.spriteData.offsetInternal.X * dataTemplate.spriteData.scale;
                offsetY = dataTemplate.spriteData.offsetInternal.Y * dataTemplate.spriteData.scale;
                break;
            case TileSpriteType.Animated:
                offsetX = dataTemplate.animationData.offsetInternal.X * dataTemplate.animationData.scale;
                offsetY = dataTemplate.animationData.offsetInternal.Y * dataTemplate.animationData.scale;
                break;
            default:
                break;
        }



        Vector2 positionNormalize = (positionComponent.tilePosition * new Vector2(MeshCreator.PixelsToUnits(16), MeshCreator.PixelsToUnits(16))) + new Vector2(x, y);
        Vector2 positionCenter = positionNormalize+ new Vector2(offsetX, offsetY);
        
        Vector3 worldPosition = Vector3.Zero;

        float depthOffset = 0;
        float z = 0;
        float depthValue = 0;


        switch (dataTemplate.tileSpriteType)
        {
            case TileSpriteType.Static:
                depthOffset = MeshCreator.PixelsToUnits(dataTemplate.spriteData.yDepthRender);
                depthValue = positionNormalize.Y
                        + depthOffset
                        - positionComponent.height * CommonAtributes.HEIGHT_OFFSET
                        ;

                z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layerIndex * CommonAtributes.LAYER_OFFSET;

                worldPosition = new(positionCenter.X, positionCenter.Y, z);

                break;
            case TileSpriteType.Animated:
                depthOffset = MeshCreator.PixelsToUnits(dataTemplate.animationData.yDepthRender);
                depthValue =
                positionNormalize.Y
               + depthOffset
               - positionComponent.height * CommonAtributes.HEIGHT_OFFSET
              ;

                z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layerIndex * CommonAtributes.LAYER_OFFSET;

                worldPosition = new(positionCenter.X, positionCenter.Y, z);
                break;

        }


        if (dataTemplate.tileSpriteType == TileSpriteType.Animated)
        {
            RenderCommandQueue.Enqueue(
                new EnableEntityRenderCommand(
                    entity,
                    worldPosition,
                    dataTemplate.animationData
                )
            );
        }
        else
        {
            RenderCommandQueue.Enqueue(
                new CreateEntityInstanceCommand(
                    entity,
                    worldPosition,
                    dataTemplate.spriteData
                )
            );
        }
    }
    private void ResolveEntityRender_FirstTime(Entity entity, Vector2I chunkCoord)
    {
        int layerIndex = 4;
        var tileSpriteComponent = entity.Get<TileSpriteComponent>();
        var positionComponent = entity.Get<PositionComponent>();

        var dataTemplate = MasterDataManager.GetData<TileSpriteData>(tileSpriteComponent.idTileSprite);


        float x = MeshCreator.PixelsToUnits(16) / 2f;
        float y = MeshCreator.PixelsToUnits(16) / 2f;

        float offsetX = 0;
        float offsetY = 0;

        switch (dataTemplate.tileSpriteType)
        {
            case TileSpriteType.Static:
                offsetX = dataTemplate.spriteData.offsetInternal.X * dataTemplate.spriteData.scale;
                offsetY = dataTemplate.spriteData.offsetInternal.Y * dataTemplate.spriteData.scale;
                break;
            case TileSpriteType.Animated:
                offsetX = dataTemplate.animationData.offsetInternal.X * dataTemplate.animationData.scale;
                offsetY = dataTemplate.animationData.offsetInternal.Y * dataTemplate.animationData.scale;
                break;
            default:
                break;
        }



        Vector2 positionNormalize = (positionComponent.tilePosition * new Vector2(MeshCreator.PixelsToUnits(16), MeshCreator.PixelsToUnits(16))) + new Vector2(x, y);
        Vector2 positionCenter = positionNormalize  + new Vector2(offsetX, offsetY);

        Vector3 worldPosition = Vector3.Zero;

        float depthOffset = 0;
        float z = 0;
        float depthValue = 0;


        switch (dataTemplate.tileSpriteType)
        {
            case TileSpriteType.Static:
                depthOffset = MeshCreator.PixelsToUnits(dataTemplate.spriteData.yDepthRender);
                depthValue = positionNormalize.Y
                        + depthOffset
                        - positionComponent.height * CommonAtributes.HEIGHT_OFFSET
                        ;

                z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layerIndex * CommonAtributes.LAYER_OFFSET;

                worldPosition = new(positionCenter.X, positionCenter.Y, z);

                break;
            case TileSpriteType.Animated:
                depthOffset = MeshCreator.PixelsToUnits(dataTemplate.animationData.yDepthRender);
                depthValue =
                positionNormalize.Y
                + depthOffset
               - positionComponent.height * CommonAtributes.HEIGHT_OFFSET
              ;

                z = depthValue * CommonAtributes.LAYER_MULTIPLICATOR + layerIndex * CommonAtributes.LAYER_OFFSET;

                worldPosition = new(positionCenter.X, positionCenter.Y, z);
                break;

        }
    
        if (dataTemplate.tileSpriteType == TileSpriteType.Animated)
        {
            RenderCommandQueue.Enqueue(
                new CreateEntityAnimatedInstanceCommand(
                    entity,
                    worldPosition,
                    dataTemplate.animationData,
                    dataTemplate.id,
                    dataTemplate.animationData.frameDuration,
                    depthValue,
                    dataTemplate.animationData.scale,
                    new Vector2(offsetX, offsetY),
                    layerIndex
                )
            );
        }
        else
        {
            RenderCommandQueue.Enqueue(
                new CreateEntityInstanceCommand(
                    entity,
                    worldPosition,
                    dataTemplate.spriteData
                )
            );
        }
    }

    private void OnChunkUnload(Vector2I chunkCoord)
    {
        var bucket = spatialMap.GetBucket(chunkCoord);
        if (bucket == null) return;

        for (int i = 0; i < bucket.Count; i++)
        {
            var entity = bucket.Entities[i];

            // ❌ nunca tuvo render → ignorar
            if (!entity.Has<RenderInstanceComponent>())
                continue;

            var render = entity.Get<RenderInstanceComponent>();

            // ❌ ya está desactivado → ignorar
            if (!render.isActive)
                continue;

            // 🔥 desactivar render (GPU)
            RenderCommandQueue.Enqueue(
                new DisableEntityRenderCommand(
                    entity,
                    render.rid,
                    render.instance,
                    render.materialId
                )
            );
        }
    }
}
