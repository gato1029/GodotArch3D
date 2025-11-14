// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowSearchTileMaterial : Window
{
    public delegate void EventNotifyChangued(WindowSearchTileMaterial objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private Button ButtonSearch;
    private CheckButton CheckButtonKeepOpen;
    private WindowKuroTiles ControlKuroTiles;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/MarginContainer/HBoxContainer/LineEditName");
        ButtonSearch = GetNode<Button>("MarginContainer/VBoxContainer/MarginContainer/HBoxContainer/ButtonSearch");
        CheckButtonKeepOpen = GetNode<CheckButton>("MarginContainer/VBoxContainer/MarginContainer/HBoxContainer/CheckButtonKeepOpen");
        ControlKuroTiles = GetNode<WindowKuroTiles>("MarginContainer/VBoxContainer/ControlKuroTiles");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}