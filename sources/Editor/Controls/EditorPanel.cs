using Godot;
using ProtoBuf.WellKnownTypes;
using System;

public partial class EditorPanel : VBoxContainer
{
    bool isCollapse =false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonHidde.Pressed += ButtonHidde_Pressed;
        AnimatedPanel.CreateNewTween().AnimatedHide(0.6);
        isCollapse = true;
    }

    private void ButtonHidde_Pressed()
    {
        if (isCollapse)
        {
            AnimatedPanel.CreateNewTween().AnimatedShow(0.6);
            isCollapse = false;
           
        }
        else
        {
            AnimatedPanel.CreateNewTween().AnimatedHide(0.6);
            isCollapse = true;
        }
        

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
	
}
