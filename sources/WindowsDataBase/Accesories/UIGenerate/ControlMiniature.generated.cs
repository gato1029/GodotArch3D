// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlMiniature : PanelContainer
{
    private Button ButtonNew;
    private Button ButtonSelect;
    private TextureRect TextureRectImage;

    public  void InitializeUI()
    {
        ButtonNew = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonNew");
        ButtonSelect = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ButtonSelect");
        TextureRectImage = GetNode<TextureRect>("MarginContainer/VBoxContainer/TextureRectImage");
    }
}