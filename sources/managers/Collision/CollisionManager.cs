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

    public SpatialHashMapColliders<Flecs.NET.Core.Entity> characterEntitiesFlecs;

    public SpatialHashMapColliders<TerrainDataGame> terrainColliders;

    public SpatialHashMapColliders<Flecs.NET.Core.Entity> ResourceSourceCollidersFlecs;

    public SpatialHashMapColliders<Entity> BuildingsColliders;
    public SpatialHashMapColliders<Flecs.NET.Core.Entity> BuildingsCollidersFlecs;
    protected override void Initialize()
    {
        characterCollidersEntities = new SpatialHashMapColliders<Entity>(3);
        

        dynamicCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
        MoveCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128
        tileColliders = new SpatialHashMap<IDataTile>(8, delegate (IDataTile er) { return er.IdCollider; });
        spriteColliders = new SpatialHashMap<TerrainDataGame>(8, delegate (TerrainDataGame er) { return er.idUnique; });

        terrainColliders = new SpatialHashMapColliders<TerrainDataGame>(2);        
        BuildingsColliders = new SpatialHashMapColliders<Entity>(4);

        characterEntitiesFlecs = new SpatialHashMapColliders<Flecs.NET.Core.Entity>(2);
        BuildingsCollidersFlecs = new SpatialHashMapColliders<Flecs.NET.Core.Entity>(2);
        ResourceSourceCollidersFlecs = new SpatialHashMapColliders<Flecs.NET.Core.Entity>(2);
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
      
        if (Instance.BuildingsColliders.IntersectsAABB(aabb))
        {
          
            return true;
        }
        return false;
    }

    public static bool CheckAnyCollisionMoveUnitOnly(
        int idCollider,
        Vector2 movementNext,
        GeometricShape2D collisionShape)
    {
        Rect2 aabb = new Rect2(movementNext - (collisionShape.GetSizeQuad() / 2), collisionShape.GetSizeQuad());

        if (Instance.characterCollidersEntities.IntersectsAABB(aabb, idCollider))
        {
            return true;
        }
 
        return false;
    }

    internal static bool CheckSegmentCollision(Vector2 startPos, Vector2 endPos, GeometricShape2D collisionShape)
    {
        // 1. Rectángulo que cubre el trayecto completo
        Vector2 shapeSize = collisionShape.GetSizeQuad();
        //+(shapeSize / 2f)
        Rect2 sweptAABB = new Rect2(
            new Vector2(Mathf.Min(startPos.X, endPos.X), Mathf.Min(startPos.Y, endPos.Y)) +collisionShape.OriginCurrent,
            new Vector2(Mathf.Abs(endPos.X - startPos.X), Mathf.Abs(endPos.Y - startPos.Y)) //+ shapeSize
        );
        // 2. Revisar primero si cae en alguna celda de estáticos
        if (!Instance.terrainColliders.IntersectsAABB(sweptAABB) &&
          
            !Instance.BuildingsColliders.IntersectsAABB(sweptAABB))
        {
            return false; // rápido: no hay nada en el trayecto
        }
        else
        {
            
        }
        return true;
    }

    // 🔹 Empuja al punto fuera de la colisión según el tipo de collider
    public static Vector2 ResolvePenetration(Vector2 position, Vector2 pointNextCollision, GeometricShape2D shape)
    {
        switch (shape)
        {
            case Circle circle:
                return ResolveCirclePenetration(position, circle, pointNextCollision);

            case Rectangle rect:
                return ResolveAabbPenetration(position, rect, pointNextCollision);

            default:
                // Si no sabemos manejar la forma, devolvemos la posición original
                return position;
        }
    }

    private static Vector2 ResolveCirclePenetration(Vector2 pos, Circle circle, Vector2 pointNextCollision)
    {
        Vector2 center = circle.OriginCurrent;
        float radius = circle.Radius;

        Vector2 dir = pos - center;
        float dist = dir.Length();

        if (dist < radius)
        {
            // saca al punto justo en la frontera del círculo
            dir = dir.Normalized();
            pos = center + dir * (radius + 0.001f); // pequeño epsilon
        }

        return pos;
    }

    private static Vector2 ResolveAabbPenetration(Vector2 pos, Rectangle rect, Vector2 pointNextCollision)
    {
        Rect2 aabb = new Rect2(pointNextCollision - (rect.GetSizeQuad() / 2), rect.GetSizeQuad());
        WireShape.Instance.DrawFilledSquare(aabb.Size, aabb.Position, 40, Colors.Red, 1, WireShape.TypeDraw.NORMAL);
        if (aabb.HasPoint(pos))
        {
            // calcula cuánto está penetrando por cada lado
            float left = pos.X - aabb.Position.X;
            float right = (aabb.Position.X + aabb.Size.X) - pos.X;
            float top = pos.Y - aabb.Position.Y;
            float bottom = (aabb.Position.Y + aabb.Size.Y) - pos.Y;

            // elige el desplazamiento más corto
            float min = Math.Min(Math.Min(left, right), Math.Min(top, bottom));

            if (min == left) pos.X = aabb.Position.X - 0.001f;
            else if (min == right) pos.X = aabb.Position.X + aabb.Size.X + 0.001f;
            else if (min == top) pos.Y = aabb.Position.Y - 0.001f;
            else if (min == bottom) pos.Y = aabb.Position.Y + aabb.Size.Y + 0.001f;
        }

        return pos;
    }
}

