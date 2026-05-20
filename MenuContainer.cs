using Godot;
using GodotEcsArch.sources.Helpers;
using System;

public partial class MenuContainer : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonDualGrid.Pressed += ButtonDualGrid_Pressed;
        ButtonTerreno.Pressed += ButtonTerreno_Pressed;
    }

    private void ButtonTerreno_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control =  RuntimeServices.NodeRegistry.Create<RuntimeTerrainControl>();        
        ContenedorEditor.AddChild(control);
    }

    private void ButtonDualGrid_Pressed()
    {
        ContenedorEditor.ClearChildrens();
        var control =  RuntimeServices.NodeRegistry.Create<ControlDualGrid>();        
        ContenedorEditor.AddChild(control);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
