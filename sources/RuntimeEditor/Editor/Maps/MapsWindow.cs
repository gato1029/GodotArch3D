using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.utils;

using System;
using System.Xml.Linq;

public partial class MapsWindow : Window
{
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonSave.Pressed += ButtonSave_Pressed;
        foreach (BlackyWorldTypeDetail item in Enum.GetValues(typeof(BlackyWorldTypeDetail)))
        {
            OptionButtonType.AddItem(item.ToString());
        }
    }

    private void ButtonSave_Pressed()
    {           
        BlackyWorld blackyWorld = new BlackyWorld(LineEditName.Text, (BlackyWorldTypeDetail)OptionButtonType.Selected, 32, 5, 41245134, new Vector2I((int)SpinBoxWidth.Value, (int)SpinBox2Height.Value));
        QueueFree();
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
