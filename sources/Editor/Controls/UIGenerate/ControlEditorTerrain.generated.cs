// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlEditorTerrain : MarginContainer
{
    public delegate void EventNotifyChangued(ControlEditorTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerFocus;
    private TabContainer TabContainerPrincipal;
    private VBoxContainer Diseño;
    private TextureRect TextureRectImage;
    private Button ButtonSearch;
    private SpinBox SpinBoxSeed;
    private Button ButtonSeedRandom;
    private SpinBox SpinBoxChunkX;
    private SpinBox SpinBoxChunkY;
    private Button ButtonAutomaticTerrain;
    private Button ButtonRefresh;
    private TabContainer TabContainerLevels;
    private MarginContainer DisenioTextura;
    private Button ButtonSearchTexture;
    private WindowKuroTiles KuroTilesTexture;
    private MarginContainer CapasDiseño;
    private VBoxContainer ContainerLayers;

    public void InitializeUI()
    {
        PanelContainerFocus = GetNode<PanelContainer>("PanelContainerFocus");
        TabContainerPrincipal = GetNode<TabContainer>("PanelContainerFocus/MarginContainer/TabContainerPrincipal");
        Diseño = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño");
        TextureRectImage = GetNode<TextureRect>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/HBoxContainer/VBoxContainer/TextureRectImage");
        ButtonSearch = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/HBoxContainer/VBoxContainer/ButtonSearch");
        SpinBoxSeed = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/VBoxContainer/HBoxContainer/SpinBoxSeed");
        ButtonSeedRandom = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/VBoxContainer/HBoxContainer/ButtonSeedRandom");
        SpinBoxChunkX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/VBoxContainer/GridContainer/SpinBoxChunkX");
        SpinBoxChunkY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/VBoxContainer/GridContainer/SpinBoxChunkY");
        ButtonAutomaticTerrain = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/VBoxContainer/ButtonAutomaticTerrain");
        ButtonRefresh = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/HBoxContainer2/ButtonRefresh");
        TabContainerLevels = GetNode<TabContainer>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/Diseño/TabContainerLevels");
        DisenioTextura = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/DisenioTextura");
        ButtonSearchTexture = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/DisenioTextura/VBoxContainer/ButtonSearchTexture");
        KuroTilesTexture = GetNode<WindowKuroTiles>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/DisenioTextura/VBoxContainer/KuroTilesTexture");
        CapasDiseño = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/CapasDiseño");
        ContainerLayers = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainerPrincipal/CapasDiseño/ScrollContainer/ContainerLayers");
    }
}