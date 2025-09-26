using Godot;
using GodotEcsArch.sources.managers.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
public class ColliderData<T>
{
    public int Id { get; }
    public T Owner { get; }
    public Vector2 Position { get; set; } // posición global del collider
    public List<GeometricShape2D> Shapes { get; }   

    public ColliderData(int id, T owner)
    {
        Id = id;
        Owner = owner;
        Shapes = new List<GeometricShape2D>();     
    }

    public void AddShape(GeometricShape2D shape)
    {
        Shapes.Add(shape);
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
    public int Count = 0;
    public SpatialHashMapColliders(float cellSize)
    {
        this.cellSize = MeshCreator.PixelsToUnits(16 * cellSize); ;
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
            DebugDraw.Quad(item.Value, 1, color, 100); //debug    
           
        }

    }
    void addGridPosition(Vector2 positionCell)
    {
        
        float sizeRealWorld = cellSize;
        
        if (!gridPositions.ContainsKey(positionCell))
        {

            Vector2 plot = (sizeRealWorld * positionCell) + new Vector2(sizeRealWorld/2, sizeRealWorld/2);
            Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
            xform = xform.Scaled(new Vector3(sizeRealWorld, sizeRealWorld, 1));
            xform.Origin = new Vector3(plot.X, plot.Y, 1);
            gridPositions.Add(positionCell, xform);            
            
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
    private List<int> GetCellsCoveredBy(Rect2 aabb, out List<Vector2I> puntos)
    {
        List<int> cells = new();
        puntos = new List<Vector2I>();
        int startX = (int)Math.Floor(aabb.Position.X / cellSize);
        int startY = (int)Math.Floor(aabb.Position.Y / cellSize);
        int endX = (int)Math.Floor((aabb.Position.X + aabb.Size.X) / cellSize);
        int endY = (int)Math.Floor((aabb.Position.Y + aabb.Size.Y) / cellSize);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                int cell = CantorPairing(x, y);
               // GD.Print("Celdas a dibujar:" + x +"<-->"+  y);
                cells.Add(cell);
                puntos.Add(new Vector2I(x, y));
            }
        }

        return cells;
    }
    public int AddColliderObject(T owner, GeometricShape2D shape, Vector2 colliderPosition)
    {
        return AddColliderObject(owner, new List<GeometricShape2D> { shape }, colliderPosition);
    }
    public int AddColliderObject(T owner, List<GeometricShape2D> shapes, Vector2 colliderPosition)
    {
       
        int colliderId;
        ColliderData<T> data;

        // Obtener o crear el ColliderData
        if (!objectToColliderId.TryGetValue(owner, out colliderId))
        {
            colliderId = UniqueIdGenerator.GetNextId<T>();
            data = new ColliderData<T>(colliderId, owner)
            {
                Position = colliderPosition
            };

            colliders[colliderId] = data;
            objectToColliderId[owner] = colliderId;
            shapeIndicesByCollider[colliderId] = new List<int>();
        }
        else
        {
            data = colliders[colliderId];
            data.Position = colliderPosition; // actualizar si se pasa nueva
        }


        // Procesar todas las shapes
        foreach (var shape in shapes)
        {
            Vector2 positionShape = colliderPosition + shape.OriginCurrent;
            // Calcular posición global de la shape
          
            // Agregar shape al collider
            int shapeIndex = data.Shapes.Count;
            data.AddShape(shape);
            shapeIndicesByCollider[colliderId].Add(shapeIndex);

            // AABB de la shape
            Rect2 shapeBounds = GetShapeBounds(shape, positionShape);
            List<int> coveredCells = GetCellsCoveredBy(shapeBounds, out List<Vector2I> puntos);

            //foreach (var point in puntos)
            //{
            //    addGridPosition(point);
            //}
            // Registrar en todas las celdas cubiertas
            foreach (int cell in coveredCells)
            {
                if (!cellMap.TryGetValue(cell, out var set))
                {
                    set = new HashSet<ColliderData<T>>();
                    cellMap[cell] = set;
                }
                set.Add(data);
                // guardar la posición para dibujar                
            }

            // Guardar referencia del shape → celdas cubiertas
            shapeToCell[HashShapeKey(colliderId, shapeIndex)] = coveredCells;
        }
        Count++;
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
    public void UpdateColliderPosition(int colliderId, Vector2 newPosition)
    {
        
        if (!colliders.TryGetValue(colliderId, out var data))
            return;

        // Iterar todas las shapes asociadas al collider
        for (int shapeIndex = 0; shapeIndex < data.Shapes.Count; shapeIndex++)
        {
            var shape = data.Shapes[shapeIndex];

            // Posición global de esta shape
            Vector2 shapeWorldPos = newPosition + shape.OriginCurrent;

            // AABB nuevo y viejo
            Rect2 newBounds = GetShapeBounds(shape, shapeWorldPos);
            List<int> newCells = GetCellsCoveredBy(newBounds, out List<Vector2I> puntos);

            int shapeKey = HashShapeKey(colliderId, shapeIndex);
            List<int> oldCells = shapeToCell.GetValueOrDefault(shapeKey) ?? new();

            //foreach (var point in puntos)
            //{
            //    addGridPosition(point);
            //}
            // Sacar de las celdas antiguas donde ya no está
            foreach (var cell in oldCells)
            {
                if (!newCells.Contains(cell) && cellMap.TryGetValue(cell, out var set))
                {
                    set.Remove(data);
                    if (set.Count == 0)
                        cellMap.Remove(cell); // limpieza opcional
                }
            }

            // Insertar en las nuevas celdas
            foreach (var cell in newCells)
            {
                if (!cellMap.TryGetValue(cell, out var set))
                {
                    set = new HashSet<ColliderData<T>>();
                    cellMap[cell] = set;
                }
                set.Add(data);
            }

            // Actualizar shape → celdas
            shapeToCell[shapeKey] = newCells;
        }

        // Actualizar la posición del collider
        data.Position = newPosition;
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
        Count--;
    }

    private static readonly ThreadLocal<HashSet<int>> visitedCellsPoolA =
    new(() => new HashSet<int>());

    private static readonly ThreadLocal<HashSet<int>> testedShapesPoolA =
        new(() => new HashSet<int>());
    public List<T> GetCollidingOwnersInAABB(Rect2 area, int excludeColliderId = 0)
    {
        var visitedCells = visitedCellsPoolA.Value!;
        var alreadyCheckedColliders = testedShapesPoolA.Value!;

        visitedCells.Clear();
        alreadyCheckedColliders.Clear();
        List<T> result = new();

        List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);

        foreach (int cell in coveredCells)
        {
            if (!visitedCells.Add(cell)) continue;

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                if (excludeColliderId != 0 && data.Id == excludeColliderId)
                    continue;
                if (!alreadyCheckedColliders.Add(data.Id)) continue;

                for (int i = 0; i < data.Shapes.Count; i++)
                {
                    var shape = data.Shapes[i];
                    Vector2 shapeWorldPos = data.Position + shape.OriginCurrent;

                    if (IsShapeInAABB(shape, shapeWorldPos, area))
                    {
                        result.Add(data.Owner);
                        break; // 💡 Ya colisionó, no seguimos evaluando más shapes de este owner
                    }
                }
            }
        }

