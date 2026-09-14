// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MenuContainer : VBoxContainer
{
    public delegate void EventNotifyChangued(MenuContainer objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private MarginContainer Menu;
    private HBoxContainer hbox;
    private KuroButton ButtonDualGrid;
    private KuroButton ButtonDecoration;
    private KuroButton ButtonSuperficie;
    private KuroButton ButtonRamps;
    private KuroButton ButtonCaminos;
    private KuroButton ButtonTerreno;
    private KuroButton ButtonBiomas;
    private HerramientaMenuBar MenuBar;
    private PopupMenu Atlas;
    private PopupMenu Creador_Tiles;
    private PopupMenu Editor;
    private PopupMenu Componentes;
    private PopupMenu Armamento;
    private PopupMenu Mapas;
    private KuroButton ButtonGuardarMod;
    private PanelContainer ContenedorEditor;

    public void InitializeUI()
    {
        Menu = GetNode<MarginContainer>("Menu");
        hbox = GetNode<HBoxContainer>("Menu/hbox");
        ButtonDualGrid = GetNode<KuroButton>("Menu/hbox/ButtonDualGrid");
        ButtonDecoration = GetNode<KuroButton>("Menu/hbox/ButtonDecoration");
        ButtonSuperficie = GetNode<KuroButton>("Menu/hbox/ButtonSuperficie");
        ButtonRamps = GetNode<KuroButton>("Menu/hbox/ButtonRamps");
        ButtonCaminos = GetNode<KuroButton>("Menu/hbox/ButtonCaminos");
        ButtonTerreno = GetNode<KuroButton>("Menu/hbox/ButtonTerreno");
        ButtonBiomas = GetNode<KuroButton>("Menu/hbox/ButtonBiomas");
        MenuBar = GetNode<HerramientaMenuBar>("Menu/hbox/MenuBar");
        Atlas = GetNode<PopupMenu>("Menu/hbox/MenuBar/Atlas");
        Creador_Tiles = GetNode<PopupMenu>("Menu/hbox/MenuBar/Creador Tiles");
        Editor = GetNode<PopupMenu>("Menu/hbox/MenuBar/Editor");
        Componentes = GetNode<PopupMenu>("Menu/hbox/MenuBar/Componentes");
        Armamento = GetNode<PopupMenu>("Menu/hbox/MenuBar/Armamento");
        Mapas = GetNode<PopupMenu>("Menu/hbox/MenuBar/Mapas");
        ButtonGuardarMod = GetNode<KuroButton>("Menu/hbox/ButtonGuardarMod");
        ContenedorEditor = GetNode<PanelContainer>("ContenedorEditor");
    }
}