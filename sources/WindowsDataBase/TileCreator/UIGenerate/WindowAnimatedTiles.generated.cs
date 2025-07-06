// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowAnimatedTiles : Window
{
    public delegate void EventNotifyChangued(WindowAnimatedTiles objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonBuscar;
    private Button ButtonNuevo;
    private SpinBox SpinBoxWidthPixel;
    private SpinBox SpinBox2HeightPixel;
    private Button ButtonSplit;
    private SpinBox SpinBoxZoom;
    private ItemList ViewItems;
    private ControlFramesArray FramesArray;
    private VBoxContainer ContainerMain;
    private SpinBox SpinBoxZoomGrid;
    private ScrollContainer ScrollContainerGrid;
    private Control PanelBase;
    private ControlGrid ControlGrid;
    private Control ControlSprite;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private GridContainer GridContainerAnimacion;
    private Label LabelSeleccion;
    private CheckBox CheckBoxModeSelection;
    private Label LabelFrameDuplicate;
    private CheckBox CheckBoxFrameDuplicate;
    private SpinBox SpinBoxDuration;
    private CheckBox CheckBoxLoop;
    private CheckBox CheckBoxMirror;
    private CheckBox CheckBoxMirrorV;
    private CheckBox CheckBoxHasCollider;
    private SpinBox SpinBoxOffsetX;
    private SpinBox SpinBoxOffsetY;
    private ColorPickerButton ColorButtonBase;
    private SpinBox SpinBoxScale;
    private ColliderScene ColliderContainer;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ButtonBuscar = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer2/ButtonBuscar");
        ButtonNuevo = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer2/ButtonNuevo");
        SpinBoxWidthPixel = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/SpinBoxWidthPixel");
        SpinBox2HeightPixel = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/SpinBox2HeightPixel");
        ButtonSplit = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/ButtonSplit");
        SpinBoxZoom = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/HBoxContainer/SpinBoxZoom");
        ViewItems = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/ViewItems");
        FramesArray = GetNode<ControlFramesArray>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/VBoxContainer/FramesArray");
        ContainerMain = GetNode<VBoxContainer>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain");
        SpinBoxZoomGrid = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/VBoxContainer/HBoxContainer/SpinBoxZoomGrid");
        ScrollContainerGrid = GetNode<ScrollContainer>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/VBoxContainer/ScrollContainerGrid");
        PanelBase = GetNode<Control>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/VBoxContainer/ScrollContainerGrid/PanelBase");
        ControlGrid = GetNode<ControlGrid>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid");
        ControlSprite = GetNode<Control>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSprite");
        Sprite2DView = GetNode<Sprite2D>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSprite/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSprite/CollisionShapeView");
        GridContainerAnimacion = GetNode<GridContainer>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion");
        LabelSeleccion = GetNode<Label>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/LabelSeleccion");
        CheckBoxModeSelection = GetNode<CheckBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/CheckBoxModeSelection");
        CheckBoxModeSelection.Pressed += CheckBoxModeSelection_PressedUI;
        CheckBoxModeSelection_PressedUI();
        LabelFrameDuplicate = GetNode<Label>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/LabelFrameDuplicate");
        CheckBoxFrameDuplicate = GetNode<CheckBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/CheckBoxFrameDuplicate");
        CheckBoxFrameDuplicate.Pressed += CheckBoxFrameDuplicate_PressedUI;
        CheckBoxFrameDuplicate_PressedUI();
        SpinBoxDuration = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/SpinBoxDuration");
        CheckBoxLoop = GetNode<CheckBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/CheckBoxLoop");
        CheckBoxLoop.Pressed += CheckBoxLoop_PressedUI;
        CheckBoxLoop_PressedUI();
        CheckBoxMirror = GetNode<CheckBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/CheckBoxMirror");
        CheckBoxMirror.Pressed += CheckBoxMirror_PressedUI;
        CheckBoxMirror_PressedUI();
        CheckBoxMirrorV = GetNode<CheckBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/CheckBoxMirrorV");
        CheckBoxMirrorV.Pressed += CheckBoxMirrorV_PressedUI;
        CheckBoxMirrorV_PressedUI();
        CheckBoxHasCollider = GetNode<CheckBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/GridContainerAnimacion/CheckBoxHasCollider");
        CheckBoxHasCollider.Pressed += CheckBoxHasCollider_PressedUI;
        CheckBoxHasCollider_PressedUI();
        SpinBoxOffsetX = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GridContainer/SpinBoxOffsetX");
        SpinBoxOffsetY = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GridContainer/SpinBoxOffsetY");
        ColorButtonBase = GetNode<ColorPickerButton>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GridContainer/ColorButtonBase");
        SpinBoxScale = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/HBoxContainer/VBoxContainer/GridContainer/SpinBoxScale");
        ColliderContainer = GetNode<ColliderScene>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/ColliderContainer");
        ButtonSave = GetNode<Button>("Panel/MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/PanelContainer/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
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

    private void CheckBoxHasCollider_PressedUI()
    {
        if (CheckBoxHasCollider.ButtonPressed)
            CheckBoxHasCollider.Text = "Habilitado";
        else
            CheckBoxHasCollider.Text = "No Habilitado";
    }
}