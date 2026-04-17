using Godot;
using System;

public partial class WindowGroupTileTexture : Window
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
		SceneRegistry.Register(this);
        KuroTextureButtonAdd.Pressed += KuroTextureButtonAdd_Pressed;
	}

    private void KuroTextureButtonAdd_Pressed()
    {
        var widget = SceneRegistry.Instantiate<RuleTextureControl>();
        Contenedor.AddChild(widget);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
