// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using System;

public partial class TileTextureConfigControl : Window
{
    public delegate void EventNotifyChangued(TileTextureConfigControl objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private PanelContainer Container;
    private KuroTextureButton KuroTextureButtonSearch;
    private KuroCheckButton KuroCheckButtonCollider;
    private ComboBox ComboBoxCollider;
    private ComboBox ComboBoxColliderTriangulos;
    private KuroTextureButton KuroTextureButtonSave;
    private KuroTextureButton KuroTextureButtonDelete;
    private SpinBox SpinBoxfps;
    private KuroScrollZoomView KuroScrollZoomView;
    private CenterContainer CenterContainerTile;
    private TextureRect TextureRectTile;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        Container = GetNode<PanelContainer>("Container");
        KuroTextureButtonSearch = GetNode<KuroTextureButton>("Container/MarginContainer/VBoxContainer/HBoxContainer/KuroTextureButtonSearch");
        KuroCheckButtonCollider = GetNode<KuroCheckButton>("Container/MarginContainer/VBoxContainer/HBoxContainer/KuroCheckButtonCollider");
        ComboBoxCollider = GetNode<ComboBox>("Container/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/ComboBoxCollider");
        ComboBoxColliderTriangulos = GetNode<ComboBox>("Container/MarginContainer/VBoxContainer/HBoxContainer/VBoxContainer/ComboBoxColliderTriangulos");
        KuroTextureButtonSave = GetNode<KuroTextureButton>("Container/MarginContainer/VBoxContainer/HBoxContainer/KuroTextureButtonSave");
        KuroTextureButtonDelete = GetNode<KuroTextureButton>("Container/MarginContainer/VBoxContainer/HBoxContainer/KuroTextureButtonDelete");
        SpinBoxfps = GetNode<SpinBox>("Container/MarginContainer/VBoxContainer/HBoxContainer2/SpinBoxfps");
        KuroScrollZoomView = GetNode<KuroScrollZoomView>("Container/MarginContainer/VBoxContainer/KuroScrollZoomView");
        CenterContainerTile = GetNode<CenterContainer>("Container/MarginContainer/VBoxContainer/KuroScrollZoomView/CenterContainerTile");
        TextureRectTile = GetNode<TextureRect>("Container/MarginContainer/VBoxContainer/KuroScrollZoomView/CenterContainerTile/TextureRectTile");
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}