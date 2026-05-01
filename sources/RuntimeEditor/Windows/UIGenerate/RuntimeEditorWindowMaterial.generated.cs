// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeEditorWindowMaterial : Window
{
    public delegate void EventNotifyChangued(RuntimeEditorWindowMaterial objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroOptionButton ListaTipos;
    private KuroItemList ItemsList;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ListaTipos = GetNode<KuroOptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/ListaTipos");
        ItemsList = GetNode<KuroItemList>("PanelContainer/MarginContainer/VBoxContainer/ScrollContainer/ItemsList");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}