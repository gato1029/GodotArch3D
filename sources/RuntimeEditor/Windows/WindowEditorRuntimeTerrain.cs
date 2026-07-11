using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using static Flecs.NET.Core.Ecs.Metrics;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class WindowEditorRuntimeTerrain : Window
{
    enum ModeEditorTerrain
    {
        SELECCION,
        CREACION,
        ELIMINACION,
    }
    enum ModePaint
    {
        NORMAL,
        AUTO_DUAL,
    }

    private MaterialData materialSelected;
    private RuntimeWindowDockController dockController;
    private DualTileTemplate dualTileTemplate;

    private long idTileSpriteCurrent=0;
    private RampsData rampsDataSelected;
    private DecorationData decorationDataSelected;
    private SuperficieData superficieDataSelected;
    private CaminosData caminosDataSelected;


    ModeEditorTerrain modeEditorTerrain = ModeEditorTerrain.SELECCION;
    ModePaint modePaint = ModePaint.NORMAL;

    TileSelectionMatrixData matrixCurrent = null;
    private Vector2I lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
    private bool uiMouseCaptured = false;
    bool AnyWindowOpen = false;
    private BrushType brushType = BrushType.Square;
    private int sizeBrush = 1;
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        dockController = new RuntimeWindowDockController(this);
        dockController.DockWindow(RuntimeWindowDockController.WindowDockPosition.TopRight, 10, 100);
        EditorTextura.OnNotifySelectionMatrix += EditorTextura_OnNotifySelectionMatrix;
        KuroButtonBuscar.Pressed += KuroButtonBuscar_Pressed;
        //KuroButtonBuscarAutomatico.Pressed += KuroButtonBuscarAutomatico_Pressed;
        KuroButtonBorrar.Pressed += KuroButtonBorrar_Pressed;
        KuroButtonCrear.Pressed += KuroButtonCrear_Pressed;
        KuroButtonSeleccion.Pressed += KuroButtonSeleccion_Pressed;
        TipoBrush.OnDataSelected += TipoBrush_OnDataSelected;
        SpinBoxSizeBrush.ValueChanged += SpinBoxSizeBrush_ValueChanged;        
        LoadCapas();
        LoadBrushs();
    }

    

    private void LoadCapas()
    {
        foreach (BlackyRenderLayer item in Enum.GetValues(typeof(BlackyRenderLayer)))
        {
            KuroOptionButtonCapa.AddItemWithData(item.ToString(), item);
        }
    }

    private void SpinBoxSizeBrush_ValueChanged(double value)
    {
        sizeBrush = (int)value;
        switch (modeEditorTerrain)
        {
            case ModeEditorTerrain.CREACION:
                switch (modePaint)
                {
                    case ModePaint.NORMAL:
                            
                        TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush),idTileSpriteCurrent);
                                             
                        break;
                    case ModePaint.AUTO_DUAL:
                        TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush), dualTileTemplate.GetSlot(15).GetData(0).GetPart(0).IdTileSpriteData);

                        break;
                }
                break;
            case ModeEditorTerrain.ELIMINACION:
                var temp = AtlasModsManager.Get<MaterialData>("Base", 1);
                // para el modo eliminación, el preview se muestra con un tile de material base cualquiera, ya que no importa el tile que se muestre
                TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush), temp, 0);
                break;
            default:
                break;
        }
    }

    private void LoadBrushs()
    {
        foreach (var item in Enum.GetValues(typeof(BrushType)))
        {
            TipoBrush.AddItemWithData(item.ToString(), item);            
        }
    }

    private void TipoBrush_OnDataSelected(object obj)
    {
        brushType = (BrushType)obj;
    }

    

    private void KuroButtonSeleccion_Pressed()
    {
        modeEditorTerrain = ModeEditorTerrain.SELECCION;
        var temp = AtlasModsManager.Get<MaterialData>("Base", 1);
        TilesEntityPreviewHelper.Create(new Vector2I(1, 1), temp, 0);
    }

    private void KuroButtonCrear_Pressed()
    {
        modeEditorTerrain = ModeEditorTerrain.CREACION;
        switch (modePaint)
        {
            case ModePaint.NORMAL:
                TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush), idTileSpriteCurrent);
                break;
            case ModePaint.AUTO_DUAL:
                TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush,sizeBrush), dualTileTemplate.GetSlot(1).GetData(0).GetPart(0).IdMod, dualTileTemplate.GetSlot(15).GetData(0).GetPart(0).TileIndex);
                break;
            default:
                break;
        }
        
    }

    public override void _ExitTree()
    {
        dockController?.Dispose();
    }

    private void KuroButtonBorrar_Pressed()
    {
        modeEditorTerrain = ModeEditorTerrain.ELIMINACION;
        var temp = AtlasModsManager.Get<MaterialData>("Base", 1);
        // para el modo eliminación, el preview se muestra con un tile de material base cualquiera, ya que no importa el tile que se muestre
        TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush), temp, 0);              
    }

    private void KuroButtonBuscar_Pressed()
    {
        AnyWindowOpen = true;
        WindowDataEditor w = RuntimeServices.NodeRegistry.Create<WindowDataEditor>();
        w.OnItemSelected += W_OnItemSelectedTerrain;
        w.CloseRequested += W_CloseRequested;
        AddChild(w);
        w.Popup();

        var layerSelected = (BlackyRenderLayer)KuroOptionButtonCapa.GetSelectedData();
        switch (layerSelected)
        {
            case BlackyRenderLayer.TerrenoBase:                
                w.LoadData<TerrainBaseData>();
                break;
            case BlackyRenderLayer.Rampas:
                w.LoadData<RampsData>();
                break;
            case BlackyRenderLayer.Superficie:
                w.LoadData<SuperficieData>();
                break;
            case BlackyRenderLayer.Caminos:
                w.LoadData<CaminosData>();
                break;
            case BlackyRenderLayer.Adornos:
                w.LoadData<DecorationData>();
                break;            
        }        
    }

    private void W_CloseRequested()
    {
        AnyWindowOpen = false;
    }

    TerrainBaseData terrainBaseDataSelected;
    private void W_OnItemSelectedTerrain(object obj)
    {
        var layerSelected = (BlackyRenderLayer)KuroOptionButtonCapa.GetSelectedData();
        DualTileTemplate dual = null;
        switch (layerSelected)
        {
            case BlackyRenderLayer.TerrenoBase:
                terrainBaseDataSelected = (TerrainBaseData)obj;
                dual = AtlasModsManager.Get<DualTileTemplate>(terrainBaseDataSelected.nameMod, terrainBaseDataSelected.idDualTemplate);
                DualTemplateSelection(dual);
                break;
            case BlackyRenderLayer.Rampas:
                rampsDataSelected = (RampsData)obj;
                SpriteSelection(rampsDataSelected.idTileSprite);
                break;
            case BlackyRenderLayer.Superficie:
                superficieDataSelected = (SuperficieData)obj;
                dual = AtlasModsManager.Get<DualTileTemplate>(superficieDataSelected.nameMod, superficieDataSelected.idDualTemplate);
                DualTemplateSelection(dual);
                break;
            case BlackyRenderLayer.Caminos:
                caminosDataSelected = (CaminosData)obj;
                dual = AtlasModsManager.Get<DualTileTemplate>(caminosDataSelected.nameMod, caminosDataSelected.idDualTemplate);
                DualTemplateSelection(dual);
                break;
            case BlackyRenderLayer.Adornos:
                decorationDataSelected = (DecorationData)obj;
                SpriteSelection(decorationDataSelected.idTileSprite);
                break;
        }

    }
    private void SpriteSelection(long idTileSprite)
    {
        idTileSpriteCurrent = idTileSprite;
        modeEditorTerrain = ModeEditorTerrain.CREACION;        
        ConfigModePaint(ModePaint.NORMAL);
        AnyWindowOpen = false;
        AtlasModsManager.GetSpriteUniqueId(idTileSprite, out TileSpriteData tileSpriteData);
        TexturaDual.Texture = tileSpriteData.textureVisual;
        TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush),idTileSprite);
    }
    private void TextureSelection(MaterialData materialData)
    {
        modeEditorTerrain = ModeEditorTerrain.CREACION;
        SetMaterial(materialData);
        ConfigModePaint(ModePaint.NORMAL);
        AnyWindowOpen = false;
    }
    private void DualTemplateSelection(GodotEcsArch.sources.WindowsDataBase.TilesTexture.DualTileTemplate objeto)
    {
        TexturaDual.Texture = objeto.textureVisual;
        LabelDualName.Text = objeto.name;
        modeEditorTerrain = ModeEditorTerrain.CREACION;
        dualTileTemplate = objeto;
        ConfigModePaint(ModePaint.AUTO_DUAL);
        // aqui debes cambiarlo por sprite
        //TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush), objeto.GetSlot(1).GetData(0).GetPart(0).IdMod, objeto.GetSlot(15).GetData(0).GetPart(0).TileIndex);
        TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush),objeto.GetSlot(1).GetData(0).GetPart(0).IdTileSpriteData);

        AnyWindowOpen = false;
    }



    private void ConfigModePaint(ModePaint mode)
    {
        modePaint = mode;
        //switch (mode)
        //{
        //    case ModePaint.NORMAL:
        //        ContenedorDual.Visible = false;
        //        EditorTextura.Visible = true;
        //        break;
        //    case ModePaint.AUTO_DUAL:
        //        ContenedorDual.Visible = true;
        //        EditorTextura.Visible = false;
        //        break;
        //    default:
        //        break;
        //}
    }
    private void SetMaterial(MaterialData materialData)
    {
        materialSelected = materialData;
        Vector2I cellsize = new Vector2I(materialData.divisionPixelX, materialData.divisionPixelY);
        EditorTextura.SetTexture(
            (Texture2D)materialData.textureMaterial,
            cellsize,
            materialData.id,
            materialData.idNameMod
        );
    }

    

    private void EditorTextura_OnNotifySelectionMatrix(TileSelectionMatrixData matrix, int arg2)
    {
        matrixCurrent = matrix;
        TilesEntityPreviewHelper.Create(materialSelected, matrixCurrent);
    }

    public override void _Process(double delta)
    {
        if (IsMouseBlockedByUI())
            return;
        Vector2I currentMouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        Vector2 offsetVisual = Vector2.Zero;

        switch (modePaint)
        {
            case ModePaint.NORMAL:

                offsetVisual = new Vector2(0.25f,0.25f);
                break;
            case ModePaint.AUTO_DUAL:

                //TilesEntityPreviewHelper.Move(currentMouseTile, new Vector2(0.25f, 0.25f));                                
                break;
            default:
                break;
        }

        TilesEntityPreviewHelper.Move(currentMouseTile,offsetVisual);

        bool leftPressed = Input.IsMouseButtonPressed(MouseButton.Left);
        bool rightPressed = Input.IsMouseButtonPressed(MouseButton.Right);

        switch (modeEditorTerrain)
        {
            case ModeEditorTerrain.SELECCION:

                break;
            case ModeEditorTerrain.CREACION:
                if (leftPressed)
                {
                    if (modePaint == ModePaint.AUTO_DUAL && dualTileTemplate == null)
                    {
                        return;
                    }
                    if (modePaint == ModePaint.NORMAL && idTileSpriteCurrent ==0) 
                    {
                        return;
                    }
                    if (currentMouseTile != lastPaintTile)
                    {
                        ApplyPaint();
                        lastPaintTile = currentMouseTile;
                    }
                    else
                    {
                        lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
                    }
                }
                if (rightPressed)
                {
                    if (currentMouseTile != lastPaintTile)
                    {
                        ApplyErase();
                        lastPaintTile = currentMouseTile;
                    }
                    else
                    {
                        lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
                    }
                }

                break;
            case ModeEditorTerrain.ELIMINACION:
                if (leftPressed || rightPressed)
                {
                    if (currentMouseTile != lastPaintTile)
                    {
                        ApplyErase();
                        lastPaintTile = currentMouseTile;
                    }
                    else
                    {
                        lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
                    }
                }
                break;
            default:
                break;
        }
    }
    
    private bool IsMouseBlockedByUI()
    {
        if (AnyWindowOpen)
        {
            return true;
        }
        bool leftPressed = Input.IsMouseButtonPressed(MouseButton.Left);
        bool rightPressed = Input.IsMouseButtonPressed(MouseButton.Right);

        // si soltó ambos botones se libera la captura
        if (!leftPressed && !rightPressed)
            uiMouseCaptured = false;

        // si ya estaba capturado por ui, seguimos bloqueando
        if (uiMouseCaptured)
            return true;

        // si en este frame el mouse está sobre ui y se empezó a presionar
        if (IsMouseOverUI())
        {
            if (leftPressed || rightPressed)
                uiMouseCaptured = true;
            return true;
        }

        return false;
    }

    private bool IsMouseOverUI()
    {
        var hoveredLocal = GetViewport().GuiGetHoveredControl();

        if (hoveredLocal != null)
            return true;

        var parentViewport = GetParent()?.GetViewport();
        if (parentViewport != null)
        {
            var hoveredParent = parentViewport.GuiGetHoveredControl();
            if (hoveredParent != null)
                return true;
        }

        return false;
    }

    
    private void ApplyErase()
    {
        int altura = (int)SpinBoxAltura.Value;
        var capa = (BlackyRenderLayer)KuroOptionButtonCapa.GetSelectedData();

        var tiles = TilesEntityPreviewHelper.GetOnlyValidTiles();
        var item = tiles[0];

        Brush brush = Brushes.Single;
        switch (brushType)
        {
            case BrushType.Square:
                brush = Brushes.Square(sizeBrush);
                break;
            case BrushType.Circle:
                brush = Brushes.Circle(sizeBrush);
                break;
            case BrushType.Ring:
                brush = Brushes.Ring(sizeBrush);
                break;
            case BrushType.Cross:
                brush = Brushes.Cross(sizeBrush);
                break;
            default:
                break;
        }
        Vector2I currentMouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        switch (capa)
        {
            case BlackyRenderLayer.TerrenoBase:
                BlackyWorldContext.PintarTerreno.SetDualTemplate(dualTileTemplate);
                BlackyWorldContext.PintarTerreno.RemoveTerrain(
                  currentMouseTile.X,
                  currentMouseTile.Y,
                  altura,
                  brush);
                break;
            case BlackyRenderLayer.Rampas:
                BlackyWorldContext.PintarRampas.RemoveTile(currentMouseTile.X, currentMouseTile.Y, altura);
                break;
            case BlackyRenderLayer.Superficie:
                break;
            case BlackyRenderLayer.Caminos:
                break;
            case BlackyRenderLayer.Adornos:
                break;            
        }
        //switch (modePaint)
        //{
        //    case ModePaint.NORMAL:
        //        foreach (var offset in brush.Cells)
        //        {
        //            int x = currentMouseTile.X + offset.x;
        //            int y = currentMouseTile.Y + offset.y;
        //            BlackyWorldContext.PintarTerreno.RemoveTerrain(
        //                    x,
        //                    y,
        //                    altura                            
        //                );
        //        }
                 
        //        break;

        //    case ModePaint.AUTO_DUAL:

        //        BlackyWorldContext.PintarTerreno.ApplyBrushRemoveDual(
        //            currentMouseTile.X,
        //            currentMouseTile.Y,
        //            altura,
        //            capa,
        //            brush,
        //            dualTileTemplate
        //        );
        //        break;
        //}
    }

    private void ApplyPaint()
    {
        int altura = (int)SpinBoxAltura.Value;
        var capa = (BlackyRenderLayer)KuroOptionButtonCapa.GetSelectedData();
        var tiles = TilesEntityPreviewHelper.GetOnlyValidTiles();
        Brush brush = Brushes.Single;
        switch (brushType)
        {
            case BrushType.Square:
                brush = Brushes.Square(sizeBrush);
                break;
            case BrushType.Circle:
                brush = Brushes.Circle(sizeBrush);
                break;
            case BrushType.Ring:
                brush = Brushes.Ring(sizeBrush);
                break;
            case BrushType.Cross:
                brush = Brushes.Cross(sizeBrush);
                break;
            default:
                break;
        }
        Vector2I currentMouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        switch (capa)
        {
            case BlackyRenderLayer.TerrenoBase:                
                BlackyWorldContext.PintarTerreno.SetTerrain(currentMouseTile.X,currentMouseTile.Y,altura,terrainBaseDataSelected,brush);
                break;
            case BlackyRenderLayer.Rampas:
                BlackyWorldContext.PintarRampas.SetRamp(currentMouseTile.X, currentMouseTile.Y, altura, rampsDataSelected, brush);
                
                break;
            case BlackyRenderLayer.Superficie:
                break;
            case BlackyRenderLayer.Caminos:
                break;
            case BlackyRenderLayer.Adornos:
                 break;
            default:
                break;
        }
        //switch (modePaint)
        //{
        //    case ModePaint.NORMAL:
        //        PaintNormal(altura, capa,tiles,brush);
        //        break;
        //    case ModePaint.AUTO_DUAL:
        //        PaintDual(altura, capa, currentMouseTile, brush);
        //        break;
        //    default:
        //        break;
        //}

    }

    //private void PaintDual(int altura, int capa, Vector2I tilePosition, Brush brush)
    //{        
    //    BlackyWorldContext.PintarTerreno.ApplyBrushCreateDual(
    //        tilePosition.X,
    //        tilePosition.Y,
    //        altura,
    //        capa,
    //        brush,
    //        dualTileTemplate
    //    );
    //}

    private void PaintNormal(int altura, BlackyRenderLayer capa, Brush brush)
    {

        //if (tiles.Count == 1) // Si solo hay una celda seleccionada, aplicamos el pincel
        //{
            //var item = tiles[0];
            //foreach (var offset in brush.Cells)
            //{
            //    int x = item.tilePosition.X + offset.x;
            //    int y = item.tilePosition.Y + offset.y;

            //    switch (capa)
            //    {
            //        case BlackyRenderLayer.Rampas:
            //            BlackyWorldContext.PintarRampas.SetTile(x, y, altura, item.data.idMod, (ushort)item.data.index);
            //            break;                    
            //        case BlackyRenderLayer.Adornos:
            //            break;
            //        default:
            //            break;
            //    }                
            //}
        //}
        //else
        //{ //
        //  // si hay varias celdas seleccionadas, aplicamos el pincel a cada una de ellas sin importar el tipo de pincel, para evitar complicaciones
        //    foreach (var item in tiles)
        //    {
        //        switch (capa)
        //        {                    
        //            case BlackyRenderLayer.Rampas:
        //                BlackyWorldContext.PintarRampas.SetTile(item.tilePosition.X, item.tilePosition.Y, altura, item.data.idMod, (ushort)item.data.index);
        //                break;                 
        //            case BlackyRenderLayer.Adornos:
        //                break;
        //            default:
        //                break;
        //        }                
        //    }
        //}

    }
}
