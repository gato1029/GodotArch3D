using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.BlackyEngine.Spatial;

public class StaticSpatialGridOptimizedGeneric<T>
{
    private readonly int _widthInCells;
    private readonly int _heightInCells;

    public readonly float _cellSize;

    // Cada celda apunta al primer node de su lista enlazada.
    private readonly int[] _heads;

    // Lista enlazada de nodes.
    private readonly int[] _next;

    // Node -> Entity ID.
    private readonly int[] _entityIDs;

    // Entity ID -> Query ID.
    // Se utiliza para evitar devolver la misma entidad varias veces.
    private readonly int[] _visited;

    private int _nextNode;

    // 0 = ID inválido.
    // Las entidades comienzan en 1.
    private int _nextEntityId = 1;

    private int _currentQueryId = 1;

    private readonly Stack<int> _freeNodes = new();
    private readonly Stack<int> _freeEntityIds = new();

    // Entity ID -> Value.
    //
    // _values[0] nunca representa una entidad válida.
    private readonly List<T> _values = new();

    private readonly float _originX;
    private readonly float _originY;

    private readonly int _maxEntities;

    public StaticSpatialGridOptimizedGeneric(
        int worldWidth,
        int worldHeight,
        float cellSize,
        int maxNodes,
        int maxEntities)
    {
        if (worldWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(worldWidth));

        if (worldHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(worldHeight));

        if (cellSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(cellSize));

        if (maxNodes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxNodes));

