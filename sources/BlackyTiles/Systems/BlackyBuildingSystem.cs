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

public class BlackyBuildingSystem
{
    private readonly BlackyChunkOccupancyMap occupancyMap;
    private readonly BlackySpatialEntityMap spatialEntityMap;
    private readonly BlackyEntityRenderSystem renderSystem;
    private readonly BlackyTerrainSystem terrain;
    private int _buildingCount = 0;
    private const bool DEBUG_COLLIDERS = true;
    private readonly FlecsManager flecsManager;
    public BlackyBuildingSystem(FlecsManager flecsManager, BlackyChunkOccupancyMap occupancyMap, BlackySpatialEntityMap spatialEntityMap, BlackyEntityRenderSystem renderSystem, BlackyTerrainSystem terrain)
    {
        this.occupancyMap = occupancyMap;
        this.spatialEntityMap = spatialEntityMap;
        this.renderSystem = renderSystem;
        this.terrain = terrain;
        this.flecsManager = flecsManager;
    }

    public Entity Create(long id, Vector2I positionTileWorld, bool renderForce = false)
    {          
        Vector2 position = TilesHelper.WorldPositionTile(positionTileWorld);

        var dataTemplate = MasterDataManager.GetData<BuildingData>(id);

        var tileSprite = MasterDataManager.GetData<TileSpriteData>(dataTemplate.idTileSpriteNormal);

        if (occupancyMap.IsOccupiedTiles(0, positionTileWorld.X, positionTileWorld.Y, tileSprite.tilesOcupancy))
        {
            return default;
        }
        _buildingCount++;

        int height = terrain.GetTopHeight(positionTileWorld);
        if (height>= 1)
        {
            height = height +1;
        }
        height = 4;
        var entity = flecsManager.WorldFlecs.Entity();

        int idCollider = 0;
        Rect2 rectangle = new Rect2();
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
                //CollisionShapeDraw.Instance.DrawCollisionShapes(tileSprite.spriteData.listCollisionBody.ToList(), position);

                Godot.Vector2 pos = position - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
                rectangle = new Rect2(pos, colliderBody.GetSizeQuad());
                idCollider = CollisionManager.Instance.BuildingsCollidersFlecs.AddColliderObject(entity, tileSprite.spriteData.listCollisionBody.ToList(), position, 1, colliderBody, false);
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
                //CollisionShapeDraw.Instance.DrawCollisionShapes(tileSprite.animationData.collisionBodyArray.ToList(), position);
                Godot.Vector2 pos = position - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
                rectangle = new Rect2(pos, colliderBody.GetSizeQuad());
                idCollider = CollisionManager.Instance.BuildingsCollidersFlecs.AddColliderObject(entity, tileSprite.animationData.collisionBodyArray.ToList(), position, 1, colliderBody, false);
                entity.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent, new Rect2(), Vector2.Zero, 0));
            }

        }

        entity.Set(new PositionComponent { position = position, tilePosition = positionTileWorld, height = height });
        entity.Set(new BuildingComponent { colliderLocal = rectangle });
        entity.Set(new TeamComponent(0));
        entity.Set(new IdGenericComponent(id, EntityType.EDIFICIO));
        entity.Set(new HealthComponent(dataTemplate.maxHealth));
        entity.Set(new DirectionComponent()); // Dirección por defecto hacia arriba
        entity.Set(new AttackPendingComponent(false, default));
        entity.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent, new Rect2(), Vector2.Zero, 0));
        entity.Set(new TileSpriteComponent(tileSprite.id));

        spatialEntityMap.Add(entity, ChunkHelper.WorldToChunkCoord(positionTileWorld));


        if (dataTemplate.attackRange > 0)
        {
            int damage = 10;
            if (dataTemplate.attackPowers != null)
            {
                foreach (var attackPower in dataTemplate.attackPowers)
                {
                    switch (attackPower.type)
                    {
                        case ElementType.BASE:
                            damage = (int)attackPower.value;
                            break;
                        case ElementType.FIRE:
                            break;
                        case ElementType.AIR:
                            break;
                        case ElementType.WATER:
                            break;
                        case ElementType.DARK:
                            break;
                        case ElementType.LIGHT:
                            break;
                        case ElementType.EARTH:
                            break;
                    }
                }
            }


            entity.Set(new RangedAttackComponent
            {
                Cooldown = 1,
                Range = dataTemplate.attackRange,
                Damage = damage,
                idProjectile = 1,
                Timer = 0,
                Homing = false,
                SpeedProjectile = 10,
            });

        }

        if (renderForce)
        {
            renderSystem.ForceRenderEntity(entity);
        }

        occupancyMap.SetTiles(0, positionTileWorld.X, positionTileWorld.Y, tileSprite.tilesOcupancy, entity.Id.Value);

        return entity;
    }

    public void Remove(Vector2I positionTileWorld)
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

            spatialEntityMap.Remove(entity);
            renderSystem.ForceDisposeEntity(entity);
            RenderCommandQueue.Enqueue(new DestroyEntityCommand(entity));
        }
        occupancyMap.ClearByEntity(0, positionTileWorld.X, positionTileWorld.Y);

    }
}
