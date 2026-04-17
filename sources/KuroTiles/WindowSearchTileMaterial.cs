using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;

public partial class WindowSearchTileMaterial : Window
{
    public delegate void EventNotifySelection(MaterialData materialData,float x, float y, float width, float height);
    public event EventNotifySelection OnNotifySelection;

    public delegate void EventNotifyMultiSelection(List<TileInfoKuro> tiles);
    public event EventNotifyMultiSelection OnNotifyMultiSelection;

    public delegate void EventNotifySelectionIndex(int index, MaterialData materialData);
    public event EventNotifySelectionIndex OnNotifySelectionIndex;

    public delegate void EventNotifyMultiSelectionIndex(List<int> indices, MaterialData materialData);
    public event EventNotifyMultiSelectionIndex OnNotifyMultiSelectionIndex;

    private MaterialData materialData;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonSearch.Pressed += ButtonSearch_Pressed;
        ControlKuroTiles.OnNotifySelection += ControlKuroTiles_OnNotifySelection;
        ControlKuroTiles.OnNotifyMultiSelection += ControlKuroTiles_OnNotifyMultiSelection;
        ControlKuroTiles.OnNotifyMultiSelectionIndex += ControlKuroTiles_OnNotifyMultiSelectionIndex;
        ControlKuroTiles.OnNotifySelectionIndex += ControlKuroTiles_OnNotifySelectionIndex;
        ControlKuroTiles.SetTextureEmpty();
        // 🔸 Conectar al evento global de entrada del viewport principal
        FocusExited += WindowSearchTileMaterial_FocusExited;
        CheckButtonKeepOpen.Pressed += CheckButtonKeepOpen_Pressed;
        
    }

    private void ControlKuroTiles_OnNotifySelectionIndex(int index)
    {
        OnNotifySelectionIndex?.Invoke(index, materialData);
    }

    private void ControlKuroTiles_OnNotifyMultiSelectionIndex(List<int> indices)
    {
        OnNotifyMultiSelectionIndex?.Invoke(indices, materialData);
    }

    public void SetMultipleSelectionMode(bool active)
    {
        ControlKuroTiles.SetModeSelection(active);
    }
    private void ControlKuroTiles_OnNotifyMultiSelection(List<TileInfoKuro> tiles)
    {
        keepOpen = false;
        OnNotifyMultiSelection?.Invoke(tiles);
    }

    private void CheckButtonKeepOpen_Pressed()
    {
        if (CheckButtonKeepOpen.ButtonPressed)
        {
            keepOpen = true;
        }
        else
        {
            keepOpen = false;
        }
    }

    bool keepOpen = false;
    private void WindowSearchTileMaterial_FocusExited()
    {
        if (CheckButtonKeepOpen.ButtonPressed)
        {
            return;
        }
        else
        {
            if (keepOpen)
            {
                return;
            }
            QueueFree();
        }
                       
    }

    public void SetSelection(int idMaterial, int indexTile)
    {
        MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
        this.materialData = materialData;
        LineEditName.Text = materialData.name;
        ControlKuroTiles.SetTileSize(materialData.divisionPixelX, materialData.divisionPixelY);
        ControlKuroTiles.SetTexture((Texture2D)materialData.textureMaterial, materialData.id);
        ControlKuroTiles.SetSelection(indexTile);
    }
    public void SetSelectionMultiple(int idMaterial, List<int> indicesTile)
    {
        MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
        this.materialData = materialData;
        LineEditName.Text = materialData.name;
        ControlKuroTiles.SetTileSize(materialData.divisionPixelX, materialData.divisionPixelY);
        ControlKuroTiles.SetTexture((Texture2D)materialData.textureMaterial, materialData.id);
        ControlKuroTiles.SetSelection(indicesTile);
    }

    public void SetSelection(int idMaterial,float x, float y, float width, float height)
    {
        MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
        this.materialData = materialData;
        LineEditName.Text = materialData.name;
        ControlKuroTiles.SetTileSize(materialData.divisionPixelX, materialData.divisionPixelY);
        ControlKuroTiles.SetTexture((Texture2D)materialData.textureMaterial,materialData.id);
        ControlKuroTiles.SetSelection(x, y, width, height);
        
    }
    public void SetSelectionMultiple(int idMaterial, List<TileInfoKuro> tiles)
    {
        MaterialData materialData = DataBaseManager.Instance.FindById<MaterialData>(idMaterial);
        this.materialData = materialData;
        LineEditName.Text = materialData.name;
        ControlKuroTiles.SetTileSize(materialData.divisionPixelX, materialData.divisionPixelY);
        ControlKuroTiles.SetTexture((Texture2D)materialData.textureMaterial, materialData.id);
        ControlKuroTiles.SetSelectionMultiple(tiles);
    }
    private void ControlKuroTiles_OnNotifySelection(float x, float y, float width, float height)
    {
        keepOpen = false;
        OnNotifySelection?.Invoke(materialData,x, y, width, height);
    }

    private void ButtonSearch_Pressed()
    {
        keepOpen = true;
        FacadeWindowDataSearch<MaterialData> windowQuery = new FacadeWindowDataSearch<MaterialData>("res://sources/WindowsDataBase/Materials/windowNewMaterial.tscn", this, WindowType.SELECTED,true);        
        windowQuery.OnNotifySelected += WindowQuery_OnNotifySelected;
    }

    private void WindowQuery_OnNotifySelected(MaterialData objectSelected)
    {
        
        materialData = objectSelected;
        LineEditName.Text = objectSelected.name;
        ControlKuroTiles.SetTexture((Texture2D)objectSelected.textureMaterial, materialData.id);
        ControlKuroTiles.CenterCamera();
        ControlKuroTiles.SetTileSize(objectSelected.divisionPixelX, objectSelected.divisionPixelY); 
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
