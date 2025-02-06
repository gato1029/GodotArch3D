using System;

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
        public TileData tileDataCentral { get; set; }     // Índice del tile correspondiente
        public TileData[] tileDataMask { get; set; }
        public TileRuleData(byte neighborMask, TileData tileData)
        {
            this.neighborMask = neighborMask;
            this.tileDataCentral = tileData;
            tileDataMask = new TileData[8];
        }

        public TileRuleData()
        {            
            tileDataMask = new TileData[8];
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
            }
            else
            {
                // Apaga el bit correspondiente a la dirección
                neighborMask &= (byte)~direction;
                tileDataMask[GetDirectionIndex(direction)] = tileData;
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
        public bool Matches(byte neighborMask)
        {
            // Compara si la máscara de vecinos coincide con la regla
            return this.neighborMask == neighborMask;
        }
    }
}
