// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class EditorWindow : PanelContainer
{
    public delegate void EventNotifyChangued(EditorWindow objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonSaveAll;
    private SpinBox SpinBoxGridX;
    private SpinBox SpinBoxGridY;
    private TabContainer TabContainerItems;
    private TabBar Terreno;
    private TabBar Recursos;
    private TabBar Unidades;

    public void InitializeUI()
    {
        ButtonSaveAll = GetNode<Button>("VBoxContainer/ButtonSaveAll");
        SpinBoxGridX = GetNode<SpinBox>("VBoxContainer/GridContainer/SpinBoxGridX");
        SpinBoxGridY = GetNode<SpinBox>("VBoxContainer/GridContainer/SpinBoxGridY");
        TabContainerItems = GetNode<TabContainer>("VBoxContainer/TabContainerItems");
        Terreno = GetNode<TabBar>("VBoxContainer/TabContainerItems/Terreno");
        Recursos = GetNode<TabBar>("VBoxContainer/TabContainerItems/Recursos");
        Unidades = GetNode<TabBar>("VBoxContainer/TabContainerItems/Unidades");
    }
}