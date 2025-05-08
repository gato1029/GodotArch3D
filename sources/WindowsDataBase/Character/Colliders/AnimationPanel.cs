using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

public partial class AnimationPanel : PanelContainer
{

	[Export] Button addButton;
    [Export] VBoxContainer container;
    [Export] CheckButton checkTypeAnimation;

    [Export] Button guardarButton;
    public delegate void RequestNotifyPreview(AnimationStateData itemData, int currentIdState);
    public event RequestNotifyPreview OnNotifyPreview;

    public delegate void RequestNotifySave();
    public event RequestNotifySave OnNotifySave;

    PackedScene ColliderScene;
    bool eightDirections = false;
    // Called when the node enters the scene tree for the first time.
    int idMaterial =-1;
    public override void _Ready()
    {
        ColliderScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/Colliders/AnimationScene.tscn");

        addButton.Pressed += AddButton_Pressed;
        checkTypeAnimation.Pressed += CheckTypeAnimation_Pressed;
        guardarButton.Pressed += GuardarButton_Pressed;
    }
    public List<AnimationStateData> GetAllAnimationData()
    {
        List<AnimationStateData> list = new List<AnimationStateData>();
        foreach (var item in container.GetChildren())
        {
            var itemScene = (AnimationScene)item;
            list.Add(itemScene.data);
        }
        return list;
    }
    private void GuardarButton_Pressed()
    {
        OnNotifySave?.Invoke();
    }

    private void SetMaterial(int idMaterial)
    {
        foreach (var item in container.GetChildren())
        {
            AnimationScene node = item as AnimationScene;
            node.SetMaterialID(idMaterial);
        }
    }
    private void CheckTypeAnimation_Pressed()
    {
        if (checkTypeAnimation.ButtonPressed)
        {
            checkTypeAnimation.Text = "8 Direcciones";
            eightDirections = true;
        }
        else
        {
            checkTypeAnimation.Text = "4 Direcciones";
            eightDirections = false;
        }
    }

    public void AddAnimation(AnimationStateData data)
    {
        PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windowCharacterAnimation.tscn");
        int i = container.GetChildCount();

        AnimationScene node = ColliderScene.Instantiate<AnimationScene>();
        container.AddChild(node);
        node.SetDataBase(i, eightDirections);
        node.SetMaterialID(idMaterial);
        node.SetData(data,idMaterial, GodotEcsArch.sources.utils.WindowState.UPDATE);
        node.OnNotifyPreview += Node_OnNotifyPreview;
    }
    private void AddButton_Pressed()
    {
        PackedScene packedScene = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/windowCharacterAnimation.tscn");
        int i = container.GetChildCount();

        AnimationScene node = ColliderScene.Instantiate<AnimationScene>();
        container.AddChild(node);
        node.SetDataBase(i, eightDirections);
        node.SetMaterialID(idMaterial);
        node.OnNotifyPreview += Node_OnNotifyPreview;
        //node.OnRequestDelete += Node_OnRequestDeleteAnimation;
        //node.OnNotifyChangue += Node_OnNotifyChangueAnimation;
        //node.OnRequestOrderItem += Node_OnRequestOrderItem;
        //node.SetMaterial(currentMaterialData.id);




    }

    private void Node_OnNotifyPreview(AnimationStateData itemData, int currentIdState)
    {
        OnNotifyPreview?.Invoke(itemData, currentIdState);
    }



    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
