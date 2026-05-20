// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowDataGenericMod : Window
{
    public delegate void EventNotifyChangued(WindowDataGenericMod objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroControlWindow KuroControlWindow;
    private MarginContainer Contenido;
    private KuroSearchItems KuroItemsData;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        KuroControlWindow = GetNode<KuroControlWindow>("KuroControlWindow");
        Contenido = GetNode<MarginContainer>("KuroControlWindow/Contenido");
        KuroItemsData = GetNode<KuroSearchItems>("KuroControlWindow/Contenido/KuroItemsData");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}