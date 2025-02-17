using Godot;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.utils
{
    public class TilesHelper
    {
        public static Vector2I GetNeighborPosition(Vector2I tilePos, NeighborDirection direction)
        {
            switch (direction)
            {
                case NeighborDirection.Up: return new Vector2I(tilePos.X, tilePos.Y + 1);
                case NeighborDirection.Right: return new Vector2I(tilePos.X + 1, tilePos.Y);
                case NeighborDirection.Down: return new Vector2I(tilePos.X, tilePos.Y - 1);
                case NeighborDirection.Left: return new Vector2I(tilePos.X - 1, tilePos.Y);
                case NeighborDirection.UpRight: return new Vector2I(tilePos.X + 1, tilePos.Y + 1);
                case NeighborDirection.DownRight: return new Vector2I(tilePos.X + 1, tilePos.Y - 1);                                
                case NeighborDirection.DownLeft: return new Vector2I(tilePos.X - 1, tilePos.Y - 1);
                case NeighborDirection.UpLeft: return new Vector2I(tilePos.X - 1, tilePos.Y + 1);
                default: return tilePos;
            }
        }
    }
}
