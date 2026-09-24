
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.BlackyTiles.Entities;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.Services.Paint;

public struct CreateResourceCommand
{
    public ushort IdResource;
    public Vector2I TilePosition;
    public int Height;
}
public struct RemoveResourceCommand
{
    public Vector2I TilePosition;
    
}
public class BlackyResourcesCreator
{
    private readonly BlackyChunkOccupancyMap occupancyMap;
    private readonly BlackySpatialEntityMap spatialEntityMap;

    private readonly BlackyTerrainWorldData terrain;
    private readonly FlecsManager flecsManager;
    private readonly StaticSpatialGridOptimizedGeneric<Entity> staticHashResource;
    private int _resourcesCount = 0;

    private const bool DEBUG_COLLIDERS = true;
    private int layer = (int)BlackyRenderLayer.Personajes_Arboles_Edificios;
    private Dictionary<int, List<int>> _colliderDebugMap = new();
    private readonly ConcurrentQueue<CreateResourceCommand> _commandQueue = new();
    private const int MaxPerFrame = 100;
    private readonly ConcurrentQueue<RemoveResourceCommand> _removeCommandQueue = new();
    private const int MaxRemovalsPerFrame = 100; // O el presupuesto que prefieras
    public BlackyResourcesCreator(StaticSpatialGridOptimizedGeneric<Entity> staticHash, FlecsManager flecsManager, BlackyChunkOccupancyMap occupancyMap, BlackySpatialEntityMap spatialEntityMap, BlackyEntityRenderSystem renderSystem, BlackyTerrainWorldData terrain)
    {
        this.occupancyMap = occupancyMap;
        this.spatialEntityMap = spatialEntityMap;
        this.terrain = terrain;
        this.flecsManager = flecsManager;
        this.staticHashResource = staticHash;
    }



    // 1. Método público: Se llama desde cualquier hilo para solicitar la eliminación
    public void RemoveResource(Vector2I tilePosition)
    {
        _removeCommandQueue.Enqueue(new RemoveResourceCommand
        {
            TilePosition = tilePosition,

        });
    }

