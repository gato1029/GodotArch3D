using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using System;

public partial class ControlProyectile : MarginContainer
{
    int id = 0;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        PanelBase.GuiInput += _GuiInputPanel;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void _GuiInputPanel(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            switch (mouseEvent.ButtonIndex)
            {
                case MouseButton.Left:
                    FacadeWindowDataSearch<BulletData> windowTile = new FacadeWindowDataSearch<BulletData>("res://sources/WindowsDataBase/Projectile/WindowProjectile.tscn", this, WindowType.SELECTED, true, true);                    
                    windowTile.OnNotifySelected += WindowTile_OnNotifySelected;
                    break;
            }
        }
    }

    private void WindowTile_OnNotifySelected(BulletData objectSelected)
    {
        id = objectSelected.id;
        TextureImage.Texture = objectSelected.textureVisual;
    }



    public int GetData()
    {
        return id;
    }
    public void SetData(int idProyectile)
    {
        if (idProyectile==0)
        {
            return;
        }
        id = idProyectile;
        var data = MasterDataManager.GetData<BulletData>(idProyectile);
        TextureImage.Texture = data.textureVisual;
    }

}
