
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Resources;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using ProtoBuf;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GodotEcsArch.sources.managers.Maps;


public class ResourceSourceMap
{    
    private Vector2I chunkDimencion;

    private string carpet = "ResourceSource";

    private string name = "ResourceSourceData";

    public string pathMapParent { get; set; }

    public int layer { get; set; }

    public string pathCurrentCarpet { get; set; }


    public ResourceSourceMap(string pathMapParent, int Layer)
    {      
        layer = 20;      
    }

  
    public void AddUpdateResource(Vector2I tilePositionGlobal,long idResource, int indexTileSpriteSelected)
    {
        Vector2 positionCenter = TilesHelper.WorldPositionTile(tilePositionGlobal);

        var data = MasterDataManager.GetData<ResourceSourceData>(idResource);
        long idTileSpriteData = data.listIdTileSpriteData[indexTileSpriteSelected];
        var dataTileSprite = MasterDataManager.GetData<TileSpriteData>(idTileSpriteData);
                
        bool isPosible =EntityChunkMap.Instance.IsPosibleAddEntity(tilePositionGlobal, dataTileSprite.tilesOcupancy, layer);
        if (!isPosible)
        {
            return;
        }
                

      
        //Entity entityResource = FlecsManager.Instance.WorldFlecs.Entity();

        //entityResource.Set(new PositionComponent { position = positionCenter, tilePosition = tilePositionGlobal });
        //entityResource.Set(new IdGenericComponent(idResource, EntityType.RECURSO));
        //entityResource.Set(new TileSpriteComponent(idTileSpriteData));

        //switch (dataTileSprite.tileSpriteType)
        //{
        //    case TileSpriteType.Static:
        //        if (dataTileSprite.spriteData.listCollisionBody != null && dataTileSprite.spriteData.listCollisionBody.Length > 0)
        //        {
        //            var colliderBody = dataTileSprite.spriteData.listCollisionBody[0];
        //            //if (colliderBody is Polygon)
        //            //{
        //            //    var temp = (Polygon)colliderBody;
        //            //    temp.UpdatePosition(positionCenter);
        //            //}
        //            Godot.Vector2 pos = positionCenter - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
        //            var rectangle = new Rect2(pos, colliderBody.GetSizeQuad());
        //            int idCollider = CollisionManager.Instance.ResourceSourceCollidersFlecs.AddColliderObject(entityResource, dataTileSprite.spriteData.listCollisionBody.ToList(), positionCenter,0, colliderBody);
        //            entityResource.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent, new Rect2(), Vector2.Zero,0));
        //            CollisionShapeDraw.Instance.DrawCollisionShapes(colliderBody, positionCenter);
        //        }
        //        break;
        //    case TileSpriteType.Animated:
        //        if (dataTileSprite.animationData.collisionBodyArray != null && dataTileSprite.animationData.collisionBodyArray.Count > 0)
        //        {
        //            var colliderBody = dataTileSprite.animationData.collisionBodyArray[0];
        //            Godot.Vector2 pos = positionCenter - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
        //            var rectangle = new Rect2(pos, colliderBody.GetSizeQuad());
        //            int idCollider = CollisionManager.Instance.ResourceSourceCollidersFlecs.AddColliderObject(entityResource, colliderBody, positionCenter,0, colliderBody);
        //            entityResource.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent, new Rect2(), Vector2.Zero, 0));
        //            CollisionShapeDraw.Instance.DrawCollisionShapes(colliderBody, positionCenter);
        //        }
        //        break;
        //    default:
        //        break;
        //}
       
       
        

        //EntityChunkMap.Instance.AddEntityToChunk(entityResource, tilePositionGlobal, dataTileSprite.tilesOcupancy, layer);
    }

  

    public void RemoveTile(Vector2I tilePositionGlobal)
    {
        var entity= EntityChunkMap.Instance.GetEntityInChunk(tilePositionGlobal, EntityType.RECURSO, layer);
        if (entity!=Entity.Null())
        {
            EntityChunkMap.Instance.RemoveEntityFromChunk(entity, tilePositionGlobal);
        }
        
    }
}

