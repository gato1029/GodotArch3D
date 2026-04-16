// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowTerrainTransition : Window
{
    public delegate void EventNotifyChangued(WindowTerrainTransition objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextEdit TextEditName;
    private SpinBox SpinBoxAncho;
    private ControlTerrain ControlTerrainOrigin;
    private ControlTerrain ControlTerrainDestin;
    private ControlTerrain ControlTerrainResult;
    private KuroTextureButton ButtonSave;
    private KuroTextureButton ButtonDelete;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TextEditName = GetNode<TextEdit>("Panel/MarginContainer/VBoxContainer/HBoxContainer4/TextEditName");
        SpinBoxAncho = GetNode<SpinBox>("Panel/MarginContainer/VBoxContainer/HBoxContainer5/SpinBoxAncho");
        ControlTerrainOrigin = GetNode<ControlTerrain>("Panel/MarginContainer/VBoxContainer/HBoxContainer/ControlTerrainOrigin");
        ControlTerrainDestin = GetNode<ControlTerrain>("Panel/MarginContainer/VBoxContainer/HBoxContainer/ControlTerrainDestin");
        ControlTerrainResult = GetNode<ControlTerrain>("Panel/MarginContainer/VBoxContainer/HBoxContainer2/ControlTerrainResult");
        ButtonSave = GetNode<KuroTextureButton>("Panel/MarginContainer/VBoxContainer/HBoxContainer3/ButtonSave");
        ButtonDelete = GetNode<KuroTextureButton>("Panel/MarginContainer/VBoxContainer/HBoxContainer3/ButtonDelete");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}