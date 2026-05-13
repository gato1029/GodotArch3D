using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture.Brushes;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
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

    ModeEditorTerrain modeEditorTerrain = ModeEditorTerrain.SELECCION;
    ModePaint modePaint = ModePaint.NORMAL;

    TileSelectionMatrixData matrixCurrent = null;
    private Vector2I lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
    private bool uiMouseCaptured = false;

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
        KuroButtonBuscarAutomatico.Pressed += KuroButtonBuscarAutomatico_Pressed;
        KuroButtonBorrar.Pressed += KuroButtonBorrar_Pressed;
        KuroButtonCrear.Pressed += KuroButtonCrear_Pressed;
        KuroButtonSeleccion.Pressed += KuroButtonSeleccion_Pressed;
        TipoBrush.OnDataSelected += TipoBrush_OnDataSelected;
        SpinBoxSizeBrush.ValueChanged += SpinBoxSizeBrush_ValueChanged;
        LoadBrushs();
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
                        if (TilesEntityPreviewHelper.GetOnlyValidTiles().Count== 1)
                        {
                            var item =TilesEntityPreviewHelper.GetOnlyValidTiles()[0];
                            
                            TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush),item.data.idMod, item.data.index);
                        }
                        else
                        {
                            TilesEntityPreviewHelper.Create(materialSelected, matrixCurrent);
                        }
                        
                        break;
                    case ModePaint.AUTO_DUAL:
                        TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush, sizeBrush), dualTileTemplate.GetSlot(1).GetData(0).GetPart(0).IdMod, dualTileTemplate.GetSlot(15).GetData(0).GetPart(0).TileIndex);

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

    private void KuroButtonBuscarAutomatico_Pressed()
    {
        var wDual = RuntimeServices.NodeRegistry.Create<WindowRuntimeDualTilesTerrain>();
        wDual.OnSelection += WDual_OnSelection;
        AddChild(wDual);        
        wDual.Popup();
        
    }

    private void WDual_OnSelection(GodotEcsArch.sources.WindowsDataBase.TilesTexture.DualTileTemplate objeto)
    {
        TexturaDual.Texture = objeto.textureVisual;
        LabelDualName.Text = objeto.name;
        modeEditorTerrain = ModeEditorTerrain.CREACION;        
        dualTileTemplate = objeto;
        ConfigModePaint(ModePaint.AUTO_DUAL);
        TilesEntityPreviewHelper.Create(new Vector2I(sizeBrush,sizeBrush), objeto.GetSlot(1).GetData(0).GetPart(0).IdMod, objeto.GetSlot(15).GetData(0).GetPart(0).TileIndex);

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
                TilesEntityPreviewHelper.Create(materialSelected, matrixCurrent);
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
        var w = RuntimeServices.NodeRegistry.Create<RuntimeEditorWindowMaterial>();
        w.OnSelection += W_OnSelection;
        AddChild(w);
        w.SetTipoTexturas(GodotEcsArch.sources.WindowsDataBase.Materials.MaterialType.TERRENO);
        w.Popup();
    }

    private void W_OnSelection(MaterialData materialData, ModInfo modInfo)
    {
        modeEditorTerrain = ModeEditorTerrain.CREACION;
        SetMaterial(materialData);
        ConfigModePaint(ModePaint.NORMAL);
    }

    private void ConfigModePaint(ModePaint mode)
    {
        modePaint = mode;
        switch (mode)
        {
            case ModePaint.NORMAL:
                ContenedorDual.Visible = false;
                EditorTextura.Visible = true;
                break;
            case ModePaint.AUTO_DUAL:
                ContenedorDual.Visible = true;
                EditorTextura.Visible = false;
                break;
            default:
                break;
        }
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
        TilesEntityPreviewHelper.Move(currentMouseTile);

        bool leftPressed = Input.IsMouseButtonPressed(MouseButton.Left);
        bool rightPressed = Input.IsMouseButtonPressed(MouseButton.Right);

        switch (modeEditorTerrain)
        {
            case ModeEditorTerrain.SELECCION:

                break;
            case ModeEditorTerrain.CREACION:
                if (leftPressed)
                {
                    
                    if (matrixCurrent==null && dualTileTemplate==null)
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
        int capa = (int)SpinBoxCapa.Value;

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
        switch (modePaint)
        {
            case ModePaint.NORMAL:
                foreach (var offset in brush.Cells)
                {
                    int x = currentMouseTile.X + offset.x;
                    int y = currentMouseTile.Y + offset.y;
                    BlackyWorldContext.PintarTerreno.RemoveTile(
                            x,
                            y,
                            altura,
                            capa
                        );
                }
                 
                break;

            case ModePaint.AUTO_DUAL:

                BlackyWorldContext.PintarTerreno.ApplyBrushRemoveDual(
                    currentMouseTile.X,
                    currentMouseTile.Y,
                    altura,
                    capa,
                    brush,
                    dualTileTemplate
                );
                break;
        }        
    }
    
    private void ApplyPaint()
    {
        int altura = (int)SpinBoxAltura.Value;
        int capa = (int)SpinBoxCapa.Value;
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
        switch (modePaint)
        {
            case ModePaint.NORMAL:
                PaintNormal(altura, capa,tiles,brush);
                break;
            case ModePaint.AUTO_DUAL:
                PaintDual(altura, capa, currentMouseTile, brush);
                break;
            default:
                break;
        }

    }

    private void PaintDual(int altura, int capa, Vector2I tilePosition, Brush brush)
    {        
        BlackyWorldContext.PintarTerreno.ApplyBrushCreateDual(
            tilePosition.X,
            tilePosition.Y,
            altura,
            capa,
            brush,
            dualTileTemplate
        );
    }

    private void PaintNormal(int altura, int capa, List<TilePreviewWithPosition> tiles, Brush brush)
    {
        if (tiles.Count==1) // Si solo hay una celda seleccionada, aplicamos el pincel
        {
            var item = tiles[0];
            foreach (var offset in brush.Cells)
            {
                int x = item.tilePosition.X + offset.x;
                int y = item.tilePosition.Y + offset.y;
                BlackyWorldContext.PintarTerreno.SetTile(
                      x,
                      y,
                      altura,
                      capa,
                      item.data.idMod,
                      (ushort)item.data.index
                  );
            }
        }
        else
        { //
          // si hay varias celdas seleccionadas, aplicamos el pincel a cada una de ellas sin importar el tipo de pincel, para evitar complicaciones
            foreach (var item in tiles)
            {
                BlackyWorldContext.PintarTerreno.SetTile(
                    item.tilePosition.X,
                    item.tilePosition.Y,
                    altura,
                    capa,
                    item.data.idMod,
                    (ushort)item.data.index
                );
            }
        }
      
    }
}
