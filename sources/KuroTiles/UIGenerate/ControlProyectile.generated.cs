// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlProyectile : MarginContainer
{
    public delegate void EventNotifyChangued(ControlProyectile objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Panel PanelBase;
    private TextureRect TextureImage;

    public void InitializeUI()
    {
        PanelBase = GetNode<Panel>("PanelBase");
        TextureImage = GetNode<TextureRect>("PanelBase/TextureImage");
    }
}