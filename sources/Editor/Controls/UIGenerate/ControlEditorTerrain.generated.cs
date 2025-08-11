// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlEditorTerrain : MarginContainer
{
    public delegate void EventNotifyChangued(ControlEditorTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerFocus;
    private VBoxContainer Diseño;
    private TextureRect TextureRectImage;
    private Button ButtonSearch;
    private SpinBox SpinBoxSeed;
    private Button ButtonSeedRandom;
    private SpinBox SpinBoxChunkX;
    private SpinBox SpinBoxChunkY;
    private Button ButtonAutomaticTerrain;
    private Button ButtonRefresh;
    private ItemList ItemListRules;
    private MarginContainer CapasDiseño;
    private VBoxContainer ContainerLayers;

    public void InitializeUI()
    {
        PanelContainerFocus = GetNode<PanelContainer>("PanelContainerFocus");
        Diseño = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/Diseño");
        TextureRectImage = GetNode<TextureRect>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/VBoxContainer/TextureRectImage");
        ButtonSearch = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/VBoxContainer/ButtonSearch");
        SpinBoxSeed = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/HBoxContainer/SpinBoxSeed");
        ButtonSeedRandom = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/HBoxContainer/ButtonSeedRandom");
        SpinBoxChunkX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkX");
        SpinBoxChunkY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkY");
        ButtonAutomaticTerrain = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/ButtonAutomaticTerrain");
        ButtonRefresh = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer2/ButtonRefresh");
        ItemListRules = GetNode<ItemList>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/ItemListRules");
        CapasDiseño = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño");
        ContainerLayers = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño/ScrollContainer/ContainerLayers");
    }
}