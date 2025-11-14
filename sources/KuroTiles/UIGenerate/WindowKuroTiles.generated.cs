// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using GodotFlecs.sources.KuroTiles;
using System;

public partial class WindowKuroTiles : MarginContainer
{
    public delegate void EventNotifyChangued(WindowKuroTiles objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private SpinBox SpinBoxX;
    private SpinBox SpinBoxY;
    private TilesSubViewport SubViewport;
    private TileGridNode2d Grid;

    public void InitializeUI()
    {
        SpinBoxX = GetNode<SpinBox>("VBoxContainer/MarginContainer/HBoxContainer/SpinBoxX");
        SpinBoxY = GetNode<SpinBox>("VBoxContainer/MarginContainer/HBoxContainer/SpinBoxY");
        SubViewport = GetNode<TilesSubViewport>("VBoxContainer/SubViewportContainer/SubViewport");
        Grid = GetNode<TileGridNode2d>("VBoxContainer/SubViewportContainer/SubViewport/Panel/Grid");
    }
}