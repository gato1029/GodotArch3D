using Flecs.NET.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision;
/* 
 * EXPLICACIÓN DEL SISTEMA DE FILTRADO POR BITS (Layer vs Mask)
 * -----------------------------------------------------------
 * LAYER (¿Quién soy?): 
 * Es la identidad de la entidad. Define a qué grupos pertenece.
 * Ejemplo: Un jugador del Equipo 1 tiene Layer = (TypePlayer | Team1).
 * 
 * MASK (¿Con quién choco?): 
 * Es el "radar" de la entidad. Define qué capas le interesa detectar.
 * Ejemplo: Una bala enemiga tiene Mask = (TypePlayer | Team1), lo que significa
 * que ignorará árboles, muros y a sus propios aliados, buscando solo al jugador.
 * 
 * LÓGICA DE COLISIÓN:
 * Se produce un choque solo si: (Mi_Mask & Su_Layer) != 0
 * 
 * - Si Mask = Everything: Choca con todo lo que tenga CUALQUIER Layer.
 * - Si Mask = None (0): Es un objeto pasivo; no busca chocar con nadie (ahorra CPU).
 * - Si Layer = None (0): Es un "fantasma"; nadie puede encontrarlo para chocar.
 */

public static class CollisionConfig
{
    // EL VALOR VACÍO
    public const uint None = 0u; // 0000...0000

    // TIPOS DE OBJETO (Bits 0-15)
    public const uint TypePlayer = 1u << 0;
    public const uint TypeBullet = 1u << 1;
    public const uint TypeWall = 1u << 2;
    public const uint TypeResource = 1u << 3; // <--- Árboles, Piedras, etc.
    public const uint TypeProp = 1u << 4; // Barriles, decoración.
    
    // BIT 10: Cuerpo físico en el suelo (Pies)
    public const uint TypeBodyFootprint = 1u << 10;

    // EQUIPOS (Bits 16-23)
    public const uint Team1 = 1u << 16;
    public const uint Team2 = 1u << 17;
    public const uint Team3 = 1u << 18;
    public const uint Team4 = 1u << 19;
    public const uint Team5 = 1u << 20;
    public const uint Team6 = 1u << 21;
    public const uint Team7 = 1u << 22;
    public const uint Team8 = 1u << 23;

    // CAPA NEUTRAL (Bit 24)
    public const uint Neutral = 1u << 24;

    // AYUDAS PARA MÁSCARAS
    public const uint AllTeams = 0xFF << 16; // Los 8 equipos juntos
    public const uint Everything = 0xFFFFFFFF;
}


public class FastSpatialHash
{
    // Configuración
    public readonly int TotalCells;
    public readonly int MaxNodes;
    public static float CellSize = 64f; // Ajusta según tu escala (píxeles o unidades)
    public static float tileSizeUnits = MeshCreator.PixelsToUnits(CellSize);

    // Estructura de Lista Ligada Estática (Data-Oriented)
    private readonly int[] _heads;       // [TotalCells] -> Apunta al primer nodo de la celda
    private readonly int[] _nextNodes;   // [MaxNodes] -> Apunta al siguiente nodo de la lista
    private readonly Entity[] _entities; // [MaxNodes] -> La entidad guardada en este nodo
    private readonly int[] _spatialIDs;  // [MaxNodes] -> El ID del objeto (para filtrado/borrado)
    private readonly int[] _nodeCellIndexes; // [MaxNodes] -> La celda a la que pertenece este nodo. Útil para Update.

    // Gestión de Nodos
    private int _nextNodeIndex = 0;
    private readonly Stack<int> _freeNodes = new();

    // Mapping para encontrar el nodo de una entidad dado su Spatial ID (SID)
    // Esto es crucial para el Update eficiente.
    private readonly Dictionary<int, int> _spatialIDToNodeIndex = new();
    private readonly int[] _visitedMarks;
    private int _currentQueryId = 1;
    public FastSpatialHash(float worldWidth, float worldHeight, int maxNodes)
    {
        // Calcular numCellsX y numCellsY
        int numCellsX = (int)MathF.Ceiling(worldWidth / tileSizeUnits);
        int numCellsY = (int)MathF.Ceiling(worldHeight / tileSizeUnits);

        // Calcular TotalCells y asegurar que sea una potencia de 2
        int calculatedTotalCells = numCellsX * numCellsY;
        TotalCells = NextPowerOfTwo(calculatedTotalCells); // Función auxiliar para encontrar la siguiente potencia de 2

        MaxNodes = maxNodes;

        _heads = new int[TotalCells]; // Usamos el TotalCells calculado
        _nextNodes = new int[maxNodes];
        _entities = new Entity[maxNodes];
        _spatialIDs = new int[maxNodes];
        _nodeCellIndexes = new int[maxNodes];
        _visitedMarks = new int[maxNodes];

        Array.Fill(_heads, -1);
        Array.Fill(_nextNodes, -1);
        Array.Fill(_spatialIDs, -1);
        Array.Fill(_nodeCellIndexes, -1);
    }

