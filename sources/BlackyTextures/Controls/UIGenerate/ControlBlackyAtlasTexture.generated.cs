// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using GodotEcsArch.sources.BlackyTextures;
using System;

public partial class ControlBlackyAtlasTexture : MarginContainer
{
    public delegate void EventNotifyChangued(ControlBlackyAtlasTexture objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private BlackyAtlasCameraViewport AtlasViewport;
    private Camera2D AtlasCamera;
    private BlackyAtlasSelectionCanvas AtlasCanvas;

    public void InitializeUI()
    {
        AtlasViewport = GetNode<BlackyAtlasCameraViewport>("PanelContainer/SubViewportContainer/AtlasViewport");
        AtlasCamera = GetNode<Camera2D>("PanelContainer/SubViewportContainer/AtlasViewport/AtlasCamera");
        AtlasCanvas = GetNode<BlackyAtlasSelectionCanvas>("PanelContainer/SubViewportContainer/AtlasViewport/AtlasCanvas");
    }
}