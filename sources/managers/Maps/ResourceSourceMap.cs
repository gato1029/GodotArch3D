using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Resources;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GodotEcsArch.sources.managers.Maps;

[ProtoContract]
public class ResourceSourceDataGame : DataItem
{
    public override void SetDataGame()
    {
        if (GetSpriteData().haveCollider)
        {
            if (idUnique != 0)
            {
                CollisionManager.Instance.ResourceSourceColliders.RemoveCollider(idUnique);
                idUnique = 0;
            }
            if (GetSpriteData().listCollisionBody != null)
            {
                foreach (var item in GetSpriteData().listCollisionBody)
                {
                    var posCollider = positionCollider + item.OriginCurrent;
                    idUnique = CollisionManager.Instance.ResourceSourceColliders.AddShapeToObject(this, item, posCollider);
                }
            }
        }
        else
        {
            if (idUnique != 0)
            {
                CollisionManager.Instance.ResourceSourceColliders.RemoveCollider(idUnique);
                idUnique = 0;
            }
        }
    }
    public override void ClearDataGame()
    {
        if (idUnique != 0)
        {
            CollisionManager.Instance.ResourceSourceColliders.RemoveCollider(idUnique);
            idUnique = 0;
        }
    }
    public override SpriteData GetSpriteData()
    {
        return ResourceSourceManager.Instance.GetData(idData).spriteData;
    }

    public override bool IsAnimation()
    {
        return ResourceSourceManager.Instance.GetData(idData).isAnimated;
    }

    public override AnimationStateData GetAnimationStateData()
    {
        return ResourceSourceManager.Instance.GetData(idData).animationData[0];
    }

    public override int GetTypeData()
    {
        return (int)ResourceSourceManager.Instance.GetData(idData).resourceSourceType;
    }
}

public class ResourceSourceMap
{
    
    private Vector2I chunkDimencion;

    private SpriteMapChunk<ResourceSourceDataGame> mapData; 

    private string carpet = "ResourceSource";

    private string name = "ResourceSourceData";


    public string pathMapParent { get; set; }

    public int layer { get; set; }

    public string pathCurrentCarpet { get; set; }
    public SpriteMapChunk<ResourceSourceDataGame> MapData { get => mapData; set => mapData = value; }

    public ResourceSourceMap(string pathMapParent, int Layer)
    {
      
        layer = 20;
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet = pathMapParent + "/" + carpet;   
        
        mapData = new SpriteMapChunk<ResourceSourceDataGame>("ResourceSource", pathCurrentCarpet, layer, ChunkManager.Instance.tiles16X16, false, true);   
    }

    public void SaveAllMap()
    {        
        mapData.SaveAll();
       
    }
    public void LoadMapData()
    {
        var pathFull = pathCurrentCarpet + "/" + name + ".json";
        mapData.SetRenderEnabled(false);
        mapData.LoadAll();
       
    }
    public void ClearFilesChunks()
    {
        mapData.ClearAllFiles();        
    }
    public void ClearMap()
    {
        mapData.ClearAllChunks();        
    }
    public void EnableLayer(bool enable)
    {
        mapData.SetRenderEnabled(enable);
    }
    public void AddUpdateTile(Vector2I tilePositionGlobal, int idData, int forceDataRuleNro = -1)
    {
        var data = ResourceSourceManager.Instance.GetData(idData);
        mapData.AddUpdatedTile(tilePositionGlobal, data.id);                
    }

    public void RemoveTile(Vector2I tilePositionGlobal, int idTerrain)
    {
        var data = ResourceSourceManager.Instance.GetData(idTerrain);
        mapData.Remove(tilePositionGlobal);                
    }
}

