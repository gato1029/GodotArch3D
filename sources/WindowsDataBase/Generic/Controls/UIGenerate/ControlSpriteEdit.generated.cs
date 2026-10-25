// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlSpriteEdit : MarginContainer
{
    public delegate void EventNotifyChangued(ControlSpriteEdit objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private SpinBox SpinBoxX;
    private SpinBox SpinBoxY;
    private CheckButton CheckButtonModeGrid;
    private CheckButton CheckButtonPath;
    private TilesSubViewport SubViewport;
    private TextureRect TextureImage;
    private TextureRect center;
    private TextureRect BarLife;
    private TileOcupancy TileOcupancy;
    private GridGeneric GridGeneric;
    private ShapesForm ShapesForm;

    public void InitializeUI()
    {
        SpinBoxX = GetNode<SpinBox>("VBoxContainer/MarginContainer/HBoxContainer/SpinBoxX");
        SpinBoxY = GetNode<SpinBox>("VBoxContainer/MarginContainer/HBoxContainer/SpinBoxY");
        CheckButtonModeGrid = GetNode<CheckButton>("VBoxContainer/MarginContainer/HBoxContainer/CheckButtonModeGrid");
        CheckButtonPath = GetNode<CheckButton>("VBoxContainer/MarginContainer/HBoxContainer/CheckButtonPath");
        SubViewport = GetNode<TilesSubViewport>("VBoxContainer/SubViewportContainer/SubViewport");
        TextureImage = GetNode<TextureRect>("VBoxContainer/SubViewportContainer/SubViewport/Panel/TextureImage");
        center = GetNode<TextureRect>("VBoxContainer/SubViewportContainer/SubViewport/Panel/center");
        BarLife = GetNode<TextureRect>("VBoxContainer/SubViewportContainer/SubViewport/Panel/BarLife");
        TileOcupancy = GetNode<TileOcupancy>("VBoxContainer/SubViewportContainer/SubViewport/Panel/TileOcupancy");
        GridGeneric = GetNode<GridGeneric>("VBoxContainer/SubViewportContainer/SubViewport/Panel/GridGeneric");
        ShapesForm = GetNode<ShapesForm>("VBoxContainer/SubViewportContainer/SubViewport/Panel/ShapesForm");
    }
}