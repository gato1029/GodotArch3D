using Godot;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;

public partial class WindowKuroTiles : MarginContainer
{
    // Called when the node enters the scene tree for the first time.

    Vector2 sizeTexture = Vector2.Zero;
    int idCurrentMaterial = -1;
    public delegate void EventNotifySelection(float x, float y, float width, float height);
    public event EventNotifySelection OnNotifySelection;

    public delegate void EventNotifyMultiSelection(List<TileInfoKuro> tiles);
    public event EventNotifyMultiSelection OnNotifyMultiSelection;

    public delegate void EventNotifySelectionIndex(int index, int idMaterial);
    public event EventNotifySelectionIndex OnNotifySelectionIndex;

    public delegate void EventNotifyMultiSelectionIndex(List<int> indices);
    public event EventNotifyMultiSelectionIndex OnNotifyMultiSelectionIndex;

    public delegate void EventNotifySelectionMatrix(TileSelectionMatrixData matrix, int idMaterial);
    public event EventNotifySelectionMatrix OnNotifySelectionMatrix;

    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        Grid.OnNotifySelection += Grid_OnNotifySelection;
        Grid.OnNotifyMultiSelection += Grid_OnNotifyMultiSelection;
        Grid.OnNotifySelectionIndex += Grid_OnNotifySelectionIndex;
        Grid.OnNotifyMultiSelectionIndex += Grid_OnNotifyMultiSelectionIndex;
        Grid.OnNotifySelectionMatrix += Grid_OnNotifySelectionMatrix;
        SubViewport.OnNotifySelectionCameraZoom += SubViewport_OnNotifySelectionCameraZoom;
        
    }

    private void Grid_OnNotifySelectionMatrix(TileSelectionMatrixData matrix)
    {
       OnNotifySelectionMatrix?.Invoke(matrix, idCurrentMaterial);
    }

    private void Grid_OnNotifyMultiSelectionIndex(List<int> indices)
    {
        OnNotifyMultiSelectionIndex?.Invoke(indices);
    }

    private void Grid_OnNotifySelectionIndex(int index)
    {
        OnNotifySelectionIndex?.Invoke(index, idCurrentMaterial);
    }

    private void Grid_OnNotifyMultiSelection(List<TileInfoKuro> tiles)
    {
        OnNotifyMultiSelection?.Invoke(tiles);
    }

    private void SubViewport_OnNotifySelectionCameraZoom(float zoom)
    {
        Grid.SetSizeLineGrid(zoom);
    }

    private void Grid_OnNotifySelection(float x, float y, float width, float height)
    {
        OnNotifySelection?.Invoke(x, y, width, height);
    }
    internal void SetTextureEmpty()
    {
        Grid.SetTextureEmpty(); 
    }
 
    public void SetTexture(Texture2D texture, int id)
    {
        if (idCurrentMaterial==id)
        {
            return;
        }
        idCurrentMaterial = id;
        sizeTexture = texture.GetSize();
        if (texture.GetSize().X > 800 || texture.GetSize().Y > 800)
        {
            SubViewport.ZoomAtScreenCenter(0.5f); // Ajustar zoom al máximo
        }
        else
        {
            SubViewport.ZoomAtScreenCenter(1); // Ajustar zoom al máximo
        }

        Grid.SetTexture(texture,id);
    }
    public void SetTexture(TextureRect texture,int id)
    {
        if (texture.Size.X>800 || texture.Size.Y>800)
        {
            SubViewport.ZoomAtScreenCenter(0.5f); // Ajustar zoom al máximo
        }
        else
        {
            SubViewport.ZoomAtScreenCenter(1); // Ajustar zoom al máximo
        }
        
        Grid.SetTexture(texture,id);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}

    public void CenterCamera()
    {
        SubViewport.CenterCameraOnViewport();
    }

    public void SetTileSize(int x, int y)
    {
        Grid.SetSizeCell(x, y);
    }
    public void SetSelection(int index)
    {
        Grid.SetSelection(index);
        Vector2 globalPos = Grid.GetGlobalPositionFromIndex(index);

        SubViewport.SetCameraPosition(globalPos.X, globalPos.Y);
    }
    public void SetSelection(List<int> indices)
    {
        Grid.SetSelection(indices);
    }

    internal void SetSelection(float x, float y, float width, float height)
    {
        Grid.SetSelection(x, y, width, height);
        Vector2 selectionCenter = new Vector2(x + width / 2f, y + height / 2f);
        Vector2 textureOffset = sizeTexture / 2f;

        // Posición final donde queremos mover la cámara
        Vector2 targetPosition = selectionCenter - textureOffset;

      

        // Mover la cámara del SubViewport
        SubViewport.SetCameraPosition(targetPosition.X, targetPosition.Y);
    }

    public void SetModeSelection(bool multiple)
    {
        if (multiple)
        {
            Grid.selectionMode = GodotFlecs.sources.KuroTiles.TileGridNode2d.SelectionMode.MultiTile;
        }
        else
        {
            Grid.selectionMode = GodotFlecs.sources.KuroTiles.TileGridNode2d.SelectionMode.SingleArea;
        }
    }
    public List<TileInfoKuro> GetTileSelection()
    {
        return  Grid.GetSelectedFrames();
    }

    internal void SetSelectionMultiple(List<TileInfoKuro> tiles)
    {
       Grid.SetSelection(tiles);
    }
}
