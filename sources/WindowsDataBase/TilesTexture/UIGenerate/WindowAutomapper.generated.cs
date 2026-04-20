// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowAutomapper : Window
{
    public delegate void EventNotifyChangued(WindowAutomapper objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private KuroTextureButton ButtonAdd;
    private KuroTextureButton ButtonDelete;
    private KuroTextureButton ButtonSave;
    private ItemList ItemsContainer;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/LineEditName");
        ButtonAdd = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/ButtonAdd");
        ButtonDelete = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/ButtonDelete");
        ButtonSave = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/ButtonSave");
        ItemsContainer = GetNode<ItemList>("PanelContainer/MarginContainer/VBoxContainer/ItemsContainer");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}