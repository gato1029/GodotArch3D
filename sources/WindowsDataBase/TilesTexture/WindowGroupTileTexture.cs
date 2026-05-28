using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.IO;
using static ControlEditorTerrain;
using static System.Formats.Asn1.AsnWriter;

public partial class WindowGroupTileTexture : Window
{
    MaterialData material;
    WindowSearchTileMaterial windowLocal = null;
    AutomapperData dataParent = null;
    

    // ventana compartida de tiles
    private WindowSearchTileMaterial sharedWindow;
    private TileTextureRuleControl currentRequester;
    private TileTextureControl currentRequesterAlter;

    AutoTilePhase ruleTextureData;
    int positionIndex = -1;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI();
        ruleTextureData = new AutoTilePhase();
        KuroTextureButtonAdd.Pressed += KuroTextureButtonAdd_Pressed;
        KuroTextureButtonSearch.Pressed += KuroTextureButtonSearch_Pressed;
        KuroCheckButtonSwitch.Pressed += KuroCheckButtonSwitch_Pressed;
        KuroTextureButtonSave.Pressed += KuroTextureButtonSave_Pressed;
        Contenedor.ChildOrderChanged += () =>
        {
            CallDeferred(nameof(UpdatePositions));
        };
    }

    private void KuroTextureButtonSave_Pressed()
    {
        ruleTextureData.rules = GetAllRules();
        ruleTextureData.name = LineEditName.Text;
        ruleTextureData.order = positionIndex;
        dataParent.Phases[positionIndex] = ruleTextureData;        
        WindowAutomapper parent = GetParent() as WindowAutomapper;
        parent.RefreshUI();
        QueueFree();
    }

    private void KuroCheckButtonSwitch_Pressed()
    {
        foreach (var item in Contenedor.GetChildren())
        {
            if (item is RuleTextureControl rule)
            {
                rule.SwitchTypeRules(KuroCheckButtonSwitch.ButtonPressed);
            }
        }
    }

    public void OpenMaterialWindow(TileTextureControl requester, Vector2I position)
    {
        currentRequesterAlter = requester;
        currentRequester = null;
        //sharedWindow.Popup();
        //sharedWindow.Position = position;

        if (requester.HasMaterial())
        {
            sharedWindow.SetSelection(requester.GetMaterialId(), requester.GetTileIndex());
        }
    }

    public void OpenMaterialWindow(TileTextureRuleControl requester, Vector2I position)
    {
        currentRequester = requester;
        currentRequesterAlter = null;
        //sharedWindow.Position = position;
        //sharedWindow.Popup();

        if (requester.HasMaterial())
        {
            sharedWindow.SetSelection(requester.GetMaterialId(), requester.GetTileIndex());
        }
    }
    public void SetClearCurrent()
    {
        if (currentRequesterAlter != null)
        {
            currentRequesterAlter.StopEditing();
        }
        if (currentRequester != null)
        {
            currentRequester.StopEditing();
        }
        currentRequester = null;
        currentRequesterAlter = null;
    }
    public void SetCurrent(TileTextureRuleControl newControl)
    {
        // apagar anterior
        if (currentRequester != null && currentRequester != newControl)
        {
            currentRequester.StopEditing();
        }

        if (currentRequesterAlter != null)
        {
            currentRequesterAlter.StopEditing();
        }
        currentRequester = newControl;
    }

    public void SetCurrent(TileTextureControl newControl)
    {
        // apagar anterior
        if (currentRequesterAlter != null && currentRequesterAlter != newControl)
        {
            currentRequesterAlter.StopEditing();
        }

        if (currentRequester != null)
        {
            currentRequester.StopEditing();
        }
        currentRequesterAlter = newControl;
    }

    private void OnMaterialSelected(int index, MaterialData materialData)
    {
        ruleTextureData.materialId = materialData.id;

        if (currentRequester != null)
        {
            currentRequester.SetMaterialData(materialData.id, index);
        }
        if (currentRequesterAlter != null)
        {
            currentRequesterAlter.SetMaterialData(materialData.id, index);
        }
    }

    FacadeWindowDataSearch<MaterialData> windowQuery = null;

    private void KuroTextureButtonSearch_Pressed()
    {
        if (windowQuery == null || sharedWindow == null)
        {
            windowQuery = new FacadeWindowDataSearch<MaterialData>(
                "res://sources/WindowsDataBase/Materials/windowNewMaterial.tscn",
                this,
                WindowType.SELECTED
            );
            windowQuery.OnNotifySelected += WindowQuery_OnNotifySelected;
        }
    }

    private void WindowQuery_OnNotifySelected(MaterialData objectSelected)
    {
        if (sharedWindow == null)
        {
            sharedWindow = GD.Load<PackedScene>(
                    "res://sources/KuroTiles/WindowSearchTileMaterial.tscn"
                )
                .Instantiate<WindowSearchTileMaterial>();
            AddChild(sharedWindow); // o GetTree().Root si quieres flotante global
            sharedWindow.SetAlwaysOpen();
        }

        // Posición y tamaño del window actual
        Vector2 parentPos = this.Position;
        Vector2 parentSize = this.Size;

        sharedWindow.Popup();

        // Posicionar a la derecha del window padre
        Vector2 finalPos = new Vector2(
            parentPos.X + parentSize.X,
            parentPos.Y
        );

        sharedWindow.CallDeferred(MethodName.SetPosition, finalPos);

        //sharedWindow.Hide();
        sharedWindow.OnNotifySelectionIndex += OnMaterialSelected;

        material = objectSelected;
        LineEditMaterial.Text = material.name;
        sharedWindow.SetSelection(material.id, 0);
        UpdateMaterial();
        //materialSelected = material;
        //EditorTile();
    }

    private void WindowLocal_OnNotifySelectionIndex(int index, MaterialData materialData)
    {
        throw new NotImplementedException();
    }

    private void KuroTextureButtonAdd_Pressed()
    {
        var scene = GD.Load<PackedScene>(
            "res://sources/WindowsDataBase/TilesTexture/RuleTextureControl.tscn"
        );
        var widget = scene.Instantiate<RuleTextureControl>();
        Contenedor.AddChild(widget);        
        widget.SetGroupParent(this);
        if (material!=null)
        {
            widget.SetMaterial(material.id);
        }
    }

    private void UpdateMaterial()
    {
        int index = 0;

        foreach (var child in Contenedor.GetChildren())
        {
            if (child is RuleTextureControl item)
            {
                item.SetMaterial(material.id);
            }
        }
    }

    private void UpdatePositions()
    {
        int index = 0;

        foreach (var child in Contenedor.GetChildren())
        {
            if (child is RuleTextureControl item)
            {
                item.SetPosition(index);
                index++;
            }
        }
    }

    public void SetAllRules(List<TileRuleTextureData> tileRuleTextureDatas)
    {
        for (int i = 0; i < tileRuleTextureDatas.Count; i++)
        {
            TileRuleTextureData item = tileRuleTextureDatas[i];
            var scene = GD.Load<PackedScene>(
                "res://sources/WindowsDataBase/TilesTexture/RuleTextureControl.tscn"
            );
            var widget = scene.Instantiate<RuleTextureControl>();
            Contenedor.AddChild(widget);
            widget.SetGroupParent(this);

            widget.SetData(tileRuleTextureDatas[i], ruleTextureData.materialId);
        }
    }

    public List<TileRuleTextureData> GetAllRules()
    {
        List<TileRuleTextureData> list = new List<TileRuleTextureData>();
        foreach (var item in Contenedor.GetChildren())
        {
            if (item is RuleTextureControl tileRule)
            {
                list.Add(tileRule.GetData());
            }
        }
        return list;
    }



    internal void SetDisableSelection()
    {
        if (currentRequester != null)
        {
            currentRequester.StopEditing();
        }
        if (currentRequesterAlter != null)
        {
            currentRequesterAlter.StopEditing();
        }
        currentRequester = null;
        currentRequesterAlter = null;
    }

    internal void SetData(AutoTilePhase element, int materialId)
    {
        LineEditName.Text = element.name;
        ruleTextureData = element;
        material = MasterDataManager.GetData<MaterialData>(materialId);
        WindowQuery_OnNotifySelected(material);
        foreach (var item in ruleTextureData.rules)
        {
            var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TilesTexture/RuleTextureControl.tscn");
            var widget = scene.Instantiate<RuleTextureControl>();
            Contenedor.AddChild(widget);
            widget.SetGroupParent(this);
            widget.SetData(item, material.id);
        }        
    }


    internal void SetParentData(AutomapperData data, int index)
    {
        dataParent = data;
        positionIndex = index;
    }



    /// <summary>
    ///     

    private void EditorTile()
    {
        modeEditorTerrain = ModeEditorTerrain.ELIMINACION;
        var temp = AtlasModsManager.Get<MaterialData>("Base", 1);
        TilesEntityPreviewHelper.Create(new Vector2I(1, 1), materialSelected, 0);
    }
    enum ModeEditorTerrain
    {
        SELECCION,
        CREACION,
        ELIMINACION,
    }

    private MaterialData materialSelected;
    

    ModeEditorTerrain modeEditorTerrain = ModeEditorTerrain.SELECCION;
    TileSelectionMatrixData matrixCurrent = null;
    private Vector2I lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
    private bool uiMouseCaptured = false;
    public override void _Process(double delta)
    {
        //if (IsMouseBlockedByUI())
        //    return;
        //Vector2I currentMouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        //TilesEntityPreviewHelper.Move(currentMouseTile);

        //bool leftPressed = Input.IsMouseButtonPressed(MouseButton.Left);
        //bool rightPressed = Input.IsMouseButtonPressed(MouseButton.Right);

        //switch (modeEditorTerrain)
        //{
        //    case ModeEditorTerrain.SELECCION:

        //        break;
        //    case ModeEditorTerrain.CREACION:
        //        if (leftPressed)
        //        {
        //            if (matrixCurrent == null)
        //            {
        //                return;
        //            }
        //            if (currentMouseTile != lastPaintTile)
        //            {
        //                ApplyPaint();
        //                lastPaintTile = currentMouseTile;
        //            }
        //            else
        //            {
        //                lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
        //            }
        //        }
        //        if (rightPressed)
        //        {
        //            if (currentMouseTile != lastPaintTile)
        //            {
        //                ApplyErase();
        //                lastPaintTile = currentMouseTile;
        //            }
        //            else
        //            {
        //                lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
        //            }
        //        }

        //        break;
        //    case ModeEditorTerrain.ELIMINACION:
        //        if (leftPressed || rightPressed)
        //        {
        //            if (currentMouseTile != lastPaintTile)
        //            {
        //                ApplyErase();
        //                lastPaintTile = currentMouseTile;
        //            }
        //            else
        //            {
        //                lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
        //            }
        //        }
        //        break;
        //    default:
        //        break;
        //}
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
        int altura = 1;
        int capa = 1;
        var tiles = TilesEntityPreviewHelper.GetOnlyValidTiles();
        //foreach (var item in tiles)
        //{
        //    BlackyWorldContext.PintarTerreno.RemoveTile(
        //        item.tilePosition.X,
        //        item.tilePosition.Y,
        //        altura,
        //        capa
        //    );
        //}
    }

    private void ApplyPaint()
    {
        int altura = 1;
        int capa = 1;
        var tiles = TilesEntityPreviewHelper.GetOnlyValidTiles();
        //foreach (var item in tiles)
        //{
        //    BlackyWorldContext.PintarTerreno.SetTile(
        //        item.tilePosition.X,
        //        item.tilePosition.Y,
        //        altura,
        //        capa,
        //        item.data.idMod,
        //        (ushort)item.data.index
        //    );
        //}
    }
}
