// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MenuEditorContenedor : HBoxContainer
{
    public delegate void EventNotifyChangued(MenuEditorContenedor objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroButton ButtonNuevoMapa;
    private KuroButton ButtonGuardarMapa;
    private KuroButton ButtonGuardarComoMapa;
    private KuroButton ButtonEliminarMapa;

    public void InitializeUI()
    {
        ButtonNuevoMapa = GetNode<KuroButton>("ButtonNuevoMapa");
        ButtonGuardarMapa = GetNode<KuroButton>("ButtonGuardarMapa");
        ButtonGuardarComoMapa = GetNode<KuroButton>("ButtonGuardarComoMapa");
        ButtonEliminarMapa = GetNode<KuroButton>("ButtonEliminarMapa");
    }
}