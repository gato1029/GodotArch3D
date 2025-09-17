// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowBuilding : Window
{
    public delegate void EventNotifyChangued(WindowBuilding objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabContainer TabContainerBase;
    private TabBar Informacion_Basica;
    private LineEdit LineEditName;
    private TextEdit TextEditDescription;
    private CheckBox CheckBoxIsAnimated;
    private OptionButton OptionButtonTypeBuilding;
    private SpinBox SpinBoxMaxHealth;
    private SpinBox SpinBoxRangeAttack;
    private SpinBox SpinBoxChargueAttack;
    private SpinBox SpinBoxTimeBuild;
    private Button ButtonSave;
    private ElementsContainer ControlAtaque;
    private VBoxContainer VBoxContainer2f;
    private ElementsContainer ControlDefensa;
    private TabBar SpriteBasico;
    private ControlSprite ControlSpriteBasico;
    private TabBar Disposicion;
    private ControlBuildingGrid ControlBuildingGridBasic;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TabContainerBase = GetNode<TabContainer>("MarginContainer/TabContainerBase");
        Informacion_Basica = GetNode<TabBar>("MarginContainer/TabContainerBase/Informacion Basica");
        LineEditName = GetNode<LineEdit>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/GridContainer2/LineEditName");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/GridContainer2/TextEditDescription");
        CheckBoxIsAnimated = GetNode<CheckBox>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/GridContainer2/CheckBoxIsAnimated");
        OptionButtonTypeBuilding = GetNode<OptionButton>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/OptionButtonTypeBuilding");
        OptionButtonTypeBuilding.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        SpinBoxMaxHealth = GetNode<SpinBox>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxMaxHealth");
        SpinBoxRangeAttack = GetNode<SpinBox>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxRangeAttack");
        SpinBoxChargueAttack = GetNode<SpinBox>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxChargueAttack");
        SpinBoxTimeBuild = GetNode<SpinBox>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxTimeBuild");
        ButtonSave = GetNode<Button>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2/ButtonSave");
        ControlAtaque = GetNode<ElementsContainer>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer/ControlAtaque");
        VBoxContainer2f = GetNode<VBoxContainer>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2f");
        ControlDefensa = GetNode<ElementsContainer>("MarginContainer/TabContainerBase/Informacion Basica/GridContainer/VBoxContainer2f/ControlDefensa");
        SpriteBasico = GetNode<TabBar>("MarginContainer/TabContainerBase/SpriteBasico");
        ControlSpriteBasico = GetNode<ControlSprite>("MarginContainer/TabContainerBase/SpriteBasico/ControlSpriteBasico");
        Disposicion = GetNode<TabBar>("MarginContainer/TabContainerBase/Disposicion");
        ControlBuildingGridBasic = GetNode<ControlBuildingGrid>("MarginContainer/TabContainerBase/Disposicion/ControlBuildingGridBasic");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}