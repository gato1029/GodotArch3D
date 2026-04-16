using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.BlackyTiles.Systems;
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
using System.ComponentModel;
using System.Reflection.Emit;
using static Flecs.NET.Core.Ecs.Units;
using static Godot.ClassDB;


public partial class ControlEditorTerrain : MarginContainer
{
    private TerrainData objectSelected;
    private MapTerrain terrainMap;
    private MapType mapType;
    Vector2I sizeMap = Vector2I.Zero;
    private AutoTileSpriteData currentAutoTileSpriteData=null;
    List<ItemList> itemListsArray = new List<ItemList>();
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI    
        // Configuración inicial del preview
   

        ButtonSearch.Pressed += ButtonSearch_Pressed;
        ButtonAutomaticTerrain.Pressed += ButtonAutomaticTerrain_Pressed;

        MapManagerEditor.Instance.editorMode = EditorMode.TERRENO;
      
        ButtonRefresh.Pressed += ButtonRefresh_Pressed;

        TabContainerLevels.TabChanged += TabContainerLevels_TabChanged;

        MapManagerEditor.Instance.OnMapLevelDataChanged += Instance_OnMapLevelDataChanged;
        ButtonSeedRandom.Pressed += ButtonSeedRandom_Pressed;
        SpinBoxSeed.Value = CommonOperations.GetRandomInt();

    }

    private void TabContainerLevels_TabChanged(long tab)
    {
        foreach (var item in itemListsArray)
        {
            item.DeselectAll();
        }
    }

    TerrainTileEntry currentTerrain = null;
    long currentRuleId = 0;
    BlackyTerrainLayer currentLayer = BlackyTerrainLayer.Terrain;
    int currentAltura = 0;

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
        
        foreach (var item in terrainMap.MapLayerDesign)
        {
            ControlLayerChunk controlLayer = GD.Load<PackedScene>("res://sources/Editor/Controls/ControlLayerChunk.tscn").Instantiate<ControlLayerChunk>();
            ContainerLayers.AddChild(controlLayer);
            controlLayer.SetSpriteMapChunk(item.Value);
        }

    }
    private void LoadItemsRules()
    {
        foreach (var item in TabContainerLevels.GetChildren())
        {
            item.QueueFree();
        }

        foreach (var item in objectSelected.terrains)
        {
            AddTab(item.Value);
        }
    }
  
    public void AddTab(TerrainTileEntry terrainTileEntry)
    {
        Control tab = new Control();
        tab.Name = "Altura:" + terrainTileEntry.heightReal.ToString();
        tab.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        tab.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        ItemList itemList = new ItemList();
        itemList.CustomMinimumSize = new Vector2(200, 200);
        itemList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        itemList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        tab.AddChild(itemList);
        TabContainerLevels.AddChild(tab);

        foreach (var item in terrainTileEntry.layersRelative)
        {
            var dataAuto = MasterDataManager.GetData<AutoTileSpriteData>(item.idAutoTile);
            int index = itemList.AddItem(dataAuto.name + "-" + item.layerType, dataAuto.textureVisual);
            itemList.SetItemMetadata(index, item);
        }
        
        itemList.ItemSelected += (long index) =>
        {
            currentAltura = terrainTileEntry.heightReal;
            var item = (TerrainLayer)itemList.GetItemMetadata((int)index);
            currentRuleId = item.idAutoTile;
            currentLayer = (BlackyTerrainLayer)item.layer;
            
            var data = MasterDataManager.GetData<AutoTileSpriteData>(item.idAutoTile);
            SetAutoTileSpriteData(data);
        };
        itemListsArray.Add(itemList);
    }
   
    BlackyWorld blackyWorldMap;
    private void Instance_OnMapLevelDataChanged(MapLevelData obj)
    {
        terrainMap = obj.terrainMap;
        mapType = obj.maptype;
        sizeMap = obj.size;
        blackyWorldMap = obj.blackyWorldMap;
        PlacementPreview.Instance.ConfigureFlecs(blackyWorldMap.flecsManager);
        SelectionBlueprint.Instance.ConfigureFlecs(blackyWorldMap.flecsManager);

        PlacementPreview.Instance.Configure(tileId: 1, layer: 20);
        SelectionBlueprint.Instance.Configure(1762385049549000, 30);
        // Configuración inicial del blueprint
        SelectionBlueprint.Instance.Create(
            size: new Vector2I(1, 1),
            centerTile: Vector2I.Zero
        );
        //SpinBoxSeed.Value = terrainMap.seed;
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
        Dictionary<TerrainTileType, float> layersProbabilitys= new Dictionary<TerrainTileType, float>
        {
            { TerrainTileType.Agua, 100 },
            { TerrainTileType.TierraNivel1, 90 },
            { TerrainTileType.CespedNivel1, 70 },
            { TerrainTileType.TierraNivel2, 60 }, // 60
            { TerrainTileType.CespedNivel2, 60 },
            { TerrainTileType.AdornosCesped, 30 },

        };
        int seed = (int)SpinBoxSeed.Value;
        GeneratorTerrain.Instance.ConfigGenerator(sizeMap, terrainMap, layersProbabilitys, objectSelected.id);
        GeneratorTerrain.Instance.Generate(seed);

    
    }

    private void ButtonRefresh_Pressed()
    {
        if (objectSelected == null)
            return;
        objectSelected = MasterDataManager.GetData<TerrainData>(objectSelected.id);
        LoadItemsRules();

    }
    Vector2I lastMouseTile = Vector2I.Zero;
    Vector2I CurrentMouseTile = Vector2I.Zero;
    public override void _Input(InputEvent @event)
    {
        if (terrainMap == null || !MapManagerEditor.Instance.enableEditor)
            return;

        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        CurrentMouseTile    = mouseTile;
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

            {
                blackyWorldMap.Terrain.SetTerrainList(tilesToUpdate, objectSelected.idSave,currentRuleId, currentAltura, (int)currentLayer);
            }            
            lastMouseTile = CurrentMouseTile;
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

            bool autotileErase = true; 
            if (currentLayer == BlackyTerrainLayer.Decor )
            {
                autotileErase = false;
            }
            blackyWorldMap.Terrain.RemoveTerrainList(tilesToUpdate,currentAltura, (int)currentLayer, currentRuleId,autotileErase);
        }
    }
}


