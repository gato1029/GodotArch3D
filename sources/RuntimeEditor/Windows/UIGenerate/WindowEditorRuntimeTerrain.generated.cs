// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowEditorRuntimeTerrain : Window
{
    public delegate void EventNotifyChangued(WindowEditorRuntimeTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ControlBlackyAtlasTexture EditorTextura;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        EditorTextura = GetNode<ControlBlackyAtlasTexture>("PanelContainer/EditorTextura");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}