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
    private KuroButton ButtonCargarMapa;
    private KuroButton ButtonEliminarMapa;
    private KuroCheckButton KuroCheckButtonGrid;

    public void InitializeUI()
    {
        ButtonNuevoMapa = GetNode<KuroButton>("ButtonNuevoMapa");
        ButtonGuardarMapa = GetNode<KuroButton>("ButtonGuardarMapa");
        ButtonGuardarComoMapa = GetNode<KuroButton>("ButtonGuardarComoMapa");
        ButtonCargarMapa = GetNode<KuroButton>("ButtonCargarMapa");
        ButtonEliminarMapa = GetNode<KuroButton>("ButtonEliminarMapa");
        KuroCheckButtonGrid = GetNode<KuroCheckButton>("KuroCheckButtonGrid");
    }
}