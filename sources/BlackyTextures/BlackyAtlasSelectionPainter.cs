using Godot;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTextures;
// Solo es un helper
public static class BlackyAtlasSelectionPainter
{
    public static void DrawGrid(
        Node2D canvas,
        Vector2 startPos,
        int cols,
        int rows,
        Vector2I cellSize,
        Color lineColor,
        float lineWidth)
    {
        int endX = (int)(startPos.X + cols * cellSize.X);
        int endY = (int)(startPos.Y + rows * cellSize.Y);

        for (int y = 0; y <= rows; y++)
        {
            float py = startPos.Y + y * cellSize.Y;
            canvas.DrawLine(new Vector2(startPos.X, py), new Vector2(endX, py), lineColor, lineWidth);
        }

        for (int x = 0; x <= cols; x++)
        {
            float px = startPos.X + x * cellSize.X;
            canvas.DrawLine(new Vector2(px, startPos.Y), new Vector2(px, endY), lineColor, lineWidth);
        }
    }

    public static void DrawAreaSelection(
        Node2D canvas,
        Vector2 startPos,
        Rect2I bounds,
        Vector2I cellSize)
    {
        Rect2 rect = new Rect2(
            startPos + new Vector2(bounds.Position.X * cellSize.X, bounds.Position.Y * cellSize.Y),
            new Vector2(bounds.Size.X * cellSize.X, bounds.Size.Y * cellSize.Y)
        );

        canvas.DrawRect(rect, new Color(1f, 1f, 1f, 0.15f));
        canvas.DrawRect(rect, new Color(1f, 0.2f, 0.2f, 1f), false, 2f);
    }

    public static void DrawMultiSelection(
        Node2D canvas,
        Vector2 startPos,
        List<Vector2I> cells,
        Vector2I cellSize)
    {
        if (cells == null || cells.Count == 0)
            return;

        for (int i = 0; i < cells.Count; i++)
        {
            Vector2I cell = cells[i];

            Rect2 rect = new Rect2(
                startPos + new Vector2(cell.X * cellSize.X, cell.Y * cellSize.Y),
                cellSize
            );

            canvas.DrawRect(rect, new Color(1f, 1f, 1f, 0.15f));
            canvas.DrawRect(rect, new Color(1f, 0.2f, 0.2f, 1f), false, 2f);

            canvas.DrawString(
                ThemeDB.FallbackFont,
                rect.Position + cellSize / 2,
                (i + 1).ToString(),
                HorizontalAlignment.Center,
                -1,
                22,
                Colors.White
            );
        }
    }
}