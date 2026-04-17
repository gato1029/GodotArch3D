// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileTextureControl : Control
{
    public delegate void EventNotifyChangued(TileTextureControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TextureImage;

    public void InitializeUI()
    {
        TextureImage = GetNode<TextureRect>("PanelContainer/TextureImage");
    }
}