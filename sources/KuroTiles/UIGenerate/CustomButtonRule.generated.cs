// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class CustomButtonRule : Panel
{
    public delegate void EventNotifyChangued(CustomButtonRule objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private TextureRect TextureImage;
    private TextureRect TextureIcon;

    public void InitializeUI()
    {
        TextureImage = GetNode<TextureRect>("TextureImage");
        TextureIcon = GetNode<TextureRect>("TextureIcon");
    }
}