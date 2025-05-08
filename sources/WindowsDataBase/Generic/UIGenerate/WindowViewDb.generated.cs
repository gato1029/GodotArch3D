// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowViewDb : Window
{
    private Button ButtonFindMaterial;
    private Button ButtonFindCustom;
    private ItemList Items;

    public  void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;

        ButtonFindMaterial = GetNode<Button>("Panel/MarginContainer/VBoxContainer/ButtonFindMaterial");
        ButtonFindCustom = GetNode<Button>("Panel/MarginContainer/VBoxContainer/ButtonFindCustom");
        Items = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/Items");
    }

    private void CloseRequestedWindow()
    {

        QueueFree(); //  cerrar, ventana

    }

}