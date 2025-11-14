
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GodotEcsArch.sources.managers.Maps;


public class ResourceSourceMap
{
    
    private Vector2I chunkDimencion;
    private EntityChunkRender entityChunkRender { get; set; }

    private string carpet = "ResourceSource";

    private string name = "ResourceSourceData";

    public string pathMapParent { get; set; }

    public int layer { get; set; }

    public string pathCurrentCarpet { get; set; }


    public ResourceSourceMap(string pathMapParent, int Layer)
    {      
        layer = 20;
        entityChunkRender = new EntityChunkRender(ChunkManager.Instance.tiles16X16);
    }

  
    public void AddUpdateResource(Vector2I tilePositionGlobal,long idResource)
    {
        bool isPosible =EntityChunkMap.Instance.IsPosibleAddEntity(tilePositionGlobal, layer);
        if (!isPosible)
        {
            return;
        }

        var data = MasterDataManager.GetData<ResourceSourceData>(idResource);
        int randomIndex = GD.RandRange(0, data.listIdTileSpriteData.Count - 1);

        long idTileSpriteData = data.listIdTileSpriteData[randomIndex];

        var dataTileSprite = MasterDataManager.GetData<TileSpriteData>(idTileSpriteData);

        Vector2I tileSize =ChunkManager.Instance.tiles16X16.chunkDimencion;
        float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;


        Vector2 positionNormalize = tilePositionGlobal * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
        Vector2 positionCenter = positionNormalize + new Vector2(x, y);

        var colliderBody = dataTileSprite.spriteData.collisionBody;

        Godot.Vector2 pos = positionCenter - (colliderBody.GetSizeQuad() / 2) + colliderBody.OriginCurrent;
        var rectangle = new Rect2(pos, colliderBody.GetSizeQuad());

        Entity entityResource = FlecsManager.Instance.WorldFlecs.Entity();
        int idCollider = CollisionManager.Instance.BuildingsCollidersFlecs.AddColliderObject(entityResource, colliderBody, positionCenter, colliderBody);

        entityResource.Set(new PositionComponent { position = positionCenter, tilePosition = tilePositionGlobal });
        entityResource.Set(new IdGenericComponent(idResource, EntityType.RECURSO));
        entityResource.Set(new TileSpriteComponent(idTileSpriteData));        
        entityResource.Set(new ColliderComponent(idCollider, rectangle, colliderBody.OriginCurrent, new Rect2(), Vector2.Zero));

        EntityChunkMap.Instance.AddEntityToChunk(entityResource, tilePositionGlobal, layer);
    }

    public void RemoveTile(Vector2I tilePositionGlobal)
    {
        var entity= EntityChunkMap.Instance.GetEntityInChunk(tilePositionGlobal, EntityType.RECURSO, layer);
        EntityChunkMap.Instance.RemoveEntityFromChunk(entity,tilePositionGlobal);          
    }
}

