using Godot;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

public class BlackyPathRegistryManager
{
    private struct PathData
    {
        public Vector2[] Points;
        public int ReferenceCount;
        public bool IsActive;
    }

    // Colecciones genéricas fuertemente tipadas
    private readonly List<PathData> _pathPool = new ();
    private readonly Stack<int> _freeIndices = new Stack<int> ();
    private readonly ConcurrentQueue<int> _pendingReleases = new ();

    public int RegisterPath(Vector2[] points)
    {
        int pathId;

        if (_freeIndices.Count > 0)
        {
            pathId = _freeIndices.Pop();
            _pathPool[pathId] = new PathData
            {
                Points = points,
                ReferenceCount = 0,
                IsActive = true
            };
        }
        else
        {
            pathId = _pathPool.Count;
            _pathPool.Add(new PathData
            {
                Points = points,
                ReferenceCount = 0,
                IsActive = true
            });
        }

        return pathId;
    }

    public void AddReference(int pathId)
    {
        var data = _pathPool[pathId];
        data.ReferenceCount++;
        _pathPool[pathId] = data;
    }

    public Vector2 GetWaypoint(int pathId, int index) => _pathPool[pathId].Points[index];

    public int GetPathLength(int pathId) => _pathPool[pathId].Points.Length;

    // --- COMANDOS THREAD-SAFE ---

    // Llamado desde hilos secundarios en paralelo
    public void EnqueueReleasePath(int pathId)
    {
        _pendingReleases.Enqueue(pathId);
    }

    // Consumido exclusivamente en el thread principal
    public void ProcessPendingReleases()
    {
        while (_pendingReleases.TryDequeue(out int pathId))
        {
            ReleasePath(pathId);
        }
    }

    // ----------------------------

    public void ReleasePath(int pathId)
    {
        var data = _pathPool[pathId];
        data.ReferenceCount--;

        if (data.ReferenceCount <= 0)
        {
            data.IsActive = false;
            data.Points = null;
            _pathPool[pathId] = data;
            _freeIndices.Push(pathId);
        }
        else
        {
            _pathPool[pathId] = data;
        }
    }
}