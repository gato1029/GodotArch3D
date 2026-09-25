using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

public class BlackyPathRegistryManager
{
    private struct PathData
    {
        public Vector2[] Points;
        public int ReferenceCount;
        public bool IsActive;
    }

    // Almacenamiento plano y contiguo en memoria
    private List<PathData> _pathPool = new();

    // Pila para reciclar los espacios de rutas que ya fueron eliminadas
    private Stack<int> _freeIndices = new();

    // 1. Registrar o reutilizar un espacio
    public int RegisterPath(Vector2[] points)
    {
        int pathId;

        if (_freeIndices.Count > 0)
        {
            // Reutilizamos un índice que ya no se usa (evita crecer el array innecesariamente)
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
            // Si no hay huecos libres, agregamos al final
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

    // Acceso directo por índice (Ultra rápido, sin hashing)
    public Vector2 GetWaypoint(int pathId, int index) => _pathPool[pathId].Points[index];

    public int GetPathLength(int pathId) => _pathPool[pathId].Points.Length;

    // 2. Liberar ruta y reciclar su índice
    public void ReleasePath(int pathId)
    {
        var data = _pathPool[pathId];
        data.ReferenceCount--;

        if (data.ReferenceCount <= 0)
        {
            // Desactivamos y limpiamos la referencia al array de puntos para ayudar al GC
            data.IsActive = false;
            data.Points = null;
            _pathPool[pathId] = data;

            // Guardamos el índice para que la próxima ruta lo reutilice
            _freeIndices.Push(pathId);
        }
        else
        {
            _pathPool[pathId] = data;
        }
    }
}
