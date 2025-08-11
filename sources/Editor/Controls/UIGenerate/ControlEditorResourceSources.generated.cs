// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlEditorResourceSources : MarginContainer
{
    public delegate void EventNotifyChangued(ControlEditorResourceSources objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerFocus;
    private VBoxContainer Diseño;
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
        Diseño = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/Diseño");
        TextureRectImage = GetNode<TextureRect>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer2/TextureRectImage");
        SpinBoxSeed = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/HBoxContainer/SpinBoxSeed");
        ButtonSeedRandom = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/HBoxContainer/ButtonSeedRandom");
        SpinBoxChunkX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkX");
        SpinBoxChunkY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkY");
        ButtonAutomatic = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/ButtonAutomatic");
        ButtonRefresh = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer2/ButtonRefresh");
        ItemListData = GetNode<ItemList>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/ItemListData");
        CapasDiseño = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño");
        ContainerLayers = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño/ScrollContainer/ContainerLayers");
    }
}