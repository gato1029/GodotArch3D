// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlEditorTerrain : MarginContainer
{
    public delegate void EventNotifyChangued(ControlEditorTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerFocus;
    private VBoxContainer Diseño;
    private SpinBox SpinBoxGridX;
    private SpinBox SpinBoxGridY;
    private TextureRect TextureRectImage;
    private Button ButtonSearch;
    private SpinBox SpinBoxChunkX;
    private SpinBox SpinBoxChunkY;
    private Button ButtonAutomaticTerrain;
    private Button ButtonRefresh;
    private OptionButton OptionButtonLayer;
    private ItemList ItemListRules;
    private GridContainer Capas;
    private CheckBox CheckBoxBasic;
    private CheckBox CheckBoxFloor;
    private CheckBox CheckBoxPath;
    private CheckBox CheckBoxWater;
    private CheckBox CheckBoxOrnament;

    public void InitializeUI()
    {
        PanelContainerFocus = GetNode<PanelContainer>("PanelContainerFocus");
        Diseño = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/Diseño");
        SpinBoxGridX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/GridContainer/SpinBoxGridX");
        SpinBoxGridY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/GridContainer/SpinBoxGridY");
        TextureRectImage = GetNode<TextureRect>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/VBoxContainer/TextureRectImage");
        ButtonSearch = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer/VBoxContainer/ButtonSearch");
        SpinBoxChunkX = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkX");
        SpinBoxChunkY = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/GridContainer/SpinBoxChunkY");
        ButtonAutomaticTerrain = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/VBoxContainer/ButtonAutomaticTerrain");
        ButtonRefresh = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer2/ButtonRefresh");
        OptionButtonLayer = GetNode<OptionButton>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/HBoxContainer3/OptionButtonLayer");
        OptionButtonLayer.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        ItemListRules = GetNode<ItemList>("PanelContainerFocus/MarginContainer/TabContainer/Diseño/ItemListRules");
        Capas = GetNode<GridContainer>("PanelContainerFocus/MarginContainer/TabContainer/Capas");
        CheckBoxBasic = GetNode<CheckBox>("PanelContainerFocus/MarginContainer/TabContainer/Capas/CheckBoxBasic");
        CheckBoxBasic.Pressed += CheckBoxBasic_PressedUI;
        CheckBoxBasic_PressedUI();
        CheckBoxFloor = GetNode<CheckBox>("PanelContainerFocus/MarginContainer/TabContainer/Capas/CheckBoxFloor");
        CheckBoxFloor.Pressed += CheckBoxFloor_PressedUI;
        CheckBoxFloor_PressedUI();
        CheckBoxPath = GetNode<CheckBox>("PanelContainerFocus/MarginContainer/TabContainer/Capas/CheckBoxPath");
        CheckBoxPath.Pressed += CheckBoxPath_PressedUI;
        CheckBoxPath_PressedUI();
        CheckBoxWater = GetNode<CheckBox>("PanelContainerFocus/MarginContainer/TabContainer/Capas/CheckBoxWater");
        CheckBoxWater.Pressed += CheckBoxWater_PressedUI;
        CheckBoxWater_PressedUI();
        CheckBoxOrnament = GetNode<CheckBox>("PanelContainerFocus/MarginContainer/TabContainer/Capas/CheckBoxOrnament");
        CheckBoxOrnament.Pressed += CheckBoxOrnament_PressedUI;
        CheckBoxOrnament_PressedUI();
    }

    private void CheckBoxBasic_PressedUI()
    {
        if (CheckBoxBasic.ButtonPressed)
            CheckBoxBasic.Text = "";
        else
            CheckBoxBasic.Text = "No ";
    }

    private void CheckBoxFloor_PressedUI()
    {
        if (CheckBoxFloor.ButtonPressed)
            CheckBoxFloor.Text = "";
        else
            CheckBoxFloor.Text = "No ";
    }

    private void CheckBoxPath_PressedUI()
    {
        if (CheckBoxPath.ButtonPressed)
            CheckBoxPath.Text = "";
        else
            CheckBoxPath.Text = "No ";
    }

    private void CheckBoxWater_PressedUI()
    {
        if (CheckBoxWater.ButtonPressed)
            CheckBoxWater.Text = "";
        else
            CheckBoxWater.Text = "No ";
    }

    private void CheckBoxOrnament_PressedUI()
    {
        if (CheckBoxOrnament.ButtonPressed)
            CheckBoxOrnament.Text = "";
        else
            CheckBoxOrnament.Text = "No ";
    }
}