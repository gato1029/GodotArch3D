// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuleTextureControl : PanelContainer
{
    public delegate void EventNotifyChangued(RuleTextureControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroTextureButton KuroTextureButtonDelete;
    private Label LabelPosition;
    private HBoxContainer VBoxContainer;
    private VBoxContainer HBoxContainer;
    private KuroCheckButton KuroCheckButtonSwitch;
    private SpinBox SpinBoxX;
    private SpinBox SpinBoxY;
    private FixedGridContainer FixedGridTiles;
    private FixedGridContainer FixedGridRules;

    public void InitializeUI()
    {
        KuroTextureButtonDelete = GetNode<KuroTextureButton>("VBoxContainer/TextureRect/KuroTextureButtonDelete");
        LabelPosition = GetNode<Label>("VBoxContainer/TextureRect/LabelPosition");
        VBoxContainer = GetNode<HBoxContainer>("VBoxContainer/MarginContainer/VBoxContainer");
        HBoxContainer = GetNode<VBoxContainer>("VBoxContainer/MarginContainer/VBoxContainer/HBoxContainer");
        KuroCheckButtonSwitch = GetNode<KuroCheckButton>("VBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/KuroCheckButtonSwitch");
        SpinBoxX = GetNode<SpinBox>("VBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/SpinBoxX");
        SpinBoxY = GetNode<SpinBox>("VBoxContainer/MarginContainer/VBoxContainer/HBoxContainer/SpinBoxY");
        FixedGridTiles = GetNode<FixedGridContainer>("VBoxContainer/MarginContainer/VBoxContainer/VBoxContainer/FixedGridTiles");
        FixedGridRules = GetNode<FixedGridContainer>("VBoxContainer/MarginContainer/VBoxContainer/VBoxContainer/FixedGridRules");
    }
}