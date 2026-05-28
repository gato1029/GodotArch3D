// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowDataEditor : Window
{
    public delegate void EventNotifyChangued(WindowDataEditor objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroControlWindow KuroControlWindow;
    private MarginContainer Contenido;
    private KuroOptionButton KuroOptionButtonMod;
    private KuroSearchItems KuroItemsData;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroControlWindow = GetNode<KuroControlWindow>("KuroControlWindow");
        Contenido = GetNode<MarginContainer>("KuroControlWindow/Contenido");
        KuroOptionButtonMod = GetNode<KuroOptionButton>("KuroControlWindow/Contenido/VBoxContainer/KuroOptionButtonMod");
        KuroItemsData = GetNode<KuroSearchItems>("KuroControlWindow/Contenido/VBoxContainer/KuroItemsData");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}