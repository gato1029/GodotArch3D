// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeTerrainControl : PanelContainer
{
    public delegate void EventNotifyChangued(RuntimeTerrainControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroSearchItems KuroItems;
    private KuroButton ButtonNuevo;
    private LineEdit LineEditName;
    private KuroButton ButtonDual;
    private ControlListTileSprite ControlSprites;
    private KuroButton ButtonGuardar;
    private KuroButton ButtonEliminar;

    public void InitializeUI()
    {
        KuroItems = GetNode<KuroSearchItems>("HBoxContainer2/KuroItems");
        ButtonNuevo = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/ButtonNuevo");
        LineEditName = GetNode<LineEdit>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/LineEditName");
        ButtonDual = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/ButtonDual");
        ControlSprites = GetNode<ControlListTileSprite>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/ControlSprites");
        ButtonGuardar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonGuardar");
        ButtonEliminar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonEliminar");
    }
}