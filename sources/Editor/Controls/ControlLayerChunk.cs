using Godot;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using System;

public partial class ControlLayerChunk : PanelContainer
{
	private ISpriteMapChunk spriteMapChunk;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
	}

	public void SetSpriteMapChunk(ISpriteMapChunk spriteMapChunk)
	{
		this.spriteMapChunk = spriteMapChunk;
        this.spriteMapChunk.OnRenderingInstanceCountChanged += SpriteMapChunk_OnRenderingInstanceCountChanged;
        this.spriteMapChunk.OnRealInstanceCountChanged += SpriteMapChunk_OnRealInstanceCountChanged;

        CheckBoxEnable.Text = spriteMapChunk.GetName();
       // CheckBoxEnable.ButtonPressed = true;
        CheckBoxEnable.Pressed += CheckBoxEnable_Pressed;
	}

    private void SpriteMapChunk_OnRealInstanceCountChanged(int obj)
    {
        LabelReal.Text = "Reales:" + obj;
    }

    private void CheckBoxEnable_Pressed()
    {
        spriteMapChunk.SetRenderEnabled(CheckBoxEnable.ButtonPressed);        
    }

    private void SpriteMapChunk_OnRenderingInstanceCountChanged(int obj)
    {
        LabelRendering.Text = "Renderizadas:" + obj;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

}
