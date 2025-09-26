using Godot;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Buildings;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using System;
using System.Collections.Generic;

public partial class ControlEditorUnit : MarginContainer
{
    private CharacterModelBaseData objectSelected;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        MapManagerEditor.Instance.editorMode = EditorMode.UNIDADES;
        ItemListData.ItemSelected += ItemListData_ItemSelected;
        ButtonRefresh.Pressed += ButtonRefresh_Pressed;

        LoadItems();

        MapManagerEditor.Instance.OnMapLevelDataChanged += Instance_OnMapLevelDataChanged;
    }
    private void Instance_OnMapLevelDataChanged(MapLevelData data)
    {
        //mapBase = data.mapBuildings;
    }

    private void LoadItems()
    {
        ItemListData.Clear();
        List<CharacterModelBaseData> result = new List<CharacterModelBaseData>();
        //BsonExpression bsonExpression = null;


        //string expressionText = "isRule = @0";
        //bsonExpression = BsonExpression.Create(expressionText, true);
        result = DataBaseManager.Instance.FindAll<CharacterModelBaseData>();

        for (int i = 0; i < result.Count; i++)
        {
            CharacterModelBaseData item = result[i];
            AtlasTexture atlasTexture = item.textureVisual;
            ItemListData.AddItem(item.name, atlasTexture);
            ItemListData.SetItemMetadata(i, item.id);

        }

    }

    private void ButtonRefresh_Pressed()
    {
        LoadItems();
    }



    private void ItemListData_ItemSelected(long index)
    {
        int idData = (int)ItemListData.GetItemMetadata((int)index);
        this.objectSelected = CharacterModelManager.Instance.GetCharacterModel(idData);

        TextureRectImage.Texture = objectSelected.textureVisual;
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;        
    }
    public override void _Input(InputEvent @event)
    {
        if ( !MapManagerEditor.Instance.enableEditor)
            return;

        if (MapManagerEditor.Instance.editorMode != EditorMode.UNIDADES)
            return;

        

        //// Mueve blueprint y preview
        //SelectionBlueprint.Instance.Move(mouseTile);
        //PlacementPreview.Instance.Move(mouseTile);
        //// Mostrar preview si hay objeto seleccionado
        //if (objectSelected != null)
        //{
        //    // Crear preview si cambio dimencion
        //    if (PlacementPreview.Instance.Size != SelectionBlueprint.Instance.Size)
        //        PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, objectSelected.spriteData);
        //}

        
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
            Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseCamera;
            CharacterCreatorManager.Instance.CreateNewCharacter(objectSelected.id, mouseTile);
            SpinBoxTotalUnits.Value = CharacterCreatorManager.Instance.totalUnits;
        }
    }

    private void ApplyErase()
    {
        if (objectSelected != null && objectSelected.id != 0)
        { 
           
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
