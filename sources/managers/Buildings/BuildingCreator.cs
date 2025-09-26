using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Buildings;
internal class BuildingCreator:SingletonBase<BuildingCreator>
{
    public Entity CreateBuilding(int idBuilding, Godot.Vector2 position, Godot.Vector2 positionCollider)
    {
        Entity entity = EcsManager.Instance.World.Create();
        BuildingData data = BuildingManager.Instance.GetData(idBuilding);

        switch (data.buildingType)
        {
            case BuildingType.Ninguno:
                break;
            case BuildingType.ProductorMaterial:
                break;
            case BuildingType.ProductorUnidades:
                break;
            case BuildingType.ProductorRecurso:
                
                break;
            case BuildingType.TorreDefensa:
                CreateTower(entity, data, position,positionCollider);
                break;
            case BuildingType.Procesador:
                break;
            case BuildingType.GeneradorMejoras:
                break;
            case BuildingType.Adorno:
                CreateBuildingOrnament(entity, data, position);
                break;
            default:
                break;
        }


        return entity;
    }

    private void CreateBuildingOrnament(Entity entity, BuildingData data, Godot.Vector2 position)
    {
        
        entity.Add(new HealthComponent { current = data.maxHealth });

        if (data.spriteData.haveCollider)
        {
            int idCollider = CollisionManager.Instance.BuildingsColliders.AddColliderObject(entity, data.spriteData.collisionBody, position);
            entity.Add(new ColliderComponent { idCollider = idCollider });
        }
        
    }

    public void CreateTower(Entity entity, BuildingData data, Godot.Vector2 positionReal, Godot.Vector2 positionCollider)
    {
         
        GD.Print("tower:"+ positionReal);
        WireShape.Instance.DrawCircle(data.attackRange, positionReal, 30, Colors.Red);
        WireShape.Instance.DrawCircle(5, positionReal, 30, Colors.Green);
        entity.Add(new HealthComponent { current = data.maxHealth });
        entity.Add(new AttackRangeComponent { attackRange = MeshCreator.PixelsToUnits( data.attackRange) });
        entity.Add(new AttackCooldownComponent { maxCooldown = data.attackCooldown });
        entity.Add(new AttackDamageComponent { damage = 10 });
        entity.Add(new BuildingComponent { id = data.id });
        entity.Add(new TeamComponent { team = 1 });
        entity.Add(new PositionComponent { position = positionReal });
        entity.Add(new TargetingComponent { targetEntity = Entity.Null });
        entity.Add(new AttackEffectComponent { effectType = AttackEffectType.Stun, duration = 0.25f });
        if (data.spriteData.haveCollider)
        {
            int idCollider = CollisionManager.Instance.BuildingsColliders.AddColliderObject(entity, data.spriteData.collisionBody, positionCollider);
            entity.Add(new ColliderComponent { idCollider = idCollider });
        }

    }

    public Entity SpawnProjectile(int idBuilding, Godot.Vector2 positionOrigin, Godot.Vector2 positionTarget)
    {
        Entity entity = EcsManager.Instance.World.Create();
        BuildingData data = BuildingManager.Instance.GetData(idBuilding);

        Godot.Vector2 direction = (positionTarget - positionOrigin).Normalized();

        foreach (var item in data.bonusPowers)
        {
            switch (item.type)
            {
                case BonusType.DURABILITY:
                    break;
                case BonusType.VELOCITY_ATTACK:
                    break;
                case BonusType.SPACE_BAG:
                    break;
                case BonusType.VELOCITY_MOVE:
                    break;
                case BonusType.RANGO_ATAQUE_EDIFICIOS:
                    break;
                default:
                    break;
            }
        }

        entity.Add(new DirectionComponent { normalized = direction });

        return entity;
    }
    public void Destroy(Entity entity)
    {        
        CollisionManager.Instance.BuildingsColliders.RemoveCollider(entity.Get<ColliderComponent>().idCollider);
        EcsManager.Instance.World.Destroy(entity);
    }
}
