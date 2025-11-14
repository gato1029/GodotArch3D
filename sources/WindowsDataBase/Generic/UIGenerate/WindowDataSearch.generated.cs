// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowDataSearch : Window
{
    public delegate void EventNotifyChangued(WindowDataSearch objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    public HBoxContainer HBoxContainerAgrupador;
    public ControlGrouping ControlGroupingSearch;
    public LineEdit LineEditSearch;
    public Button ButtonNew;
    public ItemList ItemListView;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        HBoxContainerAgrupador = GetNode<HBoxContainer>("Panel/MarginContainer/VBoxContainer/GridContainer/HBoxContainerAgrupador");
        ControlGroupingSearch = GetNode<ControlGrouping>("Panel/MarginContainer/VBoxContainer/GridContainer/HBoxContainerAgrupador/ControlGroupingSearch");
        LineEditSearch = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/GridContainer/LineEditSearch");
        ButtonNew = GetNode<Button>("Panel/MarginContainer/VBoxContainer/ButtonNew");
        ItemListView = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/ScrollContainer/ItemListView");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}