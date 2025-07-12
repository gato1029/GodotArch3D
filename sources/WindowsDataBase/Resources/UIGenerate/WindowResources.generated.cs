// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowResources : Window
{
    public delegate void EventNotifyChangued(WindowResources objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabContainer TabContainerBase;
    private TabBar Informacion_Basica;
    private LineEdit LineEditName;
    private TextEdit TextEditDescription;
    private CheckBox CheckBoxHasAnimation;
    private SpinBox SpinBoxAmount;
    private Button ButtonSave;
    private Sprite2D Sprite2DViewCentral;
    private TabBar Miniatura;
    private ControlSprite ControlSpriteMiniatura;
    private TabBar Sprite;
    private ControlSprite ControlSprite;
    private TabBar Disposicion;
    private ControlBuildingGrid ControlBuildingGridBasic;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TabContainerBase = GetNode<TabContainer>("MarginContainer/TabContainerBase");
        Informacion_Basica = GetNode<TabBar>("MarginContainer/TabContainerBase/Informacion Basica");
        LineEditName = GetNode<LineEdit>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/LineEditName");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/TextEditDescription");
        CheckBoxHasAnimation = GetNode<CheckBox>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/CheckBoxHasAnimation");
        CheckBoxHasAnimation.Pressed += CheckBoxHasAnimation_PressedUI;
        CheckBoxHasAnimation_PressedUI();
        SpinBoxAmount = GetNode<SpinBox>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/SpinBoxAmount");
        ButtonSave = GetNode<Button>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/ButtonSave");
        Sprite2DViewCentral = GetNode<Sprite2D>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/CenterContainer/Control/Sprite2DViewCentral");
        Miniatura = GetNode<TabBar>("MarginContainer/TabContainerBase/Miniatura");
        ControlSpriteMiniatura = GetNode<ControlSprite>("MarginContainer/TabContainerBase/Miniatura/ControlSpriteMiniatura");
        Sprite = GetNode<TabBar>("MarginContainer/TabContainerBase/Sprite");
        ControlSprite = GetNode<ControlSprite>("MarginContainer/TabContainerBase/Sprite/ControlSprite");
        Disposicion = GetNode<TabBar>("MarginContainer/TabContainerBase/Disposicion");
        ControlBuildingGridBasic = GetNode<ControlBuildingGrid>("MarginContainer/TabContainerBase/Disposicion/ControlBuildingGridBasic");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }


    private void CheckBoxHasAnimation_PressedUI()
    {
        if (CheckBoxHasAnimation.ButtonPressed)
            CheckBoxHasAnimation.Text = "";
        else
            CheckBoxHasAnimation.Text = "No ";
    }
}