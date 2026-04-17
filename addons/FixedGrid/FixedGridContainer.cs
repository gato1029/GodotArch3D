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
        float[] colWidths = GetColumnWidths();
        float[] rowHeights = GetRowHeights();

        int index = 0;

        float yOffset = 0;

        for (int row = 0; row < Rows; row++)
        {
            float xOffset = 0;

            for (int col = 0; col < Columns; col++)
            {
                if (index >= GetChildCount())
                    return;

                var node = GetChild(index);

                if (node is Control child && child.Visible)
                {
                    Vector2 size = new Vector2(colWidths[col], rowHeights[row]);

                    FitChildInRect(child, new Rect2(
                        new Vector2(xOffset, yOffset),
                        size
                    ));
                }

                xOffset += colWidths[col] + Separation.X;
                index++;
            }

            yOffset += rowHeights[row] + Separation.Y;
        }
    }

    public override Vector2 _GetMinimumSize()
    {
        float[] colWidths = GetColumnWidths();
        float[] rowHeights = GetRowHeights();

        float totalWidth = 0;
        foreach (var w in colWidths)
            totalWidth += w;

        float totalHeight = 0;
        foreach (var h in rowHeights)
            totalHeight += h;

        totalWidth += Separation.X * (Columns - 1);
        totalHeight += Separation.Y * (Rows - 1);

        return new Vector2(totalWidth, totalHeight);
    }
    private float[] GetColumnWidths()
    {
        float[] widths = new float[Columns];

        int index = 0;
        foreach (var node in GetChildren())
        {
            if (node is Control child && child.Visible)
            {
                int col = index % Columns;
                Vector2 min = child.GetCombinedMinimumSize();

                widths[col] = Mathf.Max(widths[col], min.X);
                index++;
            }
        }

        return widths;
    }

    private float[] GetRowHeights()
    {
        float[] heights = new float[Rows];

        int index = 0;
        foreach (var node in GetChildren())
        {
            if (node is Control child && child.Visible)
            {
                int row = index / Columns;
                if (row >= Rows) break;

                Vector2 min = child.GetCombinedMinimumSize();

                heights[row] = Mathf.Max(heights[row], min.Y);
                index++;
            }
        }

        return heights;
    }
    private void Refresh()
    {
        UpdateMinimumSize(); // Notifica al padre que el tamaño cambió
        QueueSort();         // Llama a la notificación de reordenamiento
    }
}
