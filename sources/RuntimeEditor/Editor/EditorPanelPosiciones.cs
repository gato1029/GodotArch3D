using Godot;
using GodotEcsArch.sources.managers;
using System;

public partial class EditorPanelPosiciones : PanelContainer
{
    // Called when the node enters the scene tree for the first time.
    Label[] arrayLabels = new Label[6];
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        arrayLabels[0] = new Label();
        arrayLabels[1] = new Label();
        arrayLabels[2] = new Label();
        arrayLabels[3] = new Label();
        arrayLabels[4] = new Label();
        arrayLabels[5] = new Label();

        VBoxContainerPosiciones.AddChild(arrayLabels[0]);
        VBoxContainerPosiciones.AddChild(arrayLabels[1]);
        VBoxContainerPosiciones.AddChild(arrayLabels[2]);
        VBoxContainerPosiciones.AddChild(arrayLabels[3]);
        VBoxContainerPosiciones.AddChild(arrayLabels[4]);
        VBoxContainerPosiciones.AddChild(arrayLabels[5]);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        arrayLabels[0].Text = $"Mouse Camera:" + PositionsManager.Instance.positionMouseCamera;
        arrayLabels[1].Text = $"Tile Global:" + PositionsManager.Instance.positionMouseTileGlobal;
        arrayLabels[2].Text = $"Chunk:" + PositionsManager.Instance.positionMouseChunk;
        arrayLabels[3].Text = $"Tile in Chunk:" + PositionsManager.Instance.positionMouseTileChunk;
        arrayLabels[4].Text = $"Position Camera:" + PositionsManager.Instance.positionCamera;
        arrayLabels[5].Text = $"Mouse Camera Pixel:" + PositionsManager.Instance.positionMouseCameraPixel;
    }
}
