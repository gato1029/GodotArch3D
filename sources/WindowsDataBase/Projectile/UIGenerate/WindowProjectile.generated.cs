// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowProjectile : Window
{
    public delegate void EventNotifyChangued(WindowProjectile objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private ControlListTileSprite ControlTileSpriteItem;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/GridContainer2/LineEditName");
        ControlTileSpriteItem = GetNode<ControlListTileSprite>("MarginContainer/VBoxContainer/GridContainer2/ControlTileSpriteItem");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}