// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowAccessory : Window
{
    public delegate void EventNotifyChangued(WindowAccessory objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private OptionButton OptionButtonClass;
    private Label Label2;
    private OptionButton OptionButtonType;
    private Label Label3;
    private OptionButton OptionButtonBody;
    private Button ButtonNew;
    private ItemList ItemListView;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        OptionButtonClass = GetNode<OptionButton>("Panel/MarginContainer/VBoxContainer/GridContainer/OptionButtonClass");
        OptionButtonClass.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label2 = GetNode<Label>("Panel/MarginContainer/VBoxContainer/GridContainer/Label2");
        OptionButtonType = GetNode<OptionButton>("Panel/MarginContainer/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        Label3 = GetNode<Label>("Panel/MarginContainer/VBoxContainer/GridContainer/Label3");
        OptionButtonBody = GetNode<OptionButton>("Panel/MarginContainer/VBoxContainer/GridContainer/OptionButtonBody");
        OptionButtonBody.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        ButtonNew = GetNode<Button>("Panel/MarginContainer/VBoxContainer/ButtonNew");
        ItemListView = GetNode<ItemList>("Panel/MarginContainer/VBoxContainer/ScrollContainer/ItemListView");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}