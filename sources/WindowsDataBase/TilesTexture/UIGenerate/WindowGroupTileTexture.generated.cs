// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowGroupTileTexture : Window
{
    public delegate void EventNotifyChangued(WindowGroupTileTexture objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditMaterial;
    private KuroTextureButton KuroTextureButtonSearch;
    private KuroTextureButton KuroTextureButtonAdd;
    private KuroTextureButton KuroTextureButtonSave;
    private KuroCheckButton KuroCheckButtonSwitch;
    private KuroScrollZoomView KuroScrollZoomView;
    private GridContainer Contenedor;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditMaterial = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/LineEditMaterial");
        KuroTextureButtonSearch = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/KuroTextureButtonSearch");
        KuroTextureButtonAdd = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/KuroTextureButtonAdd");
        KuroTextureButtonSave = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/KuroTextureButtonSave");
        KuroCheckButtonSwitch = GetNode<KuroCheckButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer2/KuroCheckButtonSwitch");
        KuroScrollZoomView = GetNode<KuroScrollZoomView>("PanelContainer/MarginContainer/VBoxContainer/ScrollContainer/KuroScrollZoomView");
        Contenedor = GetNode<GridContainer>("PanelContainer/MarginContainer/VBoxContainer/ScrollContainer/KuroScrollZoomView/Contenedor");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}