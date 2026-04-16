// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlAnimationMultipleDirection : PanelContainer
{
    public delegate void EventNotifyChangued(ControlAnimationMultipleDirection objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private OptionButton OptionButtonType;
    private LineEdit LineEditName;
    private KuroTextureButton ButtonAdd;
    private VBoxContainer VBoxContainerItems;
    private ControlAnimatedDirection ControlAnimatedDirectionItem;

    public void InitializeUI()
    {
        OptionButtonType = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer2/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/HBoxContainer/LineEditName");
        ButtonAdd = GetNode<KuroTextureButton>("MarginContainer/VBoxContainer/HBoxContainer/ButtonAdd");
        VBoxContainerItems = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ScrollContainer/VBoxContainerItems");
        ControlAnimatedDirectionItem = GetNode<ControlAnimatedDirection>("MarginContainer/VBoxContainer/ControlAnimatedDirectionItem");
    }
}