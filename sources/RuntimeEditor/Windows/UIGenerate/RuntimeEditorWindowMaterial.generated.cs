// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class RuntimeEditorWindowMaterial : Window
{
    public delegate void EventNotifyChangued(RuntimeEditorWindowMaterial objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ComboBox ComboBoxTipos;
    private ItemList ItemListElementos;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ComboBoxTipos = GetNode<ComboBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/ComboBoxTipos");
        ItemListElementos = GetNode<ItemList>("PanelContainer/MarginContainer/VBoxContainer/ScrollContainer/ItemListElementos");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}