using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using System;
using System.ComponentModel.Design;

public partial class MenuEditorContenedor : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonNuevoMapa.Pressed += ButtonNuevoMapa_Pressed;
        ButtonGuardarMapa.Pressed += ButtonGuardarMapa_Pressed;
        ButtonGuardarComoMapa.Pressed += ButtonGuardarComoMapa_Pressed;
        ButtonEliminarMapa.Pressed += ButtonEliminarMapa_Pressed;
	}

    private void ButtonEliminarMapa_Pressed()
    {
        
    }

    private void ButtonGuardarComoMapa_Pressed()
    {
        
    }

    private void ButtonGuardarMapa_Pressed()
    {
        BlackyWorldContext.Persistence.SaveAllDirtyRegions();
    }

    private void ButtonNuevoMapa_Pressed()
    {
       var window = RuntimeServices.NodeRegistry.Create<MapsWindow>();
       AddChild(window);
       window.Popup();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
