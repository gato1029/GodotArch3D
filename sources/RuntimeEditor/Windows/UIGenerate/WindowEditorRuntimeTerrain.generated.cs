// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowEditorRuntimeTerrain : Window
{
    public delegate void EventNotifyChangued(WindowEditorRuntimeTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroButton KuroButtonBuscar;
    private KuroButton KuroButtonBuscarAutomatico;
    private KuroButton KuroButtonCrear;
    private KuroButton KuroButtonSeleccion;
    private KuroButton KuroButtonBorrar;
    private SpinBox SpinBoxAltura;
    private SpinBox SpinBoxCapa;
    private ControlBlackyAtlasTexture EditorTextura;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroButtonBuscar = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonBuscar");
        KuroButtonBuscarAutomatico = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonBuscarAutomatico");
        KuroButtonCrear = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonCrear");
        KuroButtonSeleccion = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonSeleccion");
        KuroButtonBorrar = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonBorrar");
        SpinBoxAltura = GetNode<SpinBox>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/SpinBoxAltura");
        SpinBoxCapa = GetNode<SpinBox>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/SpinBoxCapa");
        EditorTextura = GetNode<ControlBlackyAtlasTexture>("PanelContainer/ScrollContainer/VBoxContainer/EditorTextura");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}