using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers.Collision;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Flecs.NET.Core.Ecs.Units.Angles;
using static Godot.HttpRequest;
public class ColliderData<T>
{
    public int Id { get; }
    public T Owner { get; }
    public int Team { get; set; }
    public Vector2 Position { get; set; } // posición global del collider
    public List<GeometricShape2D> Shapes { get; }   
    public GeometricShape2D ShapeMove { get; set; }
  //  public Rect2 rect2Move { get; set; }

    public ColliderData(int id, T owner, int team)
    {
        Id = id;
        Team = team;
        Owner = owner;
        Shapes = new List<GeometricShape2D>();     
    }

    public void AddShape(GeometricShape2D shape)
    {
        Shapes.Add(shape);
    }
    public void AddShapeMove(GeometricShape2D shape)
    {
        ShapeMove = shape;
    }
    //public void AddShapeMoveRect2(Rect2 rect2)
    //{
    //    rect2Move = rect2;
    //}
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
    private readonly float cellSizePixel;
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
        this.cellSizePixel = (16 * cellSize); ;
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
    public void DrawGrid(Godot.Color color)
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
            //Transform3D xform = new Transform3D(Basis.Identity, Vector3.Zero);
            //xform = xform.Scaled(new Vector3(sizeRealWorld, sizeRealWorld, 1));
            //xform.Origin = new Vector3(plot.X, plot.Y, 1);
            gridPositions.Add(positionCell, default);
            WireShape.Instance.DrawSquare(new Vector2(cellSizePixel, cellSizePixel), plot, 30, Godot.Colors.BlueViolet, WireShape.TypeDraw.PIXEL);
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
   
    
    public int AddColliderObject(T owner, GeometricShape2D shape, Vector2 colliderPosition,int team, GeometricShape2D shapeMove = null, bool debugDraw=false)
    {   
        return AddColliderObject(owner, new List<GeometricShape2D> { shape }, colliderPosition, team,shapeMove, debugDraw);
    }
    public int AddColliderObject(T owner, List<GeometricShape2D> shapes, Vector2 colliderPosition,int team, GeometricShape2D shapeMove =null, bool debugDraw = false)
    {
        int colliderId;
        ColliderData<T> data;

        // Obtener o crear el ColliderData
        if (!objectToColliderId.TryGetValue(owner, out colliderId))
        {
            colliderId = UniqueIdGenerator.GetNextId<T>();
            data = new ColliderData<T>(colliderId, owner,team)
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
        if (shapeMove!=null)
        {
            Vector2 positionShape = colliderPosition + shapeMove.OriginCurrent;
            data.AddShapeMove(shapeMove);
            //data.AddShapeMoveRect2(new Rect2(positionShape - (shapeMove.GetSizeQuad() / 2), shapeMove.GetSizeQuad()));
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
        
        
        //if ( data.Owner is Entity entity)
        //{
        //    if (entity.IsAlive())
        //    {
        //        if (entity.Has<ProjectileComponent>())
        //        {
        //            // ✅ Aquí entity es del tipo correcto
        //            GD.Print($"Entity del mal id: {entity.Id}");
        //        }
        //    }            
        //}
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

    public List<GeometricShape2D> GetCollidersBodyShape(int id)
    {
        if (!colliders.TryGetValue(id, out var data))
        {
            return data.Shapes;
        }
        return null;
    }

    public (GeometricShape2D forma, Vector2 position) GetCollidersMoveShape(int id)
    {
        if (colliders.TryGetValue(id, out var data))
        {
            return (data.ShapeMove, data.Position);
        }
        return default;
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

    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryAABBBInCirclePoints(Vector2 position, float radius, int excludeColliderId = 0, int numberNeighbord = 0, List<int> filterTeam = null, int filterTeamIgnoreAliados = 0)
    {
        // Construimos el área rectangular que cubre el círculo
        var area = new Rect2(
            position - new Vector2(radius, radius),
            new Vector2(radius * 2f, radius * 2f)
        );

        // Llamamos al método original
        return QueryAABBBrutePoints(area, excludeColliderId,numberNeighbord, filterTeam);
    }
    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryCirclePoints(Vector2 position, float radius, int excludeColliderId = 0, int numberNeighbord=0, List<int> filterTeam = null, int filterTeamIgnoreAliados = 0)
    {
        var area = new Rect2(position - new Vector2(radius, radius), new Vector2(radius * 2f, radius * 2f));

        var results = QueryAABBBrutePoints(area, excludeColliderId,numberNeighbord, filterTeam, filterTeamIgnoreAliados);

        // ✅ Filtramos los que estén dentro del radio
        float radiusSq = radius * radius;
        results.RemoveAll(r => r.Position.DistanceSquaredTo(position) > radiusSq);

        return results;
    }
    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryAABBBrutePoints(Rect2 area, int excludeColliderId = 0, int numberNeighbord = 0, List<int> filterTeam = null, int filterTeamIgnoreAliados = 0)
    {
        HashSet<int> visitedCells = new();
        HashSet<(int, int)> addedShapes = new(); // (colliderId, shapeIndex)

        List<(T, GeometricShape2D, Vector2)> result = new();
        var cellBounds = GetCellBounds(area);
        //List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);
        for (int x = cellBounds.StartX; x <= cellBounds.EndX; x++)
        {
            for (int y = cellBounds.StartY; y <= cellBounds.EndY; y++)
            {
                int cell = CantorPairing(x, y);
               
                if (visitedCells.Contains(cell)) continue;
                visitedCells.Add(cell);

                if (!cellMap.TryGetValue(cell, out var list)) continue;

                foreach (var data in list)
                {
                    if (excludeColliderId != 0 && data.Id == excludeColliderId)
                        continue;
                    if (filterTeam==null)
                    {
                        if (data.Team == filterTeamIgnoreAliados)
                        {
                            continue; // si el equipo del collider es el mismo que el que queremos ignorar, lo saltamos
                        }
                        for (int i = 0; i < data.Shapes.Count; i++)
                        {
                            var shape = data.Shapes[i];
                            Vector2 position = data.Position;

                            var shapeKey = (data.Id, i);
                            if (addedShapes.Contains(shapeKey)) continue;
                            addedShapes.Add(shapeKey);
                            // ✅ Aquí filtramos: solo añadimos si el punto está dentro del área
                            if (area.HasPoint(position))
                            {
                                result.Add((data.Owner, shape, position));
                                if (numberNeighbord != 0 && result.Count >= numberNeighbord)
                                {
                                    return result;
                                }
                            }

                        }
                    }
                    else
                    {
                        foreach (var itemTeam in filterTeam)
                        {
                            if (data.Team == itemTeam)
                            {
                                for (int i = 0; i < data.Shapes.Count; i++)
                                {
                                    var shape = data.Shapes[i];
                                    Vector2 position = data.Position;

                                    var shapeKey = (data.Id, i);
                                    if (addedShapes.Contains(shapeKey)) continue;
                                    addedShapes.Add(shapeKey);
                                    // ✅ Aquí filtramos: solo añadimos si el punto está dentro del área
                                    if (area.HasPoint(position))
                                    {
                                        result.Add((data.Owner, shape, position));
                                        if (numberNeighbord != 0 && result.Count >= numberNeighbord)
                                        {
                                            return result;
                                        }
                                    }

                                }
                                break; // si ya encontramos el equipo, no necesitamos seguir revisando otros equipos para este collider
                            }
                        }
                    }
                   
                }                
            }
        }

        return result;
    }

    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryBruteShape(float radius, Vector2 position, int excludeColliderId = 0)
    {
        var area = new Rect2(position - new Vector2(radius, radius), new Vector2(radius * 2f, radius * 2f));
        return QueryAABBBrute(area, excludeColliderId);

    }

    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryBruteShape(GeometricShape2D origen, Vector2 position, int excludeColliderId=0)
    {
        var size = origen.GetSizeQuad();
        var area = new Rect2(position - size,size);

        return QueryAABBBrute(area, excludeColliderId);
        
    }

    public List<(T Owner, GeometricShape2D Shape, Vector2 Position)> QueryAABBBrute(Rect2 area, int excludeColliderId = 0)
    {
        HashSet<int> visitedCells = new();
        HashSet<(int, int)> addedShapes = new(); // (colliderId, shapeIndex)

        List<(T, GeometricShape2D, Vector2)> result = new();

        var cellBounds = GetCellBounds(area);

        //List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);
        for (int x = cellBounds.StartX; x <= cellBounds.EndX; x++)
        {
            for (int y = cellBounds.StartY; y <= cellBounds.EndY; y++)
            {
                int cell = CantorPairing(x, y);
              
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
                        Vector2 shapeWorldPos = data.Position + shape.OriginCurrent;

                        var shapeKey = (data.Id, i);
                        if (addedShapes.Contains(shapeKey)) continue;
                        addedShapes.Add(shapeKey);
                        if (IsShapeInAABB(shape, shapeWorldPos, area))
                        {
                            result.Add((data.Owner, shape, shapeWorldPos));
                        }
                    }
                }                
            }
        }

        return result;
    }

    private (int StartX,int StartY,int EndX,int EndY) GetCellBounds(Rect2 aabb)
    {
        int startX = (int)Math.Floor(aabb.Position.X / cellSize);
        int startY = (int)Math.Floor(aabb.Position.Y / cellSize);
        int endX = (int)Math.Floor((aabb.Position.X + aabb.Size.X) / cellSize);
        int endY = (int)Math.Floor((aabb.Position.Y + aabb.Size.Y) / cellSize);
        return (startX, startY, endX, endY);
    }
    private List<int>  GetCellsCoveredBy(Rect2 aabb, out List<Vector2I> puntos)
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
    //public bool IntersecMovePreciseColliders(int idColliderOrigin, Vector2 position)
    //{
    //    if (colliders.TryGetValue(idColliderOrigin, out var collider))
    //    {
    //        var shape = collider.ShapeMove;
    //        var positionShape = position + shape.OriginCurrent;
    //        Rect2 aabb = new Rect2(positionShape - (shape.GetSizeQuad() / 2), shape.GetSizeQuad());
    //        var resultColliders = IntersectsMoveAABBColliders(aabb, idColliderOrigin);
    //        foreach (var item in resultColliders)
    //        {

    //        }
    //    }
    //    return false;
    //}
    public bool IntersecMoveAABBColliders(int idColliderOrigin, Vector2 position)
    {
        if (colliders.TryGetValue(idColliderOrigin, out var collider))
        {
          var shape = collider.ShapeMove;
          var positionShape = position + shape.OriginCurrent;
          Rect2 aabb = new Rect2(positionShape - (shape.GetSizeQuad() / 2), shape.GetSizeQuad());
            return IntersectsMoveAABB(aabb, idColliderOrigin);
        }
        return false;
    }
    public bool IntersectsMoveAABB(Rect2 area, int excludeColliderId = 0)
    {
        var visitedCells = visitedCellsPool.Value!;
        
        visitedCells.Clear();        

        List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);

        foreach (int cell in coveredCells)
        {
            if (!visitedCells.Add(cell)) continue; // si ya estaba, lo ignora

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                if (excludeColliderId != 0 && data.Id == excludeColliderId)
                    continue;

                var shape = data.ShapeMove;
                Vector2 shapeWorldPos = data.Position + shape.OriginCurrent;

                if (IsShapeInAABB(shape, shapeWorldPos, area))
                    return true; // apenas encuentra uno, corta
            }
        }

        return false; // no encontró ninguno
    }
    public List< (T objeto, GeometricShape2D shape, Vector2 position)> IntersectsMoveAABBCollidersvsStatic(Rect2 area, int excludeColliderId = 0)
    {
        List<(T, GeometricShape2D, Vector2)> formas = new List<(T, GeometricShape2D, Vector2)>();
        var visitedCells = visitedCellsPool.Value!;

        visitedCells.Clear();

        List<int> coveredCells = GetCellsCoveredBy(area, out List<Vector2I> puntos);

        foreach (int cell in coveredCells)
        {
            if (!visitedCells.Add(cell)) continue; // si ya estaba, lo ignora

            if (!cellMap.TryGetValue(cell, out var list)) continue;

            foreach (var data in list)
            {
                if (excludeColliderId != 0 && data.Id == excludeColliderId)
                    continue;
                foreach (var itemShape in data.Shapes)
                {
                    var shape = itemShape;
                    Vector2 shapeWorldPos = data.Position + shape.OriginCurrent;

                    if (IsShapeInAABB(shape, shapeWorldPos, area))
                    {
                        formas.Add((data.Owner, shape, shapeWorldPos));
                        break; // apenas encuentra uno, corta
                    }
                }
                
                    //return true; // apenas encuentra uno, corta
            }
        }

        return formas; // no encontró ninguno
    }
    //public bool IntersectsMovePoint(Vector2 point, int excludeColliderId = 0)
    //{
    //    var visitedCells = visitedCellsPool.Value!;

    //    visitedCells.Clear();
    //    int X = (int)Math.Floor(point.X / cellSize);
    //    int Y = (int)Math.Floor(point.Y / cellSize);
    //    int cell = CantorPairing(X,Y);
     


    //    if (!cellMap.TryGetValue(cell, out var list)) return false;

    //    foreach (var data in list)
    //    {
    //        if (excludeColliderId != 0 && data.Id == excludeColliderId)
    //            continue;
    //        if (data.rect2Move.HasPoint(point))
    //        {
    //            return true;
    //        }
            
    //    }        

    //    return false; // no encontró ninguno
    //}

    private bool IsShapeInAABB(GeometricShape2D shape, Vector2 position, Rect2 aabb)
    {
        var size = shape.GetSizeQuad();
        var shapeBounds = new Rect2(position - (size / 2), size);
        return shapeBounds.Intersects(aabb);
    }

    private int HashShapeKey(int colliderId, int shapeIndex)
    {
        return colliderId * 1000 + shapeIndex;
    }
}
