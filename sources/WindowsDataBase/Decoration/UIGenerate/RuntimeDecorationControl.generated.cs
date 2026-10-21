// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeDecorationControl : PanelContainer
{
    public delegate void EventNotifyChangued(RuntimeDecorationControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroSearchItems KuroItems;
    private KuroButton ButtonNuevo;
    private VBoxContainer ContenedorDatos;
    private LineEdit LineEditName;
    private TileSpritePreview TileSpriteSelector;
    private KuroButton ButtonGuardar;
    private KuroButton ButtonEliminar;

    public void InitializeUI()
    {
        KuroItems = GetNode<KuroSearchItems>("HBoxContainer2/KuroItems");
        ButtonNuevo = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer2/HBoxContainer/ButtonNuevo");
        ContenedorDatos = GetNode<VBoxContainer>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer2/ContenedorDatos");
        LineEditName = GetNode<LineEdit>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer2/ContenedorDatos/GridContainer/LineEditName");
        TileSpriteSelector = GetNode<TileSpritePreview>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer2/ContenedorDatos/GridContainer/TileSpriteSelector");
        ButtonGuardar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer2/ContenedorDatos/HBoxContainer2/ButtonGuardar");
        ButtonEliminar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer2/ContenedorDatos/HBoxContainer2/ButtonEliminar");
    }
}