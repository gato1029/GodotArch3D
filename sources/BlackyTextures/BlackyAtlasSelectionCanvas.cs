using Godot;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTextures;

public partial class BlackyAtlasSelectionCanvas : Node2D
{
    [Export] public float LineWidth = 1f;
    [Export] public Color LineColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);

    private BlackyTileAtlasInteractionController controller;

    private BlackyAtlasCameraViewport atlasViewport;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        SetProcessInput(true);
    }
    public void Initialize(
        BlackyTileAtlasInteractionController interactionController,
        BlackyAtlasCameraViewport viewport)
    {
        controller = interactionController;
        atlasViewport = viewport;
        QueueRedraw();
    }
    private Vector2 GetWorldMouse()
    {
        return atlasViewport.ScreenToWorld(GetViewport().GetMousePosition());
    }
    public override void _Draw()
    {
        if (controller == null || !controller.Context.HasTexture)
            return;

        Texture2D atlas = controller.Context.AtlasTexture;
        Vector2 textureSize = controller.Context.TextureSize;
        Vector2 drawStart = -textureSize / 2f;

        // =========================================
        // 1. FONDO CHECKER ALINEADO AL TILESIZE
        // =========================================
        BlackyAtlasBackgroundDrawer.Draw(
            this,
            drawStart,
            controller.Context.Columns,
            controller.Context.Rows,
            controller.Context.CellSize
        );

        // =========================================
        // 2. SOMBRA SUAVE DETRÁS DEL ATLAS
        // =========================================
        Rect2 shadowRect = new Rect2(
            drawStart + new Vector2(6, 6),
            textureSize
        );

        //DrawRect(shadowRect, new Color(0, 0, 0, 0.22f));

        // =========================================
        // 3. TEXTURA BASE
        // =========================================
        DrawTexture(atlas, drawStart);

        // =========================================
        // 4. LEVE ILUMINACIÓN PREMIUM
        // =========================================
        DrawRect(
            new Rect2(drawStart, textureSize),
            new Color(1f, 1f, 1f, 0.025f)
        );

        // =========================================
        // 5. BORDE DEL ATLAS
        // =========================================
        DrawRect(
            new Rect2(drawStart, textureSize),
            new Color(1f, 1f, 1f, 0.12f),
            false,
            2f
        );

        // =========================================
        // 6. GRID
        // =========================================
        BlackyAtlasSelectionPainter.DrawGrid(
            this,
            drawStart,
            controller.Context.Columns,
            controller.Context.Rows,
            controller.Context.CellSize,
            LineColor,
            LineWidth
        );

        // =========================================
        // 7. SELECCIÓN SINGLE AREA
        // =========================================
        if (controller.SelectionState.CurrentMode == BlackyTileAtlasSelectionState.SelectionMode.SingleArea)
        {
            if (controller.SelectionState.DragStartCell.X >= 0)
            {
                BlackyAtlasSelectionPainter.DrawAreaSelection(
                    this,
                    drawStart,
                    controller.SelectionState.GetCurrentCellBounds(),
                    controller.Context.CellSize
                );
            }
        }

        // =========================================
        // 8. MULTI SELECTION
        // =========================================
        if (controller.SelectionState.CurrentMode == BlackyTileAtlasSelectionState.SelectionMode.MultiTile)
        {
            BlackyAtlasSelectionPainter.DrawMultiSelection(
                this,
                drawStart,
                controller.SelectionState.MultiSelectedTiles,
                controller.Context.CellSize
            );
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (controller == null || !controller.Context.HasTexture)
            return;

        HandleLeftMouse(@event);
        HandleMouseMotion(@event);
        HandleRightMouse(@event);
    }

    private void HandleLeftMouse(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouse || mouse.ButtonIndex != MouseButton.Left)
            return;

        Vector2 localMouse = GetWorldMouse();

        if (!IsInsideAtlas(localMouse))
            return;

        Vector2I cell = MouseToCell(localMouse);

        if (mouse.Pressed)
        {
            controller.SelectionState.BeginClick(localMouse, cell);

            if (controller.SelectionState.CurrentMode == BlackyTileAtlasSelectionState.SelectionMode.SingleArea)
                controller.SelectionState.IsDragging = true;

            QueueRedraw();
        }
        else
        {
            if (controller.SelectionState.CurrentMode == BlackyTileAtlasSelectionState.SelectionMode.SingleArea)
            {
                controller.UpdateSingleAreaSelection(
                    controller.SelectionState.DragStartCell,
                    controller.SelectionState.DragEndCell
                );
            }
            else
            {
                if (controller.SelectionState.IsDragging)
                {
                    controller.UpdateMultiSelection(controller.SelectionState.MultiSelectedTiles);
                }
                else if (controller.SelectionState.ClickJustPressed)
                {
                    bool additive = Input.IsKeyPressed(Key.Ctrl);

                    controller.ToggleMultiSelection(cell, additive);
                }
            }

            controller.SelectionState.EndDrag();
            QueueRedraw();
        }
    }

    private void HandleMouseMotion(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion)
            return;

        Vector2 localMouse = GetWorldMouse();

        if (!IsInsideAtlas(localMouse))
            return;

        Vector2I cell = MouseToCell(localMouse);

        if (controller.SelectionState.CurrentMode == BlackyTileAtlasSelectionState.SelectionMode.SingleArea)
        {
            if (controller.SelectionState.IsDragging)
            {
                controller.SelectionState.UpdateDragCell(cell);
                QueueRedraw();
            }
        }
        else
        {
            if (controller.SelectionState.ShouldStartDrag(localMouse))
            {
                controller.SelectionState.BeginDrag();
            }

            if (controller.SelectionState.IsDragging)
            {
                controller.SelectionState.UpdateDragCell(cell);

                Rect2I bounds = controller.SelectionState.GetCurrentCellBounds();
                List<Vector2I> cells = BuildCellsFromBounds(bounds);

                controller.SelectionState.ReplaceMultiSelection(cells);
                QueueRedraw();
            }
        }
    }

    private void HandleRightMouse(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouse &&
            mouse.ButtonIndex == MouseButton.Right &&
            mouse.Pressed)
        {
            controller.ClearSelection();
            QueueRedraw();
        }
    }

    private bool IsInsideAtlas(Vector2 localPos)
    {
        Vector2 size = controller.Context.TextureSize;
        Vector2 start = -size / 2f;
        return localPos.X >= start.X &&
               localPos.Y >= start.Y &&
               localPos.X < start.X + size.X &&
               localPos.Y < start.Y + size.Y;
    }

    private Vector2I MouseToCell(Vector2 localPos)
    {
        Vector2 size = controller.Context.TextureSize;
        Vector2 start = -size / 2f;

        Vector2 corrected = localPos - start;

        int x = Mathf.FloorToInt(corrected.X / controller.Context.CellSize.X);
        int y = Mathf.FloorToInt(corrected.Y / controller.Context.CellSize.Y);

        x = Mathf.Clamp(x, 0, controller.Context.Columns - 1);
        y = Mathf.Clamp(y, 0, controller.Context.Rows - 1);

        return new Vector2I(x, y);
    }

    private List<Vector2I> BuildCellsFromBounds(Rect2I bounds)
    {
        List<Vector2I> result = new List<Vector2I>();

        for (int y = bounds.Position.Y; y < bounds.Position.Y + bounds.Size.Y; y++)
        {
            for (int x = bounds.Position.X; x < bounds.Position.X + bounds.Size.X; x++)
            {
                result.Add(new Vector2I(x, y));
            }
        }

        return result;
    }

    public void RefreshLineWidth(float zoom)
    {
        LineWidth = Mathf.Clamp(1f / zoom, 1f, 20f);
        QueueRedraw();
    }
}