    // Función auxiliar para encontrar la siguiente potencia de 2
    private static int NextPowerOfTwo(int n)
    {
        if (n == 0) return 1;
        n--;
        n |= n >> 1;
        n |= n >> 2;
        n |= n >> 4;
        n |= n >> 8;
        n |= n >> 16;
        return n + 1;
    }

    // --- MÉTODOS DE REGISTRO ---

    public void RegisterDirect(int sid, int tx, int ty, Entity e)
    {
        int cell = GetHashDirect(tx, ty, TotalCells);
        int nodeIdx = GetNewNodeIndex();

        _entities[nodeIdx] = e;
        _spatialIDs[nodeIdx] = sid;
        _nodeCellIndexes[nodeIdx] = cell; // Guardamos la celda del nodo

        // El nuevo nodo se inserta al principio de la lista de la celda
        _nextNodes[nodeIdx] = _heads[cell];
        _heads[cell] = nodeIdx;

        // Mapeamos el SID al índice del nodo
        _spatialIDToNodeIndex[sid] = nodeIdx;
    }

    public void Register(int sid, float x, float y, Entity e)
    {
        int cell = GetHash(x, y, TotalCells);
        int nodeIdx = GetNewNodeIndex();

        _entities[nodeIdx] = e;
        _spatialIDs[nodeIdx] = sid;
        _nodeCellIndexes[nodeIdx] = cell; // Guardamos la celda del nodo

        _nextNodes[nodeIdx] = _heads[cell];
        _heads[cell] = nodeIdx;

        // Mapeamos el SID al índice del nodo
        _spatialIDToNodeIndex[sid] = nodeIdx;
    }

    // --- MÉTODOS DE UNREGISTER (LIMPIEZA) ---

    public void UnregisterDirect(int sid, int tx, int ty)
    {
        int cell = GetHashDirect(tx, ty, TotalCells);
        RemoveFromCell(cell, sid);
        _spatialIDToNodeIndex.Remove(sid); // Quitamos del mapeo
    }

    public void Unregister(int sid, float x, float y)
    {
        int cell = GetHash(x, y, TotalCells);
        RemoveFromCell(cell, sid);
        _spatialIDToNodeIndex.Remove(sid); // Quitamos del mapeo
    }

    private void RemoveFromCell(int cell, int sid)
    {
        int current = _heads[cell];
        int prev = -1;

        while (current != -1)
        {
            if (_spatialIDs[current] == sid)
            {
                // Puenteamos la lista ligada
                if (prev == -1) _heads[cell] = _nextNodes[current];
                else _nextNodes[prev] = _nextNodes[current];

                int toFree = current;
                current = _nextNodes[current];

                // Limpiamos y liberamos el nodo
                _nextNodes[toFree] = -1;
                _entities[toFree] = default;
                _spatialIDs[toFree] = -1;
                _nodeCellIndexes[toFree] = -1; // Limpiamos la celda del nodo
                _freeNodes.Push(toFree);

                // NOTA IMPORTANTE: Si un Spatial ID (SID) puede tener múltiples entradas en la misma celda,
                // esto solo eliminará la primera que encuentre. Si los SIDs son únicos por entidad,
                // entonces esto está bien. Si no, necesitarías un bucle para eliminar todas las ocurrencias,
                // pero eso complica el manejo de _spatialIDToNodeIndex. Para un Spatial Hash típico,
                // cada entidad tiene un SID único.

                return; // Asumiendo SID único por entidad para esta operación
            }
            prev = current;
            current = _nextNodes[current];
        }
    }

    // --- NUEVO MÉTODO DE ACTUALIZACIÓN ---

