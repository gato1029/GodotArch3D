// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlListTileSprite : MarginContainer
{
    public delegate void EventNotifyChangued(ControlListTileSprite objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerTile;
    private HBoxContainer HBoxContainerTiles;

    public void InitializeUI()
    {
        PanelContainerTile = GetNode<PanelContainer>("PanelContainerTile");
        HBoxContainerTiles = GetNode<HBoxContainer>("PanelContainerTile/ScrollContainer/MarginContainer/HBoxContainerTiles");
    }
}