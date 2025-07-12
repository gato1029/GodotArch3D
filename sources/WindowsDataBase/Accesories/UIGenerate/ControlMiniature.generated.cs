// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlMiniature : PanelContainer
{
    public delegate void EventNotifyChangued(ControlMiniature objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TextureRectImage;
    private Button ButtonSelect;

    public void InitializeUI()
    {
        TextureRectImage = GetNode<TextureRect>("MarginContainer/VBoxContainer/TextureRectImage");
        ButtonSelect = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonSelect");
    }
}