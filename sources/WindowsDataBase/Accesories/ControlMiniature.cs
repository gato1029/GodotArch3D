using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class ControlMiniature : PanelContainer
{

    SpriteData objectData;
    

    public SpriteData ObjectData { get => objectData; set => objectData = value; }

    public void SetData(SpriteData pObjectData)
    {
        if (pObjectData!=null)
        {
            objectData = pObjectData;
            var atlas = MaterialManager.Instance.GetAtlasTexture(objectData.idMaterial, objectData.x, objectData.y, objectData.widht, objectData.height);
            TextureRectImage.Texture = atlas;
        }
        
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		InitializeUI();
        
        ButtonSelect.Pressed += ButtonSelect_Pressed;
	}
   

    private void ButtonSelect_Pressed()
    {
        var windowMini = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/WindowMiniature.tscn").Instantiate<WindowMiniature>();
        windowMini.Show();
        AddChild(windowMini);
        if (objectData != null)
        {
            windowMini.SetData(objectData);
        }
        windowMini.OnNotifyChangued += WindowMini_OnNotifyChangued;
        
    }

    private void WindowMini_OnNotifyChangued(WindowMiniature objectControl)
    {
        objectData = objectControl.ObjectData;
        SetData(objectData);    
    }
}
