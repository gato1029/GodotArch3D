using Godot;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using System;
using System.Collections.Generic;

public partial class BonusContainer : PanelContainer
{
    [Export]
    Button buttonNew;

    [Export]
    VBoxContainer boxContainer;

    PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Accesories/ControlBonus.tscn");

    public List<BonusData> GetAllStats()
    {
        List<BonusData> stats = new List<BonusData>();
        foreach (var item in boxContainer.GetChildren())
        {
            var data = (ControlBonus)item;

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
        var node = packedScene.Instantiate<ControlBonus>();
        boxContainer.AddChild(node);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    internal void SetAllData(BonusData[] bonusDatas)
    {
        if (bonusDatas != null)
        {
            foreach (var item in bonusDatas)
            {
                var node = packedScene.Instantiate<ControlBonus>();
                node.SetData(item);
                boxContainer.AddChild(node);
            }
        }
    }
}