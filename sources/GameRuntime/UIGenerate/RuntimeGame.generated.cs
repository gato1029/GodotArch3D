// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeGame : Node
{
    public delegate void EventNotifyChangued(RuntimeGame objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Node Base;
    private Node3D mainRender;
    private CanvasLayer mainRender2D;
    private Node Game;
    private MeshInstance3D center;
    private Node Render;
    private Camera3dGodot Camera3D;
    private InputHandler InputHandlerGame;
    private CanvasLayer UI;
    private MainHudController MainHUDController;
    private WorldControlManager worldControlManager;
    private VBoxContainer GridContainer;
    private GamePanelDetalle PanelDetalle;
    private CenterContainer CenterContainter;
    private PanelContainer PanelTopCenter;
    private Button ButtonInformacion;
    private Button ButtonInventario;
    private PanelContainer Reloj;
    private AnimatedTextureRect AnimatedTextureRect;
    private Label LabelReloj;
    private Button ButtonRecursos;
    private Button ButtonPlanos;
    private PanelContainer PanelCentral;
    private Button ButtonSlot1;
    private Button ButtonSlot2;
    private Button ButtonSlot3;
    private Button ButtonSlot4;
    private Button ButtonSlot5;

    public void InitializeUI()
    {
        Base = GetNode<Node>("Base");
        mainRender = GetNode<Node3D>("Base/mainRender");
        mainRender2D = GetNode<CanvasLayer>("Base/mainRender2D");
        Game = GetNode<Node>("Game");
        center = GetNode<MeshInstance3D>("Game/center");
        Render = GetNode<Node>("Render");
        Camera3D = GetNode<Camera3dGodot>("Render/Camera3D");
        InputHandlerGame = GetNode<InputHandler>("Render/InputHandlerGame");
        UI = GetNode<CanvasLayer>("UI");
        MainHUDController = GetNode<MainHudController>("UI/MainHUDController");
        worldControlManager = GetNode<WorldControlManager>("UI/MainHUDController/worldControlManager");
        GridContainer = GetNode<VBoxContainer>("UI/MainHUDController/GridContainer");
        PanelDetalle = GetNode<GamePanelDetalle>("UI/MainHUDController/GridContainer/HBoxContainer/Control/PanelDetalle");
        CenterContainter = GetNode<CenterContainer>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter");
        PanelTopCenter = GetNode<PanelContainer>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter");
        ButtonInformacion = GetNode<Button>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonInformacion");
        ButtonInventario = GetNode<Button>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonInventario");
        Reloj = GetNode<PanelContainer>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/Reloj");
        AnimatedTextureRect = GetNode<AnimatedTextureRect>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/Reloj/HBoxContainer/AnimatedTextureRect");
        LabelReloj = GetNode<Label>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/Reloj/HBoxContainer/LabelReloj");
        ButtonRecursos = GetNode<Button>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonRecursos");
        ButtonPlanos = GetNode<Button>("UI/MainHUDController/GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonPlanos");
        PanelCentral = GetNode<PanelContainer>("UI/MainHUDController/GridContainer/PanelCentral");
        ButtonSlot1 = GetNode<Button>("UI/MainHUDController/GridContainer/PanelContainer2/HBoxContainer/ButtonSlot1");
        ButtonSlot2 = GetNode<Button>("UI/MainHUDController/GridContainer/PanelContainer2/HBoxContainer/ButtonSlot2");
        ButtonSlot3 = GetNode<Button>("UI/MainHUDController/GridContainer/PanelContainer2/HBoxContainer/ButtonSlot3");
        ButtonSlot4 = GetNode<Button>("UI/MainHUDController/GridContainer/PanelContainer2/HBoxContainer/ButtonSlot4");
        ButtonSlot5 = GetNode<Button>("UI/MainHUDController/GridContainer/PanelContainer2/HBoxContainer/ButtonSlot5");
    }
}