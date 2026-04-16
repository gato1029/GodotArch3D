// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowBuilding : Window
{
    public delegate void EventNotifyChangued(WindowBuilding objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private OptionButton OptionButtonTypeBuilding;
    private LineEdit LineEditName;
    private TextEdit TextEditDescription;
    private ControlListTileSprite ControlTileSpriteItem;
    private SpinBox SpinBoxTimeBuild;
    private SpinBox SpinBoxMaxHealth;
    private VBoxContainer ContainerAtaque;
    private SpinBox SpinBoxRangeAttack;
    private SpinBox SpinBoxChargueAttack;
    private ControlProyectile ControlProyectileItem;
    private ElementsContainer ControlAtaque;
    private VBoxContainer VBoxContainer2f;
    private ElementsContainer ControlDefensa;
    private Button ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        OptionButtonTypeBuilding = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/OptionButtonTypeBuilding");
        OptionButtonTypeBuilding.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/LineEditName");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/TextEditDescription");
        ControlTileSpriteItem = GetNode<ControlListTileSprite>("MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/ControlTileSpriteItem");
        SpinBoxTimeBuild = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/SpinBoxTimeBuild");
        SpinBoxMaxHealth = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/GridContainer2/SpinBoxMaxHealth");
        ContainerAtaque = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/ContainerAtaque");
        SpinBoxRangeAttack = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerAtaque/VBoxContainer/GridContainer/SpinBoxRangeAttack");
        SpinBoxChargueAttack = GetNode<SpinBox>("MarginContainer/VBoxContainer/HBoxContainer/ContainerAtaque/VBoxContainer/GridContainer/SpinBoxChargueAttack");
        ControlProyectileItem = GetNode<ControlProyectile>("MarginContainer/VBoxContainer/HBoxContainer/ContainerAtaque/VBoxContainer/GridContainer/ControlProyectileItem");
        ControlAtaque = GetNode<ElementsContainer>("MarginContainer/VBoxContainer/HBoxContainer/ContainerAtaque/VBoxContainer/VBoxContainer2/VBoxContainer/ControlAtaque");
        VBoxContainer2f = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/ContainerAtaque/VBoxContainer/VBoxContainer2/VBoxContainer2f");
        ControlDefensa = GetNode<ElementsContainer>("MarginContainer/VBoxContainer/HBoxContainer/ContainerAtaque/VBoxContainer/VBoxContainer2/VBoxContainer2f/ControlDefensa");
        ButtonSave = GetNode<Button>("MarginContainer/VBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}