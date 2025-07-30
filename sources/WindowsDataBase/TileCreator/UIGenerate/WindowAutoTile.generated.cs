// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowAutoTile : Window
{
    public delegate void EventNotifyChangued(WindowAutoTile objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer VBoxContainer;
    private LineEdit LineEditId;
    private LineEdit LineEditName;
    private Button ButtonSave;
    private Button ButtonSaveActive;
    private Button ButtonNewRule;
    private GridContainer GridContainerItems;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        VBoxContainer = GetNode<HBoxContainer>("Panel/MarginContainer/VBoxContainer/VBoxContainer");
        LineEditId = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEditId");
        LineEditName = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEditName");
        ButtonSave = GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/VBoxContainer/ButtonSave");
        ButtonSaveActive = GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/VBoxContainer/ButtonSaveActive");
        ButtonNewRule = GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/VBoxContainer/ButtonNewRule");
        GridContainerItems = GetNode<GridContainer>("Panel/MarginContainer/VBoxContainer/ScrollContainer/GridContainerItems");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}