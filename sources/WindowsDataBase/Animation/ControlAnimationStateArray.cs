using Godot;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

public partial class ControlAnimationStateArray : PanelContainer
{
    public delegate void EventNotifyCollision(GodotEcsArch.sources.managers.Collision.GeometricShape2D geometricShape2D);
    public event EventNotifyCollision OnNotifyCollisionSelected;

    public delegate void EventNotifyAnimation(ControlAnimationState objectControl, int animationDataPosition);
    public event EventNotifyAnimation OnNotifyAnimationSelected;

    ControlAnimationState objectControlSelect;
    int animationDataPositionSelect;

    public ControlAnimationState ObjectControlSelect { get => objectControlSelect; set => objectControlSelect = value; }
    public int AnimationDataPositionSelect { get => animationDataPositionSelect; set => animationDataPositionSelect = value; }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        InitializeUI(); // Insertado por el generador de UI
        ButtonAdd.Pressed += ButtonAdd_Pressed;
	}
    internal List<AnimationStateData> GetAllData()
    {
        var list = new List<AnimationStateData>();
        foreach (var item in VBoxContainerItems.GetChildren())
        {
            var data = (ControlAnimationState)item;
            list.Add(data.ObjectData);
        }
        return list;
    }
    internal void SetData(AnimationStateData[] animationDataArray)
    {
        foreach (var data in animationDataArray)
        {

            var node = GD.Load<PackedScene>("res://sources/WindowsDataBase/Animation/ControlAnimationState.tscn").Instantiate<ControlAnimationState>();
            VBoxContainerItems.AddChild(node);
            node.SetData(data);

            int childCount = VBoxContainerItems.GetChildCount();
            node.SetPosition(childCount - 1);
            objectControlSelect = node;
            animationDataPositionSelect = 0;

            node.OnNotifyChanguedOrder += Node_OnNotifyChanguedOrder;
            node.OnNotifyPointerSelect += Node_OnNotifyPointerSelect;
            node.OnNotifyCollisionSelected += Node_OnNotifyCollisionSelected;
        }
    }
    private void ButtonAdd_Pressed()
    {
        var node = GD.Load<PackedScene>("res://sources/WindowsDataBase/Animation/ControlAnimationState.tscn").Instantiate<ControlAnimationState>();       
        VBoxContainerItems.AddChild(node);
        int childCount = VBoxContainerItems.GetChildCount();
        node.SetPosition(childCount-1);
        objectControlSelect = node;
        animationDataPositionSelect= 0;

        node.OnNotifyChanguedOrder += Node_OnNotifyChanguedOrder;
        node.OnNotifyPointerSelect += Node_OnNotifyPointerSelect;
        node.OnNotifyCollisionSelected += Node_OnNotifyCollisionSelected;

        OnNotifyChangued?.Invoke(this);
    }

    private void Node_OnNotifyCollisionSelected(GodotEcsArch.sources.managers.Collision.GeometricShape2D geometricShape2D)
    {
        OnNotifyCollisionSelected?.Invoke(geometricShape2D);
    }

    private void Node_OnNotifyPointerSelect(ControlAnimationState objectControl, int animationDataPosition)
    {
        objectControlSelect = objectControl;
        animationDataPositionSelect = animationDataPosition;

        OnNotifyAnimationSelected?.Invoke(objectControlSelect,animationDataPosition);
    }

    private void Node_OnNotifyChanguedOrder(ControlAnimationState objectControl, DirectionArrowArray directionArrowArray)
    {
            int index = VBoxContainerItems.GetChildren().IndexOf(objectControl);
            int childCount = VBoxContainerItems.GetChildCount();

        switch (directionArrowArray)
        {
            case DirectionArrowArray.UP:
                if (index > 0)
                {
                    VBoxContainerItems.MoveChild(objectControl, index - 1);

                }
                break;

            case DirectionArrowArray.DOWN:
                if (index < childCount - 1)
                {
                    VBoxContainerItems.MoveChild(objectControl, index + 1);

                }
                break;
            case DirectionArrowArray.REMOVE:
                break;
            default:
                break;
        }
        // Actualiza las posiciones de todos los elementos, ignorando el nodo si se va a eliminar
        var children = VBoxContainerItems.GetChildren();
        int position = 0;
        foreach (Node child in children)
        {
            if (child == objectControl && directionArrowArray == DirectionArrowArray.REMOVE)
            {
                continue; // Omitir el nodo que será eliminado
            }

            if (child is ControlAnimationState control)
            {
                control.SetPosition(position);
                position++;
            }
        }
        OnNotifyChangued?.Invoke(this);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

  
}
