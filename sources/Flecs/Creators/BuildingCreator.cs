using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Buildings;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotFlecs.sources.Flecs.Creators;
internal class BuildingCreator: SingletonBase<BuildingCreator>
{
    //private int _buildingCount = 0;
    //private int layer = 20;
    //public Entity Create(long id,Vector2I positionTileWorld)
    //{
    //    Vector2 position = TilesHelper.WorldPositionTile(positionTileWorld);
    //    _buildingCount++;
    //    var dataTemplate = MasterDataManager.GetData<BuildingData>(id);
    //    var tileSprite = MasterDataManager.GetData<TileSpriteData>(dataTemplate.idTileSpriteNormal);

    //    bool isPosible = EntityChunkMap.Instance.IsPosibleAddEntity(positionTileWorld, tileSprite.tilesOcupancy, layer);
    //    if (!isPosible)
    //    {
    //        return default;
    //    }

    //    var entity = FlecsManager.Instance.WorldFlecs.Entity();

    //    var colliderBody = tileSprite.spriteData.listCollisionBody[0];

    //    Godot.Vector2 pos = position - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
    //    var rectangle = new Rect2(pos, colliderBody.GetSizeQuad() );
    //    int idCollider =CollisionManager.Instance.BuildingsCollidersFlecs.AddColliderObject(entity, tileSprite.spriteData.listCollisionBody.ToList(), position,1, colliderBody,false);
    //    CollisionShapeDraw.Instance.DrawCollisionShapes(tileSprite.spriteData.listCollisionBody.ToList(), position); 
    //    //int idDebug =CreateShapeDebug(colliderBody.GetSizeQuad(), position + colliderBody.OriginCurrent);
    //    // Aquí puedes agregar los componentes necesarios para el edificio
    //    entity.Set(new PositionComponent { position = position, tilePosition = positionTileWorld });
    //    entity.Set(new BuildingComponent { colliderLocal = rectangle});
    //    entity.Set(new TeamComponent(1));
    //    entity.Set(new IdGenericComponent(id, EntityType.EDIFICIO));
    //    entity.Set(new HealthComponent(dataTemplate.maxHealth));
    //    entity.Set(new DirectionComponent()); // Dirección por defecto hacia arriba
    //    entity.Set(new AttackPendingComponent(false, default));
    //    entity.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent,new Rect2(), Vector2.Zero,0));
    //    entity.Set(new TileSpriteComponent(tileSprite.id));
                    

    //    if (dataTemplate.attackRange>0)
    //    {
    //       int damage = 10;
    //        if (dataTemplate.attackPowers!=null)
    //        {
    //            foreach (var attackPower in dataTemplate.attackPowers)
    //            {
    //                switch (attackPower.type)
    //                {
    //                    case ElementType.BASE:
    //                        damage = (int)attackPower.value;
    //                        break;
    //                    case ElementType.FIRE:
    //                        break;
    //                    case ElementType.AIR:
    //                        break;
    //                    case ElementType.WATER:
    //                        break;
    //                    case ElementType.DARK:
    //                        break;
    //                    case ElementType.LIGHT:
    //                        break;
    //                    case ElementType.EARTH:
    //                        break;
    //                }
    //            }
    //        }
            

    //        entity.Set(new RangedAttackComponent
    //        {
    //            Cooldown = 1,
    //            Range = dataTemplate.attackRange,
    //            Damage = damage,
    //            idProjectile = 1,
    //            Timer = 0,
    //            Homing = false,
    //            SpeedProjectile = 10,
    //        });

    //    }
    //    EntityChunkMap.Instance.AddEntityToChunk(entity, positionTileWorld, tileSprite.tilesOcupancy, layer);
    //    return entity;
    //}
    //int CreateShapeDebug(Vector2 square, Vector2 position)
    //{
    //    return WireShape.Instance.DrawSquare(square, position, 30, Godot.Colors.Green, WireShape.TypeDraw.NORMAL);
    //}

    //public void RemoveTile(Vector2I tilePositionGlobal)
    //{
    //    var entity = EntityChunkMap.Instance.GetEntityInChunk(tilePositionGlobal, EntityType.EDIFICIO, layer);
    //    if (entity != Entity.Null())
    //    {
    //        ColliderComponent collider = entity.Get<ColliderComponent>();
    //        CollisionManager.Instance.BuildingsCollidersFlecs.RemoveCollider(collider.idCollider);
    //        WireShape.Instance.FreeShape(collider.idDebug);
    //        EntityChunkMap.Instance.RemoveEntityFromChunk(entity, tilePositionGlobal);
    //    }

    //}
}
