// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class EditorWindow : Window
{
    public delegate void EventNotifyChangued(EditorWindow objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabContainer TabContainerItems;
    private TabBar Terreno;
    private TabBar Recursos;
    private TabBar Unidades;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TabContainerItems = GetNode<TabContainer>("TabContainerItems");
        Terreno = GetNode<TabBar>("TabContainerItems/Terreno");
        Recursos = GetNode<TabBar>("TabContainerItems/Recursos");
        Unidades = GetNode<TabBar>("TabContainerItems/Unidades");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}