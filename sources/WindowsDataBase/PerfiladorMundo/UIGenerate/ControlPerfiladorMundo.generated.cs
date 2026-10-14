// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlPerfiladorMundo : PanelContainer
{
    public delegate void EventNotifyChangued(ControlPerfiladorMundo objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroSearchItems KuroItems;
    private KuroButton ButtonNuevo;
    private LineEdit LineEditName;
    private KuroButton ButtonAgregarBioma;
    private VBoxContainer ContedorReglas;
    private KuroOptionButton GeografiaCombo;
    private KuroOptionButton TipoMundoCombo;
    private KuroButton ButtonGuardar;
    private KuroButton ButtonEliminar;

    public void InitializeUI()
    {
        KuroItems = GetNode<KuroSearchItems>("HBoxContainer2/KuroItems");
        ButtonNuevo = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/ButtonNuevo");
        LineEditName = GetNode<LineEdit>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/LineEditName");
        ButtonAgregarBioma = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/VBoxContainer/ButtonAgregarBioma");
        ContedorReglas = GetNode<VBoxContainer>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/VBoxContainer/ScrollContainer/ContedorReglas");
        GeografiaCombo = GetNode<KuroOptionButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/GeografiaCombo");
        TipoMundoCombo = GetNode<KuroOptionButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/TipoMundoCombo");
        ButtonGuardar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonGuardar");
        ButtonEliminar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonEliminar");
    }
}