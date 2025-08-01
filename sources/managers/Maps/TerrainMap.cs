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
public class TerrainMap
{    
    [ProtoIgnore, JsonIgnore]
    private Vector2I chunkDimencion;


    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainBasic; // representaciones Basicas no renderiza

    // agua - fondo -shader N1
    // piso y borde agua, caminos P1
    // elevacion P2
    // ornamentos P3
    [ProtoIgnore, JsonIgnore]
    private LayerChunksMaps<TerrainDataGame> mapLayerReal;


    // estos se usaran solo para el diseño   

    [ProtoIgnore, JsonIgnore]
    private LayerChunksMaps<TerrainDataGame> mapLayerDesign;

    [ProtoIgnore, JsonIgnore]
    private string carpet = "Terrain";

    
    [ProtoMember(1)]
    public string pathMapParent { get; set; }
    [ProtoMember(2)]
    public int layer { get; set; }    
    [ProtoMember(3)]
    public string pathCurrentCarpet { get; set; }
    [ProtoMember(4)]
    public List<int> materialsUsed { get; set; } // esto con el fin de no demorar en cargar cada material
    public SpriteMapChunk<TerrainDataGame> MapTerrainBasic { get => mapTerrainBasic; set => mapTerrainBasic = value; }
    public LayerChunksMaps<TerrainDataGame> MapLayerDesign { get => mapLayerDesign; set => mapLayerDesign = value; }
    public LayerChunksMaps<TerrainDataGame> MapLayerReal { get => mapLayerReal; set => mapLayerReal = value; }

    public TerrainMap(string pathMapParent, int Layer, bool SerializeOnUnload=false)
    {
        materialsUsed =new List<int>();        
        layer = Layer;        
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        

        mapLayerDesign = new LayerChunksMaps<TerrainDataGame>();

        int layerDesign = layer;
        mapTerrainBasic = new SpriteMapChunk<TerrainDataGame>("Basico", layerDesign - 2, ChunkManager.Instance.tiles16X16, SerializeOnUnload,false);
        mapLayerDesign.AddLayer(TerrainType.Agua.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainType.Agua.ToString(), layerDesign - 1, ChunkManager.Instance.tiles16X16, SerializeOnUnload));
        mapLayerDesign.AddLayer(TerrainType.PisoBase.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainType.PisoBase.ToString(), layerDesign, ChunkManager.Instance.tiles16X16, SerializeOnUnload));        
        mapLayerDesign.AddLayer(TerrainType.CaminoPiso.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainType.CaminoPiso.ToString(), layerDesign + 1, ChunkManager.Instance.tiles16X16, SerializeOnUnload));        
        //mapLayerDesign.AddLayer(TerrainType.AguaBorde.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainType.AguaBorde.ToString(), layerDesign, ChunkManager.Instance.tiles16X16, SerializeOnUnload));        
        mapLayerDesign.AddLayer(TerrainType.Elevacion.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainType.Elevacion.ToString(), layerDesign + 1, ChunkManager.Instance.tiles16X16, SerializeOnUnload));
        mapLayerDesign.AddLayer(TerrainType.Ornamentos.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainType.Ornamentos.ToString(), layerDesign + 2, ChunkManager.Instance.tiles16X16, SerializeOnUnload));

        mapLayerDesign.AddAlias(TerrainType.CaminoPiso.ToString(), TerrainType.CaminoAgua.ToString());
        mapLayerDesign.AddAlias(TerrainType.PisoBase.ToString(), TerrainType.AguaBorde.ToString());
        mapLayerDesign.AddAlias(TerrainType.Elevacion.ToString(), TerrainType.ElevacionBase.ToString());


        mapLayerReal = new LayerChunksMaps<TerrainDataGame>();

        mapLayerReal.AddLayer(TerrainMapReal.Agua.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainMapReal.Agua.ToString(), layer, ChunkManager.Instance.tiles16X16, SerializeOnUnload));
        mapLayerReal.AddLayer(TerrainMapReal.Suelo.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainMapReal.Suelo.ToString(), layer + 1, ChunkManager.Instance.tiles16X16, SerializeOnUnload));
        mapLayerReal.AddLayer(TerrainMapReal.Elevacion.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainMapReal.Elevacion.ToString(), layer + 2, ChunkManager.Instance.tiles16X16, SerializeOnUnload));
        mapLayerReal.AddLayer(TerrainMapReal.Ornamentos.ToString(), new SpriteMapChunk<TerrainDataGame>(TerrainMapReal.Ornamentos.ToString(), layer + 3, ChunkManager.Instance.tiles16X16, SerializeOnUnload));




        
      //  mapTerrainCompleteLevel0.OnChunSerialize += TilemapTerrain_OnChunSerialize;


        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet =  pathMapParent + "/" + carpet;
        //AddEmptyId(1);
    }
    public static TerrainMap LoadMapfromFile(string Name)
    {
        string pathCarpet = FileHelper.GetPathGameDB(CommonAtributes.pathMaps);
        string fullPath = pathCarpet + "/" + Name+".json";
        
        TerrainMap dataInfo = SerializerManager.LoadFromFileJson<TerrainMap>(fullPath);
        TerrainMap newMap = new TerrainMap(Name, dataInfo.layer, false);                

        //newMap.mapTerrainCompleteLevel0.LoadMaterials(dataInfo.materialsUsed);
        newMap.LoadMapData();
        return newMap;
    }

    private void TilemapTerrain_OnChunSerialize(Vector2 arg1, ChunkData<TerrainDataGame> arg2)
    {
        string name = arg1.X + "_" + arg1.Y;     
        Serializer.Data.ChunkDataSerializable<TerrainDataGame> dataSer = arg2.ToSerializable();                
        SerializerManager.SaveToFileBin(dataSer, pathCurrentCarpet, name);      
    }

    public void SaveAllMap()
    {
   
        //SerializerManager.SaveToFileJson(this, pathMapParent, "DataTerrain");

        //foreach (var item in mapTerrainCompleteLevel0.dataChunks)
        //{
        //    if (item.Value.changue)
        //    {
        //        TilemapTerrain_OnChunSerialize(item.Key, item.Value);
        //    }            
        //}
    }
    public void LoadMapData()
    {
       
        //List<string> listFiles = FileHelper.GetAllFiles(pathCurrentCarpet);
        //foreach (var item in listFiles)
        //{
        //    string fullPath = item;
        //    Serializer.Data.ChunkDataSerializable<TerrainDataGame> data = SerializerManager.LoadFromFileBin<Serializer.Data.ChunkDataSerializable<TerrainDataGame>>(fullPath);
        //    tilemapTerrain.CreateChunkData(data);
        //}

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
