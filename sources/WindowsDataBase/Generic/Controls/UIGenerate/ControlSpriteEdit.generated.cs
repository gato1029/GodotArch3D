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
    private TilesSubViewport SubViewport;
    private TextureRect TextureImage;
    private TextureRect center;
    private GridGeneric GridGeneric;
    private ShapesForm ShapesForm;
    private TileOcupancy TileOcupancy;

    public void InitializeUI()
    {
        SpinBoxX = GetNode<SpinBox>("VBoxContainer/MarginContainer/HBoxContainer/SpinBoxX");
        SpinBoxY = GetNode<SpinBox>("VBoxContainer/MarginContainer/HBoxContainer/SpinBoxY");
        CheckButtonModeGrid = GetNode<CheckButton>("VBoxContainer/MarginContainer/HBoxContainer/CheckButtonModeGrid");
        SubViewport = GetNode<TilesSubViewport>("VBoxContainer/SubViewportContainer/SubViewport");
        TextureImage = GetNode<TextureRect>("VBoxContainer/SubViewportContainer/SubViewport/Panel/TextureImage");
        center = GetNode<TextureRect>("VBoxContainer/SubViewportContainer/SubViewport/Panel/center");
        GridGeneric = GetNode<GridGeneric>("VBoxContainer/SubViewportContainer/SubViewport/Panel/GridGeneric");
        ShapesForm = GetNode<ShapesForm>("VBoxContainer/SubViewportContainer/SubViewport/Panel/ShapesForm");
        TileOcupancy = GetNode<TileOcupancy>("VBoxContainer/SubViewportContainer/SubViewport/Panel/TileOcupancy");
    }
}