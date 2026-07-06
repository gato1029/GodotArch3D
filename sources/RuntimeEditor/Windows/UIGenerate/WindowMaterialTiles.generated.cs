// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowMaterialTiles : Window
{
    public delegate void EventNotifyChangued(WindowMaterialTiles objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroControlWindow KuroControlWindow;
    private MarginContainer Contenido;
    private KuroOptionButton KuroOptionButtonMod;
    private KuroTextureButton ButtonAdd;
    private KuroOptionButton KuroOptionButtonMaterial;
    private GridContainer GridContainerItems;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroControlWindow = GetNode<KuroControlWindow>("KuroControlWindow");
        Contenido = GetNode<MarginContainer>("KuroControlWindow/Contenido");
        KuroOptionButtonMod = GetNode<KuroOptionButton>("KuroControlWindow/Contenido/VBoxContainer/HBoxContainer/KuroOptionButtonMod");
        ButtonAdd = GetNode<KuroTextureButton>("KuroControlWindow/Contenido/VBoxContainer/HBoxContainer/ButtonAdd");
        KuroOptionButtonMaterial = GetNode<KuroOptionButton>("KuroControlWindow/Contenido/VBoxContainer/KuroOptionButtonMaterial");
        GridContainerItems = GetNode<GridContainer>("KuroControlWindow/Contenido/VBoxContainer/ScrollContainer/GridContainerItems");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}