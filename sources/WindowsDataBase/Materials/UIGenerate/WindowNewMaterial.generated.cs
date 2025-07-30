// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowNewMaterial : Window
{
    public delegate void EventNotifyChangued(WindowNewMaterial objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ItemList ItemListTiles;
    private LineEdit LineEditId;
    private LineEdit LineEditPath;
    private Button ButtonSearchFile;
    private LineEdit LineEditName;
    private LineEdit LineEditCategory;
    private OptionButton OptionButtonType;
    private SpinBox SpinBoxX;
    private SpinBox SpinBoxY;
    private Button ButtonDividir;
    private Button ButtonSave;
    private Button ButtonDelete;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ItemListTiles = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/ItemListTiles");
        LineEditId = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEditId");
        LineEditPath = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer/LineEditPath");
        ButtonSearchFile = GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer/ButtonSearchFile");
        LineEditName = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEditName");
        LineEditCategory = GetNode<LineEdit>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/LineEditCategory");
        OptionButtonType = GetNode<OptionButton>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        SpinBoxX = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer5/SpinBoxX");
        SpinBoxY = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer5/SpinBoxY");
        ButtonDividir = GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/GridContainer/HBoxContainer5/ButtonDividir");
        ButtonSave = GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/HBoxContainer/ButtonSave");
        ButtonDelete = GetNode<Button>("Panel/MarginContainer/VBoxContainer/VBoxContainer/HBoxContainer/ButtonDelete");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}