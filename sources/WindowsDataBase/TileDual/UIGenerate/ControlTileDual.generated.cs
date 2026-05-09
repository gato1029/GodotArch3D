// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class ControlTileDual : PanelContainer
{
    public delegate void EventNotifyChangued(ControlTileDual objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TileTexture;

    public void InitializeUI()
    {
        TileTexture = GetNode<TextureRect>("TileTexture");
    }
}