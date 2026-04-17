// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileRuleTextureControl : Control
{
    public delegate void EventNotifyChangued(TileRuleTextureControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer VBoxContainer;
    private VBoxContainer HBoxContainer;
    private KuroCheckButton KuroCheckButtonSwitch;
    private SpinBox SpinBoxX;
    private SpinBox SpinBoxY;
    private FixedGridContainer FixedGridTiles;
    private FixedGridContainer FixedGridRules;

    public void InitializeUI()
    {
        VBoxContainer = GetNode<HBoxContainer>("PanelContainer/MarginContainer/VBoxContainer");
        HBoxContainer = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer");
        KuroCheckButtonSwitch = GetNode<KuroCheckButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/KuroCheckButtonSwitch");
        SpinBoxX = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/SpinBoxX");
        SpinBoxY = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/SpinBoxY");
        FixedGridTiles = GetNode<FixedGridContainer>("PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/FixedGridTiles");
        FixedGridRules = GetNode<FixedGridContainer>("PanelContainer/MarginContainer/VBoxContainer/VBoxContainer/FixedGridRules");
    }
}