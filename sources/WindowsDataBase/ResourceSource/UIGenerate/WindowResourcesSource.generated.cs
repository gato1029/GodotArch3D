// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowResourcesSource : Window
{
    public delegate void EventNotifyChangued(WindowResourcesSource objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private OptionButton OptionButtonType;
    private LineEdit LineEditName;
    private ControlListTileSprite ControlTileSprite;
    private TextEdit TextEditDescription;
    private SpinBox SpinBoxAmount;
    private SpinBox SpinBoxHealthPoints;
    private CheckBox CheckBoxIsExploitable;
    private KuroTextureButton ButtonSave;
    private KuroTextureButton ButtonDelete;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        OptionButtonType = GetNode<OptionButton>("MarginContainer/HBoxContainer/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("MarginContainer/HBoxContainer/VBoxContainer/GridContainer/LineEditName");
        ControlTileSprite = GetNode<ControlListTileSprite>("MarginContainer/HBoxContainer/VBoxContainer/GridContainer/ControlTileSprite");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/HBoxContainer/VBoxContainer/GridContainer/TextEditDescription");
        SpinBoxAmount = GetNode<SpinBox>("MarginContainer/HBoxContainer/VBoxContainer/GridContainer/SpinBoxAmount");
        SpinBoxHealthPoints = GetNode<SpinBox>("MarginContainer/HBoxContainer/VBoxContainer/GridContainer/SpinBoxHealthPoints");
        CheckBoxIsExploitable = GetNode<CheckBox>("MarginContainer/HBoxContainer/VBoxContainer/GridContainer/CheckBoxIsExploitable");
        ButtonSave = GetNode<KuroTextureButton>("MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/ButtonSave");
        ButtonDelete = GetNode<KuroTextureButton>("MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/ButtonDelete");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}