// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowGroupTileTexture : Window
{
    public delegate void EventNotifyChangued(WindowGroupTileTexture objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditMaterial;
    private KuroTextureButton KuroTextureButtonSearch;
    private LineEdit LineEditName;
    private KuroTextureButton KuroTextureButtonAdd;
    private KuroTextureButton KuroTextureButtonSave;
    private KuroCheckButton KuroCheckButtonSwitch;
    private PerfiladorAutoTileTexture Game;
    private Node3D mainRender;
    private MeshInstance3D center;
    private Node Render;
    private Camera3dGodot Camera3D;
    private KuroScrollZoomView KuroScrollZoomView;
    private GridContainer Contenedor;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditMaterial = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/LineEditMaterial");
        KuroTextureButtonSearch = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/KuroTextureButtonSearch");
        LineEditName = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/LineEditName");
        KuroTextureButtonAdd = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/KuroTextureButtonAdd");
        KuroTextureButtonSave = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/KuroTextureButtonSave");
        KuroCheckButtonSwitch = GetNode<KuroCheckButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/KuroCheckButtonSwitch");
        Game = GetNode<PerfiladorAutoTileTexture>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer3/HSplitContainer/PanelContainer/SubViewportContainer/SubViewport/Game");
        mainRender = GetNode<Node3D>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer3/HSplitContainer/PanelContainer/SubViewportContainer/SubViewport/Game/mainRender");
        center = GetNode<MeshInstance3D>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer3/HSplitContainer/PanelContainer/SubViewportContainer/SubViewport/Game/center");
        Render = GetNode<Node>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer3/HSplitContainer/PanelContainer/SubViewportContainer/SubViewport/Render");
        Camera3D = GetNode<Camera3dGodot>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer3/HSplitContainer/PanelContainer/SubViewportContainer/SubViewport/Render/Camera3D");
        KuroScrollZoomView = GetNode<KuroScrollZoomView>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer3/HSplitContainer/PanelContainer2/ScrollContainer/KuroScrollZoomView");
        Contenedor = GetNode<GridContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer3/HSplitContainer/PanelContainer2/ScrollContainer/KuroScrollZoomView/Contenedor");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}