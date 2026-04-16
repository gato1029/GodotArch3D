using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles.Commands;
using GodotEcsArch.sources.BlackyTiles.Entities;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Systems;

public class BlackyResourcesSourceSystem
{
    private readonly BlackyChunkOccupancyMap occupancyMap;
    private readonly BlackySpatialEntityMap spatialEntityMap;
    private readonly BlackyEntityRenderSystem renderSystem;
    private readonly BlackyTerrainSystem terrain;
    private readonly FlecsManager flecsManager;
    private readonly StaticSpatialGridOptimized staticHash; 
    private int _resourcesCount = 0;

    private const bool DEBUG_COLLIDERS = false;

    public BlackyResourcesSourceSystem(StaticSpatialGridOptimized staticHash,FlecsManager flecsManager, BlackyChunkOccupancyMap occupancyMap, BlackySpatialEntityMap spatialEntityMap, BlackyEntityRenderSystem renderSystem, BlackyTerrainSystem terrain)
    {
        this.occupancyMap = occupancyMap;
        this.spatialEntityMap = spatialEntityMap;
        this.renderSystem = renderSystem;
        this.terrain = terrain;
        this.flecsManager = flecsManager;
        this.staticHash = staticHash;
    }

    public void EnqueueCreate(ushort id, Vector2I positionTileWorld, bool renderForce = false)
    {
        RenderCommandQueue.Enqueue(
            new CreateResourceSourceCommand(this, id, positionTileWorld, renderForce)
        );
    }

