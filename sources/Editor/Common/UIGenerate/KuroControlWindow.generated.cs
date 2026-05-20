// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class KuroControlWindow : PanelContainer
{
    public delegate void EventNotifyChangued(KuroControlWindow objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect HeadTexture;
    private Label LabelNameWindow;
    private KuroButton ButtonClose;

    public void InitializeUI()
    {
        HeadTexture = GetNode<TextureRect>("MarginContainer/MarginContainer/MarginContainer/HeadTexture");
        LabelNameWindow = GetNode<Label>("MarginContainer/MarginContainer/MarginContainer/HeadTexture/LabelNameWindow");
        ButtonClose = GetNode<KuroButton>("MarginContainer/ButtonClose");
    }
}