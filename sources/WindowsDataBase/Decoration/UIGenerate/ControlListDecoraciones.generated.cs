// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlListDecoraciones : PanelContainer
{
    public delegate void EventNotifyChangued(ControlListDecoraciones objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer ContenedorDatos;
    private KuroButton KuroButtonAgregar;

    public void InitializeUI()
    {
        ContenedorDatos = GetNode<HBoxContainer>("MarginContainer/ScrollContainer/ContenedorDatos");
        KuroButtonAgregar = GetNode<KuroButton>("KuroButtonAgregar");
    }
}