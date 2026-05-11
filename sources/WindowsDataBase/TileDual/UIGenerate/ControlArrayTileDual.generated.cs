// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlArrayTileDual : PanelContainer
{
    public delegate void EventNotifyChangued(ControlArrayTileDual objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Label LabelAltura;
    private VBoxContainer VBoxContainerItems;
    private ControlTileDual ControlTileDual;
    private KuroButton ButtonAgregar;
    private KuroButton ButtonRemoverTodo;

    public void InitializeUI()
    {
        LabelAltura = GetNode<Label>("MarginContainer/VBoxContainer/LabelAltura");
        VBoxContainerItems = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ScrollContainer/VBoxContainerItems");
        ControlTileDual = GetNode<ControlTileDual>("MarginContainer/VBoxContainer/ScrollContainer/VBoxContainerItems/ControlTileDual");
        ButtonAgregar = GetNode<KuroButton>("MarginContainer/PanelContainer/ButtonAgregar");
        ButtonRemoverTodo = GetNode<KuroButton>("MarginContainer/PanelContainer2/ButtonRemoverTodo");
    }
}