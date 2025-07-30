using LiteDB;
using System;
using System.Linq;

namespace GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase
{
    [Flags]
    public enum NeighborDirection
    {
        None = 0,
        Up = 1 << 0,
        Right = 1 << 1,
        Down = 1 << 2,
        Left = 1 << 3,
        UpRight = 1 << 4,
        DownRight = 1 << 5,
        DownLeft = 1 << 6,
        UpLeft = 1 << 7
    }

    public enum NeighborState
    {
        Any = -1,
        Empty = 0,
        Filled = 1
    }

    public class NeighborCondition
    {
        public NeighborState State { get; set; } = NeighborState.Any;
        public int SpecificTileId { get; set; }

        public bool Matches(bool isConnected, TileData neighborTile)
        {
            if (State == NeighborState.Any)
                return true;

            if (State == NeighborState.Filled && !isConnected)
                return false;

            if (State == NeighborState.Empty && isConnected)
                return false;

            if (SpecificTileId!=0)
            {
                if (neighborTile == null || neighborTile.id != SpecificTileId)
                    return false;
            }

            return true;
        }
        public bool Matches(bool isConnected, int neighborTileId)
        {
            if (State == NeighborState.Any)
                return true;

            if (State == NeighborState.Filled && !isConnected)
                return false;

            if (State == NeighborState.Empty && isConnected)
                return false;

            if (SpecificTileId != 0)
            {
                if (neighborTileId == 0 || neighborTileId != SpecificTileId)
                    return false;
            }

            return true;
        }
    }

    public class TileRuleData
    {
        public byte neighborMask { get; set; }

        public bool checkIsNull { get; set; }

        public int idTileDataCentral { get; set; }
        public int[] idsTileDataMask { get; set; }

        [BsonIgnore]
        public TileData tileDataCentral { get; set; }

        [BsonIgnore]
        public TileData[] tileDataMask { get; set; }

        public NeighborCondition[] neighborConditions { get; set; }

        public TileRuleData(byte neighborMask, TileData tileData)
        {
            this.neighborMask = neighborMask;
            this.tileDataCentral = tileData;
            tileDataMask = new TileData[8];
            idsTileDataMask = new int[8];
            neighborConditions = Enumerable.Range(0, 8).Select(_ => new NeighborCondition()).ToArray();
        }

        public TileRuleData()
        {
            tileDataMask = new TileData[8];
            idsTileDataMask = new int[8];
            neighborConditions = Enumerable.Range(0, 8).Select(_ => new NeighborCondition()).ToArray();
            neighborMask = 0;

            for (int i = 0; i < 8; i++)
            {
                neighborConditions[i] = new NeighborCondition
                {
                    State = NeighborState.Empty, // 👈 aquí se asigna
                    SpecificTileId = 0
                };
            }
        }

        public void UpdateNeighborMask(NeighborDirection direction, bool isConnected = false, TileData tileData = null)
        {
            int index = GetDirectionIndex(direction);

            if (isConnected)
            {
                neighborMask |= (byte)direction;
                tileDataMask[index] = tileData;
                idsTileDataMask[index] = tileData?.id ?? 0;
            }
            else
            {
                neighborMask &= (byte)~direction;
                tileDataMask[index] = null;
                idsTileDataMask[index] = 0;
            }
        }

        private int GetDirectionIndex(NeighborDirection direction)
        {
            return direction switch
            {
                NeighborDirection.Up => 0,
                NeighborDirection.Right => 1,
                NeighborDirection.Down => 2,
                NeighborDirection.Left => 3,
                NeighborDirection.UpRight => 4,
                NeighborDirection.DownRight => 5,
                NeighborDirection.DownLeft => 6,
                NeighborDirection.UpLeft => 7,
                _ => throw new ArgumentException("Dirección no válida."),
            };
        }

        public NeighborDirection GetDirectionFromIndex(int index)
        {
            return index switch
            {
                0 => NeighborDirection.Up,
                1 => NeighborDirection.Right,
                2 => NeighborDirection.Down,
                3 => NeighborDirection.Left,
                4 => NeighborDirection.UpRight,
                5 => NeighborDirection.DownRight,
                6 => NeighborDirection.DownLeft,
                7 => NeighborDirection.UpLeft,
                _ => throw new ArgumentException("Índice no válido. Debe estar entre 0 y 7."),
            };
        }

        public bool IsDirectionConnected(NeighborDirection direction)
        {
            return (neighborMask & (byte)direction) != 0;
        }

        public bool IsDirectionConnected(int directionIndex)
        {
            NeighborDirection direction = GetDirectionFromIndex(directionIndex);
            return (neighborMask & (byte)direction) != 0;
        }

        public byte GetOppositeMask(byte mask)
        {
            return (byte)(~mask & 0xFF);
        }

        public void SetDirection(NeighborDirection direction, NeighborState state, int specificTileId)
        {
            int index = GetDirectionIndex(direction);
            neighborConditions[index].State = state;
            neighborConditions[index].SpecificTileId = specificTileId;
        }

        /// <summary>
        /// Compara un inputMask y los tiles vecinos para verificar si esta regla aplica.
        /// </summary>
        public bool Matches(byte inputMask, TileData[] neighborTiles)
        {
            for (int i = 0; i < 8; i++)
            {
                var condition = neighborConditions[i];
                bool isConnected = (inputMask & (1 << i)) != 0;
                TileData neighbor = neighborTiles[i];

                if (!condition.Matches(isConnected, neighbor))
                    return false;
            }

            return true;
        }
        public bool Matches(byte inputMask, int[] neighborTiles)
        {
            for (int i = 0; i < 8; i++)
            {
                var condition = neighborConditions[i];
                bool isConnected = (inputMask & (1 << i)) != 0;
                int neighborId = neighborTiles[i];

                if (!condition.Matches(isConnected, neighborId))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Versión simple que ignora `TileData`, útil para reglas básicas.
        /// </summary>
        public bool Matches(byte inputMask)
        {
            for (int i = 0; i < 8; i++)
            {
                var condition = neighborConditions[i];
                bool isConnected = (inputMask & (1 << i)) != 0;

                if (!condition.Matches(isConnected, null))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Compara bit a bit que todos los bits apagados en mask1 también estén apagados en mask2.
        /// </summary>
        public bool IsCompatibleMask(byte mask1, byte mask2)
        {
            for (int i = 0; i < 8; i++)
            {
                bool bit1 = (mask1 & (1 << i)) != 0;
                bool bit2 = (mask2 & (1 << i)) != 0;

                if (!bit1 && bit2)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"Mask: {Convert.ToString(neighborMask, 2).PadLeft(8, '0')}, Central: {tileDataCentral?.id}, Conditions: [{string.Join(", ", neighborConditions.Select(c => $"{c.State} {(c.SpecificTileId !=0 ? $"(ID {c.SpecificTileId})" : "")}"))}]";
        }
    }
}
