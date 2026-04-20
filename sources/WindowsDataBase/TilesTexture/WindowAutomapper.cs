using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class WindowAutomapper : Window, IFacadeWindow<AutomapperData>
{
    public event IFacadeWindow<AutomapperData>.EventNotifyChanguedSimple OnNotifyChanguedSimple;

    AutomapperData data;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        data = new AutomapperData();
        data.ReGerenateId();
        ButtonAdd.Pressed += ButtonAdd_Pressed;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
        ButtonSave.Pressed += ButtonSave_Pressed;
        ItemsContainer.ItemSelected += ItemsContainer_ItemSelected;
	}

    private void ItemsContainer_ItemSelected(long indexExt)
    {
        int index = ItemsContainer.GetSelectedItems()[indexExt];
        AutoTilePhase element = null;
        foreach (var item in data.Phases)
        {
            if (item.name == ItemsContainer.GetItemText(index))
            {
                element = data.Phases[index];
                
                break;
            }
        }

        var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TilesTexture/WindowGroupTileTexture.tscn");
        var widget = scene.Instantiate<WindowGroupTileTexture>();
        widget.SetData(element,element.materialId);
        widget.SetParentData(data,index);
        AddChild(widget);
        widget.Popup(); ;
    }

    public void ButtonSave_Pressed()
    {
        foreach (var item in ItemsContainer.GetChildren())
        {

        }
        DataBaseManager.Instance.InsertUpdate<AutomapperData>(data);
    }

    private void ButtonDelete_Pressed()
    {
        int index = ItemsContainer.GetSelectedItems()[0];
        foreach (var item in data.Phases)
        {
            if (item.name == ItemsContainer.GetItemText(index))
            {
                data.Phases.Remove(item);
                ItemsContainer.RemoveItem(index);
                break;
            }
        }
        
    }
  
    private void ButtonAdd_Pressed()
    {
        data.Phases.Add(new AutoTilePhase());
        var scene = GD.Load<PackedScene>("res://sources/WindowsDataBase/TilesTexture/WindowGroupTileTexture.tscn");
        var widget = scene.Instantiate<WindowGroupTileTexture>();        
        AddChild(widget);
        
        widget.Popup();
        widget.SetParentData(data,1);
       
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    void IFacadeWindow<AutomapperData>.SetData(AutomapperData data)
    {
        this.data = data;
        foreach (var item in data.Phases)
        {
            ItemsContainer.AddItem(item.name);
        }
    }
}
