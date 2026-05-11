// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlDualGrid : PanelContainer
{
    public delegate void EventNotifyChangued(ControlDualGrid objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroSearchItems KuroItems;
    private LineEdit LineEditName;
    private KuroButton ButtonNuevo;
    private KuroButton ButtonGuardar;
    private KuroButton ButtonEliminar;
    private GridContainer GridContainerItems;
    private ControlBlackyAtlasTexture ControlVisualizadorTexture;
    private KuroButton ButtonBuscarTextura;

    public void InitializeUI()
    {
        KuroItems = GetNode<KuroSearchItems>("HBoxContainer2/KuroItems");
        LineEditName = GetNode<LineEdit>("HBoxContainer2/VBoxContainer/HBoxContainer2/LineEditName");
        ButtonNuevo = GetNode<KuroButton>("HBoxContainer2/VBoxContainer/HBoxContainer2/ButtonNuevo");
        ButtonGuardar = GetNode<KuroButton>("HBoxContainer2/VBoxContainer/HBoxContainer2/ButtonGuardar");
        ButtonEliminar = GetNode<KuroButton>("HBoxContainer2/VBoxContainer/HBoxContainer2/ButtonEliminar");
        GridContainerItems = GetNode<GridContainer>("HBoxContainer2/VBoxContainer/HBoxContainer/ScrollContainer/GridContainerItems");
        ControlVisualizadorTexture = GetNode<ControlBlackyAtlasTexture>("HBoxContainer2/VBoxContainer/HBoxContainer/PanelContainer/ControlVisualizadorTexture");
        ButtonBuscarTextura = GetNode<KuroButton>("HBoxContainer2/VBoxContainer/HBoxContainer/PanelContainer/ButtonBuscarTextura");
    }
}