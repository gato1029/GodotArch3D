using Godot;
using GodotEcsArch.sources.managers.Maps;
using System;

public partial class EditorWindow : Window
{
    private EditorMode selectedTabMode;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        TabContainerItems.TabSelected += TabContainerItems_TabSelected;
	}

    private void TabContainerItems_TabSelected(long tab)
    {
        selectedTabMode = (EditorMode)(int)tab;
        MapManagerEditor.Instance.editorMode= selectedTabMode;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
