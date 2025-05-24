// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlFrames : PanelContainer
{
    public delegate void EventNotifyChangued(ControlFrames objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer HBoxContainerMain;
    private SpinBox SpinBoxX;
    private HBoxContainer HBoxContainer2;
    private SpinBox SpinBoxY;
    private HBoxContainer HBoxContainer3;
    private SpinBox SpinBoxWidht;
    private HBoxContainer HBoxContainer4;
    private SpinBox SpinBoxHeight;
    private Button ButtonDelete;

    public void InitializeUI()
    {
        HBoxContainerMain = GetNode<HBoxContainer>("MarginContainer/HBoxContainerMain");
        SpinBoxX = GetNode<SpinBox>("MarginContainer/HBoxContainerMain/HBoxContainer/SpinBoxX");
        HBoxContainer2 = GetNode<HBoxContainer>("MarginContainer/HBoxContainerMain/HBoxContainer2");
        SpinBoxY = GetNode<SpinBox>("MarginContainer/HBoxContainerMain/HBoxContainer2/SpinBoxY");
        HBoxContainer3 = GetNode<HBoxContainer>("MarginContainer/HBoxContainerMain/HBoxContainer3");
        SpinBoxWidht = GetNode<SpinBox>("MarginContainer/HBoxContainerMain/HBoxContainer3/SpinBoxWidht");
        HBoxContainer4 = GetNode<HBoxContainer>("MarginContainer/HBoxContainerMain/HBoxContainer4");
        SpinBoxHeight = GetNode<SpinBox>("MarginContainer/HBoxContainerMain/HBoxContainer4/SpinBoxHeight");
        ButtonDelete = GetNode<Button>("MarginContainer/HBoxContainerMain/ButtonDelete");
    }
}