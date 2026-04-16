// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTerrain : Window
{
    public delegate void EventNotifyChangued(WindowTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabBar Niveles;
    private LineEdit LineEditName;
    private GridContainer HBoxContainer4;
    private SpinBox SpinBoxTempMin;
    private SpinBox SpinBoxTempMax;
    private SpinBox SpinBoxHumetyMin;
    private SpinBox SpinBoxHumetyMax;
    private CheckBox CheckBoxTransicion;
    private SpinBox SpinBoxAltura;
    private CheckBox CheckBoxisWater;
    private SpinBox SpinBoxBorde;
    private KuroTextureButton ButtonAdd;
    private VBoxContainer VBoxContainerTerrain;
    private TabBar Fuente_Recursos;
    private ControlListResourcesSources RecursosNivel0;
    private ControlListResourcesSources RecursosNivel1;
    private ControlListResourcesSources RecursosNivel2;
    private ControlListResourcesSources RecursosNivel3;
    private KuroTextureButton ButtonSave;
    private KuroTextureButton ButtonDelete;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        Niveles = GetNode<TabBar>("VBoxContainer/TabContainer/Niveles");
        LineEditName = GetNode<LineEdit>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer3/LineEditName");
        HBoxContainer4 = GetNode<GridContainer>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4");
        SpinBoxTempMin = GetNode<SpinBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/SpinBoxTempMin");
        SpinBoxTempMax = GetNode<SpinBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/SpinBoxTempMax");
        SpinBoxHumetyMin = GetNode<SpinBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/SpinBoxHumetyMin");
        SpinBoxHumetyMax = GetNode<SpinBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/SpinBoxHumetyMax");
        CheckBoxTransicion = GetNode<CheckBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/CheckBoxTransicion");
        SpinBoxAltura = GetNode<SpinBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/SpinBoxAltura");
        CheckBoxisWater = GetNode<CheckBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/CheckBoxisWater");
        SpinBoxBorde = GetNode<SpinBox>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/HBoxContainer4/SpinBoxBorde");
        ButtonAdd = GetNode<KuroTextureButton>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/ButtonAdd");
        VBoxContainerTerrain = GetNode<VBoxContainer>("VBoxContainer/TabContainer/Niveles/ScrollContainer/VBoxContainer/VBoxContainerTerrain");
        Fuente_Recursos = GetNode<TabBar>("VBoxContainer/TabContainer/Fuente Recursos");
        RecursosNivel0 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosNivel0");
        RecursosNivel1 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosNivel1");
        RecursosNivel2 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosNivel2");
        RecursosNivel3 = GetNode<ControlListResourcesSources>("VBoxContainer/TabContainer/Fuente Recursos/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/RecursosNivel3");
        ButtonSave = GetNode<KuroTextureButton>("VBoxContainer/HBoxContainer2/ButtonSave");
        ButtonDelete = GetNode<KuroTextureButton>("VBoxContainer/HBoxContainer2/ButtonDelete");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}