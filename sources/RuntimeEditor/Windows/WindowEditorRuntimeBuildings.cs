using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;

public partial class WindowEditorRuntimeBuildings : Window
{
    enum ModeEditorBuilding
    {
        SELECCION,
        CREACION,
        ELIMINACION,
    }
    ModeEditorBuilding modeEditor = ModeEditorBuilding.CREACION;
    private Vector2I lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
    private bool uiMouseCaptured = false;
    BuildingData selectedBuilding = null;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        KuroOptionButtonMod.OnDataSelected += KuroOptionButtonMod_OnDataSelected;
        LoadMods();
        KuroControlWindowItem.OnCloseWindow += KuroControlWindowItem_OnCloseWindow;
    }

    private void KuroControlWindowItem_OnCloseWindow(object obj)
    {
        TilesEntityPreviewHelper.Clear();
    }



    private void LoadMods()
    {
        IEnumerable<KeyValuePair<ushort, ModInfo>> items = TableMods.Instance.ObtenerTodos();
        ModInfo last = null;
        foreach (var item in items)
        {
            KuroOptionButtonMod.AddItemWithData(item.Value.Name, item.Value);
            last = item.Value;
        }
        KuroOptionButtonMod_OnDataSelected(last);
    }

    private void KuroOptionButtonMod_OnDataSelected(object obj)
    {
        var modInfo = (ModInfo)obj;
        var list = AtlasModsManager.GetAll<BuildingData>(modInfo.Name);
        foreach (var item in list)
        {
            long idTileSprite = item.IdTileSpriteNormal;

            int spriteId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);
            AtlasModsManager.TryGetTileSprite(spriteId, out var sprite);
            CreateItem(sprite, item);
        }
    }
    private void CreateItem(TileSpriteData tileSpriteData, object obj)
    {
        var node = RuntimeServices.NodeRegistry.Create<TileSpritePreview>();
        GridContainerItems.AddChild(node);
        node.LoadData(tileSpriteData, obj);
        node.OnItemSelectedChanged += Node_OnItemSelected;
    }
    
    private void Node_OnItemSelected(TileSpriteData objectControl, object dataInternal)
    {
        selectedBuilding = (BuildingData)dataInternal;
        TilesEntityPreviewHelper.Create(new Vector2I(1, 1), objectControl.id);
    }

    public override void _Process(double delta)
    {
        if (IsMouseBlockedByUI())
            return;
        Vector2I currentMouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;

        Vector2 offsetVisual = Vector2.Zero;

        TilesEntityPreviewHelper.Move(currentMouseTile, offsetVisual);

        bool leftPressed = Input.IsMouseButtonPressed(MouseButton.Left);
        bool rightPressed = Input.IsMouseButtonPressed(MouseButton.Right);

        switch (modeEditor)
        {
            case ModeEditorBuilding.SELECCION:

                break;
            case ModeEditorBuilding.CREACION:
                if (leftPressed)
                {
                    if (currentMouseTile != lastPaintTile && selectedBuilding != null)
                    {
                        ushort idPersist = BlackyPalletesPersistence.buildingPalette.GetIdPersistence(selectedBuilding.nameMod, selectedBuilding.id, out _);
                        BlackyWorldContext.Services.BuildingPainter.RequestCreate(idPersist, currentMouseTile, 1);
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
                        BlackyWorldContext.Services.BuildingPainter.RequestRemove(currentMouseTile);
                        lastPaintTile = currentMouseTile;
                    }
                    else
                    {
                        lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
                    }
                }

                break;
            case ModeEditorBuilding.ELIMINACION:
                if (leftPressed || rightPressed)
                {
                    if (currentMouseTile != lastPaintTile)
                    {
                        BlackyWorldContext.Services.ResourcePainter.RemoveResource(currentMouseTile);
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
        var hoveredLocal = GetViewport().GuiGetHoveredControl();

        if (hoveredLocal != null)
            return true;

        var parentViewport = GetParent()?.GetViewport();
        if (parentViewport != null)
        {
            //var hoveredParent = parentViewport.GuiGetHoveredControl();
            Control hovered = parentViewport.GuiGetHoveredControl();

            while (hovered != null)
            {
                if (hovered.IsInGroup("BloqueaMundo"))
                    return true;

                hovered = hovered.GetParent() as Control;
            }
        }

        return false;
    }
}
