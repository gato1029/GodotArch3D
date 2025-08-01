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
public class ResourceDataGame : DataItem
{
    [ProtoMember(6)]
    public int amount { get; set; }
    public override AnimationStateData GetAnimationStateData()
    {
        return ResourcesGameManager.Instance.GetData(idData).animationData[0];
    }

    public override SpriteData GetSpriteData()
    {
        return ResourcesGameManager.Instance.GetData(idData).spriteData;
    }

    public override bool IsAnimation()
    {
        return ResourcesGameManager.Instance.GetData(idData).isAnimated;
    }

    public override void SetDataGame()
    {
        var data = ResourcesGameManager.Instance.GetData(idData);
        amount = data.amount;
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
        chunkMap = new SpriteMapChunk<ResourceDataGame>("Recursos",layer, ChunkManager.Instance.tiles16X16, SerializeOnUnload);
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
        chunkMap.AddUpdatedTile(positionGlobal, idResources);
    }
}
