using Godot;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

public partial class ControlAnimation : PanelContainer
{


    PanelColliders panelColliders;
    ColliderScene controlCollider;

    AnimationData objectData;

    public AnimationData ObjectData { get => objectData; set => objectData = value; }

    // Called when the node enters the scene tree for the first time.
    
    public void SetData(AnimationData data)
    {
        this.objectData = data;
        ControlFramesData.SetData(data.frameDataArray);
        if (objectData.hasCollider)
        {
            CheckBoxHasCollision.ButtonPressed = true;
            CheckBoxHasCollision_Pressed();
            controlCollider.SetData(objectData.collider);
        }
        if (objectData.hasColliderMultiple)
        {
            CheckBoxHasCollisionMultiple.ButtonPressed = true;
            CheckBoxHasCollisionMultiple_Pressed();
            panelColliders.SetData(objectData.colliderMultiple.ToList());            
        }
    }
  
	public override void _Ready()
	{
        objectData = new AnimationData();
        InitializeUI(); // Insertado por el generador de UI
        CheckBoxHasCollision.Pressed += CheckBoxHasCollision_Pressed;
        CheckBoxHasCollisionMultiple.Pressed += CheckBoxHasCollisionMultiple_Pressed;
        ButtonRemove.Pressed += ButtonRemove_Pressed;
        
    }

    private void ButtonRemove_Pressed()
    {
        QueueFree();
    }

    private void CheckBoxHasCollisionMultiple_Pressed()
    {
        objectData.hasColliderMultiple = CheckBoxHasCollisionMultiple.ButtonPressed;
        if (CheckBoxHasCollisionMultiple.ButtonPressed)
        {
            CheckBoxHasCollision.Visible = false;
            LabelCollision.Visible = false;
            panelColliders = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/Colliders/windowColliders.tscn").Instantiate<PanelColliders>();
            VBoxContainerCollision.AddChild(panelColliders);
            panelColliders.OnNotifyPreview += PanelColliders_OnNotifyPreview;
        }
        else
        {
            CheckBoxHasCollision.Visible = true;
            LabelCollision.Visible = true;
            VBoxContainerCollision.RemoveChild(panelColliders);
            objectData.colliderMultiple = null;
            
        }
    }

    private void PanelColliders_OnNotifyPreview(GeometricShape2D itemData)
    {
        objectData.colliderMultiple = panelColliders.GetAllCollidersData().ToArray();
        OnNotifyChangued?.Invoke(this);
    }

    private void CheckBoxHasCollision_Pressed()
    {
        objectData.hasCollider = CheckBoxHasCollision.ButtonPressed;
        if (CheckBoxHasCollision.ButtonPressed)
        {
            CheckBoxHasCollisionMultiple.Visible = false;
            LabelColiisionMultiple.Visible = false;
            controlCollider = GD.Load<PackedScene>("res://sources/WindowsDataBase/Character/Colliders/ColliderScene.tscn").Instantiate<ColliderScene>();
            VBoxContainerCollision.AddChild(controlCollider);
            controlCollider.SetOcluccionButton();
            controlCollider.OnNotifyPreview += ControlCollider_OnNotifyPreview;
           // CollisionShapeView.Visible = true;
        }
        else
        {
            LabelColiisionMultiple.Visible = true;
            CheckBoxHasCollisionMultiple.Visible = true;
            VBoxContainerCollision.RemoveChild(controlCollider);
            objectData.collider = null;
           // CollisionShapeView.Visible = false;
        }
    }

    private void ControlCollider_OnNotifyPreview(GeometricShape2D itemData)
    {
        objectData.collider = itemData;
        OnNotifyChangued?.Invoke(this);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
