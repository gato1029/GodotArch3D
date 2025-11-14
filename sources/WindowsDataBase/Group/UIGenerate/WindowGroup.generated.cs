// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowGroup : Window
{
    public delegate void EventNotifyChangued(WindowGroup objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private ControlKuroTile ControlTile;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/GridContainer/LineEditName");
        ControlTile = GetNode<ControlKuroTile>("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/GridContainer/ControlTile");
        ButtonSave = GetNode<Button>("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}