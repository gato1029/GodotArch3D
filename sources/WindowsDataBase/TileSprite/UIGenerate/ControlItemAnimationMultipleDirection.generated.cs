// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlItemAnimationMultipleDirection : HBoxContainer
{
    public delegate void EventNotifyChangued(ControlItemAnimationMultipleDirection objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private OptionButton OptionButtonType;
    private LineEdit LineEditName;
    private KuroTextureButton ButtonSelect;
    private KuroTextureButton ButtonDelete;

    public void InitializeUI()
    {
        OptionButtonType = GetNode<OptionButton>("OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("LineEditName");
        ButtonSelect = GetNode<KuroTextureButton>("ButtonSelect");
        ButtonDelete = GetNode<KuroTextureButton>("ButtonDelete");
    }
}