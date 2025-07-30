// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTerrainDetail : Window
{
    public delegate void EventNotifyChangued(WindowTerrainDetail objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TabContainer TabContainerBase;
    private TabBar Informacion_Basica;
    private LineEdit LineEditName;
    private OptionButton OptionButtonCategory;
    private OptionButton OptionButtonType;
    private CheckBox CheckBoxHasAnimation;
    private CheckBox CheckBoxHasRule;
    private ControlSprite ControlSprite;
    private Button ButtonSave;
    private Button ButtonSaveActive;
    private Button ButtonDuplicate;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TabContainerBase = GetNode<TabContainer>("MarginContainer/TabContainerBase");
        Informacion_Basica = GetNode<TabBar>("MarginContainer/TabContainerBase/Informacion Basica");
        LineEditName = GetNode<LineEdit>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/VBoxContainer/GridContainer/LineEditName");
        OptionButtonCategory = GetNode<OptionButton>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/VBoxContainer/GridContainer/OptionButtonCategory");
        OptionButtonCategory.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        OptionButtonType = GetNode<OptionButton>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/VBoxContainer/GridContainer/OptionButtonType");
        OptionButtonType.GetPopup().AlwaysOnTop = GetWindow().AlwaysOnTop;
        CheckBoxHasAnimation = GetNode<CheckBox>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/VBoxContainer/GridContainer/CheckBoxHasAnimation");
        CheckBoxHasAnimation.Pressed += CheckBoxHasAnimation_PressedUI;
        CheckBoxHasAnimation_PressedUI();
        CheckBoxHasRule = GetNode<CheckBox>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/VBoxContainer/GridContainer/CheckBoxHasRule");
        CheckBoxHasRule.Pressed += CheckBoxHasRule_PressedUI;
        CheckBoxHasRule_PressedUI();
        ControlSprite = GetNode<ControlSprite>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/ControlSprite");
        ButtonSave = GetNode<Button>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/HBoxContainer/ButtonSave");
        ButtonSaveActive = GetNode<Button>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/HBoxContainer/ButtonSaveActive");
        ButtonDuplicate = GetNode<Button>("MarginContainer/TabContainerBase/Informacion Basica/MarginContainer/ScrollContainer/VBoxContainer/HBoxContainer/ButtonDuplicate");
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

    private void CheckBoxHasRule_PressedUI()
    {
        if (CheckBoxHasRule.ButtonPressed)
            CheckBoxHasRule.Text = "";
        else
            CheckBoxHasRule.Text = "No ";
    }
}