// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTerrain : Window
{
    public delegate void EventNotifyChangued(WindowTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabBar Terreno;
    private LineEdit LineEditName;
    private ControlKuroAutoTile ControlAutoAgua;
    private ControlKuroAutoTile ControlAutoTierra1;
    private ControlKuroAutoTile ControlAutoCesped1;
    private ControlKuroAutoTile ControlAutoTierra2;
    private ControlKuroAutoTile ControlAutoCesped2;
    private ControlKuroAutoTile ControlAutoAdornoCesped;
    private ControlKuroAutoTile ControlAutoAdornosTierra;
    private ControlKuroAutoTile ControlAutoAdornoAgua;
    private ControlKuroAutoTile ControlAutoElevacionTierraNivel2;
    private TabBar Fuente_Recursos;
    private ControlListResourcesSources RecursosTierraNivel1;
    private ControlListResourcesSources RecursosCespedNivel1;
    private ControlListResourcesSources RecursosTierraNivel2;
    private ControlListResourcesSources RecursosCespedNivel2;
    private KuroTextureButton ButtonSave;
    private KuroTextureButton ButtonDelete;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        Terreno = GetNode<TabBar>("VBoxContainer/TabContainer/Terreno");
        LineEditName = GetNode<LineEdit>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer3/LineEditName");
        ControlAutoAgua = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/ControlAutoAgua");
        ControlAutoTierra1 = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/ControlAutoTierra1");
        ControlAutoCesped1 = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/ControlAutoCesped1");
        ControlAutoTierra2 = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/ControlAutoTierra2");
        ControlAutoCesped2 = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/ControlAutoCesped2");
        ControlAutoAdornoCesped = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/ControlAutoAdornoCesped");
        ControlAutoAdornosTierra = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/ControlAutoAdornosTierra");
        ControlAutoAdornoAgua = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/ControlAutoAdornoAgua");
        ControlAutoElevacionTierraNivel2 = GetNode<ControlKuroAutoTile>("VBoxContainer/TabContainer/Terreno/Panel/MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/ControlAutoElevacionTierraNivel2");
        Fuente_Recursos = GetNode<TabBar>("VBoxContainer/TabContainer/Fuente Recursos");
        RecursosTierraNivel1 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosTierraNivel1");
        RecursosCespedNivel1 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosCespedNivel1");
        RecursosTierraNivel2 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosTierraNivel2");
        RecursosCespedNivel2 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosCespedNivel2");
        ButtonSave = GetNode<KuroTextureButton>("VBoxContainer/HBoxContainer2/ButtonSave");
        ButtonDelete = GetNode<KuroTextureButton>("VBoxContainer/HBoxContainer2/ButtonDelete");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}