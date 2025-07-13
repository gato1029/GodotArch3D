using Godot;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.SpriteMapChunk;


public class ChunkRenderGPU
{
    public Vector2 positionLocal { get; private set; }
    public SpriteRender[,] tiles;
    public Vector2 size;
    public ChunkRenderGPU(Vector2 positionChunk, Vector2I size)
    {
        this.size = size;
        this.positionLocal = positionChunk;
        tiles = new SpriteRender[size.X, size.Y];
    }

    public void CreateUpdate(int x, int y, Rid rid, int instance, int TextureBatchPosition, Vector3 worldPosition, SpriteData dataBase)
    {
        if (tiles[x, y] == null)
        {
            tiles[x, y] = new SpriteRender();
        }
        tiles[x, y].UpdateTile(rid, instance, TextureBatchPosition, worldPosition, dataBase);      
    }
    public void CreateUpdate(int x, int y, Rid rid, int instance, int TextureBatchPosition, Vector3 worldPosition, SpriteData dataBase, AnimationStateData dataAnimation)
    {
        if (tiles[x, y] == null)
        {
            tiles[x, y] = new SpriteRender();
        }
        tiles[x, y].UpdateTile(rid, instance, TextureBatchPosition,worldPosition, dataBase, dataAnimation);
    }
    public void CreateTile(Vector2I position, SpriteRender renderData)
    {
        if (tiles[position.X, position.Y] == null)
        {
            tiles[position.X, position.Y] = renderData;
        }
    }

    public bool ExistTile(Vector2 locaPos)
    {
        if (GetTileAt(locaPos) != null)
        {
            return true;
        }
        return false;
    }
    public SpriteRender GetTileAt(Vector2 localPos)
    {
        int x = (int)localPos.X;
        int y = (int)localPos.Y;

        if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
        {
            return tiles[x, y];
        }

        return default; // Fuera de los límites
    }
}
