using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;
[ProtoContract]
public class MapLevelData
{
    public string name { get; set; }
    public string description { get; set; }

    [ProtoIgnore, JsonIgnore]
    public Vector2I size { get; set; }
    
    [ProtoIgnore, JsonIgnore]
    TerrainMap terrainMap;
    // ResourcesMap resourcesMap;
    // buildsMap buildsMap;

    [ProtoMember(0)]
    public ProtoVector2I sizeSerialized
    {
        get => size;
        set => size = value;
    }

    public void LoadMapsResources()
    {
        
    }
}
