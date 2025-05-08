using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;

public partial class ElementsContainer : PanelContainer
{
    [Export]
    Button buttonNew;

    [Export]
    VBoxContainer boxContainer;

    PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ControlElements.tscn");



    public List<ElementsData> GetAllStats()
    {
        List<ElementsData> stats = new List<ElementsData>();
        foreach (var item in boxContainer.GetChildren())
        {
            var data = (ControlElements)item;

            stats.Add(data.GetData());

        }
        return stats;
    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        buttonNew.Pressed += ButtonNew_Pressed;

    }

    private void ButtonNew_Pressed()
    {
        var node = packedScene.Instantiate<ControlElements>();
        boxContainer.AddChild(node);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    internal void SetAllData(ElementsData[] elementsDatas)
    {
        if (elementsDatas!=null)
        {
            foreach (var item in elementsDatas)
            {
                var node = packedScene.Instantiate<ControlElements>();
                node.SetData(item);
                boxContainer.AddChild(node);
            }
        }
        
    }
}
