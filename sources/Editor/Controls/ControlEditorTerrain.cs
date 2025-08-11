using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.managers.Terrain;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;


public partial class ControlEditorTerrain : MarginContainer
{
    private TerrainData objectSelected;
    private TerrainMap terrainMap;
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

        ButtonSearch.Pressed += ButtonSearch_Pressed;
        ButtonAutomaticTerrain.Pressed += ButtonAutomaticTerrain_Pressed;

        MapManagerEditor.Instance.editorMode = EditorMode.TERRENO;
        ItemListRules.ItemSelected += ItemListRules_ItemSelected;
        ButtonRefresh.Pressed += ButtonRefresh_Pressed;

        LoadItemsRules();

        MapManagerEditor.Instance.OnMapLevelDataChanged += Instance_OnMapLevelDataChanged;
        ButtonSeedRandom.Pressed += ButtonSeedRandom_Pressed;
        
    }
    private void ItemListRules_ItemSelected(long index)
    {
        int idData = (int)ItemListRules.GetItemMetadata((int)index);
        var data = TerrainManager.Instance.GetData(idData);
        WinTerrain_OnNotifySelected(data);
    }
    private void WinTerrain_OnNotifySelected(TerrainData objectSelected)
    {
        this.objectSelected = objectSelected;
        TextureRectImage.Texture = objectSelected.textureVisual;
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, objectSelected.spriteData);
    }
    private void LoadItemsLayers()
    {
        foreach (var item in ContainerLayers.GetChildren())
        {
            ContainerLayers.RemoveChild(item);
        }
        ControlLayerChunk controlLayerBasic = GD.Load<PackedScene>("res://sources/Editor/Controls/ControlLayerChunk.tscn").Instantiate<ControlLayerChunk>();
        ContainerLayers.AddChild(controlLayerBasic);
        controlLayerBasic.SetSpriteMapChunk(terrainMap.MapTerrainBasic);
        foreach (var item in terrainMap.MapLayerDesign)
        {
            ControlLayerChunk controlLayer = GD.Load<PackedScene>("res://sources/Editor/Controls/ControlLayerChunk.tscn").Instantiate<ControlLayerChunk>();
            ContainerLayers.AddChild(controlLayer);
            controlLayer.SetSpriteMapChunk(item.Value);
        }

    }
    private void LoadItemsRules()
    {
        ItemListRules.Clear();
        BsonExpression bsonExpression = null;
        List<TerrainData> result = new List<TerrainData>();

        string expressionText = "isRule = @0";
        bsonExpression = BsonExpression.Create(expressionText, true);
        result = DataBaseManager.Instance.FindAllFilter<TerrainData>(bsonExpression);

        for (int i = 0; i < result.Count; i++)
        {
            TerrainData item = result[i];
            AtlasTexture atlasTexture = MaterialManager.Instance.GetAtlasTextureInternal(item.spriteData);
            ItemListRules.AddItem(item.name, atlasTexture);
            ItemListRules.SetItemMetadata(i, item.id);
        }

    }
    private void Instance_OnMapLevelDataChanged(MapLevelData obj)
    {
        terrainMap = obj.terrainMap;
        sizeMap = obj.size;
        SpinBoxSeed.Value = terrainMap.seed;
        LoadItemsLayers();
    }

    private void ButtonSearch_Pressed()
    {
        if (terrainMap != null)
        {
            FacadeWindowDataSearch<TerrainData> winTerrain = new FacadeWindowDataSearch<TerrainData>("res://sources/WindowsDataBase/Terrain/windowTerrain.tscn", this, WindowType.SELECTED);
            winTerrain.OnNotifySelected += WinTerrain_OnNotifySelected;
        }
        else
        {
            Message.ShowMessage(this, "No se encontro Mapa");
        }


    }

    private void ButtonSeedRandom_Pressed()
    {
        SpinBoxSeed.Value = CommonOperations.GetRandomInt();
    }
    private void ButtonAutomaticTerrain_Pressed()
    {
        if (terrainMap != null)
        {
            foreach (var item in terrainMap.MapLayerDesign)
            {
                item.Value.SetRenderEnabled(false);
            }
            TerrainGenerator terrainGenerator = new TerrainGenerator(terrainMap.MapTerrainBasic, (int)SpinBoxSeed.Value, true, false);
            using (new ProfileScope("Terrain Generator"))
            {
                terrainMap.ClearMap();
                terrainMap.ClearFilesChunks();
           
                terrainGenerator.SetTerrainDistribution(0.1f, 0.8f, 0.1f);
                Vector2I chunk = new Vector2I((int)SpinBoxChunkX.Value, (int)SpinBoxChunkY.Value);
                // var data = terrainGenerator.GenerateTerrain(chunk, GenerateMode.RadiusAroundChunk, new Vector2I(2,2));
                terrainGenerator.GenerateTerrainOptimized(new Vector2I(0, 0), GenerateMode.CenteredByTileDimensions, sizeMap);
           
            }
            using (new ProfileScope("Terrain Generator Plot"))
            {
                terrainGenerator.AplicarMapaDisenioOptimized(new Vector2I(0, 0), GenerateMode.CenteredByTileDimensions, sizeMap, TerrainCategoryType.Bosque);               
                terrainMap.seed = (int)SpinBoxSeed.Value;
            }
            foreach (var item in terrainMap.MapLayerDesign)
            {
                item.Value.SetRenderEnabled(true);
            }
            PerformanceTimer.Instance.PrintAll(1, 1);
        }
        else
        {
            Message.ShowMessage(this, "No se encontro Mapa");
        }

    }

    private void ButtonRefresh_Pressed()
    {
        LoadItemsRules();

    }
    public override void _Input(InputEvent @event)
    {
        if (terrainMap == null || !MapManagerEditor.Instance.enableEditor)
            return;

        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        // Mueve blueprint y preview
        SelectionBlueprint.Instance.Move(mouseTile);
        PlacementPreview.Instance.Move(mouseTile);

        if (MapManagerEditor.Instance.editorMode != EditorMode.TERRENO)
            return;

        // Mostrar preview si hay objeto seleccionado
        if (objectSelected != null )
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
        foreach (var (_, tilePos) in SelectionBlueprint.Instance.IterateWithTilePositions())
        {
            terrainMap.AddUpdateTile(tilePos, objectSelected.id);
        }
    }

    private void ApplyErase()
    {
        foreach (var (_, tilePos) in SelectionBlueprint.Instance.IterateWithTilePositions())
        {
            terrainMap.RemoveTile(tilePos, objectSelected.id);
        }
    }
}


