using Arch.AOT.SourceGenerator;
using Arch.Core;
using Arch.Core.Extensions;
using Godot;
using GodotEcsArch.sources.components;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.Multimesh;
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
    // Called when the node enters the scene tree for the first time.
    TerrainData objectSelected;
    int idTileSquare = 1;
    Entity[,] bluePrintEntities = null;
    private int currentZoomValue = 1; // o el valor inicial que desees
    private int minZoomValue = 1;
    private int maxZoomValue = 10;
    private TerrainMapLevelDesign terrainMapLevelDesign;
    TerrainGenerator terrainGenerator;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        ButtonSearch.Pressed += ButtonSearch_Pressed;
        ButtonAutomaticTerrain.Pressed += ButtonAutomaticTerrain_Pressed;
        DrawTilesBluePrint(new Vector2I(1, 1), new Vector2I(0, 0));

        SpinBoxGridX.ValueChanged += SpinBoxGrid_ValueChanged;
        SpinBoxGridY.ValueChanged += SpinBoxGrid_ValueChanged;
        MapManagerEditor.Instance.editorMode = EditorMode.TERRENO;
        ItemListRules.ItemSelected += ItemListRules_ItemSelected;
        ButtonRefresh.Pressed += ButtonRefresh_Pressed;

        LoadItemsRules();
        terrainMapLevelDesign = TerrainMapLevelDesign.Piso;
        CheckBoxFloor.Pressed += CheckBoxFloor_Pressed;
        CheckBoxPath.Pressed += CheckBoxPath_Pressed;
        CheckBoxWater.Pressed += CheckBoxWater_Pressed;
        CheckBoxOrnament.Pressed += CheckBoxOrnament_Pressed;
        CheckBoxBasic.Pressed += CheckBoxBasic_Pressed;

        foreach (TerrainMapLevelDesign item in Enum.GetValues(typeof(TerrainMapLevelDesign)))
        {
            OptionButtonLayer.AddItem(item.ToString());
        }
        terrainGenerator = new TerrainGenerator(MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapTerrainBasic, 12379723, true, false);
        terrainGenerator.SetTerrainDistribution(0.1f, 0.9f, 0.0f);


    }


    private void CheckBoxBasic_Pressed()
    {
        MapManagerEditor.Instance.currentMapLevelData.terrainMap.EnableLayer(CheckBoxBasic.ButtonPressed, TerrainMapLevelDesign.Basico);
    }

    private void CheckBoxOrnament_Pressed()
    {
        MapManagerEditor.Instance.currentMapLevelData.terrainMap.EnableLayer(CheckBoxOrnament.ButtonPressed, TerrainMapLevelDesign.Ornamentos);
    }

    private void CheckBoxWater_Pressed()
    {
        MapManagerEditor.Instance.currentMapLevelData.terrainMap.EnableLayer(CheckBoxWater.ButtonPressed, TerrainMapLevelDesign.Agua);
    }

    private void CheckBoxPath_Pressed()
    {
        MapManagerEditor.Instance.currentMapLevelData.terrainMap.EnableLayer(CheckBoxPath.ButtonPressed, TerrainMapLevelDesign.Camino);
    }

    private void CheckBoxFloor_Pressed()
    {
        MapManagerEditor.Instance.currentMapLevelData.terrainMap.EnableLayer(CheckBoxFloor.ButtonPressed, TerrainMapLevelDesign.Piso);
    }

    private void ButtonRefresh_Pressed()
    {
        LoadItemsRules();

    }

    private void ItemListRules_ItemSelected(long index)
    {
        int idData = (int)ItemListRules.GetItemMetadata((int)index);
        var data =TerrainManager.Instance.GetData(idData);
        WinTerrain_OnNotifySelected(data);
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
            ItemListRules.AddItem(item.name,atlasTexture);
            ItemListRules.SetItemMetadata(i, item.id);
        }
      
    }

    private void ButtonAutomaticTerrain_Pressed()
    {
        Vector2I chunk =  new Vector2I((int)SpinBoxChunkX.Value, (int)SpinBoxChunkY.Value);        
        var data = terrainGenerator.Generate(chunk, 10);
        terrainGenerator.AplicarChunksAlMapaBase(data);
    }

    private void SpinBoxGrid_ValueChanged(double value)
    {
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        DrawTilesBluePrint(new Vector2I((int)SpinBoxGridX.Value, (int)SpinBoxGridY.Value), mouseTile);
        if (!objectSelected.isRule)
        {
            ShowPreviewBlueprint(new Vector2I((int)SpinBoxGridX.Value, (int)SpinBoxGridY.Value), mouseTile);
        }
        else
        {
            ClearPreviewBlueprint();
        }
    }

    private void ButtonSearch_Pressed()
    {
        FacadeWindowDataSearch<TerrainData> winTerrain = new FacadeWindowDataSearch<TerrainData>("res://sources/WindowsDataBase/Terrain/windowTerrain.tscn", this, WindowType.SELECTED);
        winTerrain.OnNotifySelected += WinTerrain_OnNotifySelected;
        
    }

    private void WinTerrain_OnNotifySelected(TerrainData objectSelected)
    {
        this.objectSelected = objectSelected;
        TextureRectImage.Texture = objectSelected.textureVisual;
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        if (!objectSelected.isRule)
        {
            ShowPreviewBlueprint(new Vector2I((int)SpinBoxGridX.Value, (int)SpinBoxGridY.Value), mouseTile);
        }
        else
        {
            ClearPreviewBlueprint();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
        
    }

    public override void _Input(Godot.InputEvent @event)
	{
        if (!MapManagerEditor.Instance.enableEditor)
            return;
        UpdateMouseChunk(@event);

        HandleZoomInput(@event);
        UpdateBlueprintPosition();

        if (MapManagerEditor.Instance.editorMode != EditorMode.TERRENO)
            return;
        
        HandleBlueprintPreview();
        HandleTerrainPainting();
    }

    private void UpdateMouseChunk(Godot.InputEvent @event)
    {
     

        if (@event is InputEventMouseButton mouseEvent)
        {
            // Verifica que es el botón central (Wheel) y que es doble clic
            if (mouseEvent.ButtonIndex == MouseButton.Middle && mouseEvent.DoubleClick)
            {
                Vector2 data = PositionsManager.Instance.positionMouseChunk;
                SpinBoxChunkX.Value = data.X;
                SpinBoxChunkY.Value = data.Y;
                ButtonAutomaticTerrain_Pressed();
            }
        }
    }

    private void HandleBlueprintPreview()
    {
        if (objectSelected == null || objectSelected.isRule)
            return;
     
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        if (previewEntities.Count == 0)
        {
            Vector2I blueprintSize = new Vector2I((int)SpinBoxGridX.Value, (int)SpinBoxGridY.Value);
            ShowPreviewBlueprint(blueprintSize, mouseTile);
        }

        MovePreviewToTilePosition(mouseTile);        
    }
    private void HandleZoomInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (!Input.IsKeyPressed(Key.Alt))
                return;

            int zoomDelta = 0;

            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                zoomDelta = 1;
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
                zoomDelta = -1;

            if (zoomDelta != 0)
            {
                currentZoomValue = Mathf.Clamp(currentZoomValue + zoomDelta, minZoomValue, maxZoomValue);
                SpinBoxGridX.Value = currentZoomValue;
                SpinBoxGridY.Value = currentZoomValue;
            }
        }
    }

    private void UpdateBlueprintPosition()
    {
        Vector2I posTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        MoveBlueprintToTilePosition(posTile);
    }

    private void HandleTerrainPainting()
    {
        if (objectSelected == null)
            return;

        foreach (Entity item in bluePrintEntities)
        {
            var tilePos = item.Get<TilePositionComponent>();
            Vector2I tileCoords = new(tilePos.x, tilePos.y);

            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(tileCoords, objectSelected.id, (TerrainMapLevelDesign)OptionButtonLayer.Selected);
            }
            else if (Input.IsMouseButtonPressed(MouseButton.Right))
            {
                MapManagerEditor.Instance.currentMapLevelData.terrainMap.RemoveTile(tileCoords, objectSelected.id, (TerrainMapLevelDesign)OptionButtonLayer.Selected);
            }
        }
    }
    private void ClearBluePrint()
    {
        if (bluePrintEntities != null)
        {
            for (int i = 0; i < bluePrintEntities.GetLength(0); i++)
            {
                for (int j = 0; j < bluePrintEntities.GetLength(1); j++)
                {
                    TilesManager.Instance.FreeTileEntity(bluePrintEntities[i, j]);
                }
            }
        }
    }
  
    public void DrawTilesBluePrint(Vector2I sizeBluePrintGrid, Vector2I centerTilePosition)
    {
        ClearBluePrint();
        bluePrintEntities = new Entity[sizeBluePrintGrid.X, sizeBluePrintGrid.Y];

        // Calcular desplazamiento desde el centro del blueprint
        Vector2I halfSize = new Vector2I(sizeBluePrintGrid.X / 2, sizeBluePrintGrid.Y / 2);

        for (int i = 0; i < sizeBluePrintGrid.X; i++)
        {
            for (int j = 0; j < sizeBluePrintGrid.Y; j++)
            {
                // Posición relativa al centro
                Vector2I offset = new Vector2I(i - halfSize.X, j - halfSize.Y);
                Vector2I tilePos = centerTilePosition + offset;

                Vector2 worldPos = TilesHelper.WorldPositionTile(tilePos);

                Entity entity = TilesManager.Instance.CreateTileDinamic(
                    idTileSquare, 1, worldPos, 30, Vector2.Zero
                );

                bluePrintEntities[i, j] = entity;

                // Guardar la posición del tile
                ref var tilePosition = ref entity.Get<TilePositionComponent>();
                tilePosition.x = tilePos.X;
                tilePosition.y = tilePos.Y;

                ref var position = ref entity.Get<PositionComponent>();
                position.position = worldPos;
            }
        }
    }
    public void MoveBlueprintToTilePosition(Vector2I targetTilePosition)
    {
        int width = bluePrintEntities.GetLength(0);
        int height = bluePrintEntities.GetLength(1);

        // Calcular el centro del blueprint
        Vector2I blueprintCenter = new Vector2I(width / 2, height / 2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Entity entity = bluePrintEntities[x, y];
                if (!entity.IsAlive()) continue;

                // Calcular desplazamiento relativo al centro
                Vector2I offset = new Vector2I(x - blueprintCenter.X, y - blueprintCenter.Y);
                Vector2I newTilePos = targetTilePosition + offset;

                // Obtener la posición en el mundo del nuevo tile
                Vector2 newWorldPos = TilesHelper.WorldPositionTile(newTilePos);

                // Suponiendo que las entidades tienen un componente Position con propiedad `Value`
                ref var position = ref entity.Get<PositionComponent>();
                position.position = newWorldPos;

                ref var tilePositionComponent = ref entity.Get<TilePositionComponent>();
                tilePositionComponent.x = newTilePos.X;
                tilePositionComponent.y = newTilePos.Y;
            }
        }
    }

    private List<Entity> previewEntities = new();

    public void ShowPreviewBlueprint(Vector2I size, Vector2I centerTilePosition)
    {
        ClearPreviewBlueprint();

        Vector2I halfSize = new(size.X / 2, size.Y / 2);

        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                Vector2I offset = new(x - halfSize.X, y - halfSize.Y);
                Vector2I tilePos = centerTilePosition + offset;

                Vector2 worldPos = TilesHelper.WorldPositionTile(tilePos);

                // Crea una entidad solo visual (puede tener un tipo especial o layer aparte)
                Entity previewTile = SpriteHelper.CreateEntity(objectSelected.spriteData, objectSelected.spriteData.scale, worldPos, 20, objectSelected.spriteData.offsetInternal);

                // Ej: hacerla transparente (si usas Sprite2D o MeshInstance2D)
                //previewTile.Get<Node2D>()?.Set("modulate", new Color(1, 1, 1, 0.5f));

                previewEntities.Add(previewTile);
            }
        }
    }
    public void ClearPreviewBlueprint()
    {
        foreach (var entity in previewEntities)
        {
            TilesManager.Instance.FreeTileEntity(entity);
        }
        previewEntities.Clear();
    }

    public void MovePreviewToTilePosition(Vector2I targetTilePosition)
    {
        Vector2I tileSize = new Vector2I(16, 16);
        float xx = MeshCreator.PixelsToUnits(tileSize.X) / 2f;
        float yy = MeshCreator.PixelsToUnits(tileSize.Y) / 2f;


        if (previewEntities.Count == 0)
            return;

        int width = (int)SpinBoxGridX.Value;
        int height = (int)SpinBoxGridY.Value;

        Vector2I blueprintCenter = new Vector2I(width / 2, height / 2);

        int index = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (index >= previewEntities.Count) return;

                Entity entity = previewEntities[index++];
                var spriteData = entity.Get<SpriteRenderGPUComponent>();
                if (!entity.IsAlive()) continue;

                Vector2I offset = new Vector2I(x - blueprintCenter.X, y - blueprintCenter.Y);
                Vector2I newTilePos = targetTilePosition + offset;
                Vector2 newWorldPos = TilesHelper.WorldPositionTile(newTilePos);

                Vector2 positionNormalize = newTilePos * new Vector2(MeshCreator.PixelsToUnits(tileSize.X), MeshCreator.PixelsToUnits(tileSize.Y));
                Vector2 positionCenter = positionNormalize + new Vector2(xx, yy);
                Vector2 positionReal = positionNormalize + new Vector2(xx, yy) + new Vector2(spriteData.originOffset.X * spriteData.scale, spriteData.originOffset.Y * spriteData.scale);
     

                if (entity.Has<PositionComponent>())
                {
                    ref var position = ref entity.Get<PositionComponent>();
                    position.position = positionReal;
                }

                if (entity.Has<TilePositionComponent>())
                {
                    ref var tilePos = ref entity.Get<TilePositionComponent>();
                    tilePos.x = newTilePos.X;
                    tilePos.y = newTilePos.Y;
                }
            }
        }
    }
}
