// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAnimationState : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAnimationState objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private SpinBox SpinBoxPosition;
    private Button ButtonDelete;
    private Button ButtonDown;
    private Button ButtonUp;
    private VBoxContainer VBoxContainer2;
    private Label Label11;
    private OptionButton OptionButtonDirection;
    private Label Label10;
    private OptionButton OptionButtonPosition;
    private Label Label9;
    private SpinBox SpinBoxDuration;
    private Label Label5;
    private CheckBox CheckBoxLoop;
    private Label Label6;
    private CheckBox CheckBoxMirror;
    private ControlAnimation ControlAnimationFrames;

    public void InitializeUI()
    {
        SpinBoxPosition = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/SpinBoxPosition");
        ButtonDelete = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/ButtonDelete");
        ButtonDown = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/ButtonDown");
        ButtonUp = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/ButtonUp");
        VBoxContainer2 = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/VBoxContainer2");
        Label11 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label11");
        OptionButtonDirection = GetNode<OptionButton>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/OptionButtonDirection");
        OptionButtonDirection.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label10 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label10");
        OptionButtonPosition = GetNode<OptionButton>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/OptionButtonPosition");
        OptionButtonPosition.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label9 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label9");
        SpinBoxDuration = GetNode<SpinBox>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/SpinBoxDuration");
        Label5 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label5");
        CheckBoxLoop = GetNode<CheckBox>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/CheckBoxLoop");
        CheckBoxLoop.Pressed += CheckBoxLoop_PressedUI;
        CheckBoxLoop_PressedUI();
        Label6 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label6");
        CheckBoxMirror = GetNode<CheckBox>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/CheckBoxMirror");
        CheckBoxMirror.Pressed += CheckBoxMirror_PressedUI;
        CheckBoxMirror_PressedUI();
        ControlAnimationFrames = GetNode<ControlAnimation>("MarginContainer/VBoxContainer/VBoxContainer2/ControlAnimationFrames");
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
}