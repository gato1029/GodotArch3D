// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileRuleTextureControl : Control
{
    public delegate void EventNotifyChangued(TileRuleTextureControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private SpinBox SpinBoxX;
    private SpinBox SpinBoxY;
    private FixedGridContainer FixedGridTiles;

    public void InitializeUI()
    {
        SpinBoxX = GetNode<SpinBox>("PanelContainer/VBoxContainer/HBoxContainer/SpinBoxX");
        SpinBoxY = GetNode<SpinBox>("PanelContainer/VBoxContainer/HBoxContainer/SpinBoxY");
        FixedGridTiles = GetNode<FixedGridContainer>("PanelContainer/VBoxContainer/FixedGridTiles");
    }
}