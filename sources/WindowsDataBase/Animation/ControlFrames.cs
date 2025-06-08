using Godot;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;

public partial class ControlFrames : PanelContainer
{
	FrameData objectData;
    public FrameData ObjectData { get => objectData; set => objectData = value; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        objectData = new FrameData();
        SpinBoxX.ValueChanged += SpinBoxX_ValueChanged;
        SpinBoxY.ValueChanged += SpinBoxY_ValueChanged;
        SpinBoxWidht.ValueChanged += SpinBoxWidht_ValueChanged;
        SpinBoxHeight.ValueChanged += SpinBoxHeight_ValueChanged;
        ButtonDelete.Pressed += ButtonDelete_Pressed;
    }

    private void ButtonDelete_Pressed()
    {
        QueueFree();
    }

    public void SetData(FrameData frameData)
    {
        objectData = frameData;
        SpinBoxX.Value = objectData.x;
        SpinBoxY.Value = objectData.y;
        SpinBoxWidht.Value = objectData.widht;
        SpinBoxHeight.Value = objectData.height;
        if (objectData.heightFormat == 0 || objectData.widhtFormat == 0)
        {
            objectData.heightFormat = objectData.height;
            objectData.widhtFormat = objectData.widht;
        }
    }
    private void SpinBoxHeight_ValueChanged(double value)
    {
        objectData.height = (float)value;        
    }

    private void SpinBoxWidht_ValueChanged(double value)
    {
        objectData.widht = (float)value;        
    }

    private void SpinBoxY_ValueChanged(double value)
    {
        objectData.y= (float)value;
    }

    private void SpinBoxX_ValueChanged(double value)
    {
        objectData.x= (float)value;
    }

 
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void ReverseFramesHorizontal(bool reverse)
    {
        if (reverse)
        {
            objectData.widhtFormat = (-1) * objectData.widht;
        }
        else
        {
            objectData.widhtFormat = objectData.widht;
        }
    }

    public void ReverseFramesVertical(bool reverse)
    {
        if (reverse)
        {
            objectData.heightFormat = (-1) * objectData.height;
        }
        else
        {
            objectData.heightFormat = objectData.height;
        }
    }
}
