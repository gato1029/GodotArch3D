// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowMessage : Window
{
    public delegate void EventNotifyChangued(WindowMessage objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonAccept;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ButtonAccept = GetNode<Button>("PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/ButtonAccept");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}