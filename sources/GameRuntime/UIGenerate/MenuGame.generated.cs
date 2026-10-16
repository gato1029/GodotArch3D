// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MenuGame : CanvasLayer
{
    public delegate void EventNotifyChangued(MenuGame objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer Panel;
    private Button continuar;
    private Button nuevaPartida;
    private Button salir;

    public void InitializeUI()
    {
        Panel = GetNode<PanelContainer>("Panel");
        continuar = GetNode<Button>("Panel/VBoxContainer/continuar");
        nuevaPartida = GetNode<Button>("Panel/VBoxContainer/nuevaPartida");
        salir = GetNode<Button>("Panel/VBoxContainer/salir");
    }
}