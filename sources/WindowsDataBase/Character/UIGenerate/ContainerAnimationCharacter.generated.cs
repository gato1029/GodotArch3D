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
    private Panel PanelImage;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private VBoxContainer VBoxContainerBasico;
    private Label Label3;
    private LineEdit LineEditName;
    private SpinBox SpinBoxZordering;
    private Label Label2;
    private CheckBox CheckBoxAnimationComposite;
    private ColliderScene PanelMovimiento;
    private VBoxContainer VBoxContainer2;
    private ColliderScene PanelCuerpo;
    private Button ButtonSave;
    private ContainerAnimation Animacion_Extra;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ButtonSaveAll = GetNode<Button>("MarginContainer/VBoxContainer/ButtonSaveAll");
        Animacion_Base = GetNode<ContainerAnimation>("MarginContainer/VBoxContainer/TabContainer/Animacion Base");
        Datos_Basicos = GetNode<MarginContainer>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos");
        CenterContainer = GetNode<PanelContainer>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer");
        PanelImage = GetNode<Panel>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer/PanelImage");
        Sprite2DView = GetNode<Sprite2D>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer/PanelImage/Control/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/CenterContainer/PanelImage/Control/CollisionShapeView");
        VBoxContainerBasico = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico");
        Label3 = GetNode<Label>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/Label3");
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/LineEditName");
        SpinBoxZordering = GetNode<SpinBox>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/SpinBoxZordering");
        Label2 = GetNode<Label>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/Label2");
        CheckBoxAnimationComposite = GetNode<CheckBox>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/GridContainer/CheckBoxAnimationComposite");
        CheckBoxAnimationComposite.Pressed += CheckBoxAnimationComposite_PressedUI;
        CheckBoxAnimationComposite_PressedUI();
        PanelMovimiento = GetNode<ColliderScene>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/VBoxContainer/PanelMovimiento");
        VBoxContainer2 = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/VBoxContainer2");
        PanelCuerpo = GetNode<ColliderScene>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/VBoxContainer2/PanelCuerpo");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/TabContainer/Datos Basicos/HBoxContainer/VBoxContainerBasico/ButtonSave");
        Animacion_Extra = GetNode<ContainerAnimation>("MarginContainer/VBoxContainer/TabContainer/Animacion Extra");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }


    private void CheckBoxAnimationComposite_PressedUI()
    {
        if (CheckBoxAnimationComposite.ButtonPressed)
            CheckBoxAnimationComposite.Text = "Habilitada";
        else
            CheckBoxAnimationComposite.Text = "No Habilitada";
    }
}