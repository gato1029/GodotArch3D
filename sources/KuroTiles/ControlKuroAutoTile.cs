using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using System;
using static ControlKuroTile;

public partial class ControlKuroAutoTile : MarginContainer
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        TextEditSearch.GuiInput += _GuiInputPanel;
    }

    public AutoTileSpriteData objectData;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    FacadeWindowDataSearch<AutoTileSpriteData> windowLocal = null;
    public void _GuiInputPanel(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    if (windowLocal == null)
                    {
                        windowLocal = new FacadeWindowDataSearch<AutoTileSpriteData>(KuroWindowFactory.GetPath<WindowAutoTileSprite>(), this, WindowType.SELECTED, true);
                      //   = GD.Load<PackedScene>("res://sources/KuroTiles/WindowSearchTileMaterial.tscn").Instantiate<WindowSearchTileMaterial>();
                        // Escuchar cuando se cierre

                        //AddChild(windowLocal);

                        windowLocal.windowDataSearch.TreeExited += () => windowLocal = null;
                        windowLocal.windowDataSearch.CloseRequested += () => windowLocal = null;
                        // 🔸 Colocar al costado derecho del Control
                        Vector2I globalPos = (Vector2I)GetScreenPosition();
                        Vector2I size = (Vector2I)GetGlobalRect().Size;

                        // Si la ventana tiene tamaño fijo:
                        windowLocal.windowDataSearch.Position = new Vector2I(globalPos.X + size.X + 10, globalPos.Y);
                        windowLocal.OnNotifySelected += WindowLocal_OnNotifySelection;
                        windowLocal.windowDataSearch.Popup();
              
                    }
                    break;
                case MouseButton.Right:
                    break;
                case MouseButton.Middle:

                    break;
            }
        }
    }

    private void WindowLocal_OnNotifySelection(AutoTileSpriteData objectSelected)
    {
        TextEditSearch.Text = objectSelected.name;
        objectData = objectSelected;
        TextureImage.Texture = objectSelected.textureVisual;
        OnNotifyChangued?.Invoke(this);
    }
    public AutoTileSpriteData GetData()
    {
        return objectData;
    }
    internal void SetData(AutoTileSpriteData data)
    {
        objectData = data;
        TextEditSearch.Text = data.name;
        TextureImage.Texture = data.textureVisual;
    }
    internal void SetData(long iddata)
    {
        if (iddata==0)
        {
            return;
        }
        objectData =MasterDataManager.GetData<AutoTileSpriteData>(iddata);           
        TextEditSearch.Text = objectData.name;
        TextureImage.Texture = objectData.textureVisual;
    }
}
