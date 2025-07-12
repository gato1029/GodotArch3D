using Arch.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
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


public interface IDataTile
{
    public int IdTile { get; set; }
    public Vector3 PositionWorld { get; set; }
    public float Scale { get; set; }
    public int IdCollider { get; set; }
    public Vector2 PositionCollider { get; set; }
    public Vector2I PositionTileWorld { get; set; }
    public Vector2I PositionTileChunk { get; set; }
}

[ProtoContract]
public class TerrainDataGame:IDataTile
{
    [ProtoMember(1)]
    // Se debe guardar
    public int idTerrain;
    [ProtoMember(2)]
    public int IdTile { get; set; }
    [ProtoMember(3)]
    public float Scale { get; set; }
    [ProtoMember(4)]
    public int IdCollider { get; set; }

    [ProtoIgnore, JsonIgnore]
    public Vector3 PositionWorld { get; set; }
    [ProtoIgnore, JsonIgnore]
    public Vector2 PositionCollider { get; set; }
    [ProtoIgnore, JsonIgnore]
    public Vector2I PositionTileWorld { get; set; }
    [ProtoIgnore, JsonIgnore]
    public Vector2I PositionTileChunk { get; set; }
    // Campos auxiliares solo para serialización
    [ProtoMember(5)]
    public ProtoVector3 PositionWorldSerialized
    {
        get => PositionWorld;
        set => PositionWorld = value;
    }

    [ProtoMember(6)]
    public ProtoVector2 PositionColliderSerialized
    {
        get => PositionCollider;
        set => PositionCollider = value;
    }
    [ProtoMember(7)]
    public ProtoVector2I PositionTileWorldSerialized
    {
        get => PositionTileWorld;
        set => PositionTileWorld = value;
    }
    [ProtoMember(8)]
    public ProtoVector2I PositionTileChunkSerialized
    {
        get => PositionTileChunk;
        set => PositionTileChunk = value;
    }
}

[ProtoContract]
public class TerrainMap
{
    [ProtoIgnore, JsonIgnore]
    private Dictionary<int, TerrainData> terrainDictionary;
    [ProtoIgnore, JsonIgnore]
    private Vector2I chunkDimencion;
    [ProtoIgnore, JsonIgnore]
    private TileMapChunkRender<TerrainDataGame> tilemapTerrain;


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

    public TerrainMap(string pathMapParent, int Layer, bool SerializeOnUnload=false)
    {
        materialsUsed =new List<int>();
        terrainDictionary = new Dictionary<int, TerrainData>();
        layer = Layer;        
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        tilemapTerrain = new TileMapChunkRender<TerrainDataGame>(layer,ChunkManager.Instance.tiles16X16, SerializeOnUnload);
        tilemapTerrain.OnChunSerialize += TilemapTerrain_OnChunSerialize;
        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet =  pathMapParent + "/" + carpet;
    }
    public static TerrainMap LoadMapfromFile(string Name)
    {
        string pathCarpet = FileHelper.GetPathGameDB(CommonAtributes.pathMaps);
        string fullPath = pathCarpet + "/" + Name+".json";
        
        TerrainMap dataInfo = SerializerManager.LoadFromFileJson<TerrainMap>(fullPath);
        TerrainMap newMap = new TerrainMap(Name, dataInfo.layer, false);                

        newMap.tilemapTerrain.LoadMaterials(dataInfo.materialsUsed);
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

        foreach (var item in tilemapTerrain.dataChunks)
        {
            if (item.Value.changue)
            {
                TilemapTerrain_OnChunSerialize(item.Key, item.Value);
            }            
        }
    }
    public void LoadMapData()
    {
       
        List<string> listFiles = FileHelper.GetAllFiles(pathCurrentCarpet);
        foreach (var item in listFiles)
        {
            string fullPath = item;
            Serializer.Data.ChunkDataSerializable<TerrainDataGame> data = SerializerManager.LoadFromFileBin<Serializer.Data.ChunkDataSerializable<TerrainDataGame>>(fullPath);
            tilemapTerrain.CreateChunkData(data);
        }

    }

    public void AddUpdateTile(Vector2I tilePositionGlobal, int idTerrain)
    {
        TerrainData terrainData;
        if (!terrainDictionary.ContainsKey(idTerrain))
        {
            terrainData = DataBaseManager.Instance.FindById<TerrainData>(idTerrain);
            terrainDictionary.Add(idTerrain, terrainData);  
        }
        terrainData = terrainDictionary[idTerrain];
        TerrainDataGame terrainDataGame = new TerrainDataGame();
        terrainDataGame.idTerrain = idTerrain;
        if (terrainData.isRule)
        {
            tilemapTerrain.AddUpdatedTileRule(tilePositionGlobal, terrainData.autoTileData, terrainDataGame);
            AddMaterialInternal(tilemapTerrain.IdMaterialLastUsed);
        }
        else
        {                 
            tilemapTerrain.AddUpdatedTile(tilePositionGlobal,  terrainData.tileData, terrainDataGame);
            AddMaterialInternal(tilemapTerrain.IdMaterialLastUsed);
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
