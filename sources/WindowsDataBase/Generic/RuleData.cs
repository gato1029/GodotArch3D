using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.Generic;
public class RuleData
{
    public byte neighborMask { get; set; }    
    public int idDataCentral { get; set; }
    public int[] idsDataMask { get; set; }

    [BsonIgnore]
    public SpriteData dataCentral;
    [BsonIgnore]
    public SpriteData[] dataNeighbor;
    public NeighborCondition[] neighborConditions { get; set; }

    public RuleData(byte neighborMask, SpriteData tileData)
    {
        this.neighborMask = neighborMask;
        this.dataCentral = tileData;        
        idsDataMask = new int[8];
        dataNeighbor = new SpriteData[8];
        neighborConditions = Enumerable.Range(0, 8).Select(_ => new NeighborCondition()).ToArray();
    }

    public RuleData()
    {        
        idsDataMask = new int[8];
        dataNeighbor = new SpriteData[8];
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

    public void UpdateNeighborMask(NeighborDirection direction, bool isConnected = false, int idData = 0)
    {
        int index = GetDirectionIndex(direction);

        if (isConnected)
        {
            neighborMask |= (byte)direction;
            
            idsDataMask[index] = idData;
        }
        else
        {
            neighborMask &= (byte)~direction;            
            idsDataMask[index] = 0;
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
        return "";
    }
}
