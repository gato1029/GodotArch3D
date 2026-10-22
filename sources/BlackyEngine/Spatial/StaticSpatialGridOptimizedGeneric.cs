using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.BlackyEngine.Spatial;

public readonly struct ColliderSpriteInstanceData
{
    public readonly int TileId;
    public readonly Vector2 Position;

    public ColliderSpriteInstanceData(
        int tileId,
        Vector2 cellPosition)
    {
        TileId = tileId;
        Position = cellPosition;
    }
}

public class StaticSpatialGridOptimizedGeneric<T>
{
    private struct EntityCellBounds
    {
        public int MinX;
        public int MinY;

        public int MaxX;
        public int MaxY;

        public bool Active;
    }
    private readonly int _widthInCells;
    private readonly int _heightInCells;

    public readonly float _cellSize;

    // Cada celda apunta al primer node de su lista enlazada.
    private readonly int[] _heads;

    // Lista enlazada de nodes.
    private readonly int[] _next;

    // Node -> Entity ID.
    private readonly int[] _entityIDs;

    // Node -> Team ID (NUEVO: Para identificar la facción/equipo por nodo)
    private readonly ushort[] _teams;

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

    // _values[0] nunca representa una entidad válida.
    private readonly List<T> _values = new();
    private readonly List<EntityCellBounds> _entityBounds = new();
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

        _teams = new ushort[maxNodes]; // NUEVO

        // Ahora el tamaño corresponde al máximo
        // de ENTIDADES y no al máximo de NODES.
        //
        // +1 porque el ID 0 es inválido.
        _visited = new int[maxEntities + 1];

        Array.Fill(_heads, -1);
        Array.Fill(_next, -1);
        Array.Fill(_entityIDs, -1);
        Array.Fill(_teams, (ushort)0); // NUEVO

