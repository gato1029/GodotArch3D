// AUTO-GENERATED FILE. DO NOT EDIT.
using Godot;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;

public partial class WindowEditorRuntimeTerrain : Window
{
    public delegate void EventNotifyChangued(WindowEditorRuntimeTerrain objectControl);
    public event EventNotifyChangued OnNotifyChangued;

    private ControlBlackyAtlasTexture EditorTextura;

    public event Action<TileSelectionMatrixData, int> OnNotifySelectionMatrix;
    public event Action<List<int>> OnNotifyMultiSelectionIndex;

    public void InitializeUI()
    {
        CloseRequested += CloseRequestedWindow;
        EditorTextura = GetNode<ControlBlackyAtlasTexture>("PanelContainer/EditorTextura");
        EditorTextura.OnNotifyMultiSelectionIndex += EditorTextura_OnNotifyMultiSelectionIndex;
        EditorTextura.OnNotifySelectionMatrix += EditorTextura_OnNotifySelectionMatrix;
    }

    private void EditorTextura_OnNotifySelectionMatrix(TileSelectionMatrixData arg1, int arg2)
    {
        OnNotifySelectionMatrix?.Invoke(arg1, arg2);
    }

    private void EditorTextura_OnNotifyMultiSelectionIndex(List<int> obj)
    {
        OnNotifyMultiSelectionIndex?.Invoke(obj);
    }

    private void CloseRequestedWindow()
    {
        QueueFree();
    }

}