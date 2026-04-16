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
    private ControlListTileSprite ControlTileSpriteData;
    private VBoxContainer VBoxContainerDown;
    private OptionButton OptionButtonBehavior;
    private OptionButton OptionButtonType;
    private LineEdit LineEditName;
    private SpinBox SpinBoxScale;
    private ColorPickerButton ColorBase;
    private TextEdit TextEditDescription;
    private VBoxContainer VBoxContainerUnits;
    private OptionButton OptionButtonUnitType;
    private OptionButton OptionButtonUnitMoveType;
    private OptionButton OptionButtonUnitDirectionType;
    private SpinBox SpinBoxRadiusMove;
    private SpinBox SpinBoxRadiusSearch;
    private Button ButtonSave;
    private BonusContainer PanelBonificaciones;
    private StatsContainer PanelEstadisticas;
    private ElementsContainer PanelAtaque;
    private ElementsContainer PanelDefensa;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        VBoxContainerBase = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase");
        ButtonSearch = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainer/ButtonSearch");
        Sprite2DView = GetNode<Sprite2D>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainer/CenterContainer/Panel/Control/Sprite2DView");
        ControlTileSpriteData = GetNode<ControlListTileSprite>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainer/CenterContainer/ControlTileSpriteData");
        VBoxContainerDown = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown");
        OptionButtonBehavior = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/OptionButtonBehavior");
        OptionButtonBehavior.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        OptionButtonType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/LineEditName");
        SpinBoxScale = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/SpinBoxScale");
        ColorBase = GetNode<ColorPickerButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/ColorBase");
        TextEditDescription = GetNode<TextEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/TextEditDescription");
        VBoxContainerUnits = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits");
        OptionButtonUnitType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/OptionButtonUnitType");
        OptionButtonUnitType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        OptionButtonUnitMoveType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/OptionButtonUnitMoveType");
        OptionButtonUnitMoveType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        OptionButtonUnitDirectionType = GetNode<OptionButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/OptionButtonUnitDirectionType");
        OptionButtonUnitDirectionType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        SpinBoxRadiusMove = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/SpinBoxRadiusMove");
        SpinBoxRadiusSearch = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerUnits/GridContainer/SpinBoxRadiusSearch");
        ButtonSave = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/ButtonSave");
        PanelBonificaciones = GetNode<BonusContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/PanelBonificaciones");
        PanelEstadisticas = GetNode<StatsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/PanelEstadisticas");
        PanelAtaque = GetNode<ElementsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer/PanelAtaque");
        PanelDefensa = GetNode<ElementsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer2/PanelDefensa");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}