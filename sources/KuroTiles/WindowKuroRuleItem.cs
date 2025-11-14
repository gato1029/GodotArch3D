using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Group;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;

[KuroRegisterWindow("res://sources/KuroTiles/WindowKuroRuleItem.tscn")]
public partial class WindowKuroRuleItem : Popup
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        ItemsGrupos.ItemSelected += ItemsGrupos_ItemSelected;
        CheckBoxAllGroup.Pressed += CheckBoxAllGroup_Pressed;
        PopupHide += WindowKuroRuleItem_PopupHide;
        ControlListTileSprite.OnNotifyChangued += ControlListTileSprite_OnNotifyChangued;
        OptionButtonUnderRender.ItemSelected += OptionButtonUnderRender_ItemSelected;
        LoadGrupos();
                    
        // Por esta forma correcta de iterar sobre los valores de un enum:
        foreach (UnderNeighborType item in Enum.GetValues(typeof(UnderNeighborType)))
        {
            OptionButtonUnderRender.AddItem(item.ToString());
        }
            
        
    }

    private void OptionButtonUnderRender_ItemSelected(long index)
    {
        ControlRuleKuroItem.tileRuleTemplate.neighborConditionTemplateCenter.UnderNeighborType = (UnderNeighborType)index;
    }

    private void ControlListTileSprite_OnNotifyChangued(ControlListTileSprite objectControl)
    {
        var list = new List<TileTemplate>();
        var g = ControlRuleKuroItem.tileRuleTemplate.TileCentral.idGroup;
        foreach (var item in objectControl.GetIdTiles())
        {
            TileTemplate tileTemplate = new TileTemplate();
            tileTemplate.idTileSprite = item;
            tileTemplate.idGroup = g;
            list.Add(tileTemplate);
        }
        ControlRuleKuroItem.tileRuleTemplate.RandomTiles = list;
        
    }

    private void WindowKuroRuleItem_PopupHide()
    {
        OnNotifyChangued?.Invoke(this);
    }

    public void SetData(TileRuleTemplate tileRuleTemplate)
    {
        if (tileRuleTemplate!=null)
        {
            ControlRuleKuroItem.SetData(tileRuleTemplate);
            ControlListTileSprite.SetData(tileRuleTemplate.RandomTiles);
            ControlRuleKuroItem.SetCentralTile(tileRuleTemplate.TileCentral);
            OptionButtonUnderRender.Selected = (int)tileRuleTemplate.neighborConditionTemplateCenter.UnderNeighborType;
        }
        
    }
    public TileRuleTemplate  GetData()
    {
        return ControlRuleKuroItem.tileRuleTemplate;
    }
    private void CheckBoxAllGroup_Pressed()
    {
        if (CheckBoxAllGroup.ButtonPressed)
        {
            ControlRuleKuroItem.SetWorkingMode(CustomButtonRule.ButtonWorkingMode.AllGroups);
            ItemsGrupos.DeselectAll();
        }
   
    }

    private void ItemsGrupos_ItemSelected(long index)
    {
        CheckBoxAllGroup.ButtonPressed = false;
        int id = (int)ItemsGrupos.GetItemMetadata((int)index);
        Texture2D icon = ItemsGrupos.GetItemIcon((int)index);        
        ControlRuleKuroItem.SetWorkingMode( CustomButtonRule.ButtonWorkingMode.Group,icon,id);
    }

    void LoadGrupos()
	{
		var list = DataBaseManager.Instance.FindAll<GroupData>();
		foreach (var item in list)
		{
			int id  = ItemsGrupos.AddItem(item.name, item.textureVisual);
            ItemsGrupos.SetItemMetadata(id , item.id); 
            ItemsGrupos.SetItemTooltip(id , item.name);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
