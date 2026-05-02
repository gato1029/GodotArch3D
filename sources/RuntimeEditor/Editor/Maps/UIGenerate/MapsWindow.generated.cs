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
    private KuroButton ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditName = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/LineEditName");
        OptionButtonType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        TextEditDescription = GetNode<TextEdit>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/TextEditDescription");
        SpinBoxWidth = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/VBoxContainer/SpinBoxWidth");
        SpinBox2Height = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/GridContainer/VBoxContainer/SpinBox2Height");
        ButtonSave = GetNode<KuroButton>("PanelContainer/MarginContainer/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}