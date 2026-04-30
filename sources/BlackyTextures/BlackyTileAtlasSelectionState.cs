using Godot;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTextures;


public class BlackyTileAtlasSelectionState
{
    public enum SelectionMode
    {
        SingleArea,
        MultiTile
    }

    public SelectionMode CurrentMode = SelectionMode.SingleArea;

    public bool IsDragging = false;
    public bool ClickJustPressed = false;

    public Vector2 ClickStartMousePos = Vector2.Zero;

    public Vector2I DragStartCell = new Vector2I(-1, -1);
    public Vector2I DragEndCell = new Vector2I(-1, -1);

    public List<Vector2I> MultiSelectedTiles = new List<Vector2I>();

    public TileSelectionMatrixData CurrentMatrix;

    public const float DragThreshold = 4f;

    public void Clear()
    {
        IsDragging = false;
        ClickJustPressed = false;

        DragStartCell = new Vector2I(-1, -1);
        DragEndCell = new Vector2I(-1, -1);

        MultiSelectedTiles.Clear();
        CurrentMatrix = null;
    }

    public void BeginClick(Vector2 mousePos, Vector2I cell)
    {
        ClickJustPressed = true;
        ClickStartMousePos = mousePos;

        DragStartCell = cell;
        DragEndCell = cell;
    }

    public void BeginDrag()
    {
        IsDragging = true;
        ClickJustPressed = false;
    }

    public void EndDrag()
    {
        IsDragging = false;
        ClickJustPressed = false;
    }

    public void UpdateDragCell(Vector2I cell)
    {
        DragEndCell = cell;
    }

    public bool ShouldStartDrag(Vector2 currentMousePos)
    {
        return ClickJustPressed && currentMousePos.DistanceTo(ClickStartMousePos) > DragThreshold;
    }

    public void ToggleMultiCell(Vector2I cell, bool additive)
    {
        if (additive)
        {
            if (!MultiSelectedTiles.Contains(cell))
                MultiSelectedTiles.Add(cell);

            return;
        }

        if (!MultiSelectedTiles.Contains(cell))
            MultiSelectedTiles.Add(cell);
        else
            MultiSelectedTiles.Remove(cell);
    }

    public void ReplaceMultiSelection(List<Vector2I> cells)
    {
        MultiSelectedTiles = cells;
    }

    public void ClearMultiSelection()
    {
        MultiSelectedTiles.Clear();
    }

    public List<int> BuildSelectedIndices(BlackyTileAtlasContext context)
    {
        List<int> result = new List<int>();

        foreach (var cell in MultiSelectedTiles)
            result.Add(context.CellToIndex(cell));

        return result;
    }

    public Rect2I GetCurrentCellBounds()
    {
        int startX = Mathf.Min(DragStartCell.X, DragEndCell.X);
        int startY = Mathf.Min(DragStartCell.Y, DragEndCell.Y);
        int endX = Mathf.Max(DragStartCell.X, DragEndCell.X);
        int endY = Mathf.Max(DragStartCell.Y, DragEndCell.Y);

        return new Rect2I(
            startX,
            startY,
            endX - startX + 1,
            endY - startY + 1
        );
    }
 
}
