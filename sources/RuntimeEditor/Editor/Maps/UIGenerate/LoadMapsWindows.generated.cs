// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class LoadMapsWindows : Window
{
    public delegate void EventNotifyChangued(LoadMapsWindows objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroControlWindow KuroControlWindowItem;
    private MarginContainer Contenido;
    private KuroItemList KuroItemsListMaps;
    private KuroButton KuroButtonCargar;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroControlWindowItem = GetNode<KuroControlWindow>("KuroControlWindowItem");
        Contenido = GetNode<MarginContainer>("KuroControlWindowItem/Contenido");
        KuroItemsListMaps = GetNode<KuroItemList>("KuroControlWindowItem/Contenido/VBoxContainer/KuroItemsListMaps");
        KuroButtonCargar = GetNode<KuroButton>("KuroControlWindowItem/Contenido/VBoxContainer/KuroButtonCargar");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}