// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlListTileSprite : MarginContainer
{
    public delegate void EventNotifyChangued(ControlListTileSprite objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerTile;
    private ScrollContainer ScrollContainerUI;
    private HBoxContainer HBoxContainerTiles;

    public void InitializeUI()
    {
        PanelContainerTile = GetNode<PanelContainer>("PanelContainerTile");
        ScrollContainerUI = GetNode<ScrollContainer>("PanelContainerTile/ScrollContainerUI");
        HBoxContainerTiles = GetNode<HBoxContainer>("PanelContainerTile/ScrollContainerUI/MarginContainer/HBoxContainerTiles");
    }
}