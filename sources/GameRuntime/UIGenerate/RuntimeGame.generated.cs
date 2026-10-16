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
    }
}