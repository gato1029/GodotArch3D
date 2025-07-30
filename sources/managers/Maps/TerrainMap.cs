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

    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainCompleteLevel0; // piso y agua Efecto
    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainCompleteLevel1; // agua y elevacion
    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainCompleteOrnaments;


    // estos se usaran para el diseño
    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainFloor;
    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainPath;
    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainWater;
    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<TerrainDataGame> mapTerrainOrnaments;

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

    public TerrainMap(string pathMapParent, int Layer, bool SerializeOnUnload=false)
    {
        materialsUsed =new List<int>();        
        layer = Layer;        
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        mapTerrainBasic = new SpriteMapChunk<TerrainDataGame>(layer, ChunkManager.Instance.tiles16X16, SerializeOnUnload);

        mapTerrainCompleteLevel0 = new SpriteMapChunk<TerrainDataGame>(layer,ChunkManager.Instance.tiles16X16, SerializeOnUnload);

        mapTerrainFloor = new SpriteMapChunk<TerrainDataGame>(layer, ChunkManager.Instance.tiles16X16, SerializeOnUnload);
        mapTerrainPath = new SpriteMapChunk<TerrainDataGame>(layer+1, ChunkManager.Instance.tiles16X16, SerializeOnUnload);
        mapTerrainWater = new SpriteMapChunk<TerrainDataGame>(layer+1, ChunkManager.Instance.tiles16X16, SerializeOnUnload);
        mapTerrainOrnaments = new SpriteMapChunk<TerrainDataGame>(layer + 2, ChunkManager.Instance.tiles16X16, SerializeOnUnload);

        
        mapTerrainCompleteLevel0.OnChunSerialize += TilemapTerrain_OnChunSerialize;


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

        newMap.mapTerrainCompleteLevel0.LoadMaterials(dataInfo.materialsUsed);
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
   
        SerializerManager.SaveToFileJson(this, pathMapParent, "DataTerrain");

        foreach (var item in mapTerrainCompleteLevel0.dataChunks)
        {
            if (item.Value.changue)
            {
                TilemapTerrain_OnChunSerialize(item.Key, item.Value);
            }            
        }
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
    public void EnableLayer(bool enable, TerrainMapLevelDesign terrainMapLevelDesign)
    {
        switch (terrainMapLevelDesign)
        {
            case TerrainMapLevelDesign.Completo:
                mapTerrainCompleteLevel0.SetRenderEnabled(enable);
                break;
            case TerrainMapLevelDesign.Piso:
                mapTerrainFloor.SetRenderEnabled(enable);
                break;
            case TerrainMapLevelDesign.Camino:
                mapTerrainPath.SetRenderEnabled(enable);
                break;
            case TerrainMapLevelDesign.Agua:
                mapTerrainWater.SetRenderEnabled(enable);
                break;
            case TerrainMapLevelDesign.Ornamentos:
                mapTerrainOrnaments.SetRenderEnabled(enable);
                break;
            case TerrainMapLevelDesign.Basico:                
                mapTerrainBasic.SetRenderEnabled(enable);
                break;
            default:
                break;
        }
    }
    public void AddUpdateTile(Vector2I tilePositionGlobal, int idTerrain, TerrainMapLevelDesign terrainMapLevelDesign)
    {
        var data = TerrainManager.Instance.GetData(idTerrain);

        switch (terrainMapLevelDesign)
        {
            case TerrainMapLevelDesign.Completo:
                if (data.isRule)
                {

                    mapTerrainCompleteLevel0.AddUpdatedTileRule(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainCompleteLevel0.AddUpdatedTile(tilePositionGlobal, idTerrain);
                }
                mapTerrainCompleteLevel0.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Piso:
                if (data.isRule)
                {

                    mapTerrainFloor.AddUpdatedTileRule(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainFloor.AddUpdatedTile(tilePositionGlobal, idTerrain);
                }
                mapTerrainFloor.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Camino:
                if (data.isRule)
                {
                    mapTerrainPath.AddUpdatedTileRule(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainPath.AddUpdatedTile(tilePositionGlobal, idTerrain);
                }
                mapTerrainPath.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Agua:
                if (data.isRule)
                {
                    mapTerrainWater.AddUpdatedTileRule(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainWater.AddUpdatedTile(tilePositionGlobal, idTerrain);
                }
                mapTerrainWater.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Ornamentos:
                if (data.isRule)
                {
                    mapTerrainOrnaments.AddUpdatedTileRule(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainOrnaments.AddUpdatedTile(tilePositionGlobal, idTerrain);
                }
                mapTerrainOrnaments.Refresh(tilePositionGlobal);
                break;
            default:
                break;
        }
   

    }

    public void RemoveTile(Vector2I tilePositionGlobal, int idTerrain, TerrainMapLevelDesign terrainMapLevelDesign)
    {
        var data = TerrainManager.Instance.GetData(idTerrain);
        switch (terrainMapLevelDesign)
        {
            case TerrainMapLevelDesign.Completo:
                if (data.isRule)
                {

                    mapTerrainCompleteLevel0.Remove(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainCompleteLevel0.Remove(tilePositionGlobal);
                }
                mapTerrainCompleteLevel0.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Piso:
                if (data.isRule)
                {

                    mapTerrainFloor.Remove(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainFloor.Remove(tilePositionGlobal);
                }
                mapTerrainFloor.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Camino:
                if (data.isRule)
                {
                    mapTerrainPath.Remove(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainPath.Remove(tilePositionGlobal);
                }
                mapTerrainPath.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Agua:
                if (data.isRule)
                {
                    mapTerrainWater.Remove(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainWater.Remove(tilePositionGlobal);
                }
                mapTerrainWater.Refresh(tilePositionGlobal);
                break;
            case TerrainMapLevelDesign.Ornamentos:
                if (data.isRule)
                {
                    mapTerrainOrnaments.Remove(tilePositionGlobal, data.rules, (int)data.terrainType);
                }
                else
                {
                    mapTerrainOrnaments.Remove(tilePositionGlobal);
                }
                mapTerrainOrnaments.Refresh(tilePositionGlobal);
                break;
            default:
                break;
        }   
    }

    public void AddTileBasicConfig(Vector2I tilePositionGlobal, TerrainType terrainType)
    {
        switch (terrainType)
        {           
            case TerrainType.PisoBase:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 68);
                break;
            case TerrainType.Elevacion:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 70);
                break;
            case TerrainType.Agua:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 69);
                break;
            case TerrainType.CaminoPiso:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 72);
                break;
            case TerrainType.CaminoAgua:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 71);
                break;                            
            default:
                break;
        }
        
    }
    private void AddMaterialInternal(int idMaterial)
    {
        if (!materialsUsed.Contains(idMaterial) && idMaterial != 0)
        {
            materialsUsed.Add(idMaterial);
        }        
    }

}
