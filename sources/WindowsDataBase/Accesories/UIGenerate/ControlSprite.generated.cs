// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlSprite : ScrollContainer
{
    public delegate void EventNotifyChangued(ControlSprite objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonSearchMaterial;
    private TabContainer TabContainerOptions;
    private TabBar PorSegmentos;
    private SpinBox SpinBoxWidthPixel;
    private SpinBox SpinBox2HeightPixel;
    private Button ButtonSplit;
    private SpinBox SpinBoxZoom;
    private ItemList ViewItems;
    private TabBar Customizado;
    private ControlSeleccionTexture ControlTextureLocal;
    private SpinBox SpinBoxZoomGrid;
    private ScrollContainer ScrollContainerGrid;
    private Control PanelBase;
    private ControlGrid ControlGrid;
    private Control ControlSpriteInternal;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private GridContainer GridContainerAnimacion;
    private SpinBox SpinBoxOffsetX;
    private SpinBox SpinBoxOffsetY;
    private ColorPickerButton ColorButtonBase;
    private SpinBox SpinBoxScale;
    private CheckBox CheckBoxMirror;
    private CheckBox CheckBoxMirrorV;
    private CheckBox CheckBoxHasCollider;
    private ColliderScene ColliderContainer;

    public void InitializeUI()
    {
        ButtonSearchMaterial = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonSearchMaterial");
        TabContainerOptions = GetNode<TabContainer>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions");
        PorSegmentos = GetNode<TabBar>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos");
        SpinBoxWidthPixel = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/SpinBoxWidthPixel");
        SpinBox2HeightPixel = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/SpinBox2HeightPixel");
        ButtonSplit = GetNode<Button>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/ButtonSplit");
        SpinBoxZoom = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/HBoxContainer/SpinBoxZoom");
        ViewItems = GetNode<ItemList>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/PorSegmentos/VBoxContainer/ViewItems");
        Customizado = GetNode<TabBar>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/Customizado");
        ControlTextureLocal = GetNode<ControlSeleccionTexture>("MarginContainer/VBoxContainer/HSplitContainer/TabContainerOptions/Customizado/ControlTextureLocal");
        SpinBoxZoomGrid = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer/SpinBoxZoomGrid");
        ScrollContainerGrid = GetNode<ScrollContainer>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid");
        PanelBase = GetNode<Control>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase");
        ControlGrid = GetNode<ControlGrid>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid");
        ControlSpriteInternal = GetNode<Control>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSpriteInternal");
        Sprite2DView = GetNode<Sprite2D>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSpriteInternal/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSpriteInternal/CollisionShapeView");
        GridContainerAnimacion = GetNode<GridContainer>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/GridContainerAnimacion");
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