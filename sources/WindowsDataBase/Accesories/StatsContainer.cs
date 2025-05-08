using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using System;
using System.Collections.Generic;

public partial class StatsContainer : PanelContainer
{
	[Export]
	Button buttonNew;

    [Export]
    VBoxContainer boxContainer;

    PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ControlStats.tscn");

    public List<StatsData> GetAllStats()
    {
        List<StatsData> stats = new List<StatsData>();
        foreach (var item in boxContainer.GetChildren())
        {
            var data =(ControlStats)item;

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
        var node = packedScene.Instantiate<ControlStats>();
        boxContainer.AddChild(node);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    internal void SetAllData(StatsData[] statsDatas)
    {
        if (statsDatas != null)
        {
            foreach (var item in statsDatas)
            {
                var node = packedScene.Instantiate<ControlStats>();
                node.SetData(item);
                boxContainer.AddChild(node);
            }
        }
    }
}
