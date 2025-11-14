using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;

public partial class ControlGrouping : MarginContainer
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        PanelBase.GuiInput += _GuiInputPanel;
    }

    GroupingData objectData;
    public void SetIdGrouping(long idGrouping)
	{
        objectData = MasterDataManager.GetData<GroupingData>(idGrouping);
        if (objectData != null)
        {
            TextureImage.Texture = objectData.textureVisual;
        }
        else
        {
            TextureImage.Texture = null;
        }

    }
    public void SetData(GroupingData data)
    {
        objectData = data;
        if (objectData != null)
        {
            TextureImage.Texture = objectData.textureVisual;
            LabelName.Text = objectData.name;
        }
        else
        {
            TextureImage.Texture = null;
        }
    }
    public long GetIdGrouping()
	{	
		return objectData != null ? objectData.id : -1;
    }
    public GroupingData GetData()
    {
        return objectData;
    }

    WindowDataSearch windowLocal = null;
    public void _GuiInputPanel(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    if (windowLocal == null)
                    {
                        FacadeWindowDataSearch<GroupingData> windowTile = new FacadeWindowDataSearch<GroupingData>("res://sources/WindowsDataBase/TileSprite/WindowTileSprite.tscn", this, WindowType.SELECTED, true, true);
                        windowTile.EnableFilterGrouping(false);
                        windowLocal = windowTile.windowDataSearch;
                        windowLocal.TreeExited += () => windowLocal = null;                        
                        // 🔸 Colocar al costado derecho del Control
                        Vector2I globalPos = (Vector2I)GetScreenPosition();
                        Vector2I size = (Vector2I)GetGlobalRect().Size;
                        // Si la ventana tiene tamaño fijo:
                        windowLocal.Position = new Vector2I(globalPos.X + size.X + 10, globalPos.Y);
                        windowTile.OnNotifySelected += WindowTile_OnNotifySelected;
                        windowLocal.Popup();
                
                    }
                    break;
                case MouseButton.Right:
                    break;
                case MouseButton.Middle:

                    break;
            }
        }
    }

    private void WindowTile_OnNotifySelected(GroupingData objectSelected)
    {
        objectData = objectSelected;
        TextureImage.Texture = objectSelected.textureVisual;
        LabelName.Text = objectSelected.name;
        OnNotifyChangued?.Invoke(this);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
