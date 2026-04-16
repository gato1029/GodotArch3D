// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class AccessoryControl : Window
{
    public delegate void EventNotifyChangued(AccessoryControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ControlKuroTile ControlTile;
    private CheckBox CheckBoxAnimationBody;
    private ControlListTileSprite ControlTileSpriteData;
    private CheckBox CheckBoxRequeriment;
    private OptionButton OptionButtonClassAccesory;
    private OptionButton OptionButtonTypeAccesory;
    private OptionButton OptionButtonBodyAccesory;
    private LineEdit LineEditName;
    private Label Descripcion;
    private TextEdit TextEditDescription;
    private ColorPickerButton ColorPickerButtonColorBase;
    private Button ButtonSave;
    private Button ButtonDelete;
    private TabContainer TabContainerControl;
    private PanelContainer Bonificaciones;
    private BonusContainer ControlBonusBase;
    private StatsContainer ControlStats;
    private ElementsContainer ControlAtaque;
    private VBoxContainer VBoxContainer2f;
    private ElementsContainer ControlDefensa;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ControlTile = GetNode<ControlKuroTile>("MarginContainer/HBoxContainer/VBoxContainer2/VBoxContainer/ControlTile");
        CheckBoxAnimationBody = GetNode<CheckBox>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/HBoxContainer/CheckBoxAnimationBody");
        ControlTileSpriteData = GetNode<ControlListTileSprite>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/HBoxContainer/ControlTileSpriteData");
        CheckBoxRequeriment = GetNode<CheckBox>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/CheckBoxRequeriment");
        OptionButtonClassAccesory = GetNode<OptionButton>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/OptionButtonClassAccesory");
        OptionButtonClassAccesory.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        OptionButtonTypeAccesory = GetNode<OptionButton>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/OptionButtonTypeAccesory");
        OptionButtonTypeAccesory.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        OptionButtonBodyAccesory = GetNode<OptionButton>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/OptionButtonBodyAccesory");
        OptionButtonBodyAccesory.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/LineEditName");
        Descripcion = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Descripcion");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/TextEditDescription");
        ColorPickerButtonColorBase = GetNode<ColorPickerButton>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/ColorPickerButtonColorBase");
        ButtonSave = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/HBoxContainer/ButtonSave");
        ButtonDelete = GetNode<Button>("MarginContainer/HBoxContainer/VBoxContainer2/HBoxContainer/ButtonDelete");
        TabContainerControl = GetNode<TabContainer>("MarginContainer/HBoxContainer/TabContainerControl");
        Bonificaciones = GetNode<PanelContainer>("MarginContainer/HBoxContainer/TabContainerControl/Bonificaciones");
        ControlBonusBase = GetNode<BonusContainer>("MarginContainer/HBoxContainer/TabContainerControl/Bonificaciones/GridContainer/ControlBonusBase");
        ControlStats = GetNode<StatsContainer>("MarginContainer/HBoxContainer/TabContainerControl/Bonificaciones/GridContainer/ControlStats");
        ControlAtaque = GetNode<ElementsContainer>("MarginContainer/HBoxContainer/TabContainerControl/Bonificaciones/GridContainer/VBoxContainer/ControlAtaque");
        VBoxContainer2f = GetNode<VBoxContainer>("MarginContainer/HBoxContainer/TabContainerControl/Bonificaciones/GridContainer/VBoxContainer2f");
        ControlDefensa = GetNode<ElementsContainer>("MarginContainer/HBoxContainer/TabContainerControl/Bonificaciones/GridContainer/VBoxContainer2f/ControlDefensa");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}