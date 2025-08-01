using Godot;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.WindowsDataBase.Generic;
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

        public static Vector2 WorldPositionTile(Vector2I positionTile)
        {
            Vector2 tileSize = new Vector2(16, 16);
            
            float x = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
            float y = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;
            Vector2 positionNormalize = positionTile * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
            Vector2 positionCenter = positionNormalize + new Vector2(x, y);
            return positionCenter;
        }

        public static RuleData FindBestMatchingRule(RuleData[] arrayRules, byte mask, int[] neighborTilesIds)
        {
            foreach (var rule in arrayRules)
            {
                // Verificamos si la regla requiere evaluar conexión (es decir, cualquier estado distinto a Any)
                bool needsNeighborInfo = rule.neighborConditions.Any(c => c.State != NeighborState.Any || c.SpecificTileId != 0);

                if (needsNeighborInfo)
                {
                    if (rule.Matches(mask, neighborTilesIds))
                        return rule;
                }
                else
                {
                    if (rule.Matches(mask))
                        return rule;
                }
            }

            return null;
        }
    }
}
