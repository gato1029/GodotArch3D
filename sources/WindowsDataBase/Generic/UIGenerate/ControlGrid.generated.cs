// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlGrid : CenterContainer
{
    public delegate void EventNotifyChangued(ControlGrid objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ColorRect ControlRect;
    private TextureRect Center;

    public void InitializeUI()
    {
        ControlRect = GetNode<ColorRect>("ControlRect");
        Center = GetNode<TextureRect>("ControlRect/Center");
    }
}