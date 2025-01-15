using Godot;
using System;

public partial class HerramientaMenuBar : MenuBar
{
	
	public override void _Ready()
	{
        PopupMenu editor = GetNode<PopupMenu>("Panel/MarginContainer/VBoxContainer");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
