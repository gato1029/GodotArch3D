// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ContainerAnimationCharacter : Window
{
    public delegate void EventNotifyChangued(ContainerAnimationCharacter objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private Button ButtonSaveAll;
    private ContainerAnimation Animacion_Base;
    private MarginContainer Datos_Basicos;
    private PanelContainer CenterContainer;
    private ControlGridGeneric ControlGridGeneric;
    private Panel PanelImage;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private VBoxContainer VBoxContainerBasico;
    private LineEdit LineEditName;
    private SpinBox SpinBoxZordering;
    private CheckBox CheckBoxAnimationComposite;
    private SpinBox SpinBoxOffsetSpriteX;
    private SpinBox SpinBoxOffsetSpriteY;
    private ColliderScene PanelCuerpo;
    private ColliderScene PanelMovimiento;
    private Button ButtonSave;
    private ContainerAnimation Animacion_Extra;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ButtonSaveAll = GetNode<Button>("MarginContainer/VBoxContainer/ButtonSaveAll");
        Animacion_Base = GetNode<ContainerAnimation>("MarginContainer/VBoxContainer/TabContainer/Animacion Base");
        Datos_Basicos = GetNode<MarginContainer>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos");
        CenterContainer = GetNode<PanelContainer>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer");
        ControlGridGeneric = GetNode<ControlGridGeneric>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer/ScrollContainer/ControlGridGeneric");
        PanelImage = GetNode<Panel>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer/ScrollContainer/ControlGridGeneric/PanelImage");
        Sprite2DView = GetNode<Sprite2D>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer/ScrollContainer/ControlGridGeneric/PanelImage/Control/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer/ScrollContainer/ControlGridGeneric/PanelImage/Control/CollisionShapeView");
        VBoxContainerBasico = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico");
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/LineEditName");
        SpinBoxZordering = GetNode<SpinBox>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/SpinBoxZordering");
        CheckBoxAnimationComposite = GetNode<CheckBox>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/CheckBoxAnimationComposite");
        SpinBoxOffsetSpriteX = GetNode<SpinBox>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/VBoxContainer3/HBoxContainer/SpinBoxOffsetSpriteX");
        SpinBoxOffsetSpriteY = GetNode<SpinBox>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/VBoxContainer3/HBoxContainer/SpinBoxOffsetSpriteY");
        PanelCuerpo = GetNode<ColliderScene>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/VBoxContainer2/PanelCuerpo");
        PanelMovimiento = GetNode<ColliderScene>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/VBoxContainer/PanelMovimiento");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/ButtonSave");
        Animacion_Extra = GetNode<ContainerAnimation>("MarginContainer/VBoxContainer/TabContainer/Animacion Extra");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}