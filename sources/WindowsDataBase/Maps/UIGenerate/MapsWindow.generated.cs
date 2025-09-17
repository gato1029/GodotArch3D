// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class MapsWindow : Window
{
    public delegate void EventNotifyChangued(MapsWindow objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditName;
    private OptionButton OptionButtonType;
    private TextEdit TextEditDescription;
    private SpinBox SpinBoxWidth;
    private SpinBox SpinBox2Height;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("MarginContainer/Panel/VBoxContainer/GridContainer/LineEditName");
        OptionButtonType = GetNode<OptionButton>("MarginContainer/Panel/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        TextEditDescription = GetNode<TextEdit>("MarginContainer/Panel/VBoxContainer/GridContainer/TextEditDescription");
        SpinBoxWidth = GetNode<SpinBox>("MarginContainer/Panel/VBoxContainer/GridContainer/VBoxContainer/SpinBoxWidth");
        SpinBox2Height = GetNode<SpinBox>("MarginContainer/Panel/VBoxContainer/GridContainer/VBoxContainer/SpinBox2Height");
        ButtonSave = GetNode<Button>("MarginContainer/Panel/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}