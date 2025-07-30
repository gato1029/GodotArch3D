using Godot;
using GodotEcsArch.sources.managers.Maps;
using System;

public partial class EditorWindow : PanelContainer
{
    private EditorMode selectedTabMode;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI                 
        MapManagerEditor.Instance.editorMode = EditorMode.TERRENO;
        TabContainerItems.TabChanged += TabContainerItems_TabChanged;

      
    }

    private void TabContainerItems_TabChanged(long tab)
    {
        switch (TabContainerItems.CurrentTab)
        {
            case 0:
                selectedTabMode = EditorMode.TERRENO;
                break;
            case 1:
                selectedTabMode = EditorMode.RECURSOS;
                break;
            case 2:
                break;
                selectedTabMode = EditorMode.UNIDADES;
            default:
                break;
        }
        MapManagerEditor.Instance.editorMode = selectedTabMode;
    }
  

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
    
    }
}
