using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Resources;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using static Flecs.NET.Core.Ecs.Units;

public partial class ControlEditorResourceSources : MarginContainer
{
    // Called when the node enters the scene tree for the first time.
    private ResourceSourceData objectSelected;
    private ResourceSourceMap mapBase;
    TileSpriteData dataTileSpriteSelected;
    int indexTileSpriteSelected;
    Vector2I sizeMap = Vector2I.Zero;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI    
        // Configuración inicial del preview
        PlacementPreview.Instance.Configure(tileId: 1, layer: 20);
       // SelectionBlueprint.Instance.Configure(1, 30);
        // Configuración inicial del blueprint
        SelectionBlueprint.Instance.Create(
            size: new Vector2I(1, 1),
            centerTile: Vector2I.Zero
        );


        ButtonAutomatic.Pressed += ButtonAutomatic_Pressed; ;

        MapManagerEditor.Instance.editorMode = EditorMode.RECURSOS;
        ItemListData.ItemSelected += ItemListData_ItemSelected; ;
        ButtonRefresh.Pressed += ButtonRefresh_Pressed;

        LoadItems();

        MapManagerEditor.Instance.OnMapLevelDataChanged += Instance_OnMapLevelDataChanged;
        ButtonSeedRandom.Pressed += ButtonSeedRandom_Pressed;

    }

    private void ButtonSeedRandom_Pressed()
    {
        throw new NotImplementedException();
    }
    BlackyWorld blackyWorldMap;
    private void Instance_OnMapLevelDataChanged(MapLevelData data)
    {
        mapBase = data.resourceSourceMap;
        blackyWorldMap = data.blackyWorldMap;
        PlacementPreview.Instance.ConfigureFlecs(blackyWorldMap.flecsManager);
        SelectionBlueprint.Instance.ConfigureFlecs(blackyWorldMap.flecsManager);
    }

    private void LoadItems()
    {
        ItemListData.Clear();
        List<ResourceSourceData> result = new List<ResourceSourceData>();

        result = DataBaseManager.Instance.FindAll<ResourceSourceData>();

        for (int i = 0; i < result.Count; i++)
        {
            ResourceSourceData item = result[i];
            AtlasTexture atlasTexture = item.textureVisual;
            ItemListData.AddItem(item.name, atlasTexture);
            ItemListData.SetItemMetadata(i, item.id);
        }
    }

    private void ButtonRefresh_Pressed()
    {
        LoadItems();
    }

    private void ButtonAutomatic_Pressed()
    {
        //ResourceSourceGenerator resourceSourceGenerator = new ResourceSourceGenerator(MapManagerEditor.Instance.CurrentMapLevelData);
        //resourceSourceGenerator.Create();
    }

    private void ItemListData_ItemSelected(long index)
    {
        long idData = (long)ItemListData.GetItemMetadata((int)index);
        this.objectSelected = MasterDataManager.GetData<ResourceSourceData>(idData);
        TextureRectImage.Texture = objectSelected.textureVisual;

         indexTileSpriteSelected = GD.RandRange(0, objectSelected.listIdTileSpriteData.Count - 1);
        dataTileSpriteSelected = MasterDataManager.GetData<TileSpriteData>(objectSelected.listIdTileSpriteData[indexTileSpriteSelected]);

        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, dataTileSpriteSelected);
        ClearTilesOcupancy();
        DrawTilesOcupancy(dataTileSpriteSelected.tilesOcupancy);
    }

    public override void _Input(InputEvent @event)
    {
        if (mapBase == null || !MapManagerEditor.Instance.enableEditor)
            return;

        if (MapManagerEditor.Instance.editorMode != EditorMode.RECURSOS)
            return;

        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        // Mueve blueprint y preview
        SelectionBlueprint.Instance.Move(mouseTile);
        PlacementPreview.Instance.Move(mouseTile);
        MoveTilesOcupancy();
        // Mostrar preview si hay objeto seleccionado
        if (objectSelected != null)
        {
            // Crear preview si cambio dimencion
            if (PlacementPreview.Instance.Size != SelectionBlueprint.Instance.Size) {
                PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, dataTileSpriteSelected);
            }
               
        }

        // Pintar o borrar tiles
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            ApplyPaint();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
            ApplyErase();
    }

    List<(int,KuroTile)> idsTiles = new List<(int, KuroTile)>();
    private void DrawTilesOcupancy(List<KuroTile> tiles)
    {
        foreach (var item in tiles)
        {
            Vector2 posTile = TilesHelper.WorldPositionTile(new Vector2I(item.x, item.y));
            int id = WireShape.Instance.DrawFilledSquare(new Vector2(16, 16), posTile, 1, Godot.Colors.Blue, .5f);
           idsTiles.Add((id,item));
        }           
    }

    private void MoveTilesOcupancy()
    {
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        for (int i = 0; i < idsTiles.Count; i++)
        {
            var index = idsTiles[i].Item1;
            var tile = idsTiles[i].Item2;
            Vector2 posTile = TilesHelper.WorldPositionTile(new Vector2I(tile.x, tile.y)+ mouseTile);
            WireShape.Instance.UpdatePosition(index, posTile);            
        }
    }
    private void ClearTilesOcupancy()
    {
        foreach(var tile in idsTiles)
        {
            WireShape.Instance.FreeShape(tile.Item1);
        }
        idsTiles.Clear();
    }
    private void ApplyPaint()
    {
        if (objectSelected != null && objectSelected.id != 0)
        {
            Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

            blackyWorldMap.Resources.Create(objectSelected.idSave, mouseTile, true);

            //mapBase.AddUpdateResource(mouseTile, objectSelected.id, indexTileSpriteSelected);

        }        
    }

    private void ApplyErase()
    {
        if (objectSelected != null && objectSelected.id != 0)
        {
            Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
           blackyWorldMap.Resources.remove(mouseTile);
            //mapBase.RemoveResource(mouseTile);
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
