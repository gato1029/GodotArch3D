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
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using static Flecs.NET.Core.Ecs.Units;
using static Godot.ClassDB;


public partial class ControlEditorTerrain : MarginContainer
{
    private TerrainData objectSelected;
    private TerrainMap terrainMap;
    private MapType mapType;
    Vector2I sizeMap = Vector2I.Zero;
    private AutoTileSpriteData currentAutoTileSpriteData=null;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI    
        // Configuración inicial del preview
        PlacementPreview.Instance.Configure(tileId: 1, layer: 20);
        SelectionBlueprint.Instance.Configure(1762385049549000, 30);
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

        

        MapManagerEditor.Instance.OnMapLevelDataChanged += Instance_OnMapLevelDataChanged;
        ButtonSeedRandom.Pressed += ButtonSeedRandom_Pressed;
        SpinBoxSeed.Value = CommonOperations.GetRandomInt();

    }

    TerrainTileEntry currentTerrain=null;
    
    private void ItemListRules_ItemSelected(long index)
    {
        currentTerrain = (TerrainTileEntry)ItemListRules.GetItemMetadata((int)index);

        var data = MasterDataManager.GetData<AutoTileSpriteData>(currentTerrain.TileId);
        SetAutoTileSpriteData(data);
    }

    private void SetAutoTileSpriteData(AutoTileSpriteData data)
    {
        if (objectSelected == null)
            return;
        // Actualizar el objeto seleccionado con la nueva regla
        currentAutoTileSpriteData = data;
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        var sprite = MasterDataManager.GetData<TileSpriteData>(currentAutoTileSpriteData.tileRuleTemplates[0].TileCentral.idTileSprite);   
        PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, sprite);
    }
    private void WinTerrain_OnNotifySelected(TerrainData objectSelected)
    {
        this.objectSelected = objectSelected;
        TextureRectImage.Texture = objectSelected.textureVisual;
        //Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        //var tempo = MasterDataManager.GetData<AutoTileSpriteData>(objectSelected.idSurface);
        //var sprite = MasterDataManager.GetData<TileSpriteData>(tempo.tileRuleTemplates[0].TileCentral.idTileSprite);
        //PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, sprite.spriteData);

        LoadItemsRules();
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
        
        foreach (var item in objectSelected.idsAutoTileSprite)
        {
            var dataAuto = MasterDataManager.GetData<AutoTileSpriteData>(item.TileId);
            int index =ItemListRules.AddItem(dataAuto.name, dataAuto.textureVisual);
            ItemListRules.SetItemMetadata(index, item);
        }
    }
    private void Instance_OnMapLevelDataChanged(MapLevelData obj)
    {
        terrainMap = obj.terrainMap;
        mapType = obj.maptype;
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
            switch (mapType)
            {
                case MapType.Mapa:
                    GenerateTerrainMap();
                    break;
                case MapType.Habitacion:
                    break;
                case MapType.Mazmorra:
                    GenerateDungeon();
                    break;
                default:
                    break;
            }


        }
        else
        {
            Message.ShowMessage(this, "No se encontro Mapa");
        }

    }
    private void GenerateDungeon()
    {
        foreach (var item in terrainMap.MapLayerDesign)
        {
            item.Value.SetRenderEnabledGlobal(false);
        }
        DungeonGenerator dungeonGenerator = new DungeonGenerator(sizeMap.X, sizeMap.Y);
        dungeonGenerator.GenerateRandomFilled();
        terrainMap.ClearMap();
        terrainMap.ClearFilesChunks();
        dungeonGenerator.ExportInGame(terrainMap, TerrainCategoryType.MazmorraBase);
        foreach (var item in terrainMap.MapLayerDesign)
        {
            item.Value.SetRenderEnabledGlobal(true);
        }
    }
    private void GenerateTerrainMap()
    {
        foreach (var item in terrainMap.MapLayerDesign)
        {
            item.Value.SetRenderEnabledGlobal(false);
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
            item.Value.SetRenderEnabledGlobal(true);
        }
        PerformanceTimer.Instance.PrintAll(1, 1);
    }

    private void ButtonRefresh_Pressed()
    {
        if (objectSelected == null)
            return;
        objectSelected = MasterDataManager.GetData<TerrainData>(objectSelected.id);
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
        if (currentAutoTileSpriteData != null )
        {
            // Crear preview si cambio dimencion
            if (PlacementPreview.Instance.Size != SelectionBlueprint.Instance.Size)
            {
                var sprite = MasterDataManager.GetData<TileSpriteData>(currentAutoTileSpriteData.tileRuleTemplates[0].TileCentral.idTileSprite);
                PlacementPreview.Instance.Create(SelectionBlueprint.Instance.Size, mouseTile, sprite);                
            }
        }

        // Pintar o borrar tiles
        if (Input.IsMouseButtonPressed(MouseButton.Left))
            ApplyPaint();

        if (Input.IsMouseButtonPressed(MouseButton.Right))
            ApplyErase();
    }

    private void ApplyPaint()
    {
        
        if (objectSelected!=null && currentAutoTileSpriteData!=null)
        {
            List<Vector2I> tilesToUpdate = new List<Vector2I>();
            foreach (var (_, tilePos) in SelectionBlueprint.Instance.IterateWithTilePositions())
            {
                tilesToUpdate.Add(tilePos);                
            }
            
            terrainMap.AddUpdateTileSprite(tilesToUpdate, objectSelected.id, currentTerrain);
        }
        
    }

    private void ApplyErase()
    {
        if (objectSelected != null && currentAutoTileSpriteData != null)
        {
            List<Vector2I> tilesToUpdate = new List<Vector2I>();
            foreach (var (_, tilePos) in SelectionBlueprint.Instance.IterateWithTilePositions())
            {
                tilesToUpdate.Add(tilePos);                
            }
            terrainMap.RemoveTileSprite(tilesToUpdate, objectSelected.id, currentTerrain);
        }
    }
}


