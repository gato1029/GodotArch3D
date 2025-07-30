using Godot;
using GodotEcsArch.sources.managers.Maps;
using System;

public partial class ViewPortContainerEditor : SubViewportContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        FocusEntered += ViewPortContainerEditor_FocusEntered;
        FocusExited += ViewPortContainerEditor_FocusExited;
        MouseExited += ViewPortContainerEditor_MouseExited;
        MouseEntered += ViewPortContainerEditor_MouseEntered;
	}

    private void ViewPortContainerEditor_MouseEntered()
    {
        MapManagerEditor.Instance.enableEditor = true;
    }

    private void ViewPortContainerEditor_MouseExited()
    {
        MapManagerEditor.Instance.enableEditor = false;
    }

    private void ViewPortContainerEditor_FocusExited()
    {
        MapManagerEditor.Instance.enableEditor = false;        
    }

    private void ViewPortContainerEditor_FocusEntered()
    {
        MapManagerEditor.Instance.enableEditor = true;        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
