// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowEditorRuntimeBuildings : Window
{
    public delegate void EventNotifyChangued(WindowEditorRuntimeBuildings objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroControlWindow KuroControlWindowItem;
    private MarginContainer Contenido;
    private KuroOptionButton KuroOptionButtonMod;
    private GridContainer GridContainerItems;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroControlWindowItem = GetNode<KuroControlWindow>("KuroControlWindowItem");
        Contenido = GetNode<MarginContainer>("KuroControlWindowItem/Contenido");
        KuroOptionButtonMod = GetNode<KuroOptionButton>("KuroControlWindowItem/Contenido/VBoxContainer/HBoxContainer/KuroOptionButtonMod");
        GridContainerItems = GetNode<GridContainer>("KuroControlWindowItem/Contenido/VBoxContainer/ScrollContainer/GridContainerItems");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}