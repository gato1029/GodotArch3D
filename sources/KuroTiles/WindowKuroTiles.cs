using Godot;
using System;
using System.Collections.Generic;

public partial class WindowKuroTiles : MarginContainer
{
    // Called when the node enters the scene tree for the first time.

    public delegate void EventNotifySelection(float x, float y, float width, float height);
    public event EventNotifySelection OnNotifySelection;

    public delegate void EventNotifyMultiSelection(List<TileInfoKuro> tiles);
    public event EventNotifyMultiSelection OnNotifyMultiSelection;
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        Grid.OnNotifySelection += Grid_OnNotifySelection;
        Grid.OnNotifyMultiSelection += Grid_OnNotifyMultiSelection;
        SubViewport.OnNotifySelectionCameraZoom += SubViewport_OnNotifySelectionCameraZoom;
        
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
    Vector2 sizeTexture= Vector2.Zero;
    public void SetTexture(Texture2D texture, int id)
    {
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
    internal void SetSelection(float x, float y, float width, float height)
    {
        Grid.SetSelection(x, y, width, height);
        Vector2 selectionCenter = new Vector2(x + width / 2f, y + height / 2f);
        Vector2 textureOffset = sizeTexture / 2f;

        // Posición final donde queremos mover la cámara
        Vector2 targetPosition = selectionCenter - textureOffset;

        GD.Print("Selection Center: " + selectionCenter + " | Target: " + targetPosition);

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
