// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAnimationStateArray : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAnimationStateArray objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonAdd;
    private VBoxContainer VBoxContainerItems;

    public void InitializeUI()
    {
        ButtonAdd = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonAdd");
        VBoxContainerItems = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ScrollContainer/VBoxContainerItems");
    }
}