using Godot;
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

public partial class ControlEditorResourceSources : MarginContainer
{
    // Called when the node enters the scene tree for the first time.
    private ResourceSourceData objectSelected;
    private ResourceSourceMap mapBase;
    Vector2I sizeMap = Vector2I.Zero;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI    
        // Configuración inicial del preview
        PlacementPreview.Instance.Configure(tileId: 1, layer: 20);
        SelectionBlueprint.Instance.Configure(1, 30);
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

    private void Instance_OnMapLevelDataChanged(MapLevelData data)
    {
        mapBase = data.resourceSourceMap;
    }

    private void LoadItems()
    {
        ItemListData.Clear();
        List<ResourceSourceData> result = new List<ResourceSourceData>();
        //BsonExpression bsonExpression = null;


        //string expressionText = "isRule = @0";
        //bsonExpression = BsonExpression.Create(expressionText, true);
        result = DataBaseManager.Instance.FindAll<ResourceSourceData>();

        for (int i = 0; i < result.Count; i++)
        {
            ResourceSourceData item = result[i];
            AtlasTexture atlasTexture = MaterialManager.Instance.GetAtlasTextureInternal(item.spriteData);
            ItemListData.AddItem(item.name, atlasTexture);
            ItemListData.SetItemMetadata(i, item.id);
        }
    }

    private void ButtonRefresh_Pressed()
    {
        throw new NotImplementedException();
    }

    private void ButtonAutomatic_Pressed()
    {
        ResourceSourceGenerator resourceSourceGenerator = new ResourceSourceGenerator(MapManagerEditor.Instance.CurrentMapLevelData);
        resourceSourceGenerator.Create();
    }

    private void ItemListData_ItemSelected(long index)
    {
        int idData = (int)ItemListData.GetItemMetadata((int)index);
        this.objectSelected = ResourceSourceManager.Instance.GetData(idData);
        
        TextureRectImage.Texture = objectSelected.textureVisual;
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, objectSelected.spriteData);
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
        // Mostrar preview si hay objeto seleccionado
        if (objectSelected != null)
        {
            // Crear preview si cambio dimencion
            if (PlacementPreview.Instance.Size != SelectionBlueprint.Instance.Size)
                PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, objectSelected.spriteData);
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
            foreach (var (_, tilePos) in SelectionBlueprint.Instance.IterateWithTilePositions())
            {
                mapBase.AddUpdateTile(tilePos, objectSelected.id);
            }
        }        
    }

    private void ApplyErase()
    {
        if (objectSelected != null && objectSelected.id != 0)
        {
            foreach (var (_, tilePos) in SelectionBlueprint.Instance.IterateWithTilePositions())
            {
                mapBase.RemoveTile(tilePos, objectSelected.id);
            }
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