    private void AddCollider(Entity entity, GeometricShape2D collisionBody)
    {
        // cuerpo
        ShapeType shapeType = ShapeType.Rect;
        float width = 0;
        float height = 0;
        float offsetX = 0;
        float offsetY = 0;

        switch (collisionBody)
        {
            case Circle circle:
                shapeType = ShapeType.Circle;
                width = circle.Radius;
                height = circle.Radius;
                offsetX = circle.OriginCurrent.X ;
                offsetY = circle.OriginCurrent.Y ;
                break;
            case Rectangle rectangle:
                shapeType = ShapeType.Rect;
                width = rectangle.Width;
                height = rectangle.Height;
                offsetX = rectangle.OriginCurrent.X;
                offsetY = rectangle.OriginCurrent.Y;
                break;
            default:
                break;
        }
        var SpatialIDComponent = new SpatialIDComponent
        {
            Layer = CollisionConfig.TypeResource,
            Mask = CollisionConfig.None,
            Value = staticHash.GetNewEntityId()
        };

        FastCollider[] bodyColliders = new FastCollider[1]
        {
            new FastCollider
            {
                Shape = shapeType,
                Width = width,
                Height = height,
                Offset = new Vector2(offsetX, offsetY)
            }
        };
        var BodyComponent = new BodyColliderComponent
        (
           bodyColliders
        );

        entity.Set(SpatialIDComponent);
        entity.Set(BodyComponent);
        entity.Add<StaticTag>();

        if (collisionBody is Circle)
        { 
            width = width * 2;
            height = height * 2;
        }
        // 2. REGISTRO DIRECTO AL STATIC HASH
        // Como es estático, lo anotamos una sola vez ahora mismo.
        float actualX = entity.Get<PositionComponent>().position.X + offsetX;
        float actualY = entity.Get<PositionComponent>().position.Y + offsetY;

        var tilePositionMin =new Vector2(actualX - (width * 0.5f) - 0.01f, actualY - (width * 0.5f) - 0.01f); // quito un poco para asegurar que cubre el tile correcto aunque esté justo en el borde
        var tilePositionMax = new Vector2(actualX + (width * 0.5f) - 0.01f, actualY + (height * 0.5f) - 0.01f);    
        staticHash.RegisterStatic(SpatialIDComponent.Value, entity, tilePositionMin.X, tilePositionMin.Y, tilePositionMax.X, tilePositionMax.Y);
 
                

    }
    public Entity Create(ushort id, Vector2I positionTileWorld, bool renderForce = false)
    {

        Vector2 position = TilesHelper.WorldPositionTile(positionTileWorld);

        var dataTemplate = MasterDataManager.GetBySaveIds<ResourceSourceData>(id);

        int randomIndex = new Random().Next(0, dataTemplate.listIdTileSpriteData.Count);
        long idTileSprite = dataTemplate.listIdTileSpriteData[randomIndex];
        var tileSprite = MasterDataManager.GetData<TileSpriteData>(idTileSprite);

        if (occupancyMap.IsOccupiedTiles(0, positionTileWorld.X, positionTileWorld.Y, tileSprite.tilesOcupancy))
        {
            return default;
        }
        _resourcesCount++;

        int height = terrain.GetTopHeight(positionTileWorld);
        if (height == 1)
        {
            height = 2;
        }
        height = 4;
        var entity = flecsManager.WorldFlecs.Entity();

        GeometricShape2D colliderBody = null;
        
        if (tileSprite.tileSpriteType == TileSpriteType.Static)
        {
            if (tileSprite.spriteData.listCollisionBody != null)
            {
                colliderBody = tileSprite.spriteData.listCollisionBody[0];
                if (DEBUG_COLLIDERS)
                {
                    List<int> idsDebugsCollider = CollisionShapeDraw.Instance.DrawCollisionShapes(tileSprite.spriteData.listCollisionBody.ToList(), position);
                    entity.Set(new ColliderDebugComponent(idsDebugsCollider));
                }
                

                Godot.Vector2 pos = position - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
                var rectangle = new Rect2(pos, colliderBody.GetSizeQuad());
                int idCollider = CollisionManager.Instance.ResourceSourceCollidersFlecs.AddColliderObject(entity, tileSprite.spriteData.listCollisionBody.ToList(), position, 1, colliderBody, false);
                entity.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent, new Rect2(), Vector2.Zero, 0));
            }

        }
        else
        {
            if (tileSprite.animationData.collisionBodyArray != null)
            {
                colliderBody = tileSprite.animationData.collisionBodyArray[0];
                if (DEBUG_COLLIDERS)
                {
                    List<int> idsDebugsCollider = CollisionShapeDraw.Instance.DrawCollisionShapes(tileSprite.animationData.collisionBodyArray.ToList(), position);
                    entity.Set(new ColliderDebugComponent(idsDebugsCollider));
                }                        
                Godot.Vector2 pos = position - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
                var rectangle = new Rect2(pos, colliderBody.GetSizeQuad());
                int idCollider = CollisionManager.Instance.ResourceSourceCollidersFlecs.AddColliderObject(entity, tileSprite.animationData.collisionBodyArray.ToList(), position, 1, colliderBody, false);
                entity.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent, new Rect2(), Vector2.Zero, 0));
            }

        }

        

        entity.Set(new PositionComponent { position = position, tilePosition = positionTileWorld, height = height });
        entity.Set(new TeamComponent(0));
        entity.Set(new IdGenericComponent(id, EntityType.RECURSO));
        entity.Set(new HealthComponent(10));
        entity.Set(new TileSpriteComponent(tileSprite.id));

        spatialEntityMap.Add(entity, ChunkHelper.WorldToChunkCoord(positionTileWorld));
        AddCollider(entity, colliderBody);

        if (renderForce)
        {
            renderSystem.ForceRenderEntity(entity);
        }

        occupancyMap.SetTiles(0, positionTileWorld.X, positionTileWorld.Y, tileSprite.tilesOcupancy, entity.Id.Value);

        return entity;
    }

    public void remove(Vector2I positionTileWorld)
    {
        ulong idEntity = occupancyMap.Get(0, positionTileWorld.X, positionTileWorld.Y);
        // FlecsManager.Instance.WorldFlecs como busco la entidad a partir del idEntity

        // Si idEntity es 0, no hay entidad
        if (idEntity != 0)
        {

           Entity entity = flecsManager.WorldFlecs.Entity(idEntity);

            
            if (entity.Has<ColliderComponent>())
            {
                ColliderComponent collider = entity.Get<ColliderComponent>();
                CollisionManager.Instance.ResourceSourceCollidersFlecs.RemoveCollider(collider.idCollider);
                if (DEBUG_COLLIDERS)
                {
                    var debugCollider = entity.Get<ColliderDebugComponent>();
                    foreach (var idDebug in debugCollider.idShapes)
                    {
                        WireShape.Instance.FreeShape(idDebug);
                    }

                }
            }
            entity.Children((Entity child) =>
            {
                if (!child.Has<SpatialIDComponent>() || !child.Has<FastColliderComponent>())
                    return;

                var spatial = child.Get<SpatialIDComponent>();
                var collider = child.Get<FastColliderComponent>();
                var pos = child.Get<PositionComponent>().position;

                float actualX = pos.X + collider.OffsetX;
                float actualY = pos.Y + collider.OffsetY;

                float width = collider.Width;
                float height = collider.Height;

                if (collider.Shape == ShapeType.Circle)
                {
                    width = width * 2;
                    height = height * 2;
                }

                var tilePositionMin = new Vector2(actualX - (width * 0.5f) - 0.01f, actualY - (width * 0.5f) - 0.01f); // quito un poco para asegurar que cubre el tile correcto aunque esté justo en el borde
                var tilePositionMax = new Vector2(actualX + (width * 0.5f) - 0.01f, actualY + (height * 0.5f) - 0.01f);
                staticHash.UnregisterStatic(spatial.Value, tilePositionMin.X, tilePositionMin.Y, tilePositionMax.X, tilePositionMax.Y);

            
            });
            

            spatialEntityMap.Remove(entity);
           renderSystem.ForceDisposeEntity(entity);               
           RenderCommandQueue.Enqueue(new DestroyEntityCommand(entity));
        }
        occupancyMap.ClearByEntity(0, positionTileWorld.X, positionTileWorld.Y);

    }
}