        return result;
    }

    public List<T> GetCollidingOwnersInAABBExternal(GeometricShape2D shapeOrigin,Rect2 area, int excludeColliderId = 0)
    {
        var visitedCells = visitedCellsPoolA.Value!;
        var alreadyCheckedColliders = testedShapesPoolA.Value!;

        visitedCells.Clear();
        alreadyCheckedColliders.Clear();
        List<T> result = new();

        List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);

        foreach (int cell in coveredCells)
        {
            if (!visitedCells.Add(cell)) continue;

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                if (excludeColliderId != 0 && data.Id == excludeColliderId)
                    continue;
                if (!alreadyCheckedColliders.Add(data.Id)) continue;

                for (int i = 0; i < data.Shapes.Count; i++)
                {
                    var shape = data.Shapes[i];
                    Vector2 shapeWorldPos = data.Position + shapeOrigin.OriginCurrent;

                    if (IsShapeInAABB(shapeOrigin, shapeWorldPos, area))
                    {
                        result.Add(data.Owner);
                        break; // 💡 Ya colisionó, no seguimos evaluando más shapes de este owner
                    }
                }
            }
        }

        return result;
    }
    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryAABBBrutePoints(Rect2 area, int excludeColliderId = 0)
    {
        HashSet<int> visitedCells = new();
        HashSet<(int, int)> addedShapes = new(); // (colliderId, shapeIndex)

        List<(T, GeometricShape2D, Vector2)> result = new();

        List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);

        foreach (int cell in coveredCells)
        {
            if (visitedCells.Contains(cell)) continue;
            visitedCells.Add(cell);

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                if (excludeColliderId != 0 && data.Id == excludeColliderId)
                    continue;
                for (int i = 0; i < data.Shapes.Count; i++)
                {
                    var shape = data.Shapes[i];
                    Vector2 position = data.Position;

                    var shapeKey = (data.Id, i);
                    if (addedShapes.Contains(shapeKey)) continue;
                    
                    // ✅ Aquí filtramos: solo añadimos si el punto está dentro del área
                    if (area.HasPoint(position))
                    {
                        GD.Print("Punto:" + position);
                        result.Add((data.Owner, shape, position));
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

        List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);

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
                    Vector2 shapeWorldPos = data.Position + shape.OriginCurrent;

                    var shapeKey = (data.Id, i);
                    if (addedShapes.Contains(shapeKey)) continue;

                    if (IsShapeInAABB(shape, shapeWorldPos, area))
                    {
                        result.Add((data.Owner, shape, shapeWorldPos));
                    }
                }
            }
        }

        return result;
    }
    private static readonly ThreadLocal<HashSet<int>> visitedCellsPool =
        new(() => new HashSet<int>());

    private static readonly ThreadLocal<HashSet<(int, int)>> testedShapesPool =
        new(() => new HashSet<(int, int)>());
    public bool IntersectsAABB(Rect2 area, int excludeColliderId = 0)
    {
        var visitedCells = visitedCellsPool.Value!;
        var testedShapes = testedShapesPool.Value!;
        visitedCells.Clear();
        testedShapes.Clear();

        List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);

        foreach (int cell in coveredCells)
        {
            if (!visitedCells.Add(cell)) continue; // si ya estaba, lo ignora

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                if (excludeColliderId != 0 && data.Id == excludeColliderId)
                    continue;

                for (int i = 0; i < data.Shapes.Count; i++)
                {
                    var shapeKey = (data.Id, i);
                    if (!testedShapes.Add(shapeKey)) continue;

                    var shape = data.Shapes[i];
                    Vector2 shapeWorldPos = data.Position + shape.OriginCurrent;

                    if (IsShapeInAABB(shape, shapeWorldPos, area))
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
