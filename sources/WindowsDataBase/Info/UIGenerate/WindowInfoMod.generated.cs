// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowInfoMod : Window
{
    public delegate void EventNotifyChangued(WindowInfoMod objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextEdit TextEditName;
    private TextEdit TextEditDescription;
    private TextEdit TextEditVersion;
    private TextEdit TextEditAutor;
    private KuroTextureButton ButtonSave;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        TextEditName = GetNode<TextEdit>("PanelContainer/MarginContainer/GridContainer/TextEditName");
        TextEditDescription = GetNode<TextEdit>("PanelContainer/MarginContainer/GridContainer/TextEditDescription");
        TextEditVersion = GetNode<TextEdit>("PanelContainer/MarginContainer/GridContainer/TextEditVersion");
        TextEditAutor = GetNode<TextEdit>("PanelContainer/MarginContainer/GridContainer/TextEditAutor");
        ButtonSave = GetNode<KuroTextureButton>("PanelContainer/MarginContainer/HBoxContainer/ButtonSave");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}