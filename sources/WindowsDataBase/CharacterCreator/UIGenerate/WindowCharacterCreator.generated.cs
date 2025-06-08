// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowCharacterCreator : Window
{
    public delegate void EventNotifyChangued(WindowCharacterCreator objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private VBoxContainer VBoxContainerBase;
    private Button ButtonSearch;
    private Sprite2D Sprite2DView;
    private VBoxContainer VBoxContainerDown;
    private Label Label5;
    private OptionButton OptionButtonBehavior;
    private Label Label6;
    private OptionButton OptionButtonType;
    private LineEdit LineEditName;
    private Label Label2;
    private SpinBox SpinBoxScale;
    private Label Label3;
    private ColorPickerButton ColorBase;
    private Label Label4;
    private TextEdit TextEditDescription;
    private VBoxContainer VBoxContainerUnits;
    private Label Label15;
    private OptionButton OptionButtonUnitType;
    private Label Label16;
    private OptionButton OptionButtonUnitMoveType;
    private Label Label17;
    private OptionButton OptionButtonUnitDirectionType;
    private Label Label18;
    private SpinBox SpinBoxRadiusMove;
    private Label Label19;
    private SpinBox SpinBoxRadiusSearch;
    private Button ButtonSave;
    private BonusContainer PanelBonificaciones;
    private StatsContainer PanelEstadisticas;
    private ElementsContainer PanelAtaque;
    private VBoxContainer VBoxContainer2;
    private ElementsContainer PanelDefensa;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        VBoxContainerBase = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase");
        ButtonSearch = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainer/ButtonSearch");
        Sprite2DView = GetNode<Sprite2D>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainer/CenterContainer/Panel/Control/Sprite2DView");
        VBoxContainerDown = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown");
        Label5 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label5");
        OptionButtonBehavior = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/OptionButtonBehavior");
        OptionButtonBehavior.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label6 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label6");
        OptionButtonType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/LineEditName");
        Label2 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label2");
        SpinBoxScale = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/SpinBoxScale");
        Label3 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label3");
        ColorBase = GetNode<ColorPickerButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/ColorBase");
        Label4 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label4");
        TextEditDescription = GetNode<TextEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/TextEditDescription");
        VBoxContainerUnits = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits");
        Label15 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/Label15");
        OptionButtonUnitType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/OptionButtonUnitType");
        OptionButtonUnitType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label16 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/Label16");
        OptionButtonUnitMoveType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/OptionButtonUnitMoveType");
        OptionButtonUnitMoveType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label17 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/Label17");
        OptionButtonUnitDirectionType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/OptionButtonUnitDirectionType");
        OptionButtonUnitDirectionType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label18 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/Label18");
        SpinBoxRadiusMove = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/SpinBoxRadiusMove");
        Label19 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/Label19");
        SpinBoxRadiusSearch = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/SpinBoxRadiusSearch");
        ButtonSave = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/ButtonSave");
        PanelBonificaciones = GetNode<BonusContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/PanelBonificaciones");
        PanelEstadisticas = GetNode<StatsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/PanelEstadisticas");
        PanelAtaque = GetNode<ElementsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer/PanelAtaque");
        VBoxContainer2 = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer2");
        PanelDefensa = GetNode<ElementsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer2/PanelDefensa");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}