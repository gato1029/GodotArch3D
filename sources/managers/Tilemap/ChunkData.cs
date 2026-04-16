using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Serializer.Data;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using MessagePack;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap;

[MessagePackObject]
public class DataRender
{
    [Key(0)]
    public long idDataTileSprite { get; set; }

    [Key(1)]
    public int idGroup { get; set; }

    [Key(2)]
    public bool render { get; set; }

    [IgnoreMember]
    public Vector2I positionTileWorld { get; set; }
    [IgnoreMember]
    public int  layer { get; set; } // no Guardar
    
    [IgnoreMember]
    public Vector3 positionWorld { get; set; } // no guardar
    [IgnoreMember]
    public Vector2 positionCollider { get; set; } // no guardar
    [IgnoreMember]
    public Vector2 positionReal { get; set; } // no guardar
    [IgnoreMember]
    public Vector2I positionTileChunk { get; set; } // no guardar
    public TileSpriteData GetSpriteData() { return MasterDataManager.GetData<TileSpriteData>(idDataTileSprite); } // no guardar
}
public class DataItem
{
    public int idCollider { get; set; }
    public virtual void ClearDataGame() { }
    public virtual void SetDataGame(DataRender render) { }
}
[MessagePackObject]
public class ChunkData<T>
{
    [IgnoreMember]
    public bool changue; // no guardar
    [IgnoreMember]
    public Vector2I size; // no guardar
    [Key(0)]
    public DataRender[,] renderTiles; //Guardar
    [IgnoreMember]
    public T[,] tiles; // no guardar

    public ChunkData() { }
    public ChunkData(Vector2 positionChunk, Vector2I size)
    {
        this.size = size;
        this.positionChunk = positionChunk;
        tiles = new T[size.X, size.Y];
        renderTiles = new DataRender[size.X, size.Y];
        changue = false;
    }
    [IgnoreMember]
    public Vector2 positionChunk { get; private set; }
    public void CreateUpdateTile(Vector2I position, DataRender dataRender, T data) 
    {
        changue = true;                
        renderTiles[position.X, position.Y] = dataRender;                
        tiles[position.X, position.Y] = data;
        
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
    public ( T data, DataRender render) GetTileAt(Vector2 localPos)
    {
        int x = (int)localPos.X;
        int y = (int)localPos.Y;

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return (tiles[x, y], renderTiles[x,y]);
        }

        return default; // Fuera de los límites
    }

    public (T data ,DataRender render) GetTileAt(int x, int y)
    {
        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return (tiles[x, y], renderTiles[x,y]);
        }

        return default; // Fuera de los límites
    }

    public void RemoveTile(Vector2I position)
    {
        changue = true;
        if (tiles[position.X, position.Y] != null)
        {
            tiles[position.X, position.Y] = default;
            renderTiles[position.X, position.Y] = null;
        }        
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
