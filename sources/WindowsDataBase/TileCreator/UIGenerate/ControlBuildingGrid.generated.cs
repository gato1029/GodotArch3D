// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlBuildingGrid : MarginContainer
{
    public delegate void EventNotifyChangued(ControlBuildingGrid objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private SpinBox SpinBoxCellSize;
    private SpinBox SpinBoxGridSizeX;
    private SpinBox SpinBoxGridSizeY;
    private ScrollContainer ContainerSelectionTexture;
    private CenterContainer CenterContainerBase;
    private TextureRect TextureDraw;
    private ColorRect GridBase;
    private ColorRect Selection;

    public void InitializeUI()
    {
        SpinBoxCellSize = GetNode<SpinBox>("VBoxContainer/HBoxContainer/SpinBoxCellSize");
        SpinBoxGridSizeX = GetNode<SpinBox>("VBoxContainer/HBoxContainer/SpinBoxGridSizeX");
        SpinBoxGridSizeY = GetNode<SpinBox>("VBoxContainer/HBoxContainer/SpinBoxGridSizeY");
        ContainerSelectionTexture = GetNode<ScrollContainer>("VBoxContainer/ContainerSelectionTexture");
        CenterContainerBase = GetNode<CenterContainer>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase");
        TextureDraw = GetNode<TextureRect>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase/TextureDraw");
        GridBase = GetNode<ColorRect>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase/GridBase");
        Selection = GetNode<ColorRect>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase/Selection");
    }
}