// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlBiomaRegla : PanelContainer
{
    public delegate void EventNotifyChangued(ControlBiomaRegla objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroButton ButtonEliminar;
    private Button ButtonBioma;
    private SpinBox SpinBoxAlturaMinima;
    private SpinBox SpinBoxAlturaMaxima;
    private SpinBox SpinBoxPeso;
    private CheckBox CheckBoxBiomaBase;

    public void InitializeUI()
    {
        ButtonEliminar = GetNode<KuroButton>("ButtonEliminar");
        ButtonBioma = GetNode<Button>("MarginContainer/VBoxContainer/GridContainer/ButtonBioma");
        SpinBoxAlturaMinima = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBoxAlturaMinima");
        SpinBoxAlturaMaxima = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBoxAlturaMaxima");
        SpinBoxPeso = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBoxPeso");
        CheckBoxBiomaBase = GetNode<CheckBox>("MarginContainer/VBoxContainer/GridContainer/CheckBoxBiomaBase");
    }
}