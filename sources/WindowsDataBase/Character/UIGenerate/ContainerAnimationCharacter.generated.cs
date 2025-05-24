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
    private OptionButton OptionButtonAnimation;
    private ItemList ViewItems;
    private VBoxContainer ContainerMain;
    private PanelContainer CenterContainer;
    private Panel PanelImage;
    private Sprite2D Sprite2DView;
    private CollisionShape2D CollisionShapeView;
    private Button ButtonSave;
    private HBoxContainer HBoxContainerAnimacion;
    private GridContainer GridContainerAnimacion;
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
    private Label Label10;
    private CheckBox CheckBoxHasCollisionMultiple;
    private VBoxContainer VBoxContainerCollision;
    private MarginContainer ContainerBasico;
    private VBoxContainer VBoxContainerBasico;
    private ColliderScene PanelMovimiento;
    private VBoxContainer VBoxContainer2;
    private ColliderScene PanelCuerpo;
    private SpinBox SpinBoxZordering;
    private Label Label2;
    private CheckBox CheckBoxAnimationComposite;

    public void InitializeUI()
    {
        HBoxContainer2 = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer2");
        ButtonBuscar = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonBuscar");
        ButtonNuevo = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer2/ButtonNuevo");
        OptionButtonAnimation = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer2/OptionButtonAnimation");
        OptionButtonAnimation.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        ViewItems = GetNode<ItemList>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ViewItems");
        ContainerMain = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain");
        CenterContainer = GetNode<PanelContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer");
        PanelImage = GetNode<Panel>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/PanelImage");
        Sprite2DView = GetNode<Sprite2D>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/PanelImage/Control/Sprite2DView");
        CollisionShapeView = GetNode<CollisionShape2D>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/CenterContainer/PanelImage/Control/CollisionShapeView");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/VBoxContainer/PanelContainer/ButtonSave");
        HBoxContainerAnimacion = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion");
        GridContainerAnimacion = GetNode<GridContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion");
        Label8 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label8");
        CheckBoxModeSelection = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/CheckBoxModeSelection");
        CheckBoxModeSelection.Pressed += CheckBoxModeSelection_PressedUI;
        CheckBoxModeSelection_PressedUI();
        LabelFrameDuplicate = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/LabelFrameDuplicate");
        CheckBoxFrameDuplicate = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/CheckBoxFrameDuplicate");
        CheckBoxFrameDuplicate.Pressed += CheckBoxFrameDuplicate_PressedUI;
        CheckBoxFrameDuplicate_PressedUI();
        Label3 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label3");
        TextEditFrames = GetNode<TextEdit>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/TextEditFrames");
        Label4 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label4");
        ButtonForcedFrames = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/ButtonForcedFrames");
        Label9 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label9");
        SpinBoxDuration = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/SpinBoxDuration");
        Label5 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label5");
        CheckBoxLoop = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/CheckBoxLoop");
        CheckBoxLoop.Pressed += CheckBoxLoop_PressedUI;
        CheckBoxLoop_PressedUI();
        Label6 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label6");
        CheckBoxMirror = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/CheckBoxMirror");
        CheckBoxMirror.Pressed += CheckBoxMirror_PressedUI;
        CheckBoxMirror_PressedUI();
        Label7 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label7");
        CheckBoxHasCollision = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/CheckBoxHasCollision");
        CheckBoxHasCollision.Pressed += CheckBoxHasCollision_PressedUI;
        CheckBoxHasCollision_PressedUI();
        Label10 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/Label10");
        CheckBoxHasCollisionMultiple = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/GridContainerAnimacion/CheckBoxHasCollisionMultiple");
        CheckBoxHasCollisionMultiple.Pressed += CheckBoxHasCollisionMultiple_PressedUI;
        CheckBoxHasCollisionMultiple_PressedUI();
        VBoxContainerCollision = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/HBoxContainerAnimacion/VBoxContainerCollision");
        ContainerBasico = GetNode<MarginContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico");
        VBoxContainerBasico = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico/VBoxContainerBasico");
        PanelMovimiento = GetNode<ColliderScene>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico/VBoxContainerBasico/VBoxContainer/PanelMovimiento");
        VBoxContainer2 = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico/VBoxContainerBasico/VBoxContainer2");
        PanelCuerpo = GetNode<ColliderScene>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico/VBoxContainerBasico/VBoxContainer2/PanelCuerpo");
        SpinBoxZordering = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico/VBoxContainerBasico/GridContainer/SpinBoxZordering");
        Label2 = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico/VBoxContainerBasico/GridContainer/Label2");
        CheckBoxAnimationComposite = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/HSplitContainer/ContainerMain/HBoxContainer/TabContainer/ContainerBasico/VBoxContainerBasico/GridContainer/CheckBoxAnimationComposite");
        CheckBoxAnimationComposite.Pressed += CheckBoxAnimationComposite_PressedUI;
        CheckBoxAnimationComposite_PressedUI();
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

    private void CheckBoxHasCollisionMultiple_PressedUI()
    {
        if (CheckBoxHasCollisionMultiple.ButtonPressed)
            CheckBoxHasCollisionMultiple.Text = "Collision";
        else
            CheckBoxHasCollisionMultiple.Text = "No Collision";
    }

    private void CheckBoxAnimationComposite_PressedUI()
    {
        if (CheckBoxAnimationComposite.ButtonPressed)
            CheckBoxAnimationComposite.Text = "Habilitada";
        else
            CheckBoxAnimationComposite.Text = "No Habilitada";
    }
}