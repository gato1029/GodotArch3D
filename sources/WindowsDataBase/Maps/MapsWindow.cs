using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.serializer;
using GodotEcsArch.sources.utils;
using System;

public partial class MapsWindow : Window
{
    MapLevelData mapLevelData;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonSave.Pressed += ButtonSave_Pressed;
        mapLevelData = new MapLevelData();

    }

    private void ButtonSave_Pressed()
    {
        mapLevelData.size = new Vector2I((int)SpinBoxWidth.Value, (int)SpinBox2Height.Value);
        mapLevelData.name = LineEditName.Text;
        mapLevelData.description = TextEditDescription.Text;

        string path = CommonAtributes.pathMaps;
        string pathCarpet = FileHelper.GetPathGameDB(path);

        SerializerManager.SaveToFileJson(mapLevelData, pathCarpet, mapLevelData.name);
        
        MapManagerEditor.Instance.currentMapLevelData = mapLevelData;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
