using Godot;
using GodotEcsArch.sources.managers.Serializer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap;
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
