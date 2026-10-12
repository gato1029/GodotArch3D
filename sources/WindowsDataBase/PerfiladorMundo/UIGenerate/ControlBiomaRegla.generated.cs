// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlBiomaRegla : PanelContainer
{
    public delegate void EventNotifyChangued(ControlBiomaRegla objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonBioma;
    private SpinBox SpinBoxAlturaMinima;
    private SpinBox SpinBoxAlturaMaxima;
    private SpinBox SpinBoxPeso;

    public void InitializeUI()
    {
        ButtonBioma = GetNode<Button>("MarginContainer/VBoxContainer/GridContainer/ButtonBioma");
        SpinBoxAlturaMinima = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBoxAlturaMinima");
        SpinBoxAlturaMaxima = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBoxAlturaMaxima");
        SpinBoxPeso = GetNode<SpinBox>("MarginContainer/VBoxContainer/GridContainer/SpinBoxPeso");
    }
}