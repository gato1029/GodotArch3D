// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlRules : PanelContainer
{
    public delegate void EventNotifyChangued(ControlRules objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonNewRule;
    private Button ButtonSave;
    private GridContainer GridContainerItems;

    public void InitializeUI()
    {
        ButtonNewRule = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonNewRule");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonSave");
        GridContainerItems = GetNode<GridContainer>("MarginContainer/VBoxContainer/ScrollContainer/GridContainerItems");
    }
}