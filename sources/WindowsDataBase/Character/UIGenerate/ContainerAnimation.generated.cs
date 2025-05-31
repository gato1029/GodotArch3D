// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ContainerAnimation : PanelContainer
{
    public delegate void EventNotifyChangued(ContainerAnimation objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private HBoxContainer HBoxContainer2;
    private Button ButtonBuscar;
    private Button ButtonNuevo;
    private Button ButtonSave;
    private SpinBox SpinBoxWidthPixel;
    private Label Label2;
    private SpinBox SpinBox2HeightPixel;
    private Button ButtonSplit;
    private Label Label3;
    private SpinBox SpinBoxZoom;
    private ItemList ViewItems;
    private ControlFramesArray FramesArray;
    private VBoxContainer ContainerMain;
    private PanelContainer CenterContainer;
    private Panel PanelImage;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private GridContainer GridContainerAnimacion;
    private Label Label8;
    private CheckBox CheckBoxModeSelection;
    private Label LabelFrameDuplicate;
    private CheckBox CheckBoxFrameDuplicate;
    private Label Label9;
    private SpinBox SpinBoxDuration;
    private Label Label5;
    private CheckBox CheckBoxLoop;
    private Label Label6;
    private CheckBox CheckBoxMirror;
    private Label Label7;
    private CheckBox CheckBoxMirrorV;
    private Button ButtonLinked;
    private ControlAnimationStateArray ControlAnimationItems;

    public void InitializeUI()
    {
        HBoxContainer2 = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer2");
        ButtonBuscar = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonBuscar");
        ButtonNuevo = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonNuevo");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonSave");
        SpinBoxWidthPixel = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/SpinBoxWidthPixel");
        Label2 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/Label2");
        SpinBox2HeightPixel = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/SpinBox2HeightPixel");
        ButtonSplit = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/ButtonSplit");
        Label3 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/Label3");
        SpinBoxZoom = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/SpinBoxZoom");
        ViewItems = GetNode<ItemList>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/ViewItems");
        FramesArray = GetNode<ControlFramesArray>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/FramesArray");
        ContainerMain = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain");
        CenterContainer = GetNode<PanelContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer");
        PanelImage = GetNode<Panel>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/PanelImage");
        Sprite2DView = GetNode<Sprite2D>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/PanelImage/Control/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/PanelImage/Control/CollisionShapeView");
        GridContainerAnimacion = GetNode<GridContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion");
        Label8 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/Label8");
        CheckBoxModeSelection = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/CheckBoxModeSelection");
        CheckBoxModeSelection.Pressed += CheckBoxModeSelection_PressedUI;
        CheckBoxModeSelection_PressedUI();
        LabelFrameDuplicate = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/LabelFrameDuplicate");
        CheckBoxFrameDuplicate = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/CheckBoxFrameDuplicate");
        CheckBoxFrameDuplicate.Pressed += CheckBoxFrameDuplicate_PressedUI;
        CheckBoxFrameDuplicate_PressedUI();
        Label9 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/Label9");
        SpinBoxDuration = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/SpinBoxDuration");
        Label5 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/Label5");
        CheckBoxLoop = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/CheckBoxLoop");
        CheckBoxLoop.Pressed += CheckBoxLoop_PressedUI;
        CheckBoxLoop_PressedUI();
        Label6 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/Label6");
        CheckBoxMirror = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/CheckBoxMirror");
        CheckBoxMirror.Pressed += CheckBoxMirror_PressedUI;
        CheckBoxMirror_PressedUI();
        Label7 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/Label7");
        CheckBoxMirrorV = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/GridContainerAnimacion/CheckBoxMirrorV");
        CheckBoxMirrorV.Pressed += CheckBoxMirrorV_PressedUI;
        CheckBoxMirrorV_PressedUI();
        ButtonLinked = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/VBoxContainer/ButtonLinked");
        ControlAnimationItems = GetNode<ControlAnimationStateArray>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/ControlAnimationItems");
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

    private void CheckBoxMirrorV_PressedUI()
    {
        if (CheckBoxMirrorV.ButtonPressed)
            CheckBoxMirrorV.Text = "Habilitado";
        else
            CheckBoxMirrorV.Text = "No Habilitado";
    }
}