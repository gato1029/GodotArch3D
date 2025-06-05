// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAccesoryAnimationBodyView : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAccesoryAnimationBodyView objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonSearch;
    private ControlAnimationState ControlAnimationState;

    public void InitializeUI()
    {
        ButtonSearch = GetNode<Button>("MarginContainer/VBoxContainer/ButtonSearch");
        ControlAnimationState = GetNode<ControlAnimationState>("MarginContainer/VBoxContainer/ControlAnimationState");
    }
}