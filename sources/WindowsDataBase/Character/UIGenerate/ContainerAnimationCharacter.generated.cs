// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ContainerAnimationCharacter : PanelContainer
{
    public delegate void EventNotifyChangued(ContainerAnimationCharacter objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer HBoxContainer2;
    private Button ButtonBuscar;
    private Button ButtonNuevo;
    private ItemList ViewItems;
    private VBoxContainer ContainerMain;
    private PanelContainer CenterContainer;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private Label Label8;
    private CheckBox CheckBoxModeSelection;
    private Label LabelFrameDuplicate;
    private CheckBox CheckBoxFrameDuplicate;
    private Label Label3;
    private TextEdit TextEditFrames;
    private Label Label4;
    private Button ButtonForcedFrames;
    private Label Label9;
    private SpinBox SpinBoxDuration;
    private Label Label5;
    private CheckBox CheckBoxLoop;
    private Label Label6;
    private CheckBox CheckBoxMirror;
    private Label Label7;
    private CheckBox CheckBoxHasCollision;

    public void InitializeUI()
    {
        HBoxContainer2 = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer2");
        ButtonBuscar = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonBuscar");
        ButtonNuevo = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonNuevo");
        ViewItems = GetNode<ItemList>("MarginContainer/VBoxContainer/HBoxContainer/ViewItems");
        ContainerMain = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain");
        CenterContainer = GetNode<PanelContainer>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer");
        Sprite2DView = GetNode<Sprite2D>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/Panel/Control/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/Panel/Control/CollisionShapeView");
        Label8 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/Label8");
        CheckBoxModeSelection = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/CheckBoxModeSelection");
        CheckBoxModeSelection.Pressed += CheckBoxModeSelection_PressedUI;
        CheckBoxModeSelection_PressedUI();
        LabelFrameDuplicate = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/LabelFrameDuplicate");
        CheckBoxFrameDuplicate = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/CheckBoxFrameDuplicate");
        CheckBoxFrameDuplicate.Pressed += CheckBoxFrameDuplicate_PressedUI;
        CheckBoxFrameDuplicate_PressedUI();
        Label3 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/Label3");
        TextEditFrames = GetNode<TextEdit>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/TextEditFrames");
        Label4 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/Label4");
        ButtonForcedFrames = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/ButtonForcedFrames");
        Label9 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/Label9");
        SpinBoxDuration = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/SpinBoxDuration");
        Label5 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/Label5");
        CheckBoxLoop = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/CheckBoxLoop");
        CheckBoxLoop.Pressed += CheckBoxLoop_PressedUI;
        CheckBoxLoop_PressedUI();
        Label6 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/Label6");
        CheckBoxMirror = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/CheckBoxMirror");
        CheckBoxMirror.Pressed += CheckBoxMirror_PressedUI;
        CheckBoxMirror_PressedUI();
        Label7 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/Label7");
        CheckBoxHasCollision = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerMain/HBoxContainer/VBoxContainer/GridContainer/CheckBoxHasCollision");
        CheckBoxHasCollision.Pressed += CheckBoxHasCollision_PressedUI;
        CheckBoxHasCollision_PressedUI();
    }

    private void CheckBoxModeSelection_PressedUI()
    {
        if (CheckBoxModeSelection.ButtonPressed)
            CheckBoxModeSelection.Text = "Ordenada";
        else
            CheckBoxModeSelection.Text = "No Ordenada";
    }

    private void CheckBoxFrameDuplicate_PressedUI()
    {
        if (CheckBoxFrameDuplicate.ButtonPressed)
            CheckBoxFrameDuplicate.Text = "Habilitado";
        else
            CheckBoxFrameDuplicate.Text = "No Habilitado";
    }

    private void CheckBoxLoop_PressedUI()
    {
        if (CheckBoxLoop.ButtonPressed)
            CheckBoxLoop.Text = "Bucle";
        else
            CheckBoxLoop.Text = "No Bucle";
    }

    private void CheckBoxMirror_PressedUI()
    {
        if (CheckBoxMirror.ButtonPressed)
            CheckBoxMirror.Text = "Habilitado";
        else
            CheckBoxMirror.Text = "No Habilitado";
    }

    private void CheckBoxHasCollision_PressedUI()
    {
        if (CheckBoxHasCollision.ButtonPressed)
            CheckBoxHasCollision.Text = "Collision";
        else
            CheckBoxHasCollision.Text = "No Collision";
    }
}