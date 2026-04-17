// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowGroupTileTexture : Window
{
    public delegate void EventNotifyChangued(WindowGroupTileTexture objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroTextureButton KuroTextureButton;
    private KuroTextureButton KuroTextureButtonAdd;
    private KuroTextureButton KuroTextureButtonSave;
    private KuroCheckButton KuroCheckButtonSwitch;
    private KuroScrollZoomView KuroScrollZoomView;
    private GridContainer Contenedor;
    private KuroDraggableControl KuroDraggableControl;
    private KuroTextureButton KuroTextureButton1;
    private KuroDraggableControl KuroDraggableControl2;
    private KuroTextureButton KuroTextureButton2;
    private KuroDraggableControl KuroDraggableControl3;
    private KuroTextureButton KuroTextureButton3;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroTextureButton = GetNode<KuroTextureButton>("PanelContainer/VBoxContainer/HBoxContainer/KuroTextureButton");
        KuroTextureButtonAdd = GetNode<KuroTextureButton>("PanelContainer/VBoxContainer/HBoxContainer2/KuroTextureButtonAdd");
        KuroTextureButtonSave = GetNode<KuroTextureButton>("PanelContainer/VBoxContainer/HBoxContainer2/KuroTextureButtonSave");
        KuroCheckButtonSwitch = GetNode<KuroCheckButton>("PanelContainer/VBoxContainer/HBoxContainer2/KuroCheckButtonSwitch");
        KuroScrollZoomView = GetNode<KuroScrollZoomView>("PanelContainer/VBoxContainer/KuroScrollZoomView");
        Contenedor = GetNode<GridContainer>("PanelContainer/VBoxContainer/Contenedor");
        KuroDraggableControl = GetNode<KuroDraggableControl>("PanelContainer/VBoxContainer/GridContainer/KuroDraggableControl");
        KuroTextureButton1 = GetNode<KuroTextureButton>("PanelContainer/VBoxContainer/GridContainer/KuroDraggableControl/KuroTextureButton1");
        KuroDraggableControl2 = GetNode<KuroDraggableControl>("PanelContainer/VBoxContainer/GridContainer/KuroDraggableControl2");
        KuroTextureButton2 = GetNode<KuroTextureButton>("PanelContainer/VBoxContainer/GridContainer/KuroDraggableControl2/KuroTextureButton2");
        KuroDraggableControl3 = GetNode<KuroDraggableControl>("PanelContainer/VBoxContainer/GridContainer/KuroDraggableControl3");
        KuroTextureButton3 = GetNode<KuroTextureButton>("PanelContainer/VBoxContainer/GridContainer/KuroDraggableControl3/KuroTextureButton3");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}