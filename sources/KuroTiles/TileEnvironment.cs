using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.KuroTiles;

public struct NeighborState
{
    public long TileId;     // null = no hay tile
    public int GroupId;     // Grupo al que pertenece el tile (si aplica)

    public bool IsEmpty => TileId == 0;

    public NeighborState(long tileId, int groupId)
    {
        TileId = tileId;
        GroupId = groupId;
    }
}

public class TileEnvironment
{
    // Array fijo de 8 posiciones, alineado con NeighborPosition (0..7)
    private readonly NeighborState[] _neighbors = new NeighborState[8];

    public TileEnvironment() { }

    /// <summary>
    /// Asigna el estado de un vecino en una posición específica.
    /// </summary>
    public void Set(NeighborPosition position, long tileId, int groupId)
    {
        _neighbors[(int)position] = new NeighborState(tileId, groupId);
    }

    /// <summary>
    /// Obtiene el estado de un vecino. 
    /// </summary>
    public NeighborState? Get(NeighborPosition position)
    {
        return _neighbors[(int)position];
    }

    /// <summary>
    /// Devuelve todos los estados de vecinos.
    /// </summary>
    public NeighborState[] GetAll()
    {
        return _neighbors;
    }

    /// <summary>
    /// Calcula una bitmask de vecinos llenos (1 = lleno, 0 = vacío).
    /// </summary>
    public int CalculateBitmask()
    {
        int mask = 0;
        for (int i = 0; i < _neighbors.Length; i++)
        {
            if (!_neighbors[i].IsEmpty)
                mask |= 1 << i;
        }
        return mask;
    }

    /// <summary>
    /// Limpia todo el entorno.
    /// </summary>
    public void Clear()
    {
        for (int i = 0; i < _neighbors.Length; i++)
            _neighbors[i] = new NeighborState(0, 0);
    }
}
