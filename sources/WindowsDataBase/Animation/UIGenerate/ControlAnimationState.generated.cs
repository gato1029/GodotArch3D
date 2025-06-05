// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAnimationState : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAnimationState objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    public Button ButtonSelection;
    private SpinBox SpinBoxPosition;
    private Button ButtonDelete;
    private Button ButtonDown;
    private Button ButtonUp;
    private VBoxContainer VBoxContainer2;
    private Label Label11;
    private LineEdit LineEditName;
    private Label Label12;
    private OptionButton OptionButtonDirection;
    private Label Label10;
    private OptionButton OptionButtonPosition;
    private ControlAnimation ControlAnimationFrames;

    public void InitializeUI()
    {
        ButtonSelection = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonSelection");
        SpinBoxPosition = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/SpinBoxPosition");
        ButtonDelete = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/ButtonDelete");
        ButtonDown = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/ButtonDown");
        ButtonUp = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HBoxContainer/ButtonUp");
        VBoxContainer2 = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/VBoxContainer2");
        Label11 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label11");
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/LineEditName");
        Label12 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label12");
        OptionButtonDirection = GetNode<OptionButton>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/OptionButtonDirection");
        OptionButtonDirection.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label10 = GetNode<Label>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/Label10");
        OptionButtonPosition = GetNode<OptionButton>("MarginContainer/VBoxContainer/VBoxContainer2/GridContainer/OptionButtonPosition");
        OptionButtonPosition.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        ControlAnimationFrames = GetNode<ControlAnimation>("MarginContainer/VBoxContainer/VBoxContainer2/ControlAnimationFrames");
    }
}