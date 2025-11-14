// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTileSprite : Window
{
    public delegate void EventNotifyChangued(WindowTileSprite objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditCode;
    private LineEdit LineEditGrouping;
    private OptionButton OptionButtonType;
    private CheckBox CheckBoxIsAnimation;
    private ControlKuroTile ControlTile;
    private ControlListTextures ControlListTexturesAnimated;
    private SpinBox SpinBoxOffsetX;
    private SpinBox SpinBoxOffsetY;
    private SpinBox SpinBoxScale;
    private SpinBox SpinBoxDepht;
    private ColorPickerButton ColorButtonBase;
    private CheckBox CheckBoxMirrorHorizontal;
    private CheckBox CheckBoxMirrorVertical;
    private CheckBox CheckBoxHasCollider;
    private GridContainer GridContainerAnimated;
    private SpinBox SpinBoxFps;
    private CheckBox CheckBoxLoop;
    private KuroTextureButton ButtonSave;
    private KuroTextureButton ButtonSaveSimilar;
    private KuroTextureButton ButtonDelete;
    private ControlSpriteEdit ControlSpriteEdit;
    private PanelColliders ControlCollider;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditCode = GetNode<LineEdit>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/LineEditCode");
        LineEditGrouping = GetNode<LineEdit>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/LineEditGrouping");
        OptionButtonType = GetNode<OptionButton>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        CheckBoxIsAnimation = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/CheckBoxIsAnimation");
        ControlTile = GetNode<ControlKuroTile>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/VBoxContainer/ControlTile");
        ControlListTexturesAnimated = GetNode<ControlListTextures>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/VBoxContainer/ControlListTexturesAnimated");
        SpinBoxOffsetX = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/SpinBoxOffsetX");
        SpinBoxOffsetY = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/SpinBoxOffsetY");
        SpinBoxScale = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/SpinBoxScale");
        SpinBoxDepht = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/SpinBoxDepht");
        ColorButtonBase = GetNode<ColorPickerButton>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/ColorButtonBase");
        CheckBoxMirrorHorizontal = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/CheckBoxMirrorHorizontal");
        CheckBoxMirrorVertical = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/CheckBoxMirrorVertical");
        CheckBoxHasCollider = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainer/CheckBoxHasCollider");
        GridContainerAnimated = GetNode<GridContainer>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainerAnimated");
        SpinBoxFps = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainerAnimated/SpinBoxFps");
        CheckBoxLoop = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/VBoxContainer2/GridContainerAnimated/CheckBoxLoop");
        ButtonSave = GetNode<KuroTextureButton>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/ButtonSave");
        ButtonSaveSimilar = GetNode<KuroTextureButton>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/ButtonSaveSimilar");
        ButtonDelete = GetNode<KuroTextureButton>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/VBoxContainer/HBoxContainer/ButtonDelete");
        ControlSpriteEdit = GetNode<ControlSpriteEdit>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/ControlSpriteEdit");
        ControlCollider = GetNode<PanelColliders>("MarginContainer/PanelContainer/MarginContainer/HBoxContainer/ControlCollider");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}