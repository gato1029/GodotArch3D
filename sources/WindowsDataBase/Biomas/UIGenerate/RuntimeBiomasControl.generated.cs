// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeBiomasControl : PanelContainer
{
    public delegate void EventNotifyChangued(RuntimeBiomasControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroSearchItems KuroItems;
    private KuroButton ButtonNuevo;
    private LineEdit LineEditName;
    private Button ButtonTerreno;
    private Button ButtonSuperficie;
    private Button ButtonCaminos;
    private ControlListResourcesSources ListaRecursos;
    private KuroButton ButtonGuardar;
    private KuroButton ButtonEliminar;

    public void InitializeUI()
    {
        KuroItems = GetNode<KuroSearchItems>("HBoxContainer2/KuroItems");
        ButtonNuevo = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/ButtonNuevo");
        LineEditName = GetNode<LineEdit>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/LineEditName");
        ButtonTerreno = GetNode<Button>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/ButtonTerreno");
        ButtonSuperficie = GetNode<Button>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/ButtonSuperficie");
        ButtonCaminos = GetNode<Button>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/ButtonCaminos");
        ListaRecursos = GetNode<ControlListResourcesSources>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/ListaRecursos");
        ButtonGuardar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonGuardar");
        ButtonEliminar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonEliminar");
    }
}