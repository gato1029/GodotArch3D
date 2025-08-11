using Godot;
using GodotEcsArch.sources.managers.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
public class ColliderData<T>
{
    public int Id { get; }
    public T Owner { get; }

    public List<GeometricShape2D> Shapes { get; }
    public List<Vector2> Positions { get; }

    public ColliderData(int id, T owner)
    {
        Id = id;
        Owner = owner;
        Shapes = new List<GeometricShape2D>();
        Positions = new List<Vector2>();
    }

    public void AddShape(GeometricShape2D shape, Vector2 position)
    {
        Shapes.Add(shape);
        Positions.Add(position);
    }

    public void UpdateShapePosition(int index, Vector2 newPosition)
    {
        if (index >= 0 && index < Positions.Count)
            Positions[index] = newPosition;
    }
    public override bool Equals(object obj)
    {
        return obj is ColliderData<T> other && this.Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

public class SpatialHashMapColliders<T>
{
    private readonly float cellSize;
    private readonly Dictionary<int, HashSet<ColliderData<T>>> cellMap = new Dictionary<int, HashSet<ColliderData<T>>>();
    private readonly Dictionary<int, List<int>> shapeToCell = new(); // Antes: int => int
    private readonly Dictionary<int, ColliderData<T>> colliders = new();

    private readonly Dictionary<T, int> objectToColliderId = new();
    private readonly Dictionary<int, List<int>> shapeIndicesByCollider = new(); // colliderId => shape index list

    public Dictionary<int, HashSet<ColliderData<T>>>  CellMap => cellMap;
    private readonly Dictionary<Vector2, Transform3D> gridPositions = new(); // draw grid
    public SpatialHashMapColliders(float cellSize)
    {
        this.cellSize = cellSize;
    }

    private int GetCellIndex(Vector2 position)
    {
        int cellX = (int)Math.Floor(position.X / cellSize);
        int cellY = (int)Math.Floor(position.Y / cellSize);
        return CantorPairing(cellX, cellY);
    }

    private int CantorPairing(int x, int y)
    {
        const int offset = 10000;
        x += offset;
        y += offset;
        return (x + y) * (x + y + 1) / 2 + y;
    }
    public void DrawGrid(Color color)
    {
        foreach (var item in gridPositions)
        {
            DebugDraw.Quad(item.Value, cellSize, color, 100); //debug    
        }

    }
    void addGridPosition(Vector2 positionCell)
    {
        Vector2 vector2 = GetCellIndexNormalized(positionCell);
        if (!gridPositions.ContainsKey(vector2))
        {
            Vector2 plot = (vector2 * cellSize) + new Vector2(cellSize / 2, cellSize / 2);
            Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
            xform.Origin = new Vector3(plot.X, plot.Y, 1);
            gridPositions.Add(vector2, xform);
        }
        
        

    }
    private Vector2 GetCellIndexNormalized(Vector2 position)
    {
        int cellX = (int)Math.Floor(position.X / cellSize);
        int cellY = (int)Math.Floor(position.Y / cellSize);
        return new Vector2(cellX, cellY);
    }
    private Rect2 GetShapeBounds(GeometricShape2D shape, Vector2 position)
    {
        Vector2 size = shape.GetSizeQuad(); // asumimos que es el tamaño total
        return new Rect2(position - (size / 2), size);
    }
    private List<int> GetCellsCoveredBy(Rect2 aabb)
    {
        List<int> cells = new();

        int startX = (int)Math.Floor(aabb.Position.X / cellSize);
        int startY = (int)Math.Floor(aabb.Position.Y / cellSize);
        int endX = (int)Math.Floor((aabb.Position.X + aabb.Size.X) / cellSize);
        int endY = (int)Math.Floor((aabb.Position.Y + aabb.Size.Y) / cellSize);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                int cell = CantorPairing(x, y);
                cells.Add(cell);
            }
        }

        return cells;
    }

