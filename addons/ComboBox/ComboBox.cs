using Godot;
using System;

[GlobalClass] // Esto hace que el nodo aparezca en la lista del editor
public partial class ComboBox : OptionButton
{
	public object dataInternal { get; set; }
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
