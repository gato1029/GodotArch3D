// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MenuContainer : VBoxContainer
{
    public delegate void EventNotifyChangued(MenuContainer objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private MarginContainer Menu;
    private HBoxContainer hbox;
    private KuroButton ButtonDualGrid;
    private KuroButton ButtonDecoration;
    private KuroButton ButtonSuperficie;
    private KuroButton ButtonRamps;
    private KuroButton ButtonCaminos;
    private KuroButton ButtonTerreno;
    private HerramientaMenuBar MenuBar;
    private PopupMenu Atlas;
    private PopupMenu Creador_Tiles;
    private PopupMenu Editor;
    private PopupMenu Componentes;
    private PopupMenu Armamento;
    private PopupMenu Mapas;
    private KuroButton ButtonGuardarMod;
    private PanelContainer ContenedorEditor;
    private VBoxContainer VBoxContainerRidht;
    private EditorPanel AnimatedPanelContainer;
    private Panel PanelContainer;
    private ViewPortContainerEditor SubViewportContainer;
    private Node Render;
    private Camera3dGodot Camera3D;
    private MeshInstance3D center;
    private InputHandler InputHandlerGame;

    public void InitializeUI()
    {
        Menu = GetNode<MarginContainer>("Menu");
        hbox = GetNode<HBoxContainer>("Menu/hbox");
        ButtonDualGrid = GetNode<KuroButton>("Menu/hbox/ButtonDualGrid");
        ButtonDecoration = GetNode<KuroButton>("Menu/hbox/ButtonDecoration");
        ButtonSuperficie = GetNode<KuroButton>("Menu/hbox/ButtonSuperficie");
        ButtonRamps = GetNode<KuroButton>("Menu/hbox/ButtonRamps");
        ButtonCaminos = GetNode<KuroButton>("Menu/hbox/ButtonCaminos");
        ButtonTerreno = GetNode<KuroButton>("Menu/hbox/ButtonTerreno");
        MenuBar = GetNode<HerramientaMenuBar>("Menu/hbox/MenuBar");
        Atlas = GetNode<PopupMenu>("Menu/hbox/MenuBar/Atlas");
        Creador_Tiles = GetNode<PopupMenu>("Menu/hbox/MenuBar/Creador Tiles");
        Editor = GetNode<PopupMenu>("Menu/hbox/MenuBar/Editor");
        Componentes = GetNode<PopupMenu>("Menu/hbox/MenuBar/Componentes");
        Armamento = GetNode<PopupMenu>("Menu/hbox/MenuBar/Armamento");
        Mapas = GetNode<PopupMenu>("Menu/hbox/MenuBar/Mapas");
        ButtonGuardarMod = GetNode<KuroButton>("Menu/hbox/ButtonGuardarMod");
        ContenedorEditor = GetNode<PanelContainer>("ContenedorEditor");
        VBoxContainerRidht = GetNode<VBoxContainer>("HBoxContainer/HSplitContainer/VBoxContainerRidht");
        AnimatedPanelContainer = GetNode<EditorPanel>("HBoxContainer/HSplitContainer/VBoxContainerRidht/HBoxContainer/AnimatedPanelContainer");
        PanelContainer = GetNode<Panel>("HBoxContainer/HSplitContainer/VBoxContainerRidht/HBoxContainer/PanelContainer");
        SubViewportContainer = GetNode<ViewPortContainerEditor>("HBoxContainer/HSplitContainer/VBoxContainerRidht/HBoxContainer/PanelContainer/SubViewportContainer");
        Render = GetNode<Node>("HBoxContainer/HSplitContainer/VBoxContainerRidht/HBoxContainer/PanelContainer/SubViewportContainer/SubViewport/Render");
        Camera3D = GetNode<Camera3dGodot>("HBoxContainer/HSplitContainer/VBoxContainerRidht/HBoxContainer/PanelContainer/SubViewportContainer/SubViewport/Render/Camera3D");
        center = GetNode<MeshInstance3D>("HBoxContainer/HSplitContainer/VBoxContainerRidht/HBoxContainer/PanelContainer/SubViewportContainer/SubViewport/Render/Camera3D/center");
        InputHandlerGame = GetNode<InputHandler>("HBoxContainer/HSplitContainer/VBoxContainerRidht/HBoxContainer/PanelContainer/SubViewportContainer/SubViewport/Render/InputHandlerGame");
    }
}