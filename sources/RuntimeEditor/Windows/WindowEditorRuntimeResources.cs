using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.godot;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections.Generic;


public partial class WindowEditorRuntimeResources : Window
{
    enum ModeEditorResource
    {
        SELECCION,
        CREACION,
        ELIMINACION,
    }
    ModeEditorResource modeEditor= ModeEditorResource.CREACION; 
    private Vector2I lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
    private bool uiMouseCaptured = false;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        KuroOptionButtonMod.OnDataSelected += KuroOptionButtonMod_OnDataSelected;                        
        LoadMods();        
        KuroControlWindowItem.OnCloseWindow += KuroControlWindowItem_OnCloseWindow;
        RTSSelectionManager.Instance.IsEnabled = false;
        
    }

    

    private void KuroControlWindowItem_OnCloseWindow(object obj)
    {
        RTSSelectionManager.Instance.IsEnabled = true;
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
        var list = AtlasModsManager.GetAll<ResourceSourceData>(modInfo.Name);
        foreach (var item in list)
        {
            long idTileSprite = item.listIdTileSpriteData[0];

            int spriteId = AtlasModsManager.GetSpriteUniqueId(idTileSprite);
            AtlasModsManager.TryGetTileSprite(spriteId, out var sprite);
            CreateItem(sprite,item);
        }
    }
    private void CreateItem(TileSpriteData tileSpriteData,object obj)
    {
        var node = RuntimeServices.NodeRegistry.Create<TileSpritePreview>();
        GridContainerItems.AddChild(node);
        node.LoadData(tileSpriteData,obj);
        node.OnItemSelectedChanged += Node_OnItemSelected;
    }
    ResourceSourceData selectedResource =null;
    private void Node_OnItemSelected(TileSpriteData objectControl, object dataInternal)
    {
        selectedResource = (ResourceSourceData)dataInternal;
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
            case ModeEditorResource.SELECCION:

                break;
            case ModeEditorResource.CREACION:
                if (leftPressed)
                {                    
                    if (currentMouseTile != lastPaintTile && selectedResource!=null)
                    {
                        ushort idPersist =BlackyPalletesPersistence.resourcesPalette.GetIdPersistence(selectedResource.nameMod, selectedResource.id,out _);                        
                        BlackyWorldContext.Services.ResourcePainter.CreateResource(idPersist,currentMouseTile,1);             
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
                        BlackyWorldContext.Services.ResourcePainter.RemoveResource(currentMouseTile);
                        lastPaintTile = currentMouseTile;
                    }
                    else
                    {
                        lastPaintTile = new Vector2I(int.MinValue, int.MinValue);
                    }
                }

                break;
            case ModeEditorResource.ELIMINACION:
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
