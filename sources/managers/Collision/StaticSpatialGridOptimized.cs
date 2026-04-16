using Flecs.NET.Core;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Collision;

public class StaticSpatialGridOptimized
{
    private readonly int _widthInCells;
    private readonly int _heightInCells;
    public readonly float _cellSize;

    private readonly int[] _heads;
    private readonly int[] _next;
    private readonly int[] _entityIDs;

    private int _nextNode = 0;
    private readonly Stack<int> _freeNodes = new();

    private readonly Dictionary<int, Entity> _idToEntity = new();

    private int _nextEntityId = 1;
    private readonly Stack<int> _freeEntityIds = new();
    private readonly float _originX;
    private readonly float _originY;
    private int[] _visited;
    private int _currentQueryId = 1;
    public StaticSpatialGridOptimized(int worldWidth, int worldHeight, float cellSize, int maxNodes)
    {
        _originX = worldWidth * 0.5f;
        _originY = worldHeight * 0.5f;
        _cellSize = cellSize;

        _widthInCells = (int)MathF.Ceiling(worldWidth / cellSize);
        _heightInCells = (int)MathF.Ceiling(worldHeight / cellSize);

        int totalCells = _widthInCells * _heightInCells;

        _heads = new int[totalCells];
        _next = new int[maxNodes];
        _entityIDs = new int[maxNodes];
        _visited = new int[maxNodes]; // o mejor: maxEntityIds si separas eso

        Array.Fill(_heads, -1);
        Array.Fill(_next, -1);
        Array.Fill(_entityIDs, -1);
    }

    // ===============================
    // 🧩 ENTITY ID
    // ===============================

    public int GetNewEntityId()
    {
        if (_freeEntityIds.Count > 0)
            return _freeEntityIds.Pop();

        return _nextEntityId++;
    }

    // ===============================
    // 📍 COORDINATES
    // ===============================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCellIndex(int x, int y)
    {
        if (x < 0 || y < 0 || x >= _widthInCells || y >= _heightInCells)
            return -1;

        return y * _widthInCells + x;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2I WorldToCell(float worldX, float worldY)
    {
        float shiftedX = worldX + _originX;
        float shiftedY = worldY + _originY;

        int x = (int)MathF.Floor(shiftedX / _cellSize);
        int y = (int)MathF.Floor(shiftedY / _cellSize);

        return new Vector2I(x, y);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetNewNode()
    {
        if (_freeNodes.Count > 0)
            return _freeNodes.Pop();

        if (_nextNode >= _next.Length)
            throw new Exception("SpatialGrid: Max nodes reached");

        return _nextNode++;
    }

    // ===============================
    // 🟢 REGISTER
    // ===============================

    public int RegisterStatic(ref Entity entity, float minX, float minY, float maxX, float maxY)
    {
        int id = GetNewEntityId();
        RegisterInternal(id, entity, minX, minY, maxX, maxY);
        return id;
    }

    public void RegisterStatic(int id, Entity entity, float minX, float minY, float maxX, float maxY)
    {
        RegisterInternal(id, entity, minX, minY, maxX, maxY);
    }

    private void RegisterInternal(int id, Entity entity, float minX, float minY, float maxX, float maxY)
    {
        _idToEntity[id] = entity;

        Vector2I min = WorldToCell(minX, minY);
        Vector2I max = WorldToCell(maxX, maxY);

        min.X = Math.Max(0, min.X);
        min.Y = Math.Max(0, min.Y);
        max.X = Math.Min(_widthInCells - 1, max.X);
        max.Y = Math.Min(_heightInCells - 1, max.Y);

        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                int cell = GetCellIndex(x, y);
                if (cell == -1) continue;

                int node = GetNewNode();

                _entityIDs[node] = id;
                _next[node] = _heads[cell];
                _heads[cell] = node;
            }
        }
    }

    // ===============================
    // 🔴 UNREGISTER
    // ===============================

    public void UnregisterStatic(int id, float minX, float minY, float maxX, float maxY)
    {
        if (!_idToEntity.ContainsKey(id))
            return;

        _idToEntity.Remove(id);
        _freeEntityIds.Push(id);

        Vector2I min = WorldToCell(minX, minY);
        Vector2I max = WorldToCell(maxX, maxY);

        for (int x = min.X; x <= max.X; x++)
        {
            for (int y = min.Y; y <= max.Y; y++)
            {
                int cell = GetCellIndex(x, y);
                if (cell == -1) continue;

                int current = _heads[cell];
                int prev = -1;

                while (current != -1)
                {
                    if (_entityIDs[current] == id)
                    {
                        if (prev == -1)
                            _heads[cell] = _next[current];
                        else
                            _next[prev] = _next[current];

                        int toFree = current;
                        current = _next[current];

                        _next[toFree] = -1;
                        _entityIDs[toFree] = -1;
                        _freeNodes.Push(toFree);
                        break;
                    }

                    prev = current;
                    current = _next[current];
                }
            }
        }
    }

    // ===============================
    // 🔍 QUERY (CLAVE PARA TI)
    // ===============================

    public IEnumerable<int> QueryNearbyUnique(float worldX, float worldY, int radius)
    {
        if (_currentQueryId == int.MaxValue)
        {
            Array.Fill(_visited, 0);
            _currentQueryId = 1;
        }

        _currentQueryId++;

        Vector2I center = WorldToCell(worldX, worldY);

        for (int x = center.X - radius; x <= center.X + radius; x++)
        {
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
            {
                int cell = GetCellIndex(x, y);
                if (cell == -1) continue;

                int current = _heads[cell];

                while (current != -1)
                {
                    int id = _entityIDs[current];

                    if (_visited[id] != _currentQueryId)
                    {
                        _visited[id] = _currentQueryId;
                        yield return id;
                    }

                    current = _next[current];
                }
            }
        }
    }

    public Entity? GetEntity(int id)
    {
        return _idToEntity.TryGetValue(id, out var e) ? e : null;
    }

    // ===============================
    // 🧹 CLEAR
    // ===============================

    public void Clear()
    {
        Array.Fill(_heads, -1);
        Array.Fill(_next, -1);
        Array.Fill(_entityIDs, -1);

        _nextNode = 0;
        _freeNodes.Clear();

        _idToEntity.Clear();
        _freeEntityIds.Clear();
        _nextEntityId = 1;
    }
}