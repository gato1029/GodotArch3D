// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class PerfiladorAutoTileTexture : Node
{
    public delegate void EventNotifyChangued(PerfiladorAutoTileTexture objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Node3D mainRender;
    private MeshInstance3D center;

    public void InitializeUI()
    {
        mainRender = GetNode<Node3D>("mainRender");
        center = GetNode<MeshInstance3D>("center");
    }
}