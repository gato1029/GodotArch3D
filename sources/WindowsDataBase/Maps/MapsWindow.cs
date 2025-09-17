using Godot;
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
        foreach (MapType item in Enum.GetValues(typeof(MapType)))
        {
            OptionButtonType.AddItem(item.ToString());
        }
    }

    private void ButtonSave_Pressed()
    {
        //DungeonGenerator dungeonGenerator = new DungeonGenerator((int)SpinBoxWidth.Value, (int)SpinBox2Height.Value);
        //dungeonGenerator.GenerateRandomFilled();
        //// Guardar en archivo dentro de user://
        //string path = CommonAtributes.pathMaps + "/" + "dungeonCustom.txt";
        //string fullPath = FileHelper.GetPathGameDB(path);
        //dungeonGenerator.ExportToTextFile(fullPath);

        
        MapLevelData mapLevelData = new MapLevelData(LineEditName.Text, new Vector2I((int)SpinBoxWidth.Value, (int)SpinBox2Height.Value), (MapType)OptionButtonType.Selected, 10, TextEditDescription.Text);
        MapManagerEditor.Instance.CurrentMapLevelData = mapLevelData;
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
