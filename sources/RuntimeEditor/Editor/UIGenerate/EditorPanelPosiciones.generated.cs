// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class EditorPanelPosiciones : PanelContainer
{
    public delegate void EventNotifyChangued(EditorPanelPosiciones objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private VBoxContainer VBoxContainerPosiciones;

    public void InitializeUI()
    {
        VBoxContainerPosiciones = GetNode<VBoxContainer>("Panel/VBoxContainerPosiciones");
    }
}