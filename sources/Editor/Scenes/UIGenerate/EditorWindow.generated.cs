// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class EditorWindow : PanelContainer
{
    public delegate void EventNotifyChangued(EditorWindow objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabContainer TabContainerItems;
    private TabBar Terreno;
    private ControlEditorTerrain ControlEditorTerrain;
    private TabBar Recursos;
    private TabBar Unidades;

    public void InitializeUI()
    {
        TabContainerItems = GetNode<TabContainer>("TabContainerItems");
        Terreno = GetNode<TabBar>("TabContainerItems/Terreno");
        ControlEditorTerrain = GetNode<ControlEditorTerrain>("TabContainerItems/Terreno/ControlEditorTerrain");
        Recursos = GetNode<TabBar>("TabContainerItems/Recursos");
        Unidades = GetNode<TabBar>("TabContainerItems/Unidades");
    }
}