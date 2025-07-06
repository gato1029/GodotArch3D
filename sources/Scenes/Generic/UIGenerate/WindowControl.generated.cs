// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class WindowControl : VBoxContainer
{
    public delegate void EventNotifyChangued(WindowControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TitleBar;
    private Label LabelTitle;
    private Button ButtonMax;
    private Button ButtonMin;
    private Button ButtonClose;
    private TextureRect Background;

    public void InitializeUI()
    {
        TitleBar = GetNode<TextureRect>("TitleBar");
        LabelTitle = GetNode<Label>("TitleBar/HBoxContainer2/LabelTitle");
        ButtonMax = GetNode<Button>("TitleBar/HBoxContainer2/HBoxContainer/ButtonMax");
        ButtonMin = GetNode<Button>("TitleBar/HBoxContainer2/HBoxContainer/ButtonMin");
        ButtonClose = GetNode<Button>("TitleBar/HBoxContainer2/HBoxContainer/ButtonClose");
        Background = GetNode<TextureRect>("MarginContainer/Background");
    }
}