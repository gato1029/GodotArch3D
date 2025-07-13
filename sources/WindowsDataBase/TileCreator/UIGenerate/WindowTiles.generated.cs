// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTiles : Window
{
    public delegate void EventNotifyChangued(WindowTiles objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonSearchMaterial;
    private TabContainer TabContainerOptions;
    private TabBar PorSegmentos;
    private SpinBox SpinBoxWidthPixel;
    private SpinBox SpinBox2HeightPixel;
    private Button ButtonSplit;
    private SpinBox SpinBoxZoom;
    private CheckBox CheckBoxMultiSelection;
    private CheckBox CheckBoxGenerateAll;
    private ItemList ViewItems;
    private TabBar Customizado;
    private ControlSeleccionTexture ControlTextureLocal;
    private SpinBox SpinBoxZoomGrid;
    private ScrollContainer ScrollContainerGrid;
    private Control PanelBase;
    private ControlGrid ControlGrid;
    private Control ControlSprite;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private GridContainer GridContainerAnimacion;
    private LineEdit LineEditName;
    private SpinBox SpinBoxOffsetX;
    private SpinBox SpinBoxOffsetY;
    private ColorPickerButton ColorButtonBase;
    private SpinBox SpinBoxScale;
    private CheckBox CheckBoxMirror;
    private CheckBox CheckBoxMirrorV;
    private CheckBox CheckBoxHasCollider;
    private ColliderScene ColliderContainer;
    private Button ButtonSave;
    private Button ButtonDelete;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ButtonSearchMaterial = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonSearchMaterial");
        TabContainerOptions = GetNode<TabContainer>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions");
        PorSegmentos = GetNode<TabBar>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos");
        SpinBoxWidthPixel = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/SpinBoxWidthPixel");
        SpinBox2HeightPixel = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/SpinBox2HeightPixel");
        ButtonSplit = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/ButtonSplit");
        SpinBoxZoom = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/SpinBoxZoom");
        CheckBoxMultiSelection = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer2/CheckBoxMultiSelection");
        CheckBoxMultiSelection.Pressed += CheckBoxMultiSelection_PressedUI;
        CheckBoxMultiSelection_PressedUI();
        CheckBoxGenerateAll = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer2/CheckBoxGenerateAll");
        CheckBoxGenerateAll.Pressed += CheckBoxGenerateAll_PressedUI;
        CheckBoxGenerateAll_PressedUI();
        ViewItems = GetNode<ItemList>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/ViewItems");
        Customizado = GetNode<TabBar>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/Customizado");
        ControlTextureLocal = GetNode<ControlSeleccionTexture>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/Customizado/ControlTextureLocal");
        SpinBoxZoomGrid = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer/SpinBoxZoomGrid");
        ScrollContainerGrid = GetNode<ScrollContainer>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid");
        PanelBase = GetNode<Control>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase");
        ControlGrid = GetNode<ControlGrid>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid");
        ControlSprite = GetNode<Control>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSprite");
        Sprite2DView = GetNode<Sprite2D>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSprite/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSprite/CollisionShapeView");
        GridContainerAnimacion = GetNode<GridContainer>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion");
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/LineEditName");
        SpinBoxOffsetX = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/SpinBoxOffsetX");
        SpinBoxOffsetY = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/SpinBoxOffsetY");
        ColorButtonBase = GetNode<ColorPickerButton>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/ColorButtonBase");
        SpinBoxScale = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/SpinBoxScale");
        CheckBoxMirror = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/CheckBoxMirror");
        CheckBoxMirror.Pressed += CheckBoxMirror_PressedUI;
        CheckBoxMirror_PressedUI();
        CheckBoxMirrorV = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/CheckBoxMirrorV");
        CheckBoxMirrorV.Pressed += CheckBoxMirrorV_PressedUI;
        CheckBoxMirrorV_PressedUI();
        CheckBoxHasCollider = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion/CheckBoxHasCollider");
        CheckBoxHasCollider.Pressed += CheckBoxHasCollider_PressedUI;
        CheckBoxHasCollider_PressedUI();
        ColliderContainer = GetNode<ColliderScene>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ColliderContainer");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/ButtonSave");
        ButtonDelete = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/ButtonDelete");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }


    private void CheckBoxMultiSelection_PressedUI()
    {
        if (CheckBoxMultiSelection.ButtonPressed)
            CheckBoxMultiSelection.Text = "Habilitado";
        else
            CheckBoxMultiSelection.Text = "No Habilitado";
    }

    private void CheckBoxGenerateAll_PressedUI()
    {
        if (CheckBoxGenerateAll.ButtonPressed)
            CheckBoxGenerateAll.Text = "Generar Todo";
        else
            CheckBoxGenerateAll.Text = "No Generar Todo";
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