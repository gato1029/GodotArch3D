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
    public SpatialHashMap<IDataSprite> spriteColliders;

    public QuadTree<ColliderSprite> quadTreeColliders;

    public SpatialHashMap<Entity> characterCollidersEntities;
    protected override void Initialize()
    {
        characterCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 

        dynamicCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128 
        MoveCollidersEntities = new SpatialHashMap<Entity>(8, delegate (Entity er) { return er.Id; }); // unidades de 128 x 128
        tileColliders = new SpatialHashMap<IDataTile>(8, delegate (IDataTile er) { return er.IdCollider; });
        spriteColliders = new SpatialHashMap<IDataSprite>(8, delegate (IDataSprite er) { return er.idUnique; });
    }

    protected override void Destroy()
    {
      
    }

    public static bool CheckAnyCollision(
          Entity entity,
          Vector2 movementNext,
          GeometricShape2D collisionMove)
    {
        

        Rect2 aabb = new Rect2(movementNext, collisionMove.GetSizeQuad() * 2);

        // 1. Chequeo contra entidades
        var entities = CollisionManager.Instance.characterCollidersEntities.QueryAABB(aabb);
        if (entities != null)
        {
            foreach (var item in entities.Values)
            {
                foreach (var itemInternal in item)
                {
                    if (itemInternal.Value.Id != entity.Id)
                    {
                        var characterComponentB = itemInternal.Value.Get<CharacterComponent>();
                        var dataCharacterModelB = CharacterModelManager.Instance.GetCharacterModel(characterComponentB.idCharacterBaseData);
                        var characterB = dataCharacterModelB.animationCharacterBaseData;
                        var colliderB = characterB.collisionMove.Multiplicity(dataCharacterModelB.scale);
                        var positionB = itemInternal.Value.Get<PositionComponent>().position + colliderB.OriginCurrent;

                        if (Collision2D.Collides(collisionMove, colliderB, movementNext, positionB))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        // 2. Chequeo contra tiles
        var tileData = CollisionManager.Instance.tileColliders.QueryAABB(aabb);
        if (tileData != null)
        {
            foreach (var item in tileData.Values)
            {
                foreach (var itemInternal in item)
                {
                    var tileInfo = TilesManager.Instance.GetTileData(itemInternal.Value.IdTile);
                    var colliderB = tileInfo.collisionBody.Multiplicity(tileInfo.scale);
                    var positionB = itemInternal.Value.PositionCollider;

                    if (Collision2D.Collides(collisionMove, colliderB, movementNext, positionB))
                    {
                        return true;
                    }
                }
            }
        }

        // 3. Chequeo contra sprites
        var spriteData = CollisionManager.Instance.spriteColliders.QueryAABB(aabb);
        if (spriteData != null)
        {
            foreach (var item in spriteData.Values)
            {
                foreach (var itemInternal in item)
                {
                    var spriteInfo = itemInternal.Value.GetSpriteData();
                    var colliderB = spriteInfo.collisionBody.Multiplicity(spriteInfo.scale);
                    var positionB = itemInternal.Value.positionCollider;

                    if (Collision2D.Collides(collisionMove, colliderB, movementNext, positionB))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


}

