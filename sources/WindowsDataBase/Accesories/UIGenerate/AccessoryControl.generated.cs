// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class AccessoryControl : Window
{
    public delegate void EventNotifyChangued(AccessoryControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private VBoxContainer VBoxContainer2;
    private ControlMiniature ControlMiniatura;
    private Label Label7;
    private CheckBox CheckBoxAnimationBody;
    private Label Label8;
    private CheckBox CheckBoxAnimationTiles;
    private Label Label9;
    private CheckBox CheckBoxRequeriment;
    private Label Label6;
    private OptionButton OptionButtonClassAccesory;
    private Label Label10;
    private OptionButton OptionButtonTypeAccesory;
    private Label Label11;
    private OptionButton OptionButtonBodyAccesory;
    private Label Label5;
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
        VBoxContainer2 = GetNode<VBoxContainer>("MarginContainer/HBoxContainer/VBoxContainer2");
        ControlMiniatura = GetNode<ControlMiniature>("MarginContainer/HBoxContainer/VBoxContainer2/ControlMiniatura");
        Label7 = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Label7");
        CheckBoxAnimationBody = GetNode<CheckBox>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/CheckBoxAnimationBody");
        CheckBoxAnimationBody.Pressed += CheckBoxAnimationBody_PressedUI;
        CheckBoxAnimationBody_PressedUI();
        Label8 = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Label8");
        CheckBoxAnimationTiles = GetNode<CheckBox>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/CheckBoxAnimationTiles");
        CheckBoxAnimationTiles.Pressed += CheckBoxAnimationTiles_PressedUI;
        CheckBoxAnimationTiles_PressedUI();
        Label9 = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Label9");
        CheckBoxRequeriment = GetNode<CheckBox>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/CheckBoxRequeriment");
        CheckBoxRequeriment.Pressed += CheckBoxRequeriment_PressedUI;
        CheckBoxRequeriment_PressedUI();
        Label6 = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Label6");
        OptionButtonClassAccesory = GetNode<OptionButton>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/OptionButtonClassAccesory");
        OptionButtonClassAccesory.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label10 = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Label10");
        OptionButtonTypeAccesory = GetNode<OptionButton>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/OptionButtonTypeAccesory");
        OptionButtonTypeAccesory.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label11 = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Label11");
        OptionButtonBodyAccesory = GetNode<OptionButton>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/OptionButtonBodyAccesory");
        OptionButtonBodyAccesory.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label5 = GetNode<Label>("MarginContainer/HBoxContainer/VBoxContainer2/GridContainer/Label5");
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


    private void CheckBoxAnimationBody_PressedUI()
    {
        if (CheckBoxAnimationBody.ButtonPressed)
            CheckBoxAnimationBody.Text = "Disponible";
        else
            CheckBoxAnimationBody.Text = "No Disponible";
    }

    private void CheckBoxAnimationTiles_PressedUI()
    {
        if (CheckBoxAnimationTiles.ButtonPressed)
            CheckBoxAnimationTiles.Text = "Disponible";
        else
            CheckBoxAnimationTiles.Text = "No Disponible";
    }

    private void CheckBoxRequeriment_PressedUI()
    {
        if (CheckBoxRequeriment.ButtonPressed)
            CheckBoxRequeriment.Text = "Disponible";
        else
            CheckBoxRequeriment.Text = "No Disponible";
    }
}