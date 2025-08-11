// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowResourcesSource : Window
{
    public delegate void EventNotifyChangued(WindowResourcesSource objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabContainer TabContainerBase;
    private TabBar Informacion_Basica;
    private OptionButton OptionButtonType;
    private LineEdit LineEditName;
    private TextEdit TextEditDescription;
    private CheckBox CheckBoxHasAnimation;
    private SpinBox SpinBoxAmount;
    private Button ButtonSave;
    private Sprite2D Sprite2DViewCentral;
    private TabBar Sprite;
    private ControlSprite ControlSprite;
    private TabBar Disposicion;
    private ControlBuildingGrid ControlBuildingGridBasic;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TabContainerBase = GetNode<TabContainer>("MarginContainer/TabContainerBase");
        Informacion_Basica = GetNode<TabBar>("MarginContainer/TabContainerBase/Informacion Basica");
        OptionButtonType = GetNode<OptionButton>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        LineEditName = GetNode<LineEdit>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/LineEditName");
        TextEditDescription = GetNode<TextEdit>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/TextEditDescription");
        CheckBoxHasAnimation = GetNode<CheckBox>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/CheckBoxHasAnimation");
        SpinBoxAmount = GetNode<SpinBox>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/GridContainer/SpinBoxAmount");
        ButtonSave = GetNode<Button>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/VBoxContainer/ButtonSave");
        Sprite2DViewCentral = GetNode<Sprite2D>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/HBoxContainer/CenterContainer/Control/Sprite2DViewCentral");
        Sprite = GetNode<TabBar>("MarginContainer/TabContainerBase/Sprite");
        ControlSprite = GetNode<ControlSprite>("MarginContainer/TabContainerBase/Sprite/ControlSprite");
        Disposicion = GetNode<TabBar>("MarginContainer/TabContainerBase/Disposicion");
        ControlBuildingGridBasic = GetNode<ControlBuildingGrid>("MarginContainer/TabContainerBase/Disposicion/ControlBuildingGridBasic");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}