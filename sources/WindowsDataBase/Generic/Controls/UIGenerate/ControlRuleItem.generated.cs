// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlRuleItem : PanelContainer
{
    public delegate void EventNotifyChangued(ControlRuleItem objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Label LabelRule;
    private Button ButtonCentral;
    private Button ButtonDelete;

    public void InitializeUI()
    {
        LabelRule = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/LabelRule");
        ButtonCentral = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/ButtonCentral");
        ButtonDelete = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/ButtonDelete");
    }
}