using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;

public partial class WindowTerrain : Window, IDetailWindow
{
    TerrainData terrainData;

    Button buscarButton;
    OptionButton optionMaterial;
    ItemList itemListTiles;
    
    

    Sprite2D spriteSelection;
    LineEdit nameLine;
    SpinBox idBaseSpin;

  

    CollisionShape2D collisionBody;
    public event IDetailWindow.RequestUpdateHandler OnRequestUpdate;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        spriteSelection = GetNode<Sprite2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/Control/CenterContainer/Sprite2D");
        buscarButton = GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/Button");
        collisionBody = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/Control/CenterContainer/Sprite2D/CollisionBody");
        
    
        idBaseSpin = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/GridContainer/SpinBox");
        nameLine = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/GridContainer/LineEdit");

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/HBoxContainer/Button").Pressed += button_Save;

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/Button3").Pressed += button_SearchAnimated;

        //GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/HBoxContainer/Button").Pressed += button_Save;

        buscarButton.Pressed += button_Search;
  
        terrainData = new TerrainData();
        this.CloseRequested += WindowTerrain_CloseRequested;
    }

    private void button_SearchAnimated()
    {
        WindowDataGeneric win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
        AddChild(win);
        win.Show();
        PackedScene ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileAnimate.tscn");
        win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.SELECTOR,"Tile Animado");
        win.SetLoaddBAction(() =>
        {
            var collection = DataBaseManager.Instance.FindAll<TileAnimateData>();
            List<IdData> ids = new List<IdData>();
            foreach (var item in collection)
            {
                IdData iddata = item;
                ids.Add(iddata);
            }
            return ids;
        }
        );
        win.OnRequestSelectedItem += Win_OnRequestSelectedItemAnimated;
    }

    private void Win_OnRequestSelectedItemAnimated(int id)
    {
        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileAnimateData>(id);
        spriteSelection.Texture = data.textureVisual;
        currentTile = id;
        Rectangle rect = (Rectangle)data.collisionBody;

        collisionBody.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
        var shape = (RectangleShape2D)collisionBody.Shape;
        shape.Size = new Vector2((float)rect.widthPixel, (float)rect.heightPixel);
    }

    private void WindowTerrain_CloseRequested()
    {
        QueueFree();
    }

    private void button_Search()
    {
        WindowDataGeneric win = GD.Load<PackedScene>("res://sources/WindowsDataBase/Generic/windowDataGeneric.tscn").Instantiate<WindowDataGeneric>();
        AddChild(win);
        win.Show();
        PackedScene ps = GD.Load<PackedScene>("res://sources/WindowsDataBase/TileCreator/windowTileSimple.tscn");
        win.SetWindowDetail(ps, GodotEcsArch.sources.utils.WindowState.SELECTOR,"Tile Simple");
        win.SetLoaddBAction(() =>
        {
            var collection = DataBaseManager.Instance.FindAll<TileSimpleData>();
            List<IdData> ids = new List<IdData>();
            foreach (var item in collection)
            {
                IdData iddata = item;
                ids.Add(iddata);
            }
            return ids;
        }
        );
        win.OnRequestSelectedItem += Win_OnRequestSelectedItem;
    }

    int currentTile;
    private void Win_OnRequestSelectedItem(int id)
    {
        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileSimpleData>(id);
        spriteSelection.Texture = data.textureVisual;
        currentTile = id;
        Rectangle rect = (Rectangle)data.collisionBody;

        collisionBody.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
        var shape = (RectangleShape2D)collisionBody.Shape;
        shape.Size = new Vector2((float)rect.widthPixel, (float)rect.heightPixel);

    }
    private void button_Copy()
    {
        terrainData.id = 0;
        terrainData.name = nameLine.Text + "_Copy";
        DataBaseManager.Instance.InsertUpdate(terrainData);
        OnRequestUpdate?.Invoke();
        QueueFree();
    }
    private void button_Save()
    {
        terrainData.id = (int)idBaseSpin.Value;
        terrainData.name = nameLine.Text;
        terrainData.idTile = currentTile;

        DataBaseManager.Instance.InsertUpdate(terrainData); 
        OnRequestUpdate?.Invoke();
        QueueFree();
    }

    private void bodyValueChanged(double value)
    {
  
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void LoadData(int id)
    {
        terrainData = DataBaseManager.Instance.FindById<TerrainData>(id);

        idBaseSpin.Value = terrainData.id;
        nameLine.Text  = terrainData.name;
        Win_OnRequestSelectedItem(terrainData.idTile);

     
    }
}
