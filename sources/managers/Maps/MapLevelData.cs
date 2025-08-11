using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;

public enum MapInfoType
{
    Terreno,
    FuenteRecursos
}
public enum MapType
{
    Mapa = 0,
    Habitacion = 1,
    Calabozo = 2
}
[ProtoContract]
public class MapDataMarker
{
    [ProtoMember(1)]
    public string path { get; set; }
    [ProtoMember(2)]
    public ProtoVector2 positionInSerialized
    {
        get => positionIn;
        set => positionIn = value;
    }
    [ProtoMember(3)]
    public ProtoVector2 positionOutSerialized
    {
        get => positionOut;
        set => positionOut = value;
    }
    [ProtoIgnore, JsonIgnore]
    public Vector2 positionIn { get; set; }
    [ProtoIgnore, JsonIgnore]
    public Vector2 positionOut { get; set; }
}

public class MapInfo
{
    public MapInfoType mapInfoType  { get; set; }
    public string InternalPath { get; set; }
}
[ProtoContract]
public class MapLevelData
{
    [ProtoMember(1)]
    public ProtoVector2I sizeSerialized
    {
        get => size;
        set => size = value;
    }    
    [ProtoMember(2)]
    public string name { get; set; }
    [ProtoMember(3)]
    public string pathCurrentCarpet { get; set; }
    [ProtoMember(4)]
    public string description { get; set; }
    [ProtoMember(5)]
    public bool unlimit { get; set; }
    [ProtoMember(6)]
    public MapType maptype { get; set; }

    [ProtoMember(7)]
    public int layer { get; set; }

    [ProtoMember(8)]
    public List<MapDataMarker> mapDataMarkers { get; set; }

    [ProtoMember(9)]
    public List<MapInfo> mapInfoDetail { get; set; }
        
    [ProtoIgnore, JsonIgnore]
    public Vector2I size { get; set; }

    // luego agregar tiles para miniatura y posiciones de entrada
    
    [ProtoIgnore, JsonIgnore]
    public TerrainMap terrainMap { get; set; }

    [ProtoIgnore, JsonIgnore]
    public ResourceSourceMap resourceSourceMap { get; set; }

    [ProtoIgnore, JsonIgnore]
    public MapResources mapResources { get; set; }
    // buildsMap buildsMap;

    public MapLevelData(string name, Vector2I size, MapType mapType, int layer, string description, bool unlimit =false)
    {
        this.name = name;
        this.size = size;
        this.unlimit = unlimit;
        this.maptype = mapType;        
        this.layer = layer;
        this.description = description;
        mapInfoDetail = new List<MapInfo>();
        string path = CommonAtributes.pathMaps+"/"+name;
        string pathCarpet = FileHelper.GetPathGameDB(path);
        pathCurrentCarpet = pathCarpet;
        
        terrainMap = new TerrainMap(pathCurrentCarpet + "/InternalData", layer);
        resourceSourceMap = new ResourceSourceMap(pathCurrentCarpet + "/InternalData", layer);
        //----
        mapInfoDetail.Add(new MapInfo { mapInfoType = MapInfoType.Terreno, InternalPath = terrainMap.pathCurrentCarpet});
        mapInfoDetail.Add(new MapInfo { mapInfoType = MapInfoType.FuenteRecursos, InternalPath = resourceSourceMap.pathCurrentCarpet });
        //mapResources = new MapResources(pathCurrentCarpet+"/InternalData", layer + 1);      


    }
    public void SaveAll()
    {
        SerializerManager.SaveToFileJson(this, pathCurrentCarpet, "Data");
        
        terrainMap.SaveAllMap();
        resourceSourceMap.SaveAllMap();
       // mapResources.SaveAllMap();
    }
    public void AddMap(Vector2 positionIn, string pathNewMap)
    {
        MapDataMarker mapDataMarker = new MapDataMarker();
        mapDataMarker.positionIn = positionIn;
        mapDataMarker.path = pathNewMap;
    }
    public static MapLevelData LoadMap(string pathData)
    {
        MapLevelData loadMap  = SerializerManager.LoadFromFileJson<MapLevelData>(pathData);
        loadMap.terrainMap.LoadMapData();
        loadMap.resourceSourceMap.LoadMapData();

        return loadMap;
    }

    public void ClearAll()
    {
        terrainMap.ClearMap();
        resourceSourceMap.ClearMap();
    }
}
