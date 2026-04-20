using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Generic.Facade;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TileCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
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
        dataParent.Phases[positionIndex] = ruleTextureData;
        DataBaseManager.Instance.InsertUpdate(dataParent);
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

        sharedWindow.Popup();
        //sharedWindow.Hide();
        sharedWindow.OnNotifySelectionIndex += OnMaterialSelected;

        material = objectSelected;
        LineEditMaterial.Text = material.name;
        sharedWindow.SetSelection(material.id, 0);
        UpdateMaterial();
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

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

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
        ruleTextureData = element;
        material = MasterDataManager.GetData<MaterialData>(materialId);

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
}
