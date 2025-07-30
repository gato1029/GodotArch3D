// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class EditorPanel : VBoxContainer
{
    public delegate void EventNotifyChangued(EditorPanel objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonHidde;
    private AnimatedPanelContainer AnimatedPanel;

    public void InitializeUI()
    {
        ButtonHidde = GetNode<Button>("HBoxContainer/VBoxContainer/ButtonHidde");
        AnimatedPanel = GetNode<AnimatedPanelContainer>("HBoxContainer/AnimatedPanel");
    }
}