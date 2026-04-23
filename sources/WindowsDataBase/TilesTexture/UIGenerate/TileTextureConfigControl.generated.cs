// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileTextureConfigControl : PanelContainer
{
    public delegate void EventNotifyChangued(TileTextureConfigControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private KuroTextureButton KuroTextureButtonSearch;
    private KuroCheckButton KuroCheckButtonCollider;
    private ComboBox ComboBoxCollider;
    private ComboBox ComboBoxColliderTriangulos;
    private KuroScrollZoomView KuroScrollZoomView;
    private CenterContainer CenterContainerTile;
    private TextureRect TextureRectTile;

    public void InitializeUI()
    {
        KuroTextureButtonSearch = GetNode<KuroTextureButton>("MarginContainer/VBoxContainer/HBoxContainer/KuroTextureButtonSearch");
        KuroCheckButtonCollider = GetNode<KuroCheckButton>("MarginContainer/VBoxContainer/HBoxContainer/KuroCheckButtonCollider");
        ComboBoxCollider = GetNode<ComboBox>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/ComboBoxCollider");
        ComboBoxColliderTriangulos = GetNode<ComboBox>("MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/ComboBoxColliderTriangulos");
        KuroScrollZoomView = GetNode<KuroScrollZoomView>("MarginContainer/VBoxContainer/KuroScrollZoomView");
        CenterContainerTile = GetNode<CenterContainer>("MarginContainer/VBoxContainer/KuroScrollZoomView/CenterContainerTile");
        TextureRectTile = GetNode<TextureRect>("MarginContainer/VBoxContainer/KuroScrollZoomView/CenterContainerTile/TextureRectTile");
    }
}