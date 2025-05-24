// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAnimation : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAnimation objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer HBoxContainerAnimacion;
    private ControlFramesArray ControlFramesData;
    private GridContainer GridContainerAnimacion;
    private Label LabelCollision;
    private CheckBox CheckBoxHasCollision;
    private Label LabelColiisionMultiple;
    private CheckBox CheckBoxHasCollisionMultiple;
    private VBoxContainer VBoxContainerCollision;
    private Button ButtonRemove;
    private Button ButtonPreview;

    public void InitializeUI()
    {
        HBoxContainerAnimacion = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/HBoxContainerAnimacion");
        ControlFramesData = GetNode<ControlFramesArray>("MarginContainer/VBoxContainer/HBoxContainerAnimacion/VBoxContainer/ControlFramesData");
        GridContainerAnimacion = GetNode<GridContainer>("MarginContainer/VBoxContainer/HBoxContainerAnimacion/VBoxContainer/GridContainerAnimacion");
        LabelCollision = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainerAnimacion/VBoxContainer/GridContainerAnimacion/LabelCollision");
        CheckBoxHasCollision = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainerAnimacion/VBoxContainer/GridContainerAnimacion/CheckBoxHasCollision");
        CheckBoxHasCollision.Pressed += CheckBoxHasCollision_PressedUI;
        CheckBoxHasCollision_PressedUI();
        LabelColiisionMultiple = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainerAnimacion/VBoxContainer/GridContainerAnimacion/LabelColiisionMultiple");
        CheckBoxHasCollisionMultiple = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainerAnimacion/VBoxContainer/GridContainerAnimacion/CheckBoxHasCollisionMultiple");
        CheckBoxHasCollisionMultiple.Pressed += CheckBoxHasCollisionMultiple_PressedUI;
        CheckBoxHasCollisionMultiple_PressedUI();
        VBoxContainerCollision = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainerAnimacion/VBoxContainerCollision");
        ButtonRemove = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonRemove");
        ButtonPreview = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonPreview");
    }

    private void CheckBoxHasCollision_PressedUI()
    {
        if (CheckBoxHasCollision.ButtonPressed)
            CheckBoxHasCollision.Text = "Collision";
        else
            CheckBoxHasCollision.Text = "No Collision";
    }

    private void CheckBoxHasCollisionMultiple_PressedUI()
    {
        if (CheckBoxHasCollisionMultiple.ButtonPressed)
            CheckBoxHasCollisionMultiple.Text = "Collision";
        else
            CheckBoxHasCollisionMultiple.Text = "No Collision";
    }
}