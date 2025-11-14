// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowProjectile : Window
{
    public delegate void EventNotifyChangued(WindowProjectile objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabContainer TabContainerBase;
    private TabBar Detalle;
    private LineEdit LineEditName;
    private ControlSprite ControlSpriteBasico;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TabContainerBase = GetNode<TabContainer>("MarginContainer/TabContainerBase");
        Detalle = GetNode<TabBar>("MarginContainer/TabContainerBase/Detalle");
        LineEditName = GetNode<LineEdit>("MarginContainer/TabContainerBase/Detalle/VBoxContainer/GridContainer2/LineEditName");
        ControlSpriteBasico = GetNode<ControlSprite>("MarginContainer/TabContainerBase/Detalle/VBoxContainer/ControlSpriteBasico");
        ButtonSave = GetNode<Button>("MarginContainer/TabContainerBase/Detalle/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}