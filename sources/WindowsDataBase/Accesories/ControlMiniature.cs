using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class ControlMiniature : PanelContainer
{

    MiniatureData objectData;
    WindowViewDb windowViewDb;

    public MiniatureData ObjectData { get => objectData; set => objectData = value; }

    public void SetData(MiniatureData pObjectData)
    {
        objectData = pObjectData;
        if (pObjectData.idTile>0)
        {
            var data = DataBaseManager.Instance.FindById<TileDynamicData>(pObjectData.idTile);
            TextureRectImage.Texture = data.textureVisual;
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
        ButtonNew.Pressed += ButtonNew_Pressed;
        ButtonSelect.Pressed += ButtonSelect_Pressed;
	}
    private void ButtonNew_Pressed()
    {
        var newWindow = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileDinamic.tscn").Instantiate<WindowTileDinamic>();
        AddChild(newWindow);
        newWindow.Show();
    }

    private void ButtonSelect_Pressed()
    {
        windowViewDb = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowViewDB.tscn").Instantiate<WindowViewDb>();
        AddChild(windowViewDb);
        windowViewDb.Show();
        windowViewDb.WorkMode(true);
        windowViewDb.LoadItems<TileDynamicData>();
        windowViewDb.OnRequestSelectedItem += WindowViewDb_OnRequestSelectedItem;
        windowViewDb.OnRequestFilterMaterial += WindowViewDb_OnRequestFilterMaterial;
    }


    private void WindowViewDb_OnRequestFilterMaterial(int idMaterial)
    {
        windowViewDb.LoadItemsByMaterial<TileDynamicData>();
    }

    private void WindowViewDb_OnRequestSelectedItem(int id)
    {
        var data =DataBaseManager.Instance.FindById<TileDynamicData>(id);
        objectData.idTile = data.id;
        objectData.idMaterial = data.idMaterial;
        TextureRectImage.Texture = data.textureVisual;
    }
}
