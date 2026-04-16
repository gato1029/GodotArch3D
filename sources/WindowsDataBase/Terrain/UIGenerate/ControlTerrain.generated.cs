// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlTerrain : MarginContainer
{
    public delegate void EventNotifyChangued(ControlTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Panel PanelBase;
    private TextureRect TextureImage;
    private TextEdit TextEditSearch;

    public void InitializeUI()
    {
        PanelBase = GetNode<Panel>("PanelBase");
        TextureImage = GetNode<TextureRect>("PanelBase/HBoxContainer/TextureImage");
        TextEditSearch = GetNode<TextEdit>("PanelBase/HBoxContainer/MarginContainer/TextEditSearch");
    }
}