// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileTextureConfigControl : Window
{
    public delegate void EventNotifyChangued(TileTextureConfigControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

  
    private KuroTextureButton KuroTextureButtonSearch;
    private KuroCheckButton KuroCheckButtonCollider;
    private ComboBox ComboBoxCollider;
    private ComboBox ComboBoxColliderTriangulos;
    private SpinBox SpinBoxfps;
    private KuroScrollZoomView KuroScrollZoomView;
    private CenterContainer CenterContainerTile;
    private TextureRect TextureRectTile;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        
        KuroTextureButtonSearch = GetNode<KuroTextureButton>("TileTextureConfigControl/MarginContainer/VBoxContainer/HBoxContainer/KuroTextureButtonSearch");
        KuroCheckButtonCollider = GetNode<KuroCheckButton>("TileTextureConfigControl/MarginContainer/VBoxContainer/HBoxContainer/KuroCheckButtonCollider");
        ComboBoxCollider = GetNode<ComboBox>("TileTextureConfigControl/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/ComboBoxCollider");
        ComboBoxColliderTriangulos = GetNode<ComboBox>("TileTextureConfigControl/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/ComboBoxColliderTriangulos");
        SpinBoxfps = GetNode<SpinBox>("TileTextureConfigControl/MarginContainer/VBoxContainer/HBoxContainer2/SpinBoxfps");
        KuroScrollZoomView = GetNode<KuroScrollZoomView>("TileTextureConfigControl/MarginContainer/VBoxContainer/KuroScrollZoomView");
        CenterContainerTile = GetNode<CenterContainer>("TileTextureConfigControl/MarginContainer/VBoxContainer/KuroScrollZoomView/CenterContainerTile");
        TextureRectTile = GetNode<TextureRect>("TileTextureConfigControl/MarginContainer/VBoxContainer/KuroScrollZoomView/CenterContainerTile/TextureRectTile");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}