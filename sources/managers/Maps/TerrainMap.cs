using Arch.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SpriteMultimesh;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Maps;

public enum TerrainMapReal
{
    Agua = 0,
    Suelo = 1,
    Elevacion = 2,
    Ornamentos = 3
}
public enum TerrainMapLevelDesign
{
    Basico = -1,
    Completo = 0,
    Piso = 1,
    Camino = 2,
    Agua = 3,
    Ornamentos = 4,
}

[ProtoContract]
public class TerrainDataGame:DataItem
{
    public override void SetDataGame()
    {
        if (GetSpriteData().haveCollider)
        {
            if (idUnique!=0)
            {
                CollisionManager.Instance.terrainColliders.RemoveCollider(idUnique);
                idUnique = 0;
            }            
            if (GetSpriteData().listCollisionBody != null)
            {
                foreach (var item in GetSpriteData().listCollisionBody)
                {
                    var posCollider = positionCollider + item.MultiplicityInternal(GetSpriteData().scale).OriginCurrent;
                    idUnique = CollisionManager.Instance.terrainColliders.AddShapeToObject(this, item, posCollider);
                }
            }
        }
        else
        {
            if (idUnique != 0)
            {
                CollisionManager.Instance.terrainColliders.RemoveCollider(idUnique);
                idUnique = 0;
            }
        }
    }
    public override void ClearDataGame()
    {
        if (idUnique != 0)
        {
            CollisionManager.Instance.terrainColliders.RemoveCollider(idUnique);
            idUnique = 0;
        }
    }
    public override SpriteData GetSpriteData()
    {
        return TerrainManager.Instance.GetData(idData).spriteData;
    }

    public override bool IsAnimation()
    {
        return TerrainManager.Instance.GetData(idData).isAnimated;
    }

    public override AnimationStateData GetAnimationStateData()
    {
        return TerrainManager.Instance.GetData(idData).animationData[0];
    }

    public override int GetTypeData()
    {
        return (int)TerrainManager.Instance.GetData(idData).terrainType;
    }
}


[ProtoContract]
public class TerrainMapInfo
{
    public int Seed {  get; set; }
}
[ProtoContract]
public class TerrainMap
{    
    [ProtoIgnore, JsonIgnore]
    private Vector2I chunkDimencion;


    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainBasic; // representaciones Basicas no renderiza
    
    [ProtoIgnore, JsonIgnore]
    private LayerChunksMaps<TerrainDataGame> mapLayerDesign;

    [ProtoIgnore, JsonIgnore]
    private string carpet = "Terrain";

    [ProtoIgnore, JsonIgnore]
    private string name = "TerrainData";

  
    public string pathMapParent { get; set; }
   
    public int layer { get; set; }    
   
    public string pathCurrentCarpet { get; set; }

    public int seed { get; set; }

    public List<int> materialsUsed { get; set; } // esto con el fin de no demorar en cargar cada material
    public SpriteMapChunk<TerrainDataGame> MapTerrainBasic { get => mapTerrainBasic; set => mapTerrainBasic = value; }
    public LayerChunksMaps<TerrainDataGame> MapLayerDesign { get => mapLayerDesign; set => mapLayerDesign = value; } 
    public TerrainMap(string pathMapParent, int Layer)
    {
        materialsUsed =new List<int>();        
        layer = Layer;        
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet = pathMapParent + "/" + carpet;

        mapLayerDesign = new LayerChunksMaps<TerrainDataGame>(pathCurrentCarpet);

        int layerDesign = layer;
        mapTerrainBasic = new SpriteMapChunk<TerrainDataGame>("Basico",pathCurrentCarpet, 10, ChunkManager.Instance.tiles16X16, false,false);
        mapLayerDesign.AddLayer(TerrainType.Agua.ToString(),14);
        mapLayerDesign.AddLayer(TerrainType.PisoBase.ToString(), 15);        
        mapLayerDesign.AddLayer(TerrainType.CaminoPiso.ToString(), 16);                
        mapLayerDesign.AddLayer(TerrainType.Elevacion.ToString(),  19);
        mapLayerDesign.AddLayer(TerrainType.Ornamentos.ToString(), 20);

        mapLayerDesign.AddAlias(TerrainType.CaminoPiso.ToString(), TerrainType.CaminoAgua.ToString());
        mapLayerDesign.AddAlias(TerrainType.PisoBase.ToString(), TerrainType.AguaBorde.ToString());
        mapLayerDesign.AddAlias(TerrainType.Elevacion.ToString(), TerrainType.ElevacionBase.ToString());                
    }

    public void SaveAllMap()
    {
        SerializerManager.SaveToFileJson(new TerrainMapInfo { Seed = this.seed }, pathCurrentCarpet, name);
        mapTerrainBasic.SaveAll();
        mapLayerDesign.SaveAll();
    }
    public void LoadMapData()
    {
        var pathFull = pathCurrentCarpet +"/"+ name+".json";
        var dataInfo = SerializerManager.LoadFromFileJson<TerrainMapInfo>(pathFull);
        this.seed = dataInfo.Seed;
        mapTerrainBasic.SetRenderEnabled(false);
        mapLayerDesign.SetRenderEnableAllLayers(false);
        mapTerrainBasic.LoadAll();
        mapLayerDesign.LoadAll();        
        mapLayerDesign.SetRenderEnableAllLayers(true);
    }
    public void ClearFilesChunks()
    {
        MapTerrainBasic.ClearAllFiles();
        mapLayerDesign.ClearAllFiles();
    }
    public void ClearMap()
    {        
        mapTerrainBasic.ClearAllChunks();
        mapLayerDesign.ClearAll();        
    }
    public void EnableLayer(bool enable, TerrainType terrainType)
    {
        mapLayerDesign.GetLayer(terrainType.ToString()).SetRenderEnabled(enable);        
    }
    public void EnableLayerInternal(bool enable)
    {
        mapTerrainBasic.SetRenderEnabled(enable);        
    }
    public void AddUpdateTile(Vector2I tilePositionGlobal, int idTerrain, int forceDataRuleNro =-1)
    {
        var data = TerrainManager.Instance.GetData(idTerrain);

        if (data.isRule && forceDataRuleNro == -1)
        {
            mapLayerDesign.GetLayer(data.terrainType.ToString()).AddUpdatedTileRule(tilePositionGlobal, data.rules);            
        }
        else
        {
            if (forceDataRuleNro!=-1)
            {
                idTerrain = data.rules[forceDataRuleNro].idDataCentral;
            }
            mapLayerDesign.GetLayer(data.terrainType.ToString()).AddUpdatedTile(tilePositionGlobal, idTerrain);            
        }
        //mapLayerDesign.GetLayer(data.terrainType.ToString()).Refresh(tilePositionGlobal);        
    }

    public void RemoveTile(Vector2I tilePositionGlobal, int idTerrain)
    {
        var data = TerrainManager.Instance.GetData(idTerrain);
        if (data.isRule)
        {
            mapLayerDesign.GetLayer(data.terrainType.ToString()).Remove(tilePositionGlobal, data.rules);
        }
        else
        {
            mapLayerDesign.GetLayer(data.terrainType.ToString()).Remove(tilePositionGlobal);
        }
      //  mapLayerDesign.GetLayer(data.terrainType.ToString()).Refresh(tilePositionGlobal);
    }

    private void AddMaterialInternal(int idMaterial)
    {
        if (!materialsUsed.Contains(idMaterial) && idMaterial != 0)
        {
            materialsUsed.Add(idMaterial);
        }        
    }

}
