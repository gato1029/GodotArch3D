using Godot;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTextures;

public class BlackyTileAtlasInteractionController
{
    public BlackyTileAtlasContext Context { get; private set; }
    public BlackyTileAtlasSelectionState SelectionState { get; private set; }

    public BlackyAtlasOccupancyMap OccupancyMap { get; private set; } = new BlackyAtlasOccupancyMap();

    public event Action<TileSelectionMatrixData, int> OnNotifySelectionMatrix;
    public event Action<List<int>> OnNotifyMultiSelectionIndex;

    public BlackyTileAtlasInteractionController()
    {
        Context = new BlackyTileAtlasContext();
        SelectionState = new BlackyTileAtlasSelectionState();
    }

    public void SetTexture(Texture2D texture, Vector2I cellSize, int idMaterial,string idMod="VACIO")
    {
        
        SelectionState.Clear();
        Context.SetTexture(texture, cellSize, idMaterial, idMod);
        OccupancyMap.Build(texture, Context.CellSize);
       
    }

public void SetCellSize(int x, int y)
{
    Context.SetCellSize(x, y);

    if (Context.HasTexture)
        OccupancyMap.Build(Context.AtlasTexture, Context.CellSize);
}

    public void SetSelectionMode(BlackyTileAtlasSelectionState.SelectionMode mode)
    {
        SelectionState.CurrentMode = mode;
        SelectionState.ClearMultiSelection();
    }

    public void UpdateSingleAreaSelection(Vector2I startCell, Vector2I endCell)
    {
        SelectionState.DragStartCell = startCell;
        SelectionState.DragEndCell = endCell;

        SelectionState.CurrentMatrix = BlackyTileMatrixBuilder.Build(Context, OccupancyMap,startCell, endCell);

        OnNotifySelectionMatrix?.Invoke(SelectionState.CurrentMatrix, Context.IdMaterial);
    }

    public void UpdateMultiSelection(List<Vector2I> cells)
    {
        SelectionState.ReplaceMultiSelection(cells);

        List<int> indices = SelectionState.BuildSelectedIndices(Context);

        OnNotifyMultiSelectionIndex?.Invoke(indices);
    }

    public void ToggleMultiSelection(Vector2I cell, bool additive)
    {
        SelectionState.ToggleMultiCell(cell, additive);

        List<int> indices = SelectionState.BuildSelectedIndices(Context);

        OnNotifyMultiSelectionIndex?.Invoke(indices);
    }
    public void ClearAll()
    {
        Context.Clear();
        SelectionState.Clear();
        OccupancyMap.Clear();
    }
    public void ClearSelection()
    {
        SelectionState.Clear();
    }

    public TileSelectionMatrixData GetCurrentMatrix()
    {
        return SelectionState.CurrentMatrix;
    }

    public List<int> GetCurrentMultiIndices()
    {
        return SelectionState.BuildSelectedIndices(Context);
    }

    public Vector2I IndexToCell(int index)
    {
        return Context.IndexToCell(index);
    }

    public int CellToIndex(Vector2I cell)
    {
        return Context.CellToIndex(cell);
    }
}