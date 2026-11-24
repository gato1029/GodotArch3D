// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MainHudController : Control
{
    public delegate void EventNotifyChangued(MainHudController objectControl);
    public event EventNotifyChangued OnNotifyChangued;

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
        worldControlManager = GetNode<WorldControlManager>("worldControlManager");
        GridContainer = GetNode<VBoxContainer>("GridContainer");
        PanelDetalle = GetNode<GamePanelDetalle>("GridContainer/HBoxContainer/Control/PanelDetalle");
        CenterContainter = GetNode<CenterContainer>("GridContainer/HBoxContainer/Control/CenterContainter");
        PanelTopCenter = GetNode<PanelContainer>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter");
        ButtonInformacion = GetNode<Button>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonInformacion");
        ButtonInventario = GetNode<Button>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonInventario");
        Reloj = GetNode<PanelContainer>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/Reloj");
        AnimatedTextureRect = GetNode<AnimatedTextureRect>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/Reloj/HBoxContainer/AnimatedTextureRect");
        LabelReloj = GetNode<Label>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/Reloj/HBoxContainer/LabelReloj");
        ButtonRecursos = GetNode<Button>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonRecursos");
        ButtonPlanos = GetNode<Button>("GridContainer/HBoxContainer/Control/CenterContainter/HBoxContainer/PanelTopCenter/HBoxContainer2/ButtonPlanos");
        PanelCentral = GetNode<PanelContainer>("GridContainer/PanelCentral");
        ButtonSlot1 = GetNode<Button>("GridContainer/PanelContainer2/HBoxContainer/ButtonSlot1");
        ButtonSlot2 = GetNode<Button>("GridContainer/PanelContainer2/HBoxContainer/ButtonSlot2");
        ButtonSlot3 = GetNode<Button>("GridContainer/PanelContainer2/HBoxContainer/ButtonSlot3");
        ButtonSlot4 = GetNode<Button>("GridContainer/PanelContainer2/HBoxContainer/ButtonSlot4");
        ButtonSlot5 = GetNode<Button>("GridContainer/PanelContainer2/HBoxContainer/ButtonSlot5");
    }
}