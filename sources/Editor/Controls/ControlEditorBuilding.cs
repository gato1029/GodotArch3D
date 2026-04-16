using Godot;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.Flecs.Creators;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Buildings;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using System;
using System.Collections.Generic;

public partial class ControlEditorBuilding : MarginContainer
{
    // Called when the node enters the scene tree for the first time.
    private BuildingData objectSelected;
    private MapBuildings mapBase;
    Vector2I sizeMap = Vector2I.Zero;
    public override void _Ready()
    {
        InitializeUI();
        // Configuración inicial del preview
        PlacementPreview.Instance.Configure(tileId: 1, layer: 20);
   
        // Configuración inicial del blueprint
        SelectionBlueprint.Instance.Create(
            size: new Vector2I(1, 1),
            centerTile: Vector2I.Zero
        );




        MapManagerEditor.Instance.editorMode = EditorMode.EDIFICIOS;
        ItemListData.ItemSelected += ItemListData_ItemSelected;
        ButtonRefresh.Pressed += ButtonRefresh_Pressed;

        LoadItems();

        MapManagerEditor.Instance.OnMapLevelDataChanged += Instance_OnMapLevelDataChanged;


    }

    BlackyWorld blackyWorldMap;
    private void Instance_OnMapLevelDataChanged(MapLevelData data)
    {
        mapBase = data.mapBuildings;
        blackyWorldMap = data.blackyWorldMap;
        PlacementPreview.Instance.ConfigureFlecs(blackyWorldMap.flecsManager);
        SelectionBlueprint.Instance.ConfigureFlecs(blackyWorldMap.flecsManager);
    }

    private void LoadItems()
    {
          ItemListData.Clear();
        List<BuildingData> result = new List<BuildingData>();
        //BsonExpression bsonExpression = null;


        //string expressionText = "isRule = @0";
        //bsonExpression = BsonExpression.Create(expressionText, true);
        result = DataBaseManager.Instance.FindAll<BuildingData>();

        for (int i = 0; i < result.Count; i++)
        {
            BuildingData item = result[i];
            AtlasTexture atlasTexture = item.textureVisual;
            ItemListData.AddItem(item.name, atlasTexture);
            ItemListData.SetItemMetadata(i, item.id);

        }

    }

    private void ButtonRefresh_Pressed()
    {
        LoadItems();
        if (objectSelected!=null)
        {
            var tileSprite = MasterDataManager.GetData<TileSpriteData>(objectSelected.idTileSpriteNormal);
            Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
            PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, tileSprite);
        }
        
    }



    private void ItemListData_ItemSelected(long index)
    {
        int idData = (int)ItemListData.GetItemMetadata((int)index);
        this.objectSelected = MasterDataManager.GetData<BuildingData>(idData);

        TextureRectImage.Texture = objectSelected.textureVisual;

        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        var tileSprite = MasterDataManager.GetData<TileSpriteData>( objectSelected.idTileSpriteNormal);

        PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, tileSprite);
        // PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, objectSelected.spriteData);
    }

    public override void _Input(InputEvent @event)
    {
        if (mapBase == null || !MapManagerEditor.Instance.enableEditor)
            return;

        if (MapManagerEditor.Instance.editorMode != EditorMode.EDIFICIOS)
            return;

        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        // Mueve blueprint y preview
        SelectionBlueprint.Instance.Move(mouseTile);
        PlacementPreview.Instance.Move(mouseTile);
        // Mostrar preview si hay objeto seleccionado
        if (objectSelected != null)
        {
            // Crear preview si cambio dimencion
            if (PlacementPreview.Instance.Size != SelectionBlueprint.Instance.Size) { }
               // PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, objectSelected.spriteData);
        }

        // Pintar o borrar tiles
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            ApplyPaint();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
            ApplyErase();
        
    }
    private void ApplyPaint()
    {
        if (objectSelected != null && objectSelected.id != 0)
        {
            Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
       
            blackyWorldMap.Building.Create(objectSelected.id, mouseTile,true);

            //GodotFlecs.sources.Flecs.Creators.BuildingCreator.Instance.Create(objectSelected.id,  mouseTile);
            //foreach (var (_, tilePos) in SelectionBlueprint.Instance.IterateWithTilePositions())
            //{
            //  //  GlobalData.PendingBuildingQueue.Enqueue((tilePos, objectSelected.id));
            //    mapBase.AddUpdateTile(tilePos, objectSelected.id);
            //}
        }
    }

    private void ApplyErase()
    {
        if (objectSelected != null && objectSelected.id != 0)
        {
            Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
            blackyWorldMap.Building.Remove(mouseTile);
            //GodotFlecs.sources.Flecs.Creators.BuildingCreator.Instance.RemoveTile(mouseTile);
        }
    }
}
