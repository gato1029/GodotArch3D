// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTileSprite : Window
{
    public delegate void EventNotifyChangued(WindowTileSprite objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private LineEdit LineEditCode;
    private LineEdit LineEditCodeSave;
    private GridContainer GridContainerCabecera;
    private LineEdit LineEditGrouping;
    private OptionButton OptionButtonType;
    private ControlAnimationMultipleDirection ControlMultiple;
    private GridContainer GridContainerTile;
    private ControlKuroTile ControlTile;
    private ControlListTextures ControlListTexturesAnimated;
    private VBoxContainer ContainerConfigBase;
    private KuroTextureButton ButtonHideConfigBase;
    private AnimatedPanelContainer AnimatedPanelContainerConfigBase;
    private SpinBox SpinBoxOffsetX;
    private SpinBox SpinBoxOffsetY;
    private SpinBox SpinBoxScale;
    private SpinBox SpinBoxDepht;
    private ColorPickerButton ColorButtonBase;
    private GridContainer GridContainerBasicTransform;
    private CheckBox CheckBoxMirrorHorizontal;
    private CheckBox CheckBoxMirrorVertical;
    private CheckBox CheckBoxHasCollider;
    private GridContainer GridContainerAnimated;
    private SpinBox SpinBoxFps;
    private CheckBox CheckBoxLoop;
    private KuroTextureButton ButtonSave;
    private KuroTextureButton ButtonSaveSimilar;
    private KuroTextureButton ButtonDelete;
    private VBoxContainer HBoxContainer;
    private ControlSpriteEdit ControlSpriteEdit;
    private PanelColliders ControlCollider;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        LineEditCode = GetNode<LineEdit>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/HBoxContainer/LineEditCode");
        LineEditCodeSave = GetNode<LineEdit>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/HBoxContainer/LineEditCodeSave");
        GridContainerCabecera = GetNode<GridContainer>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerCabecera");
        LineEditGrouping = GetNode<LineEdit>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerCabecera/LineEditGrouping");
        OptionButtonType = GetNode<OptionButton>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerCabecera/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        ControlMultiple = GetNode<ControlAnimationMultipleDirection>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ControlMultiple");
        GridContainerTile = GetNode<GridContainer>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerTile");
        ControlTile = GetNode<ControlKuroTile>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerTile/VBoxContainer/ControlTile");
        ControlListTexturesAnimated = GetNode<ControlListTextures>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerTile/VBoxContainer/ControlListTexturesAnimated");
        ContainerConfigBase = GetNode<VBoxContainer>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase");
        ButtonHideConfigBase = GetNode<KuroTextureButton>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase/HBoxContainer/ButtonHideConfigBase");
        AnimatedPanelContainerConfigBase = GetNode<AnimatedPanelContainer>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase/AnimatedPanelContainerConfigBase");
        SpinBoxOffsetX = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase/AnimatedPanelContainerConfigBase/GridContainer/SpinBoxOffsetX");
        SpinBoxOffsetY = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase/AnimatedPanelContainerConfigBase/GridContainer/SpinBoxOffsetY");
        SpinBoxScale = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase/AnimatedPanelContainerConfigBase/GridContainer/SpinBoxScale");
        SpinBoxDepht = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase/AnimatedPanelContainerConfigBase/GridContainer/SpinBoxDepht");
        ColorButtonBase = GetNode<ColorPickerButton>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/ContainerConfigBase/AnimatedPanelContainerConfigBase/GridContainer/ColorButtonBase");
        GridContainerBasicTransform = GetNode<GridContainer>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerBasicTransform");
        CheckBoxMirrorHorizontal = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerBasicTransform/CheckBoxMirrorHorizontal");
        CheckBoxMirrorVertical = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerBasicTransform/CheckBoxMirrorVertical");
        CheckBoxHasCollider = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerBasicTransform/CheckBoxHasCollider");
        GridContainerAnimated = GetNode<GridContainer>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerAnimated");
        SpinBoxFps = GetNode<SpinBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerAnimated/SpinBoxFps");
        CheckBoxLoop = GetNode<CheckBox>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/VBoxContainer2/GridContainerAnimated/CheckBoxLoop");
        ButtonSave = GetNode<KuroTextureButton>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/ButtonSave");
        ButtonSaveSimilar = GetNode<KuroTextureButton>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/ButtonSaveSimilar");
        ButtonDelete = GetNode<KuroTextureButton>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/VBoxContainer/HBoxContainer/ButtonDelete");
        HBoxContainer = GetNode<VBoxContainer>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/HBoxContainer");
        ControlSpriteEdit = GetNode<ControlSpriteEdit>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/HBoxContainer/VSplitContainer/ControlSpriteEdit");
        ControlCollider = GetNode<PanelColliders>("MarginContainer/PanelContainer/MarginContainer/HSplitContainer/HBoxContainer/VSplitContainer/ControlCollider");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}