using Godot;
using GodotEcsArch.sources.BlackyTextures;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class ControlBlackyAtlasTexture : MarginContainer
{
    private BlackyTileAtlasInteractionController controller;

    public event Action<TileSelectionMatrixData, int> OnNotifySelectionMatrix;
    public event Action<List<int>> OnNotifyMultiSelectionIndex;

    public override void _Ready()
    {
        InitializeUI();

        controller = new BlackyTileAtlasInteractionController();

        AtlasCanvas.Initialize(controller,AtlasViewport);

        controller.OnNotifySelectionMatrix += Controller_OnNotifySelectionMatrix;
        controller.OnNotifyMultiSelectionIndex += Controller_OnNotifyMultiSelectionIndex;
 
    }

    private void Controller_OnNotifySelectionMatrix(TileSelectionMatrixData matrix, int idMaterial)
    {
        OnNotifySelectionMatrix?.Invoke(matrix, idMaterial);
    }

    private void Controller_OnNotifyMultiSelectionIndex(List<int> indices)
    {
        OnNotifyMultiSelectionIndex?.Invoke(indices);
    }

    public void SetTexture(Texture2D texture, Vector2I cellSize, int idMaterial,string idMod = "VACIO")
    {
        controller.SetTexture(texture, cellSize, idMaterial,idMod);

        ////if (texture.GetSize().X > 800 || texture.GetSize().Y > 800)
        ////    AtlasViewport.SetAbsoluteZoom(0.5f);
        ////else
        ////    AtlasViewport.SetAbsoluteZoom(1f);

        AtlasCanvas.QueueRedraw();
    }

    public void SetTileSize(int x, int y)
    {
        controller.SetCellSize(x, y);
        AtlasCanvas.QueueRedraw();
    }

    public void SetModeSelection(bool multiple)
    {
        controller.SetSelectionMode(
            multiple
                ? BlackyTileAtlasSelectionState.SelectionMode.MultiTile
                : BlackyTileAtlasSelectionState.SelectionMode.SingleArea
        );

        AtlasCanvas.QueueRedraw();
    }

    public void CenterCamera()
    {
        AtlasViewport.CenterCameraOnViewport();
    }

    public void SetSelection(int index)
    {
        Vector2I cell = controller.IndexToCell(index);

        controller.UpdateSingleAreaSelection(cell, cell);

        Vector2 worldPos = GetWorldCenterFromCell(cell);
        AtlasViewport.FocusWorldPosition(worldPos);

        AtlasCanvas.QueueRedraw();
    }

    public void SetSelection(List<int> indices)
    {
        List<Vector2I> cells = new List<Vector2I>();

        foreach (var index in indices)
            cells.Add(controller.IndexToCell(index));

        controller.UpdateMultiSelection(cells);

        AtlasCanvas.QueueRedraw();
    }
    public void SetSelection(float x, float y, float width, float height)
    {
        Vector2I start = new Vector2I(
            Mathf.FloorToInt(x / controller.Context.CellSize.X),
            Mathf.FloorToInt(y / controller.Context.CellSize.Y)
        );

        Vector2I end = new Vector2I(
            Mathf.FloorToInt((x + width - 1) / controller.Context.CellSize.X),
            Mathf.FloorToInt((y + height - 1) / controller.Context.CellSize.Y)
        );

        controller.UpdateSingleAreaSelection(start, end);

        Vector2 centerPx = new Vector2(x + width / 2f, y + height / 2f);
        Vector2 worldPos = centerPx - controller.Context.TextureSize / 2f;

        AtlasViewport.FocusWorldPosition(worldPos);

        AtlasCanvas.QueueRedraw();
    }
    private Vector2 GetWorldCenterFromCell(Vector2I cell)
    {
        Vector2I cellSize = controller.Context.CellSize;
        Vector2 textureSize = controller.Context.TextureSize;

        Vector2 drawStart = -textureSize / 2f;

        Vector2 center =
            drawStart +
            new Vector2(cell.X * cellSize.X, cell.Y * cellSize.Y) +
            (Vector2)cellSize / 2f;

        return center.Round();
    }
    public void SetTextureEmpty()
    {
        controller.ClearAll();

        AtlasViewport.CenterCameraOnViewport();
        AtlasViewport.SetAbsoluteZoom(1f);

        AtlasCanvas.QueueRedraw();
    }

    public TileSelectionMatrixData GetCurrentSelectionMatrix()
    {
        return controller.GetCurrentMatrix();
    }

    public List<int> GetCurrentMultiIndices()
    {
        return controller.GetCurrentMultiIndices();
    }

    public void ClearSelection()
    {
        controller.ClearSelection();
        AtlasCanvas.QueueRedraw();
    }
}
