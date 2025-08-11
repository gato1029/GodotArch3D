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
    private PanelColliders ControlCollider;

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
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/ScrollContainerGrid/PanelBase/ControlGrid/Panel/ControlSpriteInternal/Sprite2DView/CollisionShapeView");
        GridContainerAnimacion = GetNode<GridContainer>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion");
        SpinBoxOffsetX = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion/SpinBoxOffsetX");
        SpinBoxOffsetY = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion/SpinBoxOffsetY");
        ColorButtonBase = GetNode<ColorPickerButton>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion/ColorButtonBase");
        SpinBoxScale = GetNode<SpinBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion/SpinBoxScale");
        CheckBoxMirror = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion/CheckBoxMirror");
        CheckBoxMirrorV = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion/CheckBoxMirrorV");
        CheckBoxHasCollider = GetNode<CheckBox>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/GridContainerAnimacion/CheckBoxHasCollider");
        ControlCollider = GetNode<PanelColliders>("MarginContainer/VBoxContainer/HSplitContainer/HBoxContainer/VBoxContainer/HBoxContainer2/ControlCollider");
    }
}