using Godot;
using System;

[GlobalClass]
[Tool]
public partial class FixedGridContainer : Container
{
    private int _columns = 4;
    private int _rows = 4;
    private Vector2 _separation = new Vector2(5, 5);

    [Export] public int Columns { get => _columns; set { _columns = value; Refresh(); } }
    [Export] public int Rows { get => _rows; set { _rows = value; Refresh(); } }
    [Export] public Vector2 Separation { get => _separation; set { _separation = value; Refresh(); } }

    public override void _Ready()
    {
        // Esto asegura que si añades o quitas un tile, el grid se recalculé
        ChildOrderChanged += Refresh;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationSortChildren)
        {
            SortMyChildren();
        }
    }

    private Vector2 GetMaxChildMinSize()
    {
        Vector2 maxSize = Vector2.Zero;
        foreach (var node in GetChildren())
        {
            if (node is Control child && child.Visible)
            {
                Vector2 min = child.GetCombinedMinimumSize();
                maxSize.X = Mathf.Max(maxSize.X, min.X);
                maxSize.Y = Mathf.Max(maxSize.Y, min.Y);
            }
        }
        return maxSize;
    }

    private void SortMyChildren()
    {
        Vector2 maxMin = GetMaxChildMinSize();

        // Calculamos el espacio de la celda basado en el tamaño actual del contenedor
        Vector2 cellSize = new Vector2(
            Mathf.Max(maxMin.X, (Size.X - (Separation.X * (Columns - 1))) / Columns),
            Mathf.Max(maxMin.Y, (Size.Y - (Separation.Y * (Rows - 1))) / Rows)
        );

        int index = 0;
        foreach (var node in GetChildren())
        {
            if (node is Control child && child.Visible)
            {
                int col = index % Columns;
                int row = index / Columns;

                if (row < Rows)
                {
                    Vector2 cellPos = new Vector2(
                        col * (cellSize.X + Separation.X),
                        row * (cellSize.Y + Separation.Y)
                    );
                    // Ajustamos al hijo al rectángulo de la celda
                    FitChildInRect(child, new Rect2(cellPos, cellSize));
                }
                else
                {
                    // Si excede las filas/columnas, podrías ocultarlo o dejarlo fuera
                    // FitChildInRect(child, new Rect2(Vector2.Zero, Vector2.Zero)); 
                }
                index++;
            }
        }
    }

    public override Vector2 _GetMinimumSize()
    {
        Vector2 maxMin = GetMaxChildMinSize();
        return new Vector2(
            (maxMin.X * Columns) + (Separation.X * (Columns - 1)),
            (maxMin.Y * Rows) + (Separation.Y * (Rows - 1))
        );
    }

    private void Refresh()
    {
        UpdateMinimumSize(); // Notifica al padre que el tamaño cambió
        QueueSort();         // Llama a la notificación de reordenamiento
    }
}
