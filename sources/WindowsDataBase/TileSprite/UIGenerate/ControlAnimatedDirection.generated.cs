// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAnimatedDirection : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAnimatedDirection objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private VBoxContainer VBoxContainerItems;

    public void InitializeUI()
    {
        VBoxContainerItems = GetNode<VBoxContainer>("VBoxContainer/VBoxContainerItems");
    }
}