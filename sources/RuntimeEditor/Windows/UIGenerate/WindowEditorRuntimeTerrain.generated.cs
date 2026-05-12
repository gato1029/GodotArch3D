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
    private KuroOptionButton TipoBrush;
    private SpinBox SpinBoxSizeBrush;
    private SpinBox SpinBoxAltura;
    private SpinBox SpinBoxCapa;
    private ControlBlackyAtlasTexture EditorTextura;
    private CenterContainer ContenedorDual;
    private TextureRect TexturaDual;
    private Label LabelDualName;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroButtonBuscar = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonBuscar");
        KuroButtonBuscarAutomatico = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonBuscarAutomatico");
        KuroButtonCrear = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonCrear");
        KuroButtonSeleccion = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonSeleccion");
        KuroButtonBorrar = GetNode<KuroButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/KuroButtonBorrar");
        TipoBrush = GetNode<KuroOptionButton>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/HBoxContainer2/TipoBrush");
        SpinBoxSizeBrush = GetNode<SpinBox>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/HBoxContainer2/SpinBoxSizeBrush");
        SpinBoxAltura = GetNode<SpinBox>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/HBoxContainer/SpinBoxAltura");
        SpinBoxCapa = GetNode<SpinBox>("PanelContainer/ScrollContainer/VBoxContainer/HBoxContainer/HBoxContainer/SpinBoxCapa");
        EditorTextura = GetNode<ControlBlackyAtlasTexture>("PanelContainer/ScrollContainer/VBoxContainer/EditorTextura");
        ContenedorDual = GetNode<CenterContainer>("PanelContainer/ScrollContainer/VBoxContainer/ContenedorDual");
        TexturaDual = GetNode<TextureRect>("PanelContainer/ScrollContainer/VBoxContainer/ContenedorDual/VBoxContainer/TexturaDual");
        LabelDualName = GetNode<Label>("PanelContainer/ScrollContainer/VBoxContainer/ContenedorDual/VBoxContainer/LabelDualName");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}