        // Reservamos el índice 0.
        _values.Add(default!);
        _entityBounds.Add(default);
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
        float maxY,
        ushort team = 0) // NUEVO: Parámetro opcional de equipo
    {
        int id = GetNewEntityId();

        RegisterInternal(
            id,
            value,
            minX,
            minY,
            maxX,
            maxY,
            team);

        return id;
    }

    public void RegisterStatic(
        int id,
        T value,
        float minX,
        float minY,
        float maxX,
        float maxY,
        ushort team = 0) // NUEVO: Parámetro opcional de equipo
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
            maxY,
            team);
    }

    public void FreeCollider(int colliderId)
    {
        if (colliderId <= 0 ||
            colliderId >= _entityBounds.Count)
        {
            return;
        }

        EntityCellBounds bounds =
            _entityBounds[colliderId];

        // Ya fue eliminado o nunca existió.
        if (!bounds.Active)
            return;

        // Recorremos únicamente las celdas que sabemos
        // que ocupa este collider.
        for (int x = bounds.MinX; x <= bounds.MaxX; x++)
        {
            for (int y = bounds.MinY; y <= bounds.MaxY; y++)
            {
                int cell = GetCellIndex(x, y);

                if (cell == -1)
                    continue;

                int current = _heads[cell];
                int previous = -1;

                // Buscamos el node que pertenece
                // a este collider dentro de la celda.
                while (current != -1)
                {
                    if (_entityIDs[current] == colliderId)
                    {
                        // Si es el primer node.
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
                        _teams[current] = 0; // NUEVO

                        _freeNodes.Push(current);

                        break;
                    }

                    previous = current;
                    current = _next[current];
                }
            }
        }

        // Liberamos el valor.
        _values[colliderId] = default!;

        // Marcamos el collider como eliminado.
        bounds.Active = false;

        _entityBounds[colliderId] = bounds;

        // El ID puede reutilizarse.
        _freeEntityIds.Push(colliderId);
    }

    private void RegisterInternal(
      int id,
      T value,
      float minX,
      float minY,
      float maxX,
      float maxY,
      ushort team)
    {
        while (_values.Count <= id)
        {
            _values.Add(default!);
            _entityBounds.Add(default);
        }

        _values[id] = value;

        Vector2I min = WorldToCell(minX, minY);
        Vector2I max = WorldToCell(maxX, maxY);

        min.X = Math.Max(0, min.X);
        min.Y = Math.Max(0, min.Y);

        max.X = Math.Min(
            _widthInCells - 1,
            max.X);

        max.Y = Math.Min(
            _heightInCells - 1,
            max.Y);

        // Guardamos las celdas ocupadas por el collider.
        _entityBounds[id] = new EntityCellBounds
        {
            MinX = min.X,
            MinY = min.Y,

            MaxX = max.X,
            MaxY = max.Y,

            Active = true
        };

        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                int cell = GetCellIndex(x, y);

                if (cell == -1)
                    continue;

                int node = GetNewNode();

                _entityIDs[node] = id;
                _teams[node] = team; // NUEVO

                _next[node] = _heads[cell];

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
                        _teams[current] = 0; // NUEVO

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
    // QUERY CONTEXT (scratch por hilo)
    // ============================================================

    /// <summary>Estado scratch de una consulta. Cada hilo debe tener el suyo.</summary>
    public sealed class QueryContext
    {
        internal int QueryId;
        internal int[] Visited = Array.Empty<int>();
    }

    public QueryContext CreateQueryContext() => new QueryContext();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NextQueryId()
    {
        if (++_currentQueryId == int.MaxValue)
        {
            Array.Fill(_visited, 0);
            _currentQueryId = 1;
        }
        return _currentQueryId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NextQueryId(QueryContext context)
    {
        int needed = _maxEntities + 1;
        if (context.Visited.Length < needed)
            Array.Resize(ref context.Visited, needed);

        if (++context.QueryId == int.MaxValue)
        {
            Array.Clear(context.Visited, 0, context.Visited.Length);
            context.QueryId = 1;
        }
        return context.QueryId;
    }

    // ============================================================
    // QUERY (ordenado por anillos desde el centro + filtro de equipo)
    // ============================================================

    /// <summary>
    /// No es thread-safe: usa el scratch interno del grid.
    /// Para consultas en paralelo usa la sobrecarga con QueryContext.
    /// </summary>
    public IEnumerable<int> QueryNearbyUnique(
        float worldX,
        float worldY,
        int radius,
        ushort teamToIgnore = ushort.MaxValue)
    {
        return QueryNearbyUniqueIterator(
            worldX, worldY, radius,
            NextQueryId(), _visited, teamToIgnore);
    }

    /// <summary>Version con contexto por hilo para consultas en paralelo.</summary>
    public IEnumerable<int> QueryNearbyUnique(
        float worldX,
        float worldY,
        int radius,
        QueryContext context,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return QueryNearbyUniqueIterator(
            worldX, worldY, radius,
            NextQueryId(context), context.Visited, teamToIgnore);
    }

    private IEnumerable<int> QueryNearbyUniqueIterator(
        float worldX,
        float worldY,
        int radius,
        int qid,
        int[] visited,
        ushort teamToIgnore)
    {
        if (radius < 0)
            yield break;

        Vector2I center = WorldToCell(worldX, worldY);

        foreach (int id in GetEntitiesFromCellSafe(center.X, center.Y, teamToIgnore, visited, qid))
            yield return id;

        for (int r = 1; r <= radius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                foreach (int id in GetEntitiesFromCellSafe(center.X + dx, center.Y - r, teamToIgnore, visited, qid))
                    yield return id;

                foreach (int id in GetEntitiesFromCellSafe(center.X + dx, center.Y + r, teamToIgnore, visited, qid))
                    yield return id;
            }

            for (int dy = -r + 1; dy <= r - 1; dy++)
            {
                foreach (int id in GetEntitiesFromCellSafe(center.X - r, center.Y + dy, teamToIgnore, visited, qid))
                    yield return id;

                foreach (int id in GetEntitiesFromCellSafe(center.X + r, center.Y + dy, teamToIgnore, visited, qid))
                    yield return id;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<int> GetEntitiesFromCellSafe(
        int x,
        int y,
        ushort teamToIgnore,
        int[] visited,
        int qid)
    {
        int cell = GetCellIndex(x, y);
        if (cell == -1)
            yield break;

        int current = _heads[cell];
        while (current != -1)
        {
            int id = _entityIDs[current];
            ushort entityTeam = _teams[current];

            if (id > 0 && id <= _maxEntities && entityTeam != teamToIgnore)
            {
                if (visited[id] != qid)
                {
                    visited[id] = qid;
                    yield return id;
                }
            }

            current = _next[current];
        }
    }

    public IEnumerable<T> QueryNearbyValues(
        float worldX,
        float worldY,
        int radius,
        ushort teamToIgnore = ushort.MaxValue)
    {
        foreach (int id in QueryNearbyUnique(worldX, worldY, radius, teamToIgnore))
        {
            if (TryGetValue(id, out T value))
                yield return value;
        }
    }

    public IEnumerable<T> QueryNearbyValues(
        float worldX,
        float worldY,
        int radius,
        QueryContext context,
        ushort teamToIgnore = ushort.MaxValue)
    {
        foreach (int id in QueryNearbyUnique(worldX, worldY, radius, context, teamToIgnore))
        {
            if (TryGetValue(id, out T value))
                yield return value;
        }
    }

    public int QueryNearbyUniqueNonAlloc(
        float worldX, float worldY, int radius,
        Span<int> results,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (radius < 0 || results.Length == 0) return 0;

        return QueryNearbyUniqueNonAllocCore(
            worldX, worldY, radius, results,
            NextQueryId(), _visited, teamToIgnore);
    }

    public int QueryNearbyUniqueNonAlloc(
        float worldX, float worldY, int radius,
        Span<int> results,
        QueryContext context,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (radius < 0 || results.Length == 0) return 0;

        return QueryNearbyUniqueNonAllocCore(
            worldX, worldY, radius, results,
            NextQueryId(context), context.Visited, teamToIgnore);
    }

    private int QueryNearbyUniqueNonAllocCore(
        float worldX, float worldY, int radius,
        Span<int> results,
        int qid, int[] visited,
        ushort teamToIgnore)
    {
        Vector2I c = WorldToCell(worldX, worldY);
        int count = 0;

        if (CollectCell(c.X, c.Y, teamToIgnore, visited, qid, results, ref count)) return count;

        for (int r = 1; r <= radius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                if (CollectCell(c.X + dx, c.Y - r, teamToIgnore, visited, qid, results, ref count)) return count;
                if (CollectCell(c.X + dx, c.Y + r, teamToIgnore, visited, qid, results, ref count)) return count;
            }
            for (int dy = -r + 1; dy <= r - 1; dy++)
            {
                if (CollectCell(c.X - r, c.Y + dy, teamToIgnore, visited, qid, results, ref count)) return count;
                if (CollectCell(c.X + r, c.Y + dy, teamToIgnore, visited, qid, results, ref count)) return count;
            }
        }
        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CollectCell(
        int x, int y,
        ushort teamToIgnore,
        int[] visited,
        int qid,
        Span<int> results,
        ref int count)
    {
        int cell = GetCellIndex(x, y);
        if (cell == -1) return false;

        int node = _heads[cell];
        while (node != -1)
        {
            int id = _entityIDs[node];
            if (id > 0 && id <= _maxEntities &&
                _teams[node] != teamToIgnore &&
                visited[id] != qid)
            {
                visited[id] = qid;
                results[count++] = id;
                if (count == results.Length) return true;
            }
            node = _next[node];
        }
        return false;
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
            id >= _values.Count ||
            id >= _entityBounds.Count)
        {
            value = default!;
            return false;
        }

        if (!_entityBounds[id].Active)
        {
            value = default!;
            return false;
        }

        value = _values[id];

        return true;
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
            _teams,
            (ushort)0); // NUEVO

        Array.Fill(
            _visited,
            0);

        _nextNode = 0;

        _nextEntityId = 1;

        _currentQueryId = 1;

        _freeNodes.Clear();

        _freeEntityIds.Clear();

        _values.Clear();
        _entityBounds.Clear();
        // ID 0 reservado como inválido.
        _values.Add(default!);
        _entityBounds.Add(default);
    }
}