// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlGrouping : MarginContainer
{
    public delegate void EventNotifyChangued(ControlGrouping objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Panel PanelBase;
    private TextureRect TextureImage;
    private Label LabelName;

    public void InitializeUI()
    {
        PanelBase = GetNode<Panel>("PanelBase");
        TextureImage = GetNode<TextureRect>("PanelBase/VBoxContainer/TextureImage");
        LabelName = GetNode<Label>("PanelBase/VBoxContainer/LabelName");
    }
}