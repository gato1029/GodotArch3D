using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Buildings;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.Flecs.Creators;
internal class BuildingCreator: SingletonBase<BuildingCreator>
{
    private int _buildingCount = 0;
    public Entity Create(long id, Godot.Vector2 position, Vector2I positionTileWorld)
    {
        _buildingCount++;
        var dataTemplate = MasterDataManager.GetData<BuildingData>(id);
                
        var entity = FlecsManager.Instance.WorldFlecs.Entity("Building" + _buildingCount);

        var colliderBody = dataTemplate.spriteData.collisionBody;

        Godot.Vector2 pos = position - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
        var rectangle = new Rect2(pos, colliderBody.GetSizeQuad() );
        int idCollider =CollisionManager.Instance.BuildingsCollidersFlecs.AddColliderObject(entity, colliderBody, position, colliderBody);
        // Aquí puedes agregar los componentes necesarios para el edificio
        entity.Set(new PositionComponent { position = position, tilePosition = positionTileWorld });
        entity.Set(new BuildingComponent { colliderLocal = rectangle});
        entity.Set(new TeamComponent(1));
        entity.Set(new IdGenericComponent(id, EntityType.EDIFICIO));
        entity.Set(new HealthComponent(dataTemplate.maxHealth));
        entity.Set(new DirectionComponent()); // Dirección por defecto hacia arriba
        entity.Set(new AttackPendingComponent(false, default));
        entity.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent,new Rect2(), Vector2.Zero));

        CreateShapeDebug(colliderBody.GetSizeQuad(), position + colliderBody.OriginCurrent);

    

        if (dataTemplate.attackRange>0)
        {
           int damage = 10;
            if (dataTemplate.attackPowers!=null)
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
        
        return entity;
    }
    int CreateShapeDebug(Vector2 square, Vector2 position)
    {
        return WireShape.Instance.DrawSquare(square, position, 30, Godot.Colors.Green, WireShape.TypeDraw.NORMAL);
    }
}
