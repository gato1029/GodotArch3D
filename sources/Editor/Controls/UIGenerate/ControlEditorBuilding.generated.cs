// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlEditorBuilding : MarginContainer
{
    public delegate void EventNotifyChangued(ControlEditorBuilding objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerFocus;
    private VBoxContainer Disenio;
    private TextureRect TextureRectImage;
    private SpinBox SpinBoxSeed;
    private Button ButtonSeedRandom;
    private SpinBox SpinBoxChunkX;
    private SpinBox SpinBoxChunkY;
    private Button ButtonAutomatic;
    private Button ButtonRefresh;
    private ItemList ItemListData;
    private MarginContainer CapasDiseño;
    private VBoxContainer ContainerLayers;

    public void InitializeUI()
    {
        PanelContainerFocus = GetNode<PanelContainer>("PanelContainerFocus");
        Disenio = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/Disenio");
        TextureRectImage = GetNode<TextureRect>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/VBoxContainer2/TextureRectImage");
        SpinBoxSeed = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/VBoxContainer/HBoxContainer/SpinBoxSeed");
        ButtonSeedRandom = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/VBoxContainer/HBoxContainer/ButtonSeedRandom");
        SpinBoxChunkX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/VBoxContainer/GridContainer/SpinBoxChunkX");
        SpinBoxChunkY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/VBoxContainer/GridContainer/SpinBoxChunkY");
        ButtonAutomatic = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/VBoxContainer/ButtonAutomatic");
        ButtonRefresh = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/HBoxContainer2/ButtonRefresh");
        ItemListData = GetNode<ItemList>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/ItemListData");
        CapasDiseño = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño");
        ContainerLayers = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño/ScrollContainer/ContainerLayers");
    }
}