// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlSlotTileDual : PanelContainer
{
    public delegate void EventNotifyChangued(ControlSlotTileDual objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TextureTemplate;
    private Label LabelSlot;
    private VBoxContainer VBoxContainerItems;
    private KuroButton ButtonAgregar;

    public void InitializeUI()
    {
        TextureTemplate = GetNode<TextureRect>("Panel/MarginContainer/TextureTemplate");
        LabelSlot = GetNode<Label>("VBoxContainer/LabelSlot");
        VBoxContainerItems = GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/VBoxContainerItems");
        ButtonAgregar = GetNode<KuroButton>("PanelContainer/ButtonAgregar");
    }
}