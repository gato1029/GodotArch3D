// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlDecoration : Control
{
    public delegate void EventNotifyChangued(ControlDecoration objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TextureRectImage;
    private SpinBox SpinBoxProbabilidad;

    public void InitializeUI()
    {
        TextureRectImage = GetNode<TextureRect>("VBoxContainer/TextureRectImage");
        SpinBoxProbabilidad = GetNode<SpinBox>("VBoxContainer/SpinBoxProbabilidad");
    }
}