    /// <summary>
    /// Actualiza la posición de una entidad en el Spatial Hash.
    /// Realiza un movimiento solo si la entidad cambia de celda.
    /// </summary>
    /// <param name="sid">El ID espacial único de la entidad.</param>
    /// <param name="newX">La nueva coordenada X de la entidad.</param>
    /// <param name="newY">La nueva coordenada Y de la entidad.</param>
    public void UpdatePosition(int sid, float newX, float newY)
    {
        if (!_spatialIDToNodeIndex.TryGetValue(sid, out int nodeIdx))
        {
            // La entidad no está registrada, podrías lanzar un error o simplemente ignorar.
            // Para "unidades dormidas" que no se actualizan, esto significa que no están en el hash.
            return;
        }

        // 1. Calcular la nueva celda basada en la nueva posición
        int newTileX = (int)MathF.Floor(newX / tileSizeUnits);
        int newTileY = (int)MathF.Floor(newY / tileSizeUnits);
        int newCell = GetHashDirect(newTileX, newTileY, TotalCells);

        // 2. Obtener la celda actual donde se encuentra la entidad
        int currentCell = _nodeCellIndexes[nodeIdx];

        // 3. Comprobar si la celda ha cambiado
        if (newCell == currentCell)
        {
            // La entidad sigue en la misma celda, no se necesita hacer nada.
            return;
        }

        // 4. Si la celda ha cambiado, necesitamos "mover" la entidad

        // 4.1. Eliminar la entidad de la celda antigua
        // Esto es más eficiente que el RemoveFromCell general porque ya sabemos el nodeIdx.
        // Necesitamos recorrer la lista ligada de la celda antigua para encontrar el 'prev'
        // y ajustar los punteros.

        int prevInOldCell = -1;
        int currentInOldCell = _heads[currentCell];

        while (currentInOldCell != -1 && currentInOldCell != nodeIdx)
        {
            prevInOldCell = currentInOldCell;
            currentInOldCell = _nextNodes[currentInOldCell];
        }

        if (currentInOldCell == nodeIdx) // Si lo encontramos en la celda antigua
        {
            if (prevInOldCell == -1)
            {
                _heads[currentCell] = _nextNodes[nodeIdx];
            }
            else
            {
                _nextNodes[prevInOldCell] = _nextNodes[nodeIdx];
            }
        }
        // Si no lo encontramos, algo anda mal (la entidad debería estar en su currentCell)

        // 4.2. Insertar la entidad en la nueva celda
        _nextNodes[nodeIdx] = _heads[newCell];
        _heads[newCell] = nodeIdx;
        _nodeCellIndexes[nodeIdx] = newCell; // Actualizar la celda del nodo
    }

    // --- UTILIDADES ---

    public int GetNewNodeIndex()
    {
        if (_freeNodes.Count > 0) return _freeNodes.Pop();
        if (_nextNodeIndex >= MaxNodes) throw new Exception("SpatialHash: Capacidad de nodos agotada.");
        return _nextNodeIndex++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetHashDirect(int ix, int iy, int totalCells)
    {
        unchecked
        {
            uint h = (uint)(ix * 73856093 ^ iy * 19349663);
            return (int)(h & (uint)(totalCells - 1));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHash(float x, float y, int totalCells)
    {
        int ix = (int)MathF.Floor(x / tileSizeUnits);
        int iy = (int)MathF.Floor(y / tileSizeUnits);
        return GetHashDirect(ix, iy, totalCells);
    }

    public static Vector2I WorldToTile(float x, float y)
    {

        int tileX = (int)MathF.Floor(x / tileSizeUnits);
        int tileY = (int)MathF.Floor(y / tileSizeUnits);
        return new Vector2I(tileX, tileY);
    }

    public void Clear()
    {
        Array.Fill(_heads, -1);
        _nextNodeIndex = 0;
        _freeNodes.Clear();
        _spatialIDToNodeIndex.Clear(); // Limpiamos también el mapeo
        Array.Fill(_spatialIDs, -1); // Limpiar SIDs por si acaso
        Array.Fill(_nodeCellIndexes, -1); // Limpiar celdas por si acaso
    }

    public int QueryNodesBounded(
    float x, float y, float radius,
    Span<int> results)
    {
        int count = 0;

        _currentQueryId++;

        if (_currentQueryId == int.MaxValue)
        {
            Array.Fill(_visitedMarks, 0);
            _currentQueryId = 1;
        }

        int centerX = (int)MathF.Floor(x / tileSizeUnits);
        int centerY = (int)MathF.Floor(y / tileSizeUnits);

        int cellRadius = (int)MathF.Ceiling(radius / tileSizeUnits);

        for (int dy = -cellRadius; dy <= cellRadius; dy++)
        {
            for (int dx = -cellRadius; dx <= cellRadius; dx++)
            {
                int tx = centerX + dx;
                int ty = centerY + dy;

                int cell = GetHashDirect(tx, ty, TotalCells);
                int node = _heads[cell];

                while (node != -1)
                {
                    if (_visitedMarks[node] != _currentQueryId)
                    {
                        _visitedMarks[node] = _currentQueryId;

                        results[count++] = node;

                        // 🔥 CORTE DURO (clave)
                        if (count == results.Length)
                            return count;
                    }

                    node = _nextNodes[node];
                }
            }
        }

        return count;
    }

    // Getters para recorrer desde afuera (Sistemas de colisión)
    public int GetHead(int cell) => _heads[cell];
    public int GetNext(int nodeIdx) => _nextNodes[nodeIdx];
    public Entity GetEntity(int nodeIdx) => _entities[nodeIdx];
    public int GetSpatialID(int nodeIdx) => _spatialIDs[nodeIdx];
}

