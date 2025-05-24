// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlFramesArray : PanelContainer
{
    public delegate void EventNotifyChangued(ControlFramesArray objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonAdd;
    private Button ButtonClearAll;
    private VBoxContainer ContainerItems;

    public void InitializeUI()
    {
        ButtonAdd = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonAdd");
        ButtonClearAll = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonClearAll");
        ContainerItems = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ScrollContainer/ContainerItems");
    }
}