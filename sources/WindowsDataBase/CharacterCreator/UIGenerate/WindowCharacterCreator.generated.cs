// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowCharacterCreator : Window
{
    public delegate void EventNotifyChangued(WindowCharacterCreator objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private VBoxContainer VBoxContainerBase;
    private Button ButtonSearch;
    private Sprite2D Sprite2DView;
    private VBoxContainer VBoxContainerDown;
    private LineEdit LineEditName;
    private Label Label2;
    private SpinBox SpinBoxScale;
    private Label Label3;
    private ColorPickerButton ColorBase;
    private Label Label4;
    private TextEdit TextEditDescription;
    private Button ButtonSave;
    private BonusContainer PanelBonificaciones;
    private StatsContainer PanelEstadisticas;
    private ElementsContainer PanelAtaque;
    private VBoxContainer VBoxContainer2;
    private ElementsContainer PanelDefensa;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        VBoxContainerBase = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase");
        ButtonSearch = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainer/ButtonSearch");
        Sprite2DView = GetNode<Sprite2D>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainer/CenterContainer/Panel/Control/Sprite2DView");
        VBoxContainerDown = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown");
        LineEditName = GetNode<LineEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/LineEditName");
        Label2 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label2");
        SpinBoxScale = GetNode<SpinBox>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/SpinBoxScale");
        Label3 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label3");
        ColorBase = GetNode<ColorPickerButton>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/ColorBase");
        Label4 = GetNode<Label>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/Label4");
        TextEditDescription = GetNode<TextEdit>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/GridContainer/TextEditDescription");
        ButtonSave = GetNode<Button>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainerBase/VBoxContainerDown/ButtonSave");
        PanelBonificaciones = GetNode<BonusContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/PanelBonificaciones");
        PanelEstadisticas = GetNode<StatsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/PanelEstadisticas");
        PanelAtaque = GetNode<ElementsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer/PanelAtaque");
        VBoxContainer2 = GetNode<VBoxContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer2");
        PanelDefensa = GetNode<ElementsContainer>("PanelContainer/MarginContainer/VBoxContainer/HBoxContainer/GridContainer/VBoxContainer2/PanelDefensa");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}