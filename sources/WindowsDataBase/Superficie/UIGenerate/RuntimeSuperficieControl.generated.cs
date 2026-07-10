// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeSuperficieControl : PanelContainer
{
    public delegate void EventNotifyChangued(RuntimeSuperficieControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroSearchItems KuroItems;
    private KuroButton ButtonNuevo;
    private LineEdit LineEditName;
    private TileSpritePreview PreviewSprite;
    private KuroButton ButtonGuardar;
    private KuroButton ButtonEliminar;

    public void InitializeUI()
    {
        KuroItems = GetNode<KuroSearchItems>("HBoxContainer2/KuroItems");
        ButtonNuevo = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/ButtonNuevo");
        LineEditName = GetNode<LineEdit>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/LineEditName");
        PreviewSprite = GetNode<TileSpritePreview>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/GridContainer/PreviewSprite");
        ButtonGuardar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonGuardar");
        ButtonEliminar = GetNode<KuroButton>("HBoxContainer2/MarginContainer/CenterContainer/VBoxContainer/HBoxContainer2/ButtonEliminar");
    }
}