using Godot;
using System;

public partial class UiStats : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
	}

	public void SetIcon(AtlasTexture  texture)
	{
		TextureIcon.Texture = texture;
	}

	public void SetValue(int value)
	{
		LabelValue.Text = value.ToString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
