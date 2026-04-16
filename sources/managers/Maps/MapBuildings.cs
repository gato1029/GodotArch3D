
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Creators;
using GodotEcsArch.sources.managers.Buildings;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Resources;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;

[ProtoContract]
public class BuildingDataGame : DataItem
{
    public Entity entity;
    private bool existEntity = false;
    public override void SetDataGame(DataRender render)
    {

        //if (GetSpriteData().haveCollider)
        //{
        //    if (idUnique != 0)
        //    {
        //        CollisionManager.Instance.BuildingsColliders.RemoveCollider(idUnique);
        //        idUnique = 0;
        //    }
        //    if (GetSpriteData().listCollisionBody != null)
        //    {
        //        idUnique = CollisionManager.Instance.BuildingsColliders.AddColliderObject(this, GetSpriteData().listCollisionBody.ToList(), positionCollider);

        //    }
        //}
        //else
        //{
        //    if (idUnique != 0)
        //    {
        //        CollisionManager.Instance.BuildingsColliders.RemoveCollider(idUnique);
        //        idUnique = 0;
        //    }
        //}
        if (!existEntity)
        {
            CreateEntity();
        }
    }
    private void CreateEntity()
    {
        existEntity = true;
        //entity = Flecs.Creators.BuildingCreator.Instance.Create(idDataTileSprite, positionReal, positionTileWorld);
    }
    public override void ClearDataGame()
    {
        //BuildingCreator.Instance.Destroy(entity);                    
    }
}
public class MapBuildings
{
    private Vector2I chunkDimencion;

    private SpriteMapChunk<BuildingDataGame> mapData;

    private string carpet = "Buildings";

    private string name = "BuildingsData";


    public string pathMapParent { get; set; }

    public int layer { get; set; }

    public string pathCurrentCarpet { get; set; }
    public SpriteMapChunk<BuildingDataGame> MapData { get => mapData; set => mapData = value; }

    public MapBuildings(string pathMapParent, int Layer)
    {

        layer = 20;
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet = pathMapParent + "/" + carpet;

        mapData = new SpriteMapChunk<BuildingDataGame>("Buildings", pathCurrentCarpet, layer, ChunkManager.Instance.tiles16X16, false, true);
    }

    public void SaveAllMap()
    {
        mapData.SaveAll();

    }
    public void LoadMapData()
    {
        var pathFull = pathCurrentCarpet + "/" + name + ".json";
        mapData.SetRenderEnabledGlobal(false);
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
        mapData.SetRenderEnabledGlobal(enable);
    }
    public void AddUpdateTile(Vector2I tilePositionGlobal, int idData, int forceDataRuleNro = -1)
    {
       
        //mapData.AddUpdatedTile(tilePositionGlobal, idData);
    }

    public void RemoveTile(Vector2I tilePositionGlobal, int idData = 0)
    {
        //var data = BuildingManager.Instance.GetData(idData);
        //mapData.Remove(tilePositionGlobal);
    }
}
