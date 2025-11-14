// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowAutoTileSprite : Window
{
    public delegate void EventNotifyChangued(WindowAutoTileSprite objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private KuroTextureButton ButtonAdd;
    private KuroTextureButton ButtonSave;
    private KuroTextureButton ButtonDuplicate;
    private KuroTextureButton ButtonDelete;
    private VBoxContainer VBoxContainerItems;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/LineEditName");
        ButtonAdd = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/ButtonAdd");
        ButtonSave = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/ButtonSave");
        ButtonDuplicate = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/ButtonDuplicate");
        ButtonDelete = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/ButtonDelete");
        VBoxContainerItems = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/ScrollContainer/VBoxContainerItems");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}