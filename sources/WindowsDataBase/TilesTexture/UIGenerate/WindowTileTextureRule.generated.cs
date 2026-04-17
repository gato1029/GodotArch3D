// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTileTextureRule : Window
{
    public delegate void EventNotifyChangued(WindowTileTextureRule objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private FixedGridContainer FixedGridContainer2;
    private KuroTextureButton KuroTextureButtonIgnorarCentro;
    private KuroTextureButton KuroTextureButtonIgnorar;
    private KuroTextureButton KuroTextureButtonVacio;
    private KuroTextureButton KuroTextureButtonLleno;
    private KuroTextureButton KuroTextureButtonExacto;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        FixedGridContainer2 = GetNode<FixedGridContainer>("PanelContainer/MarginContainer/HBoxContainer/FixedGridContainer2");
        KuroTextureButtonIgnorarCentro = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/HBoxContainer/FixedGridContainer2/KuroTextureButtonIgnorarCentro");
        KuroTextureButtonIgnorar = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/HBoxContainer/FixedGridContainer2/KuroTextureButtonIgnorar");
        KuroTextureButtonVacio = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/HBoxContainer/FixedGridContainer2/KuroTextureButtonVacio");
        KuroTextureButtonLleno = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/HBoxContainer/FixedGridContainer2/KuroTextureButtonLleno");
        KuroTextureButtonExacto = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/HBoxContainer/FixedGridContainer2/KuroTextureButtonExacto");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}