// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowBuilding : Window
{
    public delegate void EventNotifyChangued(WindowBuilding objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabBar Informacion_Basica;
    private LineEdit LineEditName;
    private TextEdit TextEditDescription;
    private OptionButton OptionButtonTypeBuilding;
    private SpinBox SpinBoxMaxHealth;
    private SpinBox SpinBoxRangeAttack;
    private SpinBox SpinBoxChargueAttack;
    private SpinBox SpinBoxTimeBuild;
    private Button ButtonSave;
    private ElementsContainer ControlAtaque;
    private VBoxContainer VBoxContainer2f;
    private ElementsContainer ControlDefensa;
    private TabBar Miniatura;
    private ControlSprite ControlSpriteMiniatura;
    private TabBar SpriteBasico;
    private ControlSprite ControlSpriteBasico;
    private TabBar Animaciones;
    private ContainerAnimation ContainerAnimationBasico;
    private TabBar Disposicion;
    private ControlBuildingGrid ControlBuildingGridBasic;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        Informacion_Basica = GetNode<TabBar>("MarginContainer/TabContainer/Informacion Basica");
        LineEditName = GetNode<LineEdit>("MarginContainer/TabContainer/Informacion Basica/GridContainer/GridContainer2/LineEditName");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/TabContainer/Informacion Basica/GridContainer/GridContainer2/TextEditDescription");
        OptionButtonTypeBuilding = GetNode<OptionButton>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/OptionButtonTypeBuilding");
        OptionButtonTypeBuilding.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        SpinBoxMaxHealth = GetNode<SpinBox>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxMaxHealth");
        SpinBoxRangeAttack = GetNode<SpinBox>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxRangeAttack");
        SpinBoxChargueAttack = GetNode<SpinBox>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxChargueAttack");
        SpinBoxTimeBuild = GetNode<SpinBox>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2/GridContainer/SpinBoxTimeBuild");
        ButtonSave = GetNode<Button>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2/ButtonSave");
        ControlAtaque = GetNode<ElementsContainer>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer/ControlAtaque");
        VBoxContainer2f = GetNode<VBoxContainer>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2f");
        ControlDefensa = GetNode<ElementsContainer>("MarginContainer/TabContainer/Informacion Basica/GridContainer/VBoxContainer2f/ControlDefensa");
        Miniatura = GetNode<TabBar>("MarginContainer/TabContainer/Miniatura");
        ControlSpriteMiniatura = GetNode<ControlSprite>("MarginContainer/TabContainer/Miniatura/ControlSpriteMiniatura");
        SpriteBasico = GetNode<TabBar>("MarginContainer/TabContainer/SpriteBasico");
        ControlSpriteBasico = GetNode<ControlSprite>("MarginContainer/TabContainer/SpriteBasico/ControlSpriteBasico");
        Animaciones = GetNode<TabBar>("MarginContainer/TabContainer/Animaciones");
        ContainerAnimationBasico = GetNode<ContainerAnimation>("MarginContainer/TabContainer/Animaciones/ScrollContainer/ContainerAnimationBasico");
        Disposicion = GetNode<TabBar>("MarginContainer/TabContainer/Disposicion");
        ControlBuildingGridBasic = GetNode<ControlBuildingGrid>("MarginContainer/TabContainer/Disposicion/ControlBuildingGridBasic");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}