// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowKuroRuleItem : Popup
{
    public delegate void EventNotifyChangued(WindowKuroRuleItem objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ControlListTileSprite ControlListTileSprite;
    private OptionButton OptionButtonUnderRender;
    private CheckBox CheckBoxAllGroup;
    private ItemList ItemsGrupos;
    private ControlKuroRuleItem ControlRuleKuroItem;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        ControlListTileSprite = GetNode<ControlListTileSprite>("MarginContainer/VBoxContainer/PanelContainer/HBoxContainer2/ControlListTileSprite");
        OptionButtonUnderRender = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer/PanelContainer/VBoxContainer/HBoxContainer/OptionButtonUnderRender");
        OptionButtonUnderRender.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        CheckBoxAllGroup = GetNode<CheckBox>("MarginContainer/VBoxContainer/HBoxContainer/PanelContainer/VBoxContainer/CheckBoxAllGroup");
        ItemsGrupos = GetNode<ItemList>("MarginContainer/VBoxContainer/HBoxContainer/PanelContainer/VBoxContainer/ItemsGrupos");
        ControlRuleKuroItem = GetNode<ControlKuroRuleItem>("MarginContainer/VBoxContainer/HBoxContainer/PanelContainer2/ControlRuleKuroItem");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}