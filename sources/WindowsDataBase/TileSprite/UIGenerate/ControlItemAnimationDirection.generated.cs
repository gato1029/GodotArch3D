// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlItemAnimationDirection : PanelContainer
{
    public delegate void EventNotifyChangued(ControlItemAnimationDirection objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private ControlListTextures ControlListTexturesAnimated;
    private KuroTextureButton ButtonVisualizar;

    public void InitializeUI()
    {
        LineEditName = GetNode<LineEdit>("HBoxContainer/LineEditName");
        ControlListTexturesAnimated = GetNode<ControlListTextures>("HBoxContainer/ControlListTexturesAnimated");
        ButtonVisualizar = GetNode<KuroTextureButton>("HBoxContainer/ButtonVisualizar");
    }
}