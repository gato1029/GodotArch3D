using Godot;
using GodotEcsArch.sources.Helpers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;

public partial class WindowMaterialTiles : Window
{
    public event Action<TileSpriteData> OnItemSelected;
    // Called when the node enters the scene tree for the first time.

    private bool AllMods = false;
    private bool EditionMode = false;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        
        KuroOptionButtonMod.OnDataSelected += KuroOptionButtonMod_OnDataSelected;
        KuroOptionButtonMaterial.OnDataSelected += KuroOptionButtonMaterial_OnDataSelected;
        AllMods = ModHelper.AllMods;
        ButtonAdd.Pressed += ButtonAdd_Pressed;
        LoadMods();
    }

    private void ButtonAdd_Pressed()
    {
        WindowTileSprite wm = RuntimeServices.NodeRegistry.Create<WindowTileSprite>();
        AddChild(wm);
        wm.Show();
    }

    public void EnableEditionMode()
    {
        EditionMode = true;
        ButtonAdd.Visible = true;
    }

    private void KuroOptionButtonMaterial_OnDataSelected(object obj)
    {
        var mat = (MaterialData)obj;
        var info = (ModInfo)KuroOptionButtonMod.GetSelectedData();
        if (AllMods)
        {
            var list =AtlasModsManager.GetAllSpriteByMaterial(info.Name,mat.id);
            foreach (var item in list)
            {
                CreateItemAtlas(item);
            }
        }
        else
        {
            var list = DataBaseManager.Instance.FindAllByMaterial<TileSpriteData>(mat.id);
            foreach (var item in list)
            {
                CreateItem(item);
            }
        }
    }
    private void CreateItem(TileSpriteData tileSpriteData)
    {        
        var node = RuntimeServices.NodeRegistry.Create<TileSpritePreview>();        
        GridContainerItems.AddChild(node);
        node.LoadData(tileSpriteData);
        node.OnItemSelected += Node_OnItemSelected;
    }

    private void Node_OnItemSelected(TileSpriteData obj)
    {
        if (EditionMode)
        {
            WindowTileSprite wm = RuntimeServices.NodeRegistry.Create<WindowTileSprite>();
            AddChild(wm);
            wm.SetData(obj);
            wm.OnNotifyChanguedSimple += Wm_OnNotifyChanguedSimple;
        }
        else
        {
            OnItemSelected?.Invoke(obj);
            QueueFree();
        }
        
    }

    private void Wm_OnNotifyChanguedSimple()
    {
        GridContainerItems.ClearChildrens();        
        KuroOptionButtonMaterial_OnDataSelected(KuroOptionButtonMaterial.GetSelectedData());
    }
    

    private void CreateItemAtlas(int idSprite)
    {
        AtlasModsManager.TryGetTileSprite(idSprite, out var tileSpriteData);
        CreateItem(tileSpriteData);
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
        var list = AtlasModsManager.GetAll<MaterialData>(modInfo.Name);
        foreach (var item in list)
        {
            KuroOptionButtonMaterial.AddItemWithData(item.name, item);
        }
    }

    
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
