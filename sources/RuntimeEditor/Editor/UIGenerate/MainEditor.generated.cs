// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MainEditor : Node
{
    public delegate void EventNotifyChangued(MainEditor objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Node Base;
    private Node3D mainRender;
    private Node Game;
    private MeshInstance3D center;
    private Node Editor;
    private EditorTerreno Terreno;
    private KuroButton KuroButton;
    private KuroButton KuroButton2;
    private KuroButton KuroButton4;
    private KuroButton KuroButton5;
    private KuroButton KuroButton3;
    private Node Render;
    private Camera3dGodot Camera3D;
    private InputHandler InputHandlerGame;

    public void InitializeUI()
    {
        Base = GetNode<Node>("Base");
        mainRender = GetNode<Node3D>("Base/mainRender");
        Game = GetNode<Node>("Game");
        center = GetNode<MeshInstance3D>("Game/center");
        Editor = GetNode<Node>("Editor");
        Terreno = GetNode<EditorTerreno>("Editor/VBoxContainer/MarginContainer/HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/Terreno");
        KuroButton = GetNode<KuroButton>("Editor/VBoxContainer/MarginContainer/HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/Terreno/KuroButton");
        KuroButton2 = GetNode<KuroButton>("Editor/VBoxContainer/MarginContainer/HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/KuroButton2");
        KuroButton4 = GetNode<KuroButton>("Editor/VBoxContainer/MarginContainer/HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/KuroButton4");
        KuroButton5 = GetNode<KuroButton>("Editor/VBoxContainer/MarginContainer/HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/KuroButton5");
        KuroButton3 = GetNode<KuroButton>("Editor/VBoxContainer/MarginContainer/HBoxContainer/PanelContainer/MarginContainer/VBoxContainer/CenterContainer/KuroButton3");
        Render = GetNode<Node>("Render");
        Camera3D = GetNode<Camera3dGodot>("Render/Camera3D");
        InputHandlerGame = GetNode<InputHandler>("Render/InputHandlerGame");
    }
}