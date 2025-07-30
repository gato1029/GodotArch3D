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
                          
    int currentTile;
    int currentRule;

    bool isRule = false;

    
    
    public event IFacadeWindow<TerrainData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI                                             
        ButtonSave.Pressed += button_Save;
        ButtonSearchAnimate.Pressed += button_SearchAnimated;
        ButtonSearchAuto.Pressed+= button_SearchRule;
        ButtonSearchTile.Pressed += button_Dinamic;         
        terrainData = new TerrainData();        

        foreach (TerrainType tipo in Enum.GetValues(typeof(TerrainType)))
        {
            OptionButtonType.AddItem(tipo.ToString());
        }
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
        Sprite2DImage.Texture = data.textureVisual;
        currentTile = objectSelected.id;
        if (data.haveCollider)
        {
            Rectangle rect = (Rectangle)data.collisionBody;
            CollisionBodyCollider.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
            var shape = (RectangleShape2D)CollisionBodyCollider.Shape;
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
        Sprite2DImage.Texture = data.textureVisual;

        currentRule = objectSelected.id ;
    }

    private void Win_OnRequestSelectedItemRule(int id)
    {
        isRule = true;
        var data = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.AutoTileData>(id);
        Sprite2DImage.Texture = data.textureVisual;
        
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
        Sprite2DImage.Texture = data.textureVisual;
        currentTile = objectSelected.id;
        if (data.haveCollider)
        {
            Rectangle rect = (Rectangle)data.collisionBody;

            CollisionBodyCollider.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
            var shape = (RectangleShape2D)CollisionBodyCollider.Shape;
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
        Sprite2DImage.Texture = data.textureVisual;
        currentTile = id;
        if (data.haveCollider)
        {
            Rectangle rect = (Rectangle)data.collisionBody;
            CollisionBodyCollider.Position = new Vector2((float)rect.originPixelX, (float)rect.originPixelY * (-1));
            var shape = (RectangleShape2D)CollisionBodyCollider.Shape;
            shape.Size = new Vector2((float)rect.widthPixel, (float)rect.heightPixel);
        }
        

    }
    private void button_Copy()
    {
        terrainData.id = 0;
        terrainData.name = LineEditName.Text + "_Copy";
        terrainData.category = LineEditCategory.Text;
        terrainData.terrainType = (TerrainType)OptionButtonType.Selected;
        DataBaseManager.Instance.InsertUpdate(terrainData);
        OnNotifyChanguedSimple?.Invoke();
        QueueFree();
    }
    private void button_Save()
    {
        terrainData.id = (int)SpinBoxId.Value;
        terrainData.name = LineEditName.Text;

        
        terrainData.isRule = isRule;
        terrainData.category = LineEditCategory.Text;
        terrainData.terrainType = (TerrainType)OptionButtonType.Selected;
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
        //terrainData = data;

        //SpinBoxId.Value = terrainData.id;
        //LineEditName.Text = terrainData.name;
        //LineEditCategory.Text = terrainData.category;
        //OptionButtonType.Selected = (int)terrainData.terrainType;
        //if (terrainData.isRule)
        //{
        //    var tileData = DataBaseManager.Instance.FindById<AutoTileData>(terrainData.idRule);
        //    WindowRule_OnNotifySelected(tileData);
            
        //}
        //else
        //{
        //    var tileData = DataBaseManager.Instance.FindById<TileData>(terrainData.idTile);
            
         
        //    if (tileData.type == "TileDynamicData")
        //    {
        //        var dataInternal = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileDynamicData>(terrainData.idTile);
        //        WindowDinamic_OnNotifySelected(dataInternal);
        //    }
        //    if (tileData.type == "TileAnimateData")
        //    {
        //        var dataInternal = DataBaseManager.Instance.FindById<GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase.TileAnimateData>(terrainData.idTile);
        //        WindowTileAnimateData_OnNotifySelected(dataInternal);
                
        //    }


        //}        
    }
    public void LoadData(int id)
    {
                    
    }

 
}
