using LiteDB;
using System;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase
{
    [Flags]
    public enum NeighborDirection
    {
        None = 0,           // Ninguna dirección
        Up = 1 << 0,        // Arriba (bit 0)
        Right = 1 << 1,     // Derecha (bit 1)
        Down = 1 << 2,      // Abajo (bit 2)
        Left = 1 << 3,      // Izquierda (bit 3)
        UpRight = 1 << 4,   // Arriba-Derecha (bit 4)
        DownRight = 1 << 5, // Abajo-Derecha (bit 5)
        DownLeft = 1 << 6,  // Abajo-Izquierda (bit 6)
        UpLeft = 1 << 7     // Arriba-Izquierda (bit 7)
    }
    public class TileRuleData
    {
        public byte neighborMask { get;  set; } // Byte que representa las conexiones

        public bool checkIsNull { get; set; }

        public int idTileDataCentral {  get; set; }
        public int[] idsTileDataMask { get; set; }

        [BsonIgnore]
        public TileData tileDataCentral { get; set; }     // Índice del tile correspondiente
        [BsonIgnore]
        public TileData[] tileDataMask { get; set; }
        public TileRuleData(byte neighborMask, TileData tileData)
        {
            this.neighborMask = neighborMask;
            this.tileDataCentral = tileData;
            tileDataMask = new TileData[8];
            idsTileDataMask = new int[8];
        }

        public TileRuleData()
        {            
            tileDataMask = new TileData[8];
            idsTileDataMask = new int[8];
            neighborMask = 0;
        }
        // Método para actualizar NeighborMask usando el enum NeighborDirection
        public void UpdateNeighborMask(NeighborDirection direction,  bool isConnected = false, TileData tileData = null)
        {
            if (isConnected)
            {
                // Enciende el bit correspondiente a la dirección
                neighborMask |= (byte)direction;
                tileDataMask[GetDirectionIndex(direction)] = tileData;
                if (tileData!=null)
                {
                    idsTileDataMask[GetDirectionIndex(direction)] = tileData.id;
                }
                else
                {
                    idsTileDataMask[GetDirectionIndex(direction)] = 0;
                }
                
            }
            else
            {
                // Apaga el bit correspondiente a la dirección
                neighborMask &= (byte)~direction;
                tileDataMask[GetDirectionIndex(direction)] = null;           
                idsTileDataMask[GetDirectionIndex(direction)] = 0;
                
            }
        }
        // Método auxiliar para convertir NeighborDirection a un índice (0-7)
        private int GetDirectionIndex(NeighborDirection direction)
        {
            switch (direction)
            {
                case NeighborDirection.Up: return 0;
                case NeighborDirection.Right: return 1;
                case NeighborDirection.Down: return 2;
                case NeighborDirection.Left: return 3;
                case NeighborDirection.UpRight: return 4;
                case NeighborDirection.DownRight: return 5;
                case NeighborDirection.DownLeft: return 6;
                case NeighborDirection.UpLeft: return 7;
                default:
                    throw new ArgumentException("Dirección no válida.");
            }
        }
        public NeighborDirection GetDirectionFromIndex(int index)
        {
            switch (index)
            {
                case 0: return NeighborDirection.Up;
                case 1: return NeighborDirection.Right;
                case 2: return NeighborDirection.Down;
                case 3: return NeighborDirection.Left;
                case 4: return NeighborDirection.UpRight;
                case 5: return NeighborDirection.DownRight;
                case 6: return NeighborDirection.DownLeft;
                case 7: return NeighborDirection.UpLeft;
                default:
                    throw new ArgumentException("Índice no válido. Debe estar entre 0 y 7.");
            }
        }
        // Método para verificar si una dirección está conectada
        public bool IsDirectionConnected(NeighborDirection direction)
        {
            return (neighborMask & (byte)direction) != 0;
        }
        public bool IsDirectionConnected(int directionIndex)
        {
            NeighborDirection direction = GetDirectionFromIndex(directionIndex);
            return (neighborMask & (byte)direction) != 0;
        }
        public bool Matches(byte neighborMaskIn)
        {
            if (checkIsNull)
            {
                return CompareNeighborMasks(this.neighborMask, neighborMaskIn);
            }
            // Compara si la máscara de vecinos coincide con la regla
            return this.neighborMask == neighborMaskIn;
        }
        public bool CompareNeighborMasks(byte mask1, byte mask2)
        {            
            for (int i = 0; i < 8; i++) // Itera sobre cada bit (posición de dirección)
            {
                bool bit1 = (mask1 & (1 << i)) != 0;
                bool bit2 = (mask2 & (1 << i)) != 0;

                if (bit1 ==false && (bit1!=bit2))
                {
                    return false;
                }
                //if (bit1==true && (bit2== true || bit2 ==false))
                //{
                    
                //}
                //else
                //{
                //    if (bit1 != bit2)
                //    {
                //        return false;
                //    }
                //}
                
            }

            return true;
        }
        public byte GetOppositeMask(byte mask)
        {
            return (byte)(~mask & 0xFF); // Invierte los bits y mantiene solo los primeros 8 bits
        }
    }
}
