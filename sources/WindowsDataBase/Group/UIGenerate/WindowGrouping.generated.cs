// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowGrouping : Window
{
    public delegate void EventNotifyChangued(WindowGrouping objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private ControlKuroTile TileTexture;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/GridContainer/LineEditName");
        TileTexture = GetNode<ControlKuroTile>("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/GridContainer/TileTexture");
        ButtonSave = GetNode<Button>("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}