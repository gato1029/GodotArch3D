using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.utils;
using System;

public partial class MapsWindow : Window
{
    
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonSave.Pressed += ButtonSave_Pressed;        
    }

    private void ButtonSave_Pressed()
    {
        MapLevelData mapLevelData = new MapLevelData(LineEditName.Text, new Vector2I((int)SpinBoxWidth.Value, (int)SpinBox2Height.Value), MapType.Mapa, 10,TextEditDescription.Text);                      
        MapManagerEditor.Instance.CurrentMapLevelData = mapLevelData;
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
