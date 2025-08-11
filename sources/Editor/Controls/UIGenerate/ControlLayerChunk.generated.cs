// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlLayerChunk : PanelContainer
{
    public delegate void EventNotifyChangued(ControlLayerChunk objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer VBoxContainer;
    private Label LabelName;
    private CheckBox CheckBoxEnable;
    private VBoxContainer HBoxContainer2;
    private Label LabelRendering;
    private Label LabelReal;

    public void InitializeUI()
    {
        VBoxContainer = GetNode<HBoxContainer>("VBoxContainer");
        LabelName = GetNode<Label>("VBoxContainer/HBoxContainer/LabelName");
        CheckBoxEnable = GetNode<CheckBox>("VBoxContainer/HBoxContainer/CheckBoxEnable");
        CheckBoxEnable.Pressed += CheckBoxEnable_PressedUI;
        CheckBoxEnable_PressedUI();
        HBoxContainer2 = GetNode<VBoxContainer>("VBoxContainer/HBoxContainer2");
        LabelRendering = GetNode<Label>("VBoxContainer/HBoxContainer2/LabelRendering");
        LabelReal = GetNode<Label>("VBoxContainer/HBoxContainer2/LabelReal");
    }

    private void CheckBoxEnable_PressedUI()
    {
        if (CheckBoxEnable.ButtonPressed)
            CheckBoxEnable.Text = "Habilitado";
        else
            CheckBoxEnable.Text = "No Habilitado";
    }
}