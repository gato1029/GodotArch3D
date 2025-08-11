using Godot;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.utils;
using System;

public partial class EditorWindow : PanelContainer
{
    private EditorMode selectedTabMode;
    private SelectionBlueprint blueprint;
    private int idTileSquare = 1;
    private int maxZoomGridValue = 10;    
    private int minZoomGridValue = 1;
    private int currentZoomGridValue = 1;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("llame padre");
        InitializeUI(); // Insertado por el generador de UI                 
        MapManagerEditor.Instance.editorMode = EditorMode.TERRENO;
        TabContainerItems.TabChanged += TabContainerItems_TabChanged;
        ButtonSaveAll.Pressed += ButtonSaveAll_Pressed;
        blueprint = SelectionBlueprint.Instance;
        blueprint.Configure(1, 30);
        blueprint.Create(new Vector2I(1, 1), new Vector2I(0, 0));
        SpinBoxGridX.ValueChanged += SpinBoxGrid_ValueChanged;
        SpinBoxGridY.ValueChanged += SpinBoxGrid_ValueChanged;

    }
    private void UpdateBlueprintPosition()
    {
        blueprint.Move((Vector2I)PositionsManager.Instance.positionMouseTileGlobal);
    }
    private void SpinBoxGrid_ValueChanged(double value)
    {
        Vector2I mouseTile = (Vector2I)PositionsManager.Instance.positionMouseTileGlobal;
        blueprint.Create(new Vector2I((int)SpinBoxGridX.Value, (int)SpinBoxGridY.Value), mouseTile);
    }

    private void ButtonSaveAll_Pressed()
    {
        MapManagerEditor.Instance.CurrentMapLevelData.SaveAll();
    }
    public override void _Input(Godot.InputEvent @event)
    {      
            HandleZoomInput(@event);
          //  UpdateBlueprintPosition();    
    }
    private void TabContainerItems_TabChanged(long tab)
    {
        switch (TabContainerItems.CurrentTab)
        {
            case 0:
                selectedTabMode = EditorMode.TERRENO;
                break;
            case 1:
                selectedTabMode = EditorMode.RECURSOS;
                break;
            case 2:
                break;
               
            default:
                break;
        }
        MapManagerEditor.Instance.editorMode = selectedTabMode;
    }

    private void HandleZoomInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (!Input.IsKeyPressed(Key.Alt))
                return;

            int zoomDelta = 0;

            if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
                zoomDelta = 1;
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
                zoomDelta = -1;

            if (zoomDelta != 0)
            {
                currentZoomGridValue = Mathf.Clamp(currentZoomGridValue + zoomDelta, minZoomGridValue, maxZoomGridValue);
                SpinBoxGridX.Value = currentZoomGridValue;
                SpinBoxGridY.Value = currentZoomGridValue;
            }
        }
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        
    }
}
