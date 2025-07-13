using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TileData = GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileData;

public partial class WindowTerrain : Window, IFacadeWindow<TerrainData>
{
    TerrainData terrainData;

    Button buscarButton;
    OptionButton optionMaterial;
    ItemList itemListTiles;
    
    

    Sprite2D spriteSelection;
    LineEdit nameLine;
    SpinBox idBaseSpin;

    int currentTile;
    int currentRule;

    bool isRule = false;

    CollisionShape2D collisionBody;
    
    public event IFacadeWindow<TerrainData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

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

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/Button2").Pressed += button_SearchRule;

        GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/Button4").Pressed += button_Dinamic;
       
  
        terrainData = new TerrainData();
        this.CloseRequested += WindowTerrain_CloseRequested;
    }

    private void button_Dinamic()
    {
        FacadeWindowDataSearch<TileDynamicData> windowDinamic = new FacadeWindowDataSearch<TileDynamicData>("res://sources/WindowsDataBase/TileCreator/WindowTiles.tscn", this, WindowType.SELECTED);     
        windowDinamic.OnNotifySelected += WindowDinamic_OnNotifySelected;
    }

    private void WindowDinamic_OnNotifySelected(TileDynamicData objectSelected)
    {
        isRule = false;
        var data = objectSelected;
        spriteSelection.Texture = data.textureVisual;
        currentTile = objectSelected.id;
        if (data.haveCollider)
        {
            Rectangle rect = (Rectangle)data.collisionBody;
            collisionBody.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
            var shape = (RectangleShape2D)collisionBody.Shape;
            shape.Size = new Vector2((float)rect.widthPixel, (float)rect.heightPixel);
        }
    }



    private void button_SearchRule()
    {
        FacadeWindowDataSearch<AutoTileData> windowRule = new FacadeWindowDataSearch<AutoTileData>("res://sources/WindowsDataBase/TileCreator/windowAutoTile.tscn", this, WindowType.SELECTED);
 
        windowRule.OnNotifySelected += WindowRule_OnNotifySelected;
    }

    private void WindowRule_OnNotifySelected(AutoTileData objectSelected)
    {
        isRule = true;
        var data = objectSelected;
        spriteSelection.Texture = data.textureVisual;

        currentRule = objectSelected.id ;
    }

    private void Win_OnRequestSelectedItemRule(int id)
    {
        isRule = true;
        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.AutoTileData>(id);
        spriteSelection.Texture = data.textureVisual;
        
        currentRule = id;
    }
     private void button_SearchAnimated()
    {

        FacadeWindowDataSearch<TileAnimateData> windowTileAnimateData = new FacadeWindowDataSearch<TileAnimateData>("res://sources/WindowsDataBase/TileCreator/WindowAnimatedTiles.tscn", this, WindowType.SELECTED);
        windowTileAnimateData.OnNotifySelected += WindowTileAnimateData_OnNotifySelected;
    }

    private void WindowTileAnimateData_OnNotifySelected(TileAnimateData objectSelected)
    {
        isRule = false;
        var data = objectSelected;
        spriteSelection.Texture = data.textureVisual;
        currentTile = objectSelected.id;
        if (data.haveCollider)
        {
            Rectangle rect = (Rectangle)data.collisionBody;

            collisionBody.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
            var shape = (RectangleShape2D)collisionBody.Shape;
            shape.Size = new Vector2((float)rect.widthPixel, (float)rect.heightPixel);
        }
    }



    private void WindowTerrain_CloseRequested()
    {
        QueueFree();
    }

   

    
    private void Win_OnRequestSelectedItem(int id)
    {
        isRule = false;
        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileSimpleData>(id);
        spriteSelection.Texture = data.textureVisual;
        currentTile = id;
        if (data.haveCollider)
        {
            Rectangle rect = (Rectangle)data.collisionBody;
            collisionBody.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
            var shape = (RectangleShape2D)collisionBody.Shape;
            shape.Size = new Vector2((float)rect.widthPixel, (float)rect.heightPixel);
        }
        

    }
    private void button_Copy()
    {
        terrainData.id = 0;
        terrainData.name = nameLine.Text + "_Copy";
        DataBaseManager.Instance.InsertUpdate(terrainData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }
    private void button_Save()
    {
        terrainData.id = (int)idBaseSpin.Value;
        terrainData.name = nameLine.Text;
        if (isRule)
        {
            terrainData.idRule = currentRule;
            terrainData.idTile = 0;
        }
        else
        {
            terrainData.idTile = currentTile;
            terrainData.idRule = 0;
        }
        
        
        terrainData.isRule = isRule;

        DataBaseManager.Instance.InsertUpdate(terrainData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }

    private void bodyValueChanged(double value)
    {
  
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
    public void SetData(TerrainData data)
    {
        terrainData = data;

        idBaseSpin.Value = terrainData.id;
        nameLine.Text = terrainData.name;
        if (terrainData.isRule)
        {
            var tileData = DataBaseManager.Instance.FindById<AutoTileData>(terrainData.idRule);
            WindowRule_OnNotifySelected(tileData);
            
        }
        else
        {
            var tileData = DataBaseManager.Instance.FindById<TileData>(terrainData.idTile);
            
         
            if (tileData.type == "TileDynamicData")
            {
                var dataInternal = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileDynamicData>(terrainData.idTile);
                WindowDinamic_OnNotifySelected(dataInternal);
            }
            if (tileData.type == "TileAnimateData")
            {
                var dataInternal = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileAnimateData>(terrainData.idTile);
                WindowTileAnimateData_OnNotifySelected(dataInternal);
                
            }


        }        
    }
    public void LoadData(int id)
    {
                    
    }

 
}
