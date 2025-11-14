using Godot;
using System;
using static Godot.BaseButton;
public enum GridDrawMode
{
    CenterLines,
    CenterTile
}
public partial class GridGeneric : Node2D
{
    [Export] public Vector2I cellSize = new Vector2I(64, 64);           // Tamaño de cada celda

    [Export] public float lineWidth = 1f;
   

    [Export] public SpinBox spinBoxX;
    [Export] public SpinBox spinBoxY;
    [Export] public TilesSubViewport subViewport;

    [Export] public GridDrawMode drawMode = GridDrawMode.CenterLines;
    Color lineColor = Colors.Gray;

    public override void _Ready()
    {
        lineColor = Colors.Gray;
        lineColor.A = 0.7f;

        spinBoxX.Value = cellSize.X;
        spinBoxY.Value = cellSize.Y;
        if (spinBoxX != null)
        {
            spinBoxX.ValueChanged += OnSpinBoxXChanged;
        }
        if (spinBoxY != null)
        {
            spinBoxY.ValueChanged += OnSpinBoxYChanged;
        }
        subViewport.OnNotifySelectionCameraZoom += SetSizeLineGrid;

        SetSizeLineGrid(0.9f);
    }


    public void SetDrawMode(GridDrawMode mode)
    {
        drawMode = mode;
        QueueRedraw(); // Redibuja el grid inmediatamente
    }
    public void SetCellSize(int x, int y)
    {
        cellSize =  new Vector2I(x, y);
        spinBoxX.Value = x;
        spinBoxY.Value = y;
    }
    private void OnSpinBoxYChanged(double value)
    {
        cellSize.Y = (int)value;
        QueueRedraw();
    }

    private void OnSpinBoxXChanged(double value)
    {
        cellSize.X = (int)value;
        QueueRedraw();
    }

    bool forced = false;
    private Vector2I scaleZoom =  new Vector2I(1,1);
    public override void _Draw()
    {

        Vector2 textureSize = subViewport.Size * scaleZoom;
        Vector2 center = Position;

        Vector2 offset = Vector2.Zero;
        int offsetLine = 0;
        if (drawMode == GridDrawMode.CenterTile)
        {
            offset = new Vector2(cellSize.X / 2f, cellSize.Y / 2f);
            offsetLine = 1;
        }
        int sizeCellsX = Mathf.CeilToInt(textureSize.X / cellSize.X);
        int sizeCellsY = Mathf.CeilToInt(textureSize.Y / cellSize.Y);

        int halfCellsX = sizeCellsX / 2;
        int halfCellsY = sizeCellsY / 2;

        float endX = (halfCellsX * cellSize.X) + offset.X;
        float endY = (halfCellsY * cellSize.Y) +offset.Y;

        // 🔹 Líneas verticales (de centro hacia ambos lados)
        for (int i = -halfCellsX; i <= halfCellsX+offsetLine; i++)
        {
            float x = i * cellSize.X;
            DrawLine(new Vector2(x-offset.X, -endY), new Vector2(x - offset.X, endY ), lineColor, lineWidth);
        }

        // 🔹 Líneas horizontales (de centro hacia ambos lados)
        for (int j = -halfCellsY; j <= halfCellsY+offsetLine; j++)
        {
            float y = j * cellSize.Y;
            DrawLine(new Vector2(-endX, y-offset.Y), new Vector2(endX, y - offset.Y), lineColor, lineWidth);
        }
        if (drawMode == GridDrawMode.CenterLines)
        {
            // 🔹 Líneas centrales (ejes X e Y)
            DrawLine(new Vector2(-endX, 0), new Vector2(endX, 0), Colors.OrangeRed, lineWidth);
            DrawLine(new Vector2(0, -endY), new Vector2(0, endY), Colors.OrangeRed, lineWidth);
        }
    }

    float zoomCurrent = 1f;
    internal void SetSizeLineGrid(float zoom)
    {
        zoomCurrent = zoom;
        if (zoom>1)
        {
            zoomCurrent = 1;
        }
        
        lineWidth = Mathf.Clamp(1f / zoomCurrent, 1f, 5f); // Ajusta min/max según necesites
        forced = true;

        scaleZoom = new Vector2I((int)lineWidth, (int)lineWidth);
        QueueRedraw();                    
        
    }

}
