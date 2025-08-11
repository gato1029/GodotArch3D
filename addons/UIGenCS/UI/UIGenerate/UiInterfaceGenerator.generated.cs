// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class UiInterfaceGenerator : Control
{
    public delegate void EventNotifyChangued(UiInterfaceGenerator objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private MenuButton MenuButtonOptions;
    private Button ButtonGenerate;

    public void InitializeUI()
    {
        MenuButtonOptions = GetNode<MenuButton>("VBoxContainer/HBoxContainer/MenuButtonOptions");
        ButtonGenerate = GetNode<Button>("VBoxContainer/HBoxContainer/ButtonGenerate");
    }
}