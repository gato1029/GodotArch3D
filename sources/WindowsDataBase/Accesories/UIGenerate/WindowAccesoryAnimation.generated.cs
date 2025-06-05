// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowAccesoryAnimation : Window
{
    public delegate void EventNotifyChangued(WindowAccesoryAnimation objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private Button ButtonSave;
    private Button ButtonDelete;
    private ContainerAnimation ControlContainerAnimation;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/LineEditName");
        ButtonSave = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/ButtonSave");
        ButtonDelete = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/ButtonDelete");
        ControlContainerAnimation = GetNode<ContainerAnimation>("Panel/MarginContainer/VBoxContainer/ControlContainerAnimation");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}