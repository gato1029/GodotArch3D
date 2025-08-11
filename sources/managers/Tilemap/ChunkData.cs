using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap;

[ProtoContract]
[ProtoInclude(100, typeof(TerrainDataGame))] // <--- IMPORTANTE
public class DataItem
{
    [ProtoIgnore, JsonIgnore] public int idUnique { get; set; }
    [ProtoMember(1)] public int idData { get; set; }
    [ProtoIgnore, JsonIgnore] public Vector3 positionWorld { get; set; }
    [ProtoIgnore, JsonIgnore] public Vector2 positionCollider { get; set; }
    [ProtoIgnore, JsonIgnore] public Vector2I positionTileWorld { get; set; }
    [ProtoIgnore, JsonIgnore] public Vector2I positionTileChunk { get; set; }
    public virtual void SetDataGame() { }
    public virtual void ClearDataGame() { }
    public virtual SpriteData GetSpriteData() { return null; }
    public virtual bool IsAnimation() { return false; }
    public virtual AnimationStateData GetAnimationStateData() { return null; }
    public virtual int GetTypeData() { return 0; }

    // Campos auxiliares solo para serialización
    [ProtoMember(2)]
    public ProtoVector3 positionWorldSerialized
    {
        get => positionWorld;
        set => positionWorld = value;
    }

    [ProtoMember(3)]
    public ProtoVector2 positionColliderSerialized
    {
        get => positionCollider;
        set => positionCollider = value;
    }
    [ProtoMember(4)]
    public ProtoVector2I positionTileWorldSerialized
    {
        get => positionTileWorld;
        set => positionTileWorld = value;
    }
    [ProtoMember(5)]
    public ProtoVector2I positionTileChunkSerialized
    {
        get => positionTileChunk;
        set => positionTileChunk = value;
    }
}
public class ChunkData<T>
{
    public Vector2 positionChunk { get; private set; }
    public T[,] tiles;
    public Vector2I size;
    public bool changue;
    public ChunkData(Vector2 positionChunk, Vector2I size)
    {
        this.size = size;
        this.positionChunk = positionChunk;
        tiles = new T[size.X, size.Y];
        changue = false; 
    }
    public bool ExistTile(Vector2I position)
    {
        if (tiles[position.X, position.Y] == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public void CreateUpdateTile(Vector2I position, T data)
    {
        changue = true;
        if (tiles[position.X, position.Y] == null)
        {
            tiles[position.X, position.Y] = data;
        }
        else
        {
            tiles[position.X, position.Y]= data;
        }

    }

    public void RemoveTile(Vector2I position)
    {
        changue = true;
        if (tiles[position.X, position.Y] != null)
        {
            tiles[position.X, position.Y] = default;
        }        
    }

    public T GetTileAt(Vector2 localPos)
    {
        int x = (int)localPos.X;
        int y = (int)localPos.Y;

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y];
        }

        return default; // Fuera de los límites
    }
    public T GetTileAt(int x, int y)
    {
        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y];
        }

        return default; // Fuera de los límites
    }
    public ChunkDataSerializable<T> ToSerializable()
    {
        var serializable = new ChunkDataSerializable<T>
        {
            PositionChunk = positionChunk,
            Size = size,
            Tiles = new List<TileEntry<T>>()
        };

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                var tile = tiles[x, y];
                if (tile != null)
                {
                    serializable.Tiles.Add(new TileEntry<T>
                    {
                        Position = new Vector2I(x, y),
                        Value = tile
                    });
                }
            }
        }
        changue = false;
        return serializable;
    }
}
