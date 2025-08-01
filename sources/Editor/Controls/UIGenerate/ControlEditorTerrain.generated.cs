// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlEditorTerrain : MarginContainer
{
    public delegate void EventNotifyChangued(ControlEditorTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerFocus;
    private VBoxContainer Diseño;
    private SpinBox SpinBoxGridX;
    private SpinBox SpinBoxGridY;
    private TextureRect TextureRectImage;
    private Button ButtonSearch;
    private SpinBox SpinBoxChunkX;
    private SpinBox SpinBoxChunkY;
    private Button ButtonAutomaticTerrain;
    private Button ButtonRefresh;
    private OptionButton OptionButtonLayer;
    private ItemList ItemListRules;
    private MarginContainer CapasDiseño;
    private VBoxContainer ContainerLayers;
    private MarginContainer CapasReales;
    private VBoxContainer ContainerLayersReal;

    public void InitializeUI()
    {
        PanelContainerFocus = GetNode<PanelContainer>("PanelContainerFocus");
        Diseño = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/Diseño");
        SpinBoxGridX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/GridContainer/SpinBoxGridX");
        SpinBoxGridY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/GridContainer/SpinBoxGridY");
        TextureRectImage = GetNode<TextureRect>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/VBoxContainer/TextureRectImage");
        ButtonSearch = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/VBoxContainer/ButtonSearch");
        SpinBoxChunkX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkX");
        SpinBoxChunkY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkY");
        ButtonAutomaticTerrain = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/ButtonAutomaticTerrain");
        ButtonRefresh = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer2/ButtonRefresh");
        OptionButtonLayer = GetNode<OptionButton>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer3/OptionButtonLayer");
        OptionButtonLayer.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        ItemListRules = GetNode<ItemList>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/ItemListRules");
        CapasDiseño = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño");
        ContainerLayers = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasDiseño/ScrollContainer/ContainerLayers");
        CapasReales = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasReales");
        ContainerLayersReal = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/CapasReales/ScrollContainer/ContainerLayersReal");
    }
}