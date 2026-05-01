// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class EditorTerreno : CenterContainer
{
    public delegate void EventNotifyChangued(EditorTerreno objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroButton KuroButton;

    public void InitializeUI()
    {
        KuroButton = GetNode<KuroButton>("KuroButton");
    }
}