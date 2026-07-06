// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileSpritePreview : TextureRect
{
    public delegate void EventNotifyChangued(TileSpritePreview objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect SpriteTexture;

    public void InitializeUI()
    {
        SpriteTexture = GetNode<TextureRect>("SpriteTexture");
    }
}