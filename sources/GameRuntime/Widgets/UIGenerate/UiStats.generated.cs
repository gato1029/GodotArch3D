// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class UiStats : HBoxContainer
{
    public delegate void EventNotifyChangued(UiStats objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TextureIcon;
    private Label LabelValue;

    public void InitializeUI()
    {
        TextureIcon = GetNode<TextureRect>("TextureIcon");
        LabelValue = GetNode<Label>("LabelValue");
    }
}