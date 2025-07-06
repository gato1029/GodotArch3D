// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlSeleccionTexture : MarginContainer
{
    public delegate void EventNotifyChangued(ControlSeleccionTexture objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private SpinBox SpinBoxCellSize;
    private ScrollContainer ContainerSelectionTexture;
    private CenterContainer CenterContainerBase;
    private TextureRect TextureDraw;
    private ColorRect GridBase;
    private ColorRect Selection;

    public void InitializeUI()
    {
        SpinBoxCellSize = GetNode<SpinBox>("VBoxContainer/HBoxContainer/SpinBoxCellSize");
        ContainerSelectionTexture = GetNode<ScrollContainer>("VBoxContainer/ContainerSelectionTexture");
        CenterContainerBase = GetNode<CenterContainer>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase");
        TextureDraw = GetNode<TextureRect>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase/TextureDraw");
        GridBase = GetNode<ColorRect>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase/GridBase");
        Selection = GetNode<ColorRect>("VBoxContainer/ContainerSelectionTexture/CenterContainerBase/Selection");
    }
}