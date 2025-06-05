// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAnimation : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAnimation objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer HBoxContainer2;
    private Label Label9;
    private SpinBox SpinBoxDuration;
    private Label Label5;
    private CheckBox CheckBoxLoop;
    private Label Label6;
    private CheckBox CheckBoxMirror;
    private Label Label7;
    private CheckBox CheckBoxMirrorV;
    private GridContainer GridContainerAnimacion;
    private Label LabelCollision;
    private CheckBox CheckBoxHasCollision;
    private Label LabelColiisionMultiple;
    private CheckBox CheckBoxHasCollisionMultiple;
    private VBoxContainer VBoxContainerAnimacion;
    private ControlFramesArray ControlFramesData;
    private VBoxContainer VBoxContainerCollision;
    private Button ButtonRemove;

    public void InitializeUI()
    {
        HBoxContainer2 = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer2");
        Label9 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/Label9");
        SpinBoxDuration = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/SpinBoxDuration");
        Label5 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/Label5");
        CheckBoxLoop = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/CheckBoxLoop");
        CheckBoxLoop.Pressed += CheckBoxLoop_PressedUI;
        CheckBoxLoop_PressedUI();
        Label6 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/Label6");
        CheckBoxMirror = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/CheckBoxMirror");
        CheckBoxMirror.Pressed += CheckBoxMirror_PressedUI;
        CheckBoxMirror_PressedUI();
        Label7 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/Label7");
        CheckBoxMirrorV = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainer/CheckBoxMirrorV");
        CheckBoxMirrorV.Pressed += CheckBoxMirrorV_PressedUI;
        CheckBoxMirrorV_PressedUI();
        GridContainerAnimacion = GetNode<GridContainer>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainerAnimacion");
        LabelCollision = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainerAnimacion/LabelCollision");
        CheckBoxHasCollision = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainerAnimacion/CheckBoxHasCollision");
        CheckBoxHasCollision.Pressed += CheckBoxHasCollision_PressedUI;
        CheckBoxHasCollision_PressedUI();
        LabelColiisionMultiple = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainerAnimacion/LabelColiisionMultiple");
        CheckBoxHasCollisionMultiple = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainer/GridContainerAnimacion/CheckBoxHasCollisionMultiple");
        CheckBoxHasCollisionMultiple.Pressed += CheckBoxHasCollisionMultiple_PressedUI;
        CheckBoxHasCollisionMultiple_PressedUI();
        VBoxContainerAnimacion = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainerAnimacion");
        ControlFramesData = GetNode<ControlFramesArray>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainerAnimacion/VBoxContainer/ControlFramesData");
        VBoxContainerCollision = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer2/VBoxContainerAnimacion/VBoxContainerCollision");
        ButtonRemove = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonRemove");
    }

    private void CheckBoxLoop_PressedUI()
    {
        if (CheckBoxLoop.ButtonPressed)
            CheckBoxLoop.Text = "Bucle";
        else
            CheckBoxLoop.Text = "No Bucle";
    }

    private void CheckBoxMirror_PressedUI()
    {
        if (CheckBoxMirror.ButtonPressed)
            CheckBoxMirror.Text = "Habilitado";
        else
            CheckBoxMirror.Text = "No Habilitado";
    }

    private void CheckBoxMirrorV_PressedUI()
    {
        if (CheckBoxMirrorV.ButtonPressed)
            CheckBoxMirrorV.Text = "Habilitado";
        else
            CheckBoxMirrorV.Text = "No Habilitado";
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