using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Resources;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GodotEcsArch.sources.managers.Maps;

[ProtoContract]
public class ResourceDataGame : IDataSprite
{
    public int idUnique { get ; set ; }
    [ProtoMember(1)] public int idData { get ; set ; }
    [ProtoMember(2)] public bool isAnimation { get ; set ; }
    [ProtoMember(3)] public int amount { get; set; }

    [ProtoIgnore, JsonIgnore] public Vector3 positionWorld { get ; set ; }
    [ProtoIgnore, JsonIgnore] public Vector2 positionCollider { get ; set ; }
    [ProtoIgnore, JsonIgnore] public Vector2I positionTileWorld { get ; set ; }
    [ProtoIgnore, JsonIgnore] public Vector2I positionTileChunk { get ; set ; }

    // Campos auxiliares solo para serialización
    [ProtoMember(4)]
    public ProtoVector3 positionWorldSerialized
    {
        get => positionWorld;
        set => positionWorld = value;
    }

    [ProtoMember(5)]
    public ProtoVector2 positionColliderSerialized
    {
        get => positionCollider;
        set => positionCollider = value;
    }
    [ProtoMember(6)]
    public ProtoVector2I positionTileWorldSerialized
    {
        get => positionTileWorld;
        set => positionTileWorld = value;
    }
    [ProtoMember(7)]
    public ProtoVector2I positionTileChunkSerialized
    {
        get => positionTileChunk;
        set => positionTileChunk = value;
    }
    public AnimationStateData GetAnimationStateData()
    {
        return ResourcesGameManager.Instance.GetData(idData).animationData[0];
    }

    public SpriteData GetSpriteData()
    {
        return ResourcesGameManager.Instance.GetData(idData).spriteData;
    }
}

public class MapResources
{
    [ProtoIgnore, JsonIgnore]
    private Vector2I chunkDimencion;
    [ProtoIgnore, JsonIgnore]
    private SpriteMapChunk<ResourceDataGame> chunkMap;

    [ProtoIgnore, JsonIgnore]
    private string carpet = "Resources";



    [ProtoMember(1)]
    public string pathMapParent { get; set; }
    [ProtoMember(2)]
    public int layer { get; set; }

    [ProtoMember(3)]
    public string pathCurrentCarpet { get; set; }
    [ProtoMember(4)]
    public List<int> materialsUsed { get; set; } // esto con el fin de no demorar en cargar cada material

    public MapResources(string pathMapParent, int Layer, bool SerializeOnUnload = false)
    {
        materialsUsed = new List<int>();

        layer = Layer;
        chunkDimencion = PositionsManager.Instance.chunkDimencion;
        chunkMap = new SpriteMapChunk<ResourceDataGame>(layer, ChunkManager.Instance.tiles16X16, SerializeOnUnload);
        chunkMap.OnChunSerialize += ChunkMap_OnChunSerialize;
        this.pathMapParent = pathMapParent;
        this.pathCurrentCarpet = pathMapParent + "/" + carpet;
    }

    private void ChunkMap_OnChunSerialize(Vector2 arg1, ChunkData<ResourceDataGame> arg2)
    {
        string name = arg1.X + "_" + arg1.Y;       
        Serializer.Data.ChunkDataSerializable<ResourceDataGame> dataSer = arg2.ToSerializable();        
        SerializerManager.SaveToFileBin(dataSer, pathCurrentCarpet, name);
    }
    public void SaveAllMap()
    {
        SerializerManager.SaveToFileJson(this, pathMapParent, "DataResources");
        foreach (var item in chunkMap.dataChunks)
        {
            if (item.Value.changue)
            {
                ChunkMap_OnChunSerialize(item.Key, item.Value);
            }
        }
    }
    public void AddSprite(Vector2I positionGlobal, int idResources)
    {
        ResourceData resourceData = ResourcesGameManager.Instance.GetData(idResources);
        chunkMap.AddUpdatedTile(positionGlobal, resourceData.spriteData,idResources, resourceData.isAnimated);
    }
}
