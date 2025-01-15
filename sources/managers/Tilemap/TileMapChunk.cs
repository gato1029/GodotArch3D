using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Tilemap
{
    internal class TileMapChunk
    {
        public Vector2 positionLocal { get; private set; }
        private Tile[,] tiles;

        public TileMapChunk(Vector2 positionChunk, int size)
        {
            this.positionLocal = positionChunk;
            tiles = new Tile[size, size];
        }

        public void CreateUpdateTile(Vector2I position, int id)
        {
            if (tiles[position.X, position.Y] == null)
            {
                tiles[position.X, position.Y] = new Tile(position, id);
            }
            else
            {
                tiles[position.X, position.Y].id = id;
            }
            
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
