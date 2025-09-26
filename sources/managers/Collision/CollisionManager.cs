using Arch.AOT.SourceGenerator;
using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Auios.QuadTree;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


[Component]
public struct ColliderShape
{
    public Rect2 aabb;
    public GeometricShape2D shape;    
}
[Component]
public struct ColliderSprite
{  
    public GeometricShape2D shapeBody;
    public GeometricShape2D shapeMove;   
}
internal class CollisionManager : SingletonBase<CollisionManager>
{
    public  SpatialHashMap<Entity> dynamicCollidersEntities;
    public  SpatialHashMap<Entity> MoveCollidersEntities;
    public SpatialHashMap<IDataTile> tileColliders;
    public SpatialHashMap<TerrainDataGame> spriteColliders;

    public QuadTree<ColliderSprite> quadTreeColliders;

    public SpatialHashMapColliders<Entity> characterCollidersEntities;

    public SpatialHashMapColliders<TerrainDataGame> terrainColliders;
    public SpatialHashMapColliders<ResourceSourceDataGame> ResourceSourceColliders;

    public SpatialHashMapColliders<Entity> BuildingsColliders;
    protected override void Initialize()
    {
        characterCollidersEntities = new SpatialHashMapColliders<Entity>(3); 

        dynamicCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
        MoveCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128
        tileColliders = new SpatialHashMap<IDataTile>(8, delegate (IDataTile er) { return er.IdCollider; });
        spriteColliders = new SpatialHashMap<TerrainDataGame>(8, delegate (TerrainDataGame er) { return er.idUnique; });

        terrainColliders = new SpatialHashMapColliders<TerrainDataGame>(2);
        ResourceSourceColliders = new SpatialHashMapColliders<ResourceSourceDataGame>(4);
        BuildingsColliders = new SpatialHashMapColliders<Entity>(4);
    }

    protected override void Destroy()
    {
      
    }
    public static bool CheckAnyCollisionMoveUnits(Entity entity,Vector2 movementNext, GeometricShape2D collisionShape)
    {
        var teamOrigin = entity.Get<TeamComponent>();
        Rect2 aabb = new Rect2(movementNext - (collisionShape.GetSizeQuad() / 2), collisionShape.GetSizeQuad());
        var data = Instance.characterCollidersEntities.GetCollidingOwnersInAABB(aabb, entity.Get<ColliderComponent>().idCollider);
        if (data != null)
        {
            foreach (var item in data)
            {
                var team = item.Get<TeamComponent>();
                if (teamOrigin.team != team.team )
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static List<Entity> CheckAnyEntityColliders(Vector2 position, GeometricShape2D shape)
    {
        List<Entity> list = new List<Entity>();
                 
        Rect2 aabb = new Rect2(position - (shape.GetSizeQuad() / 2), shape.GetSizeQuad());
        
        return Instance.characterCollidersEntities.GetCollidingOwnersInAABB(aabb);
    }
    public static bool CheckAnyCollisionStatic(
          Entity entity,
          Vector2 movementNext,
          GeometricShape2D collisionShape)
    {        
        Rect2 aabb = new Rect2(movementNext - (collisionShape.GetSizeQuad()/2), collisionShape.GetSizeQuad());

        if (Instance.terrainColliders.IntersectsAABB(aabb))
        {
            return true;
        }
        if (Instance.ResourceSourceColliders.IntersectsAABB(aabb))
        {
            return true;
        }
        if (Instance.BuildingsColliders.IntersectsAABB(aabb))
        {
            return true;
        }
        return false;
    }

    public static bool CheckAnyCollisionMoveUnitOnly(
        Entity entity,
        Vector2 movementNext,
        GeometricShape2D collisionShape)
    {
        Rect2 aabb = new Rect2(movementNext - (collisionShape.GetSizeQuad() / 2), collisionShape.GetSizeQuad());

        if (Instance.characterCollidersEntities.IntersectsAABB(aabb, entity.Get<ColliderComponent>().idCollider))
        {
            return true;
        }
 
        return false;
    }

    internal static bool CheckSegmentCollision(Vector2 startPos, Vector2 endPos, GeometricShape2D collisionShape)
    {
        // 1. Rectángulo que cubre el trayecto completo
        Vector2 shapeSize = collisionShape.GetSizeQuad();
        Rect2 sweptAABB = new Rect2(
            new Vector2(Mathf.Min(startPos.X, endPos.X), Mathf.Min(startPos.Y, endPos.Y)) - shapeSize / 2f,
            new Vector2(Mathf.Abs(endPos.X - startPos.X), Mathf.Abs(endPos.Y - startPos.Y)) + shapeSize
        );
        // 2. Revisar primero si cae en alguna celda de estáticos
        if (!Instance.terrainColliders.IntersectsAABB(sweptAABB) &&
            !Instance.ResourceSourceColliders.IntersectsAABB(sweptAABB) &&
            !Instance.BuildingsColliders.IntersectsAABB(sweptAABB))
        {
            return false; // rápido: no hay nada en el trayecto
        }
        return true;
    }
}

