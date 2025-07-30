// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTerrain : Window
{
    public delegate void EventNotifyChangued(WindowTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer HSplitContainer;
    private Button ButtonSave;
    private Button ButtonCopy;
    private Sprite2D Sprite2DImage;
    private CollisionShape2D CollisionBodyCollider;
    private TabContainer VBoxContainer;
    private ScrollContainer Basico;
    private Label LabelId;
    private SpinBox SpinBoxId;
    private LineEdit LineEditName;
    private LineEdit LineEditCategory;
    private OptionButton OptionButtonType;
    private Button ButtonSearchTile;
    private Button ButtonSearchAnimate;
    private Button ButtonSearchAuto;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        HSplitContainer = GetNode<HBoxContainer>("Panel/MarginContainer/HSplitContainer");
        ButtonSave = GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/HBoxContainer/ButtonSave");
        ButtonCopy = GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/HBoxContainer/ButtonCopy");
        Sprite2DImage = GetNode<Sprite2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/Control/CenterContainer/Sprite2DImage");
        CollisionBodyCollider = GetNode<CollisionShape2D>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer3/Control/CenterContainer/Sprite2DImage/CollisionBodyCollider");
        VBoxContainer = GetNode<TabContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer");
        Basico = GetNode<ScrollContainer>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico");
        LabelId = GetNode<Label>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/GridContainer/LabelId");
        SpinBoxId = GetNode<SpinBox>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/GridContainer/SpinBoxId");
        LineEditName = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/GridContainer/LineEditName");
        LineEditCategory = GetNode<LineEdit>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/GridContainer/LineEditCategory");
        OptionButtonType = GetNode<OptionButton>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        ButtonSearchTile = GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/ButtonSearchTile");
        ButtonSearchAnimate = GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/ButtonSearchAnimate");
        ButtonSearchAuto = GetNode<Button>("Panel/MarginContainer/HSplitContainer/HBoxContainer/VBoxContainer/Basico/VBoxContainer/ButtonSearchAuto");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}