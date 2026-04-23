// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileTextureConfigControl : PanelContainer
{
    public delegate void EventNotifyChangued(TileTextureConfigControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ComboBox ComboBoxCollider;
    private KuroScrollZoomView KuroScrollZoomView;
    private TextureRect TextureRectTile;

    public void InitializeUI()
    {
        ComboBoxCollider = GetNode<ComboBox>("MarginContainer/VBoxContainer/ComboBoxCollider");
        KuroScrollZoomView = GetNode<KuroScrollZoomView>("MarginContainer/VBoxContainer/KuroScrollZoomView");
        TextureRectTile = GetNode<TextureRect>("MarginContainer/VBoxContainer/KuroScrollZoomView/CenterContainer/TextureRectTile");
    }
}