        if (maxEntities <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxEntities));

        _maxEntities = maxEntities;

        _originX = worldWidth * 0.5f;
        _originY = worldHeight * 0.5f;

        _cellSize = cellSize;

        _widthInCells =
            (int)MathF.Ceiling(worldWidth / cellSize);

        _heightInCells =
            (int)MathF.Ceiling(worldHeight / cellSize);

        int totalCells =
            _widthInCells * _heightInCells;

        _heads = new int[totalCells];

        _next = new int[maxNodes];

        _entityIDs = new int[maxNodes];

        // Ahora el tamaño corresponde al máximo
        // de ENTIDADES y no al máximo de NODES.
        //
        // +1 porque el ID 0 es inválido.
        _visited = new int[maxEntities + 1];

        Array.Fill(_heads, -1);
        Array.Fill(_next, -1);
        Array.Fill(_entityIDs, -1);

        // Reservamos el índice 0.
        _values.Add(default!);
    }

    // ============================================================
    // ENTITY IDS
    // ============================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetNewEntityId()
    {
        if (_freeEntityIds.Count > 0)
            return _freeEntityIds.Pop();

        if (_nextEntityId > _maxEntities)
            throw new Exception(
                "SpatialGrid: Max entities reached");

        return _nextEntityId++;
    }

    // ============================================================
    // CELLS
    // ============================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCellIndex(int x, int y)
    {
        return x < 0 ||
               y < 0 ||
               x >= _widthInCells ||
               y >= _heightInCells
            ? -1
            : y * _widthInCells + x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2I WorldToCell(
        float worldX,
        float worldY)
    {
        return new Vector2I(
            (int)MathF.Floor(
                (worldX + _originX) / _cellSize),

            (int)MathF.Floor(
                (worldY + _originY) / _cellSize)
        );
    }

    // ============================================================
    // NODES
    // ============================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewNode()
    {
        if (_freeNodes.Count > 0)
            return _freeNodes.Pop();

        if (_nextNode >= _next.Length)
            throw new Exception(
                "SpatialGrid: Max nodes reached");

        return _nextNode++;
    }

    // ============================================================
    // REGISTER
    // ============================================================

    public int RegisterStatic(
        T value,
        float minX,
        float minY,
        float maxX,
        float maxY)
    {
        int id = GetNewEntityId();

        RegisterInternal(
            id,
            value,
            minX,
            minY,
            maxX,
            maxY);

        return id;
    }

    public void RegisterStatic(
        int id,
        T value,
        float minX,
        float minY,
        float maxX,
        float maxY)
    {
        if (id <= 0 || id > _maxEntities)
            throw new ArgumentOutOfRangeException(
                nameof(id));

        RegisterInternal(
            id,
            value,
            minX,
            minY,
            maxX,
            maxY);
    }

    private void RegisterInternal(
        int id,
        T value,
        float minX,
        float minY,
        float maxX,
        float maxY)
    {
        // Aseguramos que exista el índice.
        while (_values.Count <= id)
            _values.Add(default!);

        _values[id] = value;

        Vector2I min =
            WorldToCell(minX, minY);

        Vector2I max =
            WorldToCell(maxX, maxY);

        // Clamp al tamaño de la grid.
        min.X = Math.Max(0, min.X);
        min.Y = Math.Max(0, min.Y);

        max.X = Math.Min(
            _widthInCells - 1,
            max.X);

        max.Y = Math.Min(
            _heightInCells - 1,
            max.Y);

        // Registrar la entidad en todas
        // las celdas que ocupa.
        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                int cell =
                    GetCellIndex(x, y);

                if (cell == -1)
                    continue;

                int node = GetNewNode();

                _entityIDs[node] = id;

                _next[node] =
                    _heads[cell];

                _heads[cell] = node;
            }
        }
    }

    // ============================================================
    // UNREGISTER
    // ============================================================

    public void UnregisterStatic(
        int id,
        float minX,
        float minY,
        float maxX,
        float maxY)
    {
        if (id <= 0 ||
            id >= _values.Count)
        {
            return;
        }

        // Liberamos el valor.
        _values[id] = default!;

        // El ID queda disponible para reutilización.
        _freeEntityIds.Push(id);

        Vector2I min =
            WorldToCell(minX, minY);

        Vector2I max =
            WorldToCell(maxX, maxY);

        // Clamp para no recorrer fuera de la grid.
        min.X = Math.Max(0, min.X);
        min.Y = Math.Max(0, min.Y);

        max.X = Math.Min(
            _widthInCells - 1,
            max.X);

        max.Y = Math.Min(
            _heightInCells - 1,
            max.Y);

        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                int cell =
                    GetCellIndex(x, y);

                if (cell == -1)
                    continue;

                int current =
                    _heads[cell];

                int previous = -1;

                while (current != -1)
                {
                    if (_entityIDs[current] == id)
                    {
                        // El node está al principio.
                        if (previous == -1)
                        {
                            _heads[cell] =
                                _next[current];
                        }
                        else
                        {
                            _next[previous] =
                                _next[current];
                        }

                        // Liberamos el node.
                        _next[current] = -1;
                        _entityIDs[current] = -1;

                        _freeNodes.Push(current);

                        break;
                    }

                    previous = current;
                    current = _next[current];
                }
            }
        }
    }

    // ============================================================
    // QUERY
    // ============================================================

    public IEnumerable<int> QueryNearbyUnique(
        float worldX,
        float worldY,
        int radius)
    {
        if (radius < 0)
            yield break;

        // Evitar overflow del Query ID.
        if (_currentQueryId == int.MaxValue)
        {
            Array.Fill(
                _visited,
                0);

            _currentQueryId = 1;
        }

        _currentQueryId++;

        Vector2I center =
            WorldToCell(
                worldX,
                worldY);

        for (
            int x = center.X - radius;
            x <= center.X + radius;
            x++)
        {
            for (
                int y = center.Y - radius;
                y <= center.Y + radius;
                y++)
            {
                int cell =
                    GetCellIndex(x, y);

                if (cell == -1)
                    continue;

                int current =
                    _heads[cell];

                while (current != -1)
                {
                    int id =
                        _entityIDs[current];

                    // Seguridad ante nodes inválidos.
                    if (id > 0 &&
                        id <= _maxEntities)
                    {
                        if (_visited[id] !=
                            _currentQueryId)
                        {
                            _visited[id] =
                                _currentQueryId;

                            yield return id;
                        }
                    }

                    current =
                        _next[current];
                }
            }
        }
    }

    // ============================================================
    // GET VALUE
    // ============================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(
        int id,
        out T value)
    {
        if (id <= 0 ||
            id >= _values.Count)
        {
            value = default!;
            return false;
        }

        value = _values[id];

        return true;
    }

    // ============================================================
    // QUERY VALUES
    // ============================================================

    public IEnumerable<T> QueryNearbyValues(
        float worldX,
        float worldY,
        int radius)
    {
        foreach (
            int id in QueryNearbyUnique(
                worldX,
                worldY,
                radius))
        {
            if (TryGetValue(
                id,
                out T value))
            {
                yield return value;
            }
        }
    }

    // ============================================================
    // CLEAR
    // ============================================================

    public void Clear()
    {
        Array.Fill(
            _heads,
            -1);

        Array.Fill(
            _next,
            -1);

        Array.Fill(
            _entityIDs,
            -1);

        Array.Fill(
            _visited,
            0);

        _nextNode = 0;

        _nextEntityId = 1;

        _currentQueryId = 1;

        _freeNodes.Clear();

        _freeEntityIds.Clear();

        _values.Clear();

        // ID 0 reservado como inválido.
        _values.Add(default!);
    }
}