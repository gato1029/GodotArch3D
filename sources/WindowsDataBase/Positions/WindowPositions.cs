using Godot;
using GodotEcsArch.sources.managers;
using System;
using System.Collections.Generic;

public partial class WindowPositions : Window
{

    VBoxContainer vBoxContainer;

    Label [] arrayLabels = new Label [6];
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        vBoxContainer = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer");
        CloseRequested += WindowTileCreator_CloseRequested;
        arrayLabels[0] = new Label();
        arrayLabels[1] = new Label();
        arrayLabels[2] = new Label();
        arrayLabels[3] = new Label();
        arrayLabels[4] = new Label();
        arrayLabels[5] = new Label();

        vBoxContainer.AddChild(arrayLabels[0]);
        vBoxContainer.AddChild(arrayLabels[1]);
        vBoxContainer.AddChild(arrayLabels[2]);
        vBoxContainer.AddChild(arrayLabels[3]);
        vBoxContainer.AddChild(arrayLabels[4]);
        vBoxContainer.AddChild(arrayLabels[5]);
    }

    private void WindowTileCreator_CloseRequested()
    {
        //QueueFree();
    }

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