    public int AddShapeToObject(T owner, GeometricShape2D shape, Vector2 position)
    {
        int colliderId;
        ColliderData<T> data;

        // Obtener o crear el ColliderData
        if (!objectToColliderId.TryGetValue(owner, out colliderId))
        {
            colliderId = UniqueIdGenerator.GetNextId<T>();
            data = new ColliderData<T>(colliderId, owner);
            colliders[colliderId] = data;
            objectToColliderId[owner] = colliderId;
            shapeIndicesByCollider[colliderId] = new List<int>();
        }
        else
        {
            data = colliders[colliderId];
        }

        // Agregar la nueva forma
        int shapeIndex = data.Shapes.Count;
        data.AddShape(shape, position);
        shapeIndicesByCollider[colliderId].Add(shapeIndex);

        // ⚠️ En lugar de una sola celda, obtenemos el AABB del shape
        Rect2 shapeBounds = GetShapeBounds(shape, position);
        List<int> coveredCells = GetCellsCoveredBy(shapeBounds);

        // Registrar en todas las celdas cubiertas
        foreach (int cell in coveredCells)
        {
            if (!cellMap.TryGetValue(cell, out var set))
            {
                set = new HashSet<ColliderData<T>>();
                cellMap[cell] = set;
            }
            set.Add(data); // No necesitas chequear Contains
            
            //addGridPosition(CellIndexToVector(cell)); // opcional: dibujar celda
        }

        // Podés registrar la lista de celdas cubiertas si querés trackear luego
        shapeToCell[HashShapeKey(colliderId, shapeIndex)] = coveredCells;


        return colliderId;
    }
    private Vector2 CellIndexToVector(int cell)
    {
        // Inverso aproximado del Cantor Pairing, solo para visualización
        int approx = (int)Math.Floor(Math.Sqrt(2 * cell));
        for (int x = -1000; x < 1000; x++) // usar un rango fijo si no se guarda el (x,y)
        {
            for (int y = -1000; y < 1000; y++)
            {
                if (CantorPairing(x, y) == cell)
                    return new Vector2(x, y);
            }
        }
        return Vector2.Zero;
    }

    public void UpdateShapePosition(int colliderId, int shapeIndex, Vector2 newPosition)
    {
        if (!colliders.TryGetValue(colliderId, out var data)) return;
        if (shapeIndex < 0 || shapeIndex >= data.Shapes.Count) return;

        var shape = data.Shapes[shapeIndex];
        Rect2 oldBounds = GetShapeBounds(shape, data.Positions[shapeIndex]);
        Rect2 newBounds = GetShapeBounds(shape, newPosition);

        List<int> oldCells = shapeToCell.GetValueOrDefault(HashShapeKey(colliderId, shapeIndex)) ?? new();
        List<int> newCells = GetCellsCoveredBy(newBounds);

        // Eliminar de celdas antiguas donde ya no debe estar
        foreach (var cell in oldCells)
        {
            if (!newCells.Contains(cell))
                cellMap[cell]?.Remove(data);
        }

        // Agregar a nuevas celdas
        foreach (var cell in newCells)
        {
            if (!cellMap.TryGetValue(cell, out var set))
            {
                set = new HashSet<ColliderData<T>>();
                cellMap[cell] = set;
            }
            set.Add(data); // No necesitas chequear Contains
            // addGridPosition(CellIndexToVector(cell));
        }

        // Actualizar posición y celdas asociadas
        data.UpdateShapePosition(shapeIndex, newPosition);
        shapeToCell[HashShapeKey(colliderId, shapeIndex)] = newCells;
    }


    public void RemoveCollider(int colliderId)
    {
        if (!colliders.TryGetValue(colliderId, out var data)) return;

        // Eliminar todas las referencias en las celdas
        for (int i = 0; i < data.Shapes.Count; i++)
        {
            int shapeKey = HashShapeKey(colliderId, i);

            if (shapeToCell.TryGetValue(shapeKey, out var cells))
            {
                foreach (int cell in cells)
                {
                    if (cellMap.TryGetValue(cell, out var list))
                    {
                        list.Remove(data);

                        // Limpieza opcional: si la celda queda vacía, podés eliminarla
                        if (list.Count == 0)
                            cellMap.Remove(cell);
                    }
                }

                shapeToCell.Remove(shapeKey);
            }
        }

        // Limpiar datos del collider
        UniqueIdGenerator.ReleaseId<T>(colliderId);
        objectToColliderId.Remove(data.Owner);
        colliders.Remove(colliderId);
        shapeIndicesByCollider.Remove(colliderId);
    }


