using Godot;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

namespace GodotEcsArch.sources.managers.Tilemap
{
    internal class ChunkRender
    {
        public Vector2 positionLocal { get; private set; }
        public Tile[,] tiles;
        public Vector2 size;
        public ChunkRender(Vector2 positionChunk, Vector2I size)
        {
            this.size = size;
            this.positionLocal = positionChunk;
            tiles = new Tile[size.X, size.Y];
        }

        public void CreateUpdate(int x, int y,  Rid rid, int instance, int positionBatchTexture, Vector3 worldPosition, float scale, int idTile)
        {
            if (tiles[x, y] == null)
            {
                tiles[x, y] = new Tile();
            }  
            tiles[x, y].UpdateTile( rid, instance, positionBatchTexture, worldPosition, scale, idTile);            
        }
      
        public void CreateTile(Vector2I position, Tile tile)
        {
            if (tiles[position.X, position.Y] == null)
            {
                tiles[position.X, position.Y] = tile;
            }
        }

        public bool ExistTile(Vector2 locaPos)
        {
            if (GetTileAt(locaPos)!=null)
            {
                return true;
            }
            return false;
        }
        public Tile GetTileAt(Vector2 localPos)
        {
            int x = (int)localPos.X;
            int y = (int)localPos.Y;

            if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
            {
                return tiles[x, y];
            }

            return null; // Fuera de los límites
        }
    }
}
