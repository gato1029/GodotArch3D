using Godot;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;

public partial class ControlFramesArray : PanelContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonAdd.Pressed += ButtonAdd_Pressed;
        ButtonClearAll.Pressed += ButtonClearAll_Pressed;
	}

    private void ButtonClearAll_Pressed()
    {
        foreach (var item in ContainerItems.GetChildren())
        {
          item.QueueFree();
        }
    }

    private void ButtonAdd_Pressed()
    {
       var node  = GD.Load<PackedScene>("res://sources/WindowsDataBase/Animation/ControlFrames.tscn").Instantiate<ControlFrames>();
       ContainerItems.AddChild(node);
        
    }

    public List<ControlFrames> GetAllData()
	{
		var list = new List<ControlFrames>();
		foreach (var item in ContainerItems.GetChildren())
		{
            ControlFrames t = (ControlFrames)item;
			list.Add(t);
		}
		return list;
	}
    public void ReverseFramesVertical(bool reverse)
    {        
        foreach (var item in ContainerItems.GetChildren())
        {
            ControlFrames t = (ControlFrames)item;
            t.ReverseFramesVertical(reverse);
        }
    }
    public void ReverseFramesHorizontal(bool reverse)
    {
        foreach (var item in ContainerItems.GetChildren())
        {
            ControlFrames t = (ControlFrames)item;
            t.ReverseFramesHorizontal(reverse);
        }
    }
    public List<FrameData> GetAllDataFrames()
    {
        var list = new List<FrameData>();
        foreach (var item in ContainerItems.GetChildren())
        {
            ControlFrames t = (ControlFrames)item;
            list.Add(t.ObjectData);
        }
        return list;
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void AddData(FrameData frameData)
    {
        var node = GD.Load<PackedScene>("res://sources/WindowsDataBase/Animation/ControlFrames.tscn").Instantiate<ControlFrames>();
        ContainerItems.AddChild(node);
        node.SetData(frameData);
    }
    public void SetData(FrameData[] frameDataArray)
    {
       
        if (frameDataArray!=null)
        {
            RemoveAll();
            foreach (var frame in frameDataArray)
            {
                var node = GD.Load<PackedScene>("res://sources/WindowsDataBase/Animation/ControlFrames.tscn").Instantiate<ControlFrames>();
                ContainerItems.AddChild(node);
                node.SetData(frame);
            }
        }
        else
        {
            RemoveAll();
        }
      
    }

    public void RemoveAll()
    {
        foreach (var item in ContainerItems.GetChildren())
        {
            item.QueueFree();
        }
    }
}