    private Vector2[] GetRectVertices(Rect2 rect)
    {
        // Calculamos la mitad del tamaño del rectángulo
        Vector2 halfSize = rect.Size / 2;

        // Calculamos los vértices en función del centro
        Vector2[] vertices = new Vector2[4]
        {
        rect.Position - halfSize, // Superior izquierdo
        rect.Position + new Vector2(halfSize.X, -halfSize.Y), // Superior derecho
        rect.Position + new Vector2(-halfSize.X, halfSize.Y), // Inferior izquierdo
        rect.Position + halfSize, // Inferior derecho
        };

        return vertices;
    }
    public List<T> GetCollidingOwnersInAABB(Rect2 area)
    {
        HashSet<int> visitedCells = new();
        HashSet<int> alreadyCheckedColliders = new(); // Para evitar repetir el mismo objeto
        List<T> result = new();

        List<int> coveredCells = GetCellsCoveredBy(area);

        foreach (int cell in coveredCells)
        {
            if (!visitedCells.Add(cell)) continue;

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                if (!alreadyCheckedColliders.Add(data.Id)) continue;

                for (int i = 0; i < data.Shapes.Count; i++)
                {
                    var shape = data.Shapes[i];
                    var pos = data.Positions[i];

                    if (IsShapeInAABB(shape, pos, area))
                    {
                        result.Add(data.Owner);
                        break; // 💡 Ya colisionó, no seguimos evaluando más shapes de este owner
                    }
                }
            }
        }

        return result;
    }

    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryAABBBrute(Rect2 area)
    {
        HashSet<int> visitedCells = new();
        HashSet<(int, int)> addedShapes = new(); // (colliderId, shapeIndex)

        List<(T, GeometricShape2D, Vector2)> result = new();

        List<int> coveredCells = GetCellsCoveredBy(area);

        foreach (int cell in coveredCells)
        {
            if (visitedCells.Contains(cell)) continue;
            visitedCells.Add(cell);

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                for (int i = 0; i < data.Shapes.Count; i++)
                {
                    var shape = data.Shapes[i];
                    var pos = data.Positions[i];

                    var shapeKey = (data.Id, i);
                    if (addedShapes.Contains(shapeKey)) continue;

                    if (IsShapeInAABB(shape, pos, area))
                    {
                        result.Add((data.Owner, shape, pos));
                        addedShapes.Add(shapeKey);
                    }
                }
            }
        }

        return result;
    }

    public bool IntersectsAABB(Rect2 area)
    {
        HashSet<int> visitedCells = new();
        HashSet<(int, int)> testedShapes = new(); // (colliderId, shapeIndex)

        List<int> coveredCells = GetCellsCoveredBy(area);

        foreach (int cell in coveredCells)
        {
            if (!visitedCells.Add(cell)) continue; // si ya estaba, lo ignora

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                for (int i = 0; i < data.Shapes.Count; i++)
                {
                    var shapeKey = (data.Id, i);
                    if (!testedShapes.Add(shapeKey)) continue;

                    var shape = data.Shapes[i];
                    var pos = data.Positions[i];

                    if (IsShapeInAABB(shape, pos, area))
                        return true; // apenas encuentra uno, corta
                }
            }
        }

        return false; // no encontró ninguno
    }

    private bool IsShapeInAABB(GeometricShape2D shape, Vector2 position, Rect2 aabb)
    {
        var size = shape.GetSizeQuad();
        var shapeBounds = new Rect2(position - size / 2, size);
        return shapeBounds.Intersects(aabb);
    }

    private int HashShapeKey(int colliderId, int shapeIndex)
    {
        return colliderId * 1000 + shapeIndex;
    }
}
