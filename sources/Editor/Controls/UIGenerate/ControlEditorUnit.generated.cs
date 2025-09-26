// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlEditorUnit : MarginContainer
{
    public delegate void EventNotifyChangued(ControlEditorUnit objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer PanelContainerFocus;
    private VBoxContainer Disenio;
    private TextureRect TextureRectImage;
    private Button ButtonRefresh;
    private SpinBox SpinBoxTotalUnits;
    private ItemList ItemListData;
    private MarginContainer Detalle_Unidades;
    private VBoxContainer ContainerLayers;

    public void InitializeUI()
    {
        PanelContainerFocus = GetNode<PanelContainer>("PanelContainerFocus");
        Disenio = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/Disenio");
        TextureRectImage = GetNode<TextureRect>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/VBoxContainer2/TextureRectImage");
        ButtonRefresh = GetNode<Button>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/HBoxContainer2/ButtonRefresh");
        SpinBoxTotalUnits = GetNode<SpinBox>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/HBoxContainer3/SpinBoxTotalUnits");
        ItemListData = GetNode<ItemList>("PanelContainerFocus/MarginContainer/TabContainer/Disenio/ItemListData");
        Detalle_Unidades = GetNode<MarginContainer>("PanelContainerFocus/MarginContainer/TabContainer/Detalle Unidades");
        ContainerLayers = GetNode<VBoxContainer>("PanelContainerFocus/MarginContainer/TabContainer/Detalle Unidades/ScrollContainer/ContainerLayers");
    }
}