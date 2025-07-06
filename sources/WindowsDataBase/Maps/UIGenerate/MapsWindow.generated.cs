// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MapsWindow : Window
{
    public delegate void EventNotifyChangued(MapsWindow objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private Label Label2;
    private TextEdit TextEditDescription;
    private Label Label3;
    private SpinBox SpinBoxWidth;
    private SpinBox SpinBox2Height;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("MarginContainer/Panel/VBoxContainer/GridContainer/LineEditName");
        Label2 = GetNode<Label>("MarginContainer/Panel/VBoxContainer/GridContainer/Label2");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/Panel/VBoxContainer/GridContainer/TextEditDescription");
        Label3 = GetNode<Label>("MarginContainer/Panel/VBoxContainer/GridContainer/Label3");
        SpinBoxWidth = GetNode<SpinBox>("MarginContainer/Panel/VBoxContainer/GridContainer/VBoxContainer/SpinBoxWidth");
        SpinBox2Height = GetNode<SpinBox>("MarginContainer/Panel/VBoxContainer/GridContainer/VBoxContainer/SpinBox2Height");
        ButtonSave = GetNode<Button>("MarginContainer/Panel/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}