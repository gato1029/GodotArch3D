// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class GamePanelDetalle : PanelContainer
{
    public delegate void EventNotifyChangued(GamePanelDetalle objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Label Titulo;
    private TextureRect Textura;
    private GridContainer Contenedor;

    public void InitializeUI()
    {
        Titulo = GetNode<Label>("VBoxContainer/Titulo");
        Textura = GetNode<TextureRect>("VBoxContainer/HBoxContainer/VBoxContainer2/PanelContainer/Textura");
        Contenedor = GetNode<GridContainer>("VBoxContainer/HBoxContainer/Contenedor");
    }
}