    // 2. Método llamado por tu sistema de Flecs en el hilo principal
    public void ProcessPendingRemovals()
    {
        int executed = 0;
        int limit = _removeCommandQueue.Count > 2000 ? MaxRemovalsPerFrame * 3 : MaxRemovalsPerFrame;

        while (executed < limit && _removeCommandQueue.TryDequeue(out var cmd))
        {
            InternalExecuteRemoval(cmd.TilePosition);
            executed++;
        }
    }
    // 3. Lógica real que antes tenías en RemoveResource (ejecución segura en hilo principal)
    private void InternalExecuteRemoval(Vector2I tilePosition)
    {
        //no necesitamos altura por que un recurso no puede estar sobre otro
        //
        // Asumiendo la capa 0 como en tu código original:
        ulong idEntity = occupancyMap.Get(0, tilePosition.X, tilePosition.Y); // siempre cero por un recurso no esta sobre otro

        if (idEntity == 0) return;

        Entity entity = flecsManager.WorldFlecs.Entity(idEntity);

        if (entity.IsAlive())
        {
            if (entity.Has<SpatialIDComponent>())
            {
                SpatialIDComponent spatial = entity.Get<SpatialIDComponent>();
                staticHashResource.FreeCollider(spatial.Value);

                foreach (int item in _colliderDebugMap[spatial.Value])
                {
                    CollisionShapeDraw.Instance.FreeDraw(item);
                }

                _colliderDebugMap.Remove(spatial.Value);
            }

            RenderGPUComponent gpu = entity.Get<RenderGPUComponent>();
            AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);

            spatialEntityMap.Remove(entity);
            occupancyMap.ClearByEntity(0, tilePosition.X, tilePosition.Y);

            // Destruye la entidad en Flecs de forma segura
            entity.Destruct();
        }
    }
    // Desde cualquier hilo (generación, red, etc.) encolan la orden
    public void CreateResource(ushort idResource, Vector2I tilePosition, int height)
    {
        _commandQueue.Enqueue(new CreateResourceCommand
        {
            IdResource = idResource,
            TilePosition = tilePosition,
            Height = height
        });
    }

    // Llamado exclusivamente por el sistema de Flecs en el hilo principal
    public void ProcessPendingCommands()
    {
        int executed = 0;
        int limit = _commandQueue.Count > 2000 ? MaxPerFrame * 3 : MaxPerFrame;

        while (executed < limit && _commandQueue.TryDequeue(out var cmd))
        {
            InternalExecuteCreation(cmd.IdResource, cmd.TilePosition, cmd.Height, out _);
            executed++;
        }
    }

    public void CreationDataNoRender(SavedResourceData data)
    {
        InternalExecuteCreation(data.TemplateId, new Vector2I(data.WorldTileX, data.WorldTileY), data.Height, out Entity entity, false, data.Health, data.Amount);
    }
    // Método interno: Contiene la lógica real y se ejecuta de forma segura en el hilo principal
    public void InternalExecuteCreation(ushort idResource, Vector2I tilePosition, int height, out Entity entity, bool render=true, int health=0,int amount=0)
    {
        entity = default;
        Vector2 position = TilesHelper.TilePositionToWorldPosition(tilePosition);

        ResourceSourceData templateResource = BlackyPalletesPersistence.resourcesPalette.GetData(idResource);
        entity = flecsManager.WorldFlecs.Entity();

        long idTileSprite = templateResource.listIdTileSpriteData[0];

        int spriteId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);
        AtlasModsManager.TryGetTileSprite(spriteId, out var sprite);

        if (occupancyMap.IsOccupiedTiles(0, tilePosition.X, tilePosition.Y, sprite.tilesOcupancy))
        {
            return;
        }

        entity.Set(new PositionComponent { position = position, tilePosition = tilePosition, height = height });
        entity.Set(new TeamComponent(0));
        entity.Set(new ResourceDefinitionComponent(idResource, spriteId));
        if (health!=0)
        {
            entity.Set(new HealthComponent(health));
        }
        else
        {
            entity.Set(new HealthComponent(templateResource.health));
        }
        if (amount !=0)
        {
            entity.Set(new AmountComponent(amount));
        }
        else
        {
            entity.Set(new AmountComponent(templateResource.amount));
        }
        
        

        spatialEntityMap.Add(entity, ChunkHelper.WorldToChunkCoord(tilePosition),false);

        AsignarCollider(tilePosition.X, tilePosition.Y, entity, sprite);
        occupancyMap.SetTiles(0, tilePosition.X, tilePosition.Y, sprite.tilesOcupancy, entity.Id.Value);

        switch (sprite.tileSpriteType)
        {
            case TileSpriteType.Static:
                CreateSprite(entity, sprite.spriteData, height, tilePosition,render);
                break;
            case TileSpriteType.Animated:
                CreateAnimation(entity, sprite.animationData, spriteId, height, tilePosition,render);
                break;
        }
    }

    private void CreateSprite(Entity entity, WindowsDataBase.Accesories.DataBase.SpriteData spriteData, int heightRender, Vector2I positionTile, bool render)
    {
        
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(positionTile.X, positionTile.Y);
        Vector2 offset = spriteData.offsetInternal;

        float depthOffset = spriteData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, heightRender, layer, positionCenter);

        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(spriteData.scale, spriteData.scale, 1));
        if (render)
        {
            var RenderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(spriteData.idModMaterial);
            RenderingServer.MultimeshInstanceSetTransform(RenderInstance.rid, RenderInstance.instance, transform);
            RenderingServer.MultimeshInstanceSetCustomData(RenderInstance.rid, RenderInstance.instance, spriteData.uv);
            RenderingServer.MultimeshInstanceSetColor(RenderInstance.rid, RenderInstance.instance, new Godot.Color(1, 1, 1, RenderInstance.layerTexture));
            entity.Set(new RenderGPUComponent(RenderInstance.rid, RenderInstance.instance, 0, RenderInstance.layerTexture,
                     layer, depthOffset, spriteData.scale, offset));
        }
        else
        {
            entity.Set(new RenderGPUComponent(default, -1, 0, -1,layer, depthOffset, spriteData.scale, offset));
        }
        
    }

    private void CreateAnimation(Entity entity, WindowsDataBase.Accesories.DataBase.SpriteAnimationData animationData, int idSprite, int heightRender, Vector2I positionTile, bool render)
    {
        
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(positionTile);
        Vector2 offset = animationData.offsetInternal;

        float depthOffset = animationData.yDepthRenderFormat;
        float z = CommonAtributes.Calculate(depthOffset, heightRender, layer, positionCenter);
        Vector3 worldPosition = new(positionCenter.X + offset.X, positionCenter.Y + offset.Y, z);

        Transform3D transform = new(Basis.Identity, worldPosition);
        transform = transform.ScaledLocal(new Vector3(animationData.scale, animationData.scale, 1));
        entity.Set(new RenderTransformComponent(transform));

  

        if (render)
        {
            var RenderInstance = AtlasTexturesModsManager.Instance.CreateInstanceRender(animationData.idModMaterial);
            RenderingServer.MultimeshInstanceSetTransform(RenderInstance.rid, RenderInstance.instance, transform);
            RenderingServer.MultimeshInstanceSetCustomData(RenderInstance.rid, RenderInstance.instance, animationData.uvFramesArray[0]);
            RenderingServer.MultimeshInstanceSetColor(RenderInstance.rid, RenderInstance.instance, new Godot.Color(1, 1, 1, RenderInstance.layerTexture));
            entity.Set(new RenderGPUComponent(RenderInstance.rid, RenderInstance.instance, 0, RenderInstance.layerTexture,
                 layer, depthOffset, animationData.scale, offset));
            entity.Set(new AnimationSimpleComponent(idSprite, 1, 0, animationData.frameDuration, false, true, true));
            entity.Set(new RenderFrameDataComponent { uvMap = animationData.uvFramesArray[0] });

            entity.Add<SpriteSimpleAnimationTag>();
        }
        else
        {
            entity.Set(new RenderGPUComponent(default, -1, 0, -1, layer, depthOffset, animationData.scale, offset));
        }
        
    }

    private int AsignarCollider(int Mundo_x, int Mundo_y, Entity entity, TileSpriteData tileSpriteData)
    {
        if (tileSpriteData.fastCollidersBody.Count == 0) return 0;

        int idCollider = staticHashResource.GetNewEntityId();
        
        Vector2 positionCenter = TilesHelper.TilePositionToWorldPosition(Mundo_x, Mundo_y);

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;

        if (DEBUG_COLLIDERS)
        {
            _colliderDebugMap.Add(idCollider, new List<int>());
        }
            foreach (var collider in tileSpriteData.fastCollidersBody)
        {
            FastCollider fast = collider;
            float actualX = positionCenter.X + fast.Offset.X;
            float actualY = positionCenter.Y + fast.Offset.Y;
            float width = fast.Shape == ShapeType.Circle ? fast.Width * 2 : fast.Width;
            float height = fast.Shape == ShapeType.Circle ? fast.Height * 2 : fast.Height;

            float currentMinX = actualX - (width * 0.5f) - 0.01f;
            float currentMinY = actualY - (height * 0.5f) - 0.01f;
            float currentMaxX = actualX + (width * 0.5f) + 0.01f;
            float currentMaxY = actualY + (height * 0.5f) + 0.01f;

            if (currentMinX < minX) minX = currentMinX;
            if (currentMinY < minY) minY = currentMinY;
            if (currentMaxX > maxX) maxX = currentMaxX;
            if (currentMaxY > maxY) maxY = currentMaxY;

            if (DEBUG_COLLIDERS)
            {
                
                int idDebugBody = CollisionShapeDraw.Instance.DrawCollisionShapes(fast, positionCenter, Godot.Colors.OrangeRed);
                _colliderDebugMap[idCollider].Add(idDebugBody);
            }
        }

        staticHashResource.RegisterStatic(idCollider, entity, minX, minY, maxX, maxY);

        entity.Set(new SpatialIDComponent
        {
            Layer = CollisionConfig.TypeResource,
            Mask = CollisionConfig.None,
            Value = idCollider
        });

        return idCollider;
    }
}