using Godot;
using GodotEcsArch.sources.managers.Collision;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShapesForm : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public Vector2 positionShape { get; set; }
    public float radiusShape { get; set; }
    public Vector2 sizeShape { get; set; }
    
    private List<Vector2> polygonPoints { get; set; } = new();

    [Export] public Color shapeColor = Colors.Gray;

    private int draggingIndex = -1;     // Índice del punto que estoy arrastrando
    private const float pointRadius = 6f; // Radio visual y radio para detectar puntos

    private bool draggingSquare = false;
    private Vector2 squareDragOffset;

    public delegate void RequestNotifyPreviewShape(List<Vector2> PointsPoligon);
    public event RequestNotifyPreviewShape OnNotifyPreviewShape;

    public delegate void RequestNotifyMovePosition(Vector2 PositionShape, Vector2 size);
    public event RequestNotifyMovePosition OnNotifyPositionShape;

    private bool draggingCircle = false;
    private Vector2 circleDragOffset;

    private bool resizingCircle = false;
    private const float circleHandleRadius = 6f;
    private const float CircleRadiusStep = 4f;
    private const float CircleMinRadius = 6f;
    private const float CircleMaxRadius = 500f;

    private enum RectHandle
    {
        None,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private RectHandle activeHandle = RectHandle.None;
    private const float handleSize = 8f;


    [Export] CheckButton checkEnableTileOcupancy;
    public enum ShapeType
    { 
        Ninguno,
        Circulo,
        Cuadrado,
        Poligono
    }
    public ShapeType shapeType
    {
        get => _shapeType;
        set
        {
            _shapeType = value;
            if (_shapeType != ShapeType.Poligono)
                polygonPoints.Clear();
            QueueRedraw();
        }
    }
    private ShapeType _shapeType;
    public override void _Ready()
	{
        

    }
    public void SetPoligonPoints(List<Vector2> polygon)
    {
        shapeType = ShapeType.Poligono;
        polygonPoints.Clear();
        polygonPoints = polygon;
        QueueRedraw();
    }
    public override void _Input(InputEvent @event)
    {
        if (checkEnableTileOcupancy.ButtonPressed)
            return;

        switch (shapeType)
        {
            case ShapeType.Cuadrado:
                HandleSquareInput(@event);
                break;

            case ShapeType.Circulo:
                HandleCircleInput(@event);
                break;

            case ShapeType.Poligono:
                HandlePolygonInput(@event);
                break;
        }
    }

    private Dictionary<RectHandle, Vector2> GetRectangleHandles()
    {
        Vector2 half = sizeShape / 2f;

        return new Dictionary<RectHandle, Vector2>
    {
        { RectHandle.TopLeft,     positionShape + new Vector2(-half.X, -half.Y) },
        { RectHandle.TopRight,    positionShape + new Vector2( half.X, -half.Y) },
        { RectHandle.BottomLeft,  positionShape + new Vector2(-half.X,  half.Y) },
        { RectHandle.BottomRight, positionShape + new Vector2( half.X,  half.Y) }
    };
    }
    private Vector2 GetCircleHandle()
    {
        // Handle a la derecha del círculo
        return positionShape + new Vector2(radiusShape, 0);
    }
    private void HandleCircleInput(InputEvent @event)
    {
        Vector2 mouse = GetViewport().GetMousePosition();
        Vector2 localPos = GetGlobalTransformWithCanvas().AffineInverse() * mouse;

        float distanceToCenter = localPos.DistanceTo(positionShape);
        Vector2 handlePos = GetCircleHandle();

        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    // 1️⃣ Click sobre el handle → resize
                    if (localPos.DistanceTo(handlePos) <= circleHandleRadius)
                    {
                        resizingCircle = true;
                        return;
                    }

                    // 2️⃣ Click dentro del círculo → mover
                    if (distanceToCenter <= radiusShape)
                    {
                        draggingCircle = true;
                        circleDragOffset = positionShape - localPos;
                    }
                }
                else
                {
                    draggingCircle = false;
                    resizingCircle = false;
                }
            }
        }

        if (@event is InputEventMouseMotion)
        {
            // 🔧 Resize
            if (resizingCircle)
            {
                radiusShape = Mathf.Max(4f, distanceToCenter);
                QueueRedraw();
                OnNotifyPositionShape?.Invoke(positionShape, new Vector2(radiusShape, radiusShape));
                return;
            }

            // 🚚 Move
            if (draggingCircle)
            {
                positionShape = localPos + circleDragOffset;
                QueueRedraw();
                OnNotifyPositionShape?.Invoke(positionShape, new Vector2(radiusShape, radiusShape));
            }
        }
    }

    //private void HandleCircleInput(InputEvent @event)
    //{
    //    Vector2 mouse = GetViewport().GetMousePosition();
    //    Vector2 localPos = GetGlobalTransformWithCanvas().AffineInverse() * mouse;

    //    float distanceToCenter = localPos.DistanceTo(positionShape);

    //    if (@event is InputEventMouseButton mb)
    //    {
    //        if (mb.ButtonIndex == MouseButton.Left)
    //        {
    //            if (mb.Pressed && distanceToCenter <= radiusShape)
    //            {
    //                draggingCircle = true;
    //                circleDragOffset = positionShape - localPos;
    //            }
    //            else if (!mb.Pressed)
    //            {
    //                draggingCircle = false;
    //            }
    //        }
    //    }

    //    if (@event is InputEventMouseMotion && draggingCircle)
    //    {
    //        positionShape = localPos + circleDragOffset;
    //        QueueRedraw();
    //        OnNotifyPositionShape?.Invoke(positionShape, sizeShape);
    //    }
    //}
    private void HandleSquareInput(InputEvent @event)
    {
        Vector2 mouse = GetViewport().GetMousePosition();
        Vector2 localPos = GetGlobalTransformWithCanvas().AffineInverse() * mouse;

        Rect2 squareRect = new Rect2(
            positionShape - sizeShape / 2f,
            sizeShape
        );

        var handles = GetRectangleHandles();

        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (mb.Pressed)
                {
                    // 1️⃣ ¿Click sobre un handle?
                    foreach (var h in handles)
                    {
                        if (localPos.DistanceTo(h.Value) <= handleSize)
                        {
                            activeHandle = h.Key;
                            return;
                        }
                    }

                    // 2️⃣ ¿Click dentro del rectángulo?
                    if (squareRect.HasPoint(localPos))
                    {
                        draggingSquare = true;
                        squareDragOffset = positionShape - localPos;
                    }
                }
                else
                {
                    draggingSquare = false;
                    activeHandle = RectHandle.None;
                }
            }
        }

        if (@event is InputEventMouseMotion)
        {
            // 🔧 Redimensionar
            if (activeHandle != RectHandle.None)
            {
                ResizeRectangle(localPos);
                QueueRedraw();
                OnNotifyPositionShape?.Invoke(positionShape, sizeShape);
                return;
            }

            // 🚚 Mover
            if (draggingSquare)
            {
                positionShape = localPos + squareDragOffset;
                QueueRedraw();
                OnNotifyPositionShape?.Invoke(positionShape, sizeShape);
            }
        }
    }
    private void ResizeRectangle(Vector2 mousePos)
    {
        Vector2 half = sizeShape / 2f;
        Vector2 min = positionShape - half;
        Vector2 max = positionShape + half;

        switch (activeHandle)
        {
            case RectHandle.TopLeft:
                min = mousePos;
                break;
            case RectHandle.TopRight:
                min.Y = mousePos.Y;
                max.X = mousePos.X;
                break;
            case RectHandle.BottomLeft:
                min.X = mousePos.X;
                max.Y = mousePos.Y;
                break;
            case RectHandle.BottomRight:
                max = mousePos;
                break;
        }

        sizeShape = (max - min).Abs();
        positionShape = (min + max) / 2f;
    }

    
    private void HandlePolygonInput(InputEvent @event)
    {
        // Arrastrar punto existente
        if (@event is InputEventMouseMotion mm && draggingIndex != -1)
        {
            Vector2 mouse = GetViewport().GetMousePosition();
            Vector2 localPos = GetGlobalTransformWithCanvas().AffineInverse() * mouse;

            polygonPoints[draggingIndex] = localPos;
            QueueRedraw();
            return;
        }

        // Soltar punto
        if (@event is InputEventMouseButton mbRelease && !mbRelease.Pressed)
        {
            draggingIndex = -1;
            return;
        }

        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            Vector2 mouse = GetViewport().GetMousePosition();
            Vector2 localPos = GetGlobalTransformWithCanvas().AffineInverse() * mouse;

            if (mb.ButtonIndex == MouseButton.Left)
            {
                // Click sobre punto → moverlo
                for (int i = 0; i < polygonPoints.Count; i++)
                {
                    if (polygonPoints[i].DistanceTo(localPos) <= pointRadius)
                    {
                        draggingIndex = i;
                        return;
                    }
                }

                // Crear punto nuevo
                bool exists = polygonPoints.Any(p => p.DistanceTo(localPos) <= pointRadius);
                if (!exists)
                    polygonPoints.Add(localPos);
            }
            else if (mb.ButtonIndex == MouseButton.Right)
            {
                int index = polygonPoints.FindIndex(p =>
                    p.DistanceTo(localPos) <= pointRadius);

                if (index != -1)
                    polygonPoints.RemoveAt(index);
            }

            QueueRedraw();
            OnNotifyPreviewShape?.Invoke(polygonPoints);
        }
    }


    private void DrawPolygonShape()
    {
        // Dibujar lineas entre puntos
        for (int i = 0; i < polygonPoints.Count - 1; i++)
        {
            DrawLine(polygonPoints[i], polygonPoints[i + 1], shapeColor, 2);
        }

        // Si quieres cerrar el polígono automáticamente:
        if (polygonPoints.Count > 2)
        {
            DrawLine(polygonPoints[^1], polygonPoints[0], shapeColor, 2);
        }

        // Dibujar puntos
        foreach (var point in polygonPoints)
        {
            DrawCircle(point, 4f, Colors.Yellow);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        QueueRedraw();
    }
    public override void _Draw()
    {
 
        switch (shapeType)
        {
            case ShapeType.Circulo:                
                DrawCircle(positionShape, radiusShape, shapeColor);
                break;
            case ShapeType.Cuadrado:
                DrawSquare();
                break;
            case ShapeType.Poligono:
                DrawPolygonShape();
                break;

            default:
                break;
        }
     
    }
    private void DrawSquare()
    {
        // Dibujar rectángulo
        Rect2 rect = new Rect2(
            positionShape - sizeShape / 2f,
            sizeShape
        );

        DrawRect(rect, shapeColor);

        // Dibujar handles (solo para cuadrado)
        foreach (var h in GetRectangleHandles().Values)
        {
            DrawRect(
                new Rect2(
                    h - Vector2.One * handleSize / 2f,
                    Vector2.One * handleSize
                ),
                Colors.Yellow
            );
        }
    }

}
