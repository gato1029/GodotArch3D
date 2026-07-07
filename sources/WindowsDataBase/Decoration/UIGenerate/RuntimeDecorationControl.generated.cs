// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeDecorationControl : PanelContainer
{
    public delegate void EventNotifyChangued(RuntimeDecorationControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroSearchItems KuroItems;
    private KuroButton ButtonNuevo;
    private LineEdit LineEditName;
    private TileSpritePreview TileSpriteSelector;
    private KuroButton ButtonGuardar;
    private KuroButton ButtonEliminar;

    public void InitializeUI()
    {
        KuroItems = GetNode<KuroSearchItems>("HBoxContainer2/KuroItems");
        ButtonNuevo = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/ButtonNuevo");
        LineEditName = GetNode<LineEdit>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/LineEditName");
        TileSpriteSelector = GetNode<TileSpritePreview>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/TileSpriteSelector");
        ButtonGuardar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonGuardar");
        ButtonEliminar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonEliminar");
    }
}