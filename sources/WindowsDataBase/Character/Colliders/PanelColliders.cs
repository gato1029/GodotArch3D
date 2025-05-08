using Godot;
using GodotEcsArch.sources.managers.Collision;
using System;
using System.Collections.Generic;

public partial class PanelColliders : PanelContainer
{
	[Export] Button AddCollider;
	[Export] VBoxContainer container;
    [Export] Button buttonGuardar;
    PackedScene ColliderScene;
    // Called when the node enters the scene tree for the first time.
    public delegate void RequestNotifyPreview(GeometricShape2D itemData);
    public event RequestNotifyPreview OnNotifyPreview;

    public delegate void RequestNotifySave();
    public event RequestNotifySave OnNotifySave;

    public List<GeometricShape2D> GetAllCollidersData()
    {
        List<GeometricShape2D> list = new List<GeometricShape2D>();
        foreach (var item in container.GetChildren())
        {
            var itemScene = (ColliderScene)item;
            list.Add(itemScene.data);
        }
        return list;
    }

    public void addCollider(GeometricShape2D item)
    {
        var node = ColliderScene.Instantiate<ColliderScene>();
        container.AddChild(node);
        node.SetData(item);
        node.OnNotifyPreview += Node_OnNotifyPreview;
    }
    public override void _Ready()
	{       
        ColliderScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/Colliders/ColliderScene.tscn");
        AddCollider.Pressed += AddCollider_Pressed;
        buttonGuardar.Pressed += ButtonGuardar_Pressed;
    }

    private void ButtonGuardar_Pressed()
    {
        OnNotifySave?.Invoke();
    }

    private void AddCollider_Pressed()
    {
        var node = ColliderScene.Instantiate<ColliderScene>();
        container.AddChild(node);
        node.OnNotifyPreview += Node_OnNotifyPreview;        
    }

    private void Node_OnNotifyPreview(GeometricShape2D itemData)
    {
        OnNotifyPreview?.Invoke(itemData);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
