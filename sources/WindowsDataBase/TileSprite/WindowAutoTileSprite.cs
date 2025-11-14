using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.CustomWidgets.Internals;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;


[KuroRegisterWindow("res://sources/WindowsDataBase/TileSprite/WindowAutoTileSprite.tscn")]
public partial class WindowAutoTileSprite : Window, IFacadeWindow<AutoTileSpriteData>
{
    public event IFacadeWindow<AutoTileSpriteData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    AutoTileSpriteData objectData = new AutoTileSpriteData();
    // Called when the node enters the scene tree for the first time.
    public void SetData(AutoTileSpriteData data)
    {
        objectData = data;
        LineEditName.Text = data.name;
        foreach (var item in data.tileRuleTemplates)
        {
            var newRule = (ControlMiniatureRule)GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlMiniatureRule.tscn").Instantiate();
            VBoxContainerItems.AddChild(newRule);
            newRule.SetData(item);
        }
    }
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonAdd.Pressed += ButtonAdd_Pressed;
        ButtonSave.Pressed += ButtonSave_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonDuplicate.Pressed += ButtonDuplicate_Pressed;
    }

    private void ButtonDuplicate_Pressed()
    {
        objectData.ReGerenateId();
        objectData.name = LineEditName.Text;
        objectData.tileRuleTemplates = GetRules();

        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChanguedSimple?.Invoke();

        MasterDataManager.UpdateRegisterData(objectData.id, objectData);        
        //QueueFree();
    }

    private void ButtonDelete_Pressed()
    {
        DataBaseManager.Instance.RemoveDirectById<AutoTileSpriteData>(objectData.id);
        Message.ShowConfirmation(this, "Estas seguro de eliminar toda la regla?").Confirmed+=()=> { OnNotifyChanguedSimple?.Invoke(); QueueFree(); };
        
    }

    private void ButtonSave_Pressed()
    {
        objectData.name = LineEditName.Text;
        objectData.tileRuleTemplates = GetRules();

        DataBaseManager.Instance.InsertUpdate(objectData);
        OnNotifyChanguedSimple?.Invoke();

        MasterDataManager.UpdateRegisterData(objectData.id, objectData);

        Message.ShowMessage(this, "Guardado Exitoso :)!");
        //QueueFree();
    }

    public List<TileRuleTemplate> GetRules()    
    {
        List<TileRuleTemplate > rules = new List<TileRuleTemplate>();
        foreach (var item in VBoxContainerItems.GetChildren())
        {
            var data =(ControlMiniatureRule) item;
            rules.Add(data.GetData());
        }
        return rules;
    }
    private void ButtonAdd_Pressed()
    {
        //var d = KuroWindowFactory.Create<WindowKuroRuleItem>();
        ////d.Show();
        //this.AddChild(d);
        //d.OnNotifyChangued += D_OnNotifyChangued;

        var newRule = (ControlMiniatureRule)GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlMiniatureRule.tscn").Instantiate();
        VBoxContainerItems.AddChild(newRule);          
    }

    private void D_OnNotifyChangued(WindowKuroRuleItem objectControl)
    {
        var newRule = (ControlMiniatureRule)GD.Load<PackedScene>("res://sources/WindowsDataBase/TileSprite/ControlMiniatureRule.tscn").Instantiate();       
        VBoxContainerItems.AddChild(newRule);
        newRule.SetData(objectControl.GetData());
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}


}
