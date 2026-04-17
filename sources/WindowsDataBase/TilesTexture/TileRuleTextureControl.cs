using Godot;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;

public partial class TileRuleTextureControl : Control
{
    private string widgetPath = "res://sources/WindowsDataBase/TilesTexture/TileTextureControl.tscn";
    private PackedScene _widgetScene;

    private string widgetRulePath = "res://sources/WindowsDataBase/TilesTexture/TileTextureRuleControl.tscn";
    private PackedScene _widgetRuleScene;

    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        // 1. Cargar la escena del widget
        _widgetScene = GD.Load<PackedScene>(widgetPath);
        _widgetRuleScene = GD.Load<PackedScene>(widgetRulePath);

        // Hacer que el contenedor ocupe todo el espacio de este Control
        FixedGridTiles.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        FixedGridRules.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        // Ejemplo: Crear una rejilla inicial de 3x3
        SetupGrid(3, 3);
        SpinBoxX.ValueChanged += SpinBoxX_ValueChanged;
        SpinBoxY.ValueChanged += SpinBoxY_ValueChanged;

        KuroCheckButtonSwitch.Pressed += KuroCheckButtonSwitch_Pressed;
        KuroCheckButtonSwitch_Pressed();
    }

    private void KuroCheckButtonSwitch_Pressed()
    {
        bool isOn = KuroCheckButtonSwitch.ButtonPressed;

        FixedGridTiles.Visible = isOn;
        FixedGridRules.Visible = !isOn;
    }

    private void SpinBoxY_ValueChanged(double value)
    {
       SetupGrid((int)SpinBoxX.Value, (int)value);
    }

    private void SpinBoxX_ValueChanged(double value)
    {
        SetupGrid((int)value, (int)SpinBoxY.Value);
    }

    /// <summary>
    /// Configura las dimensiones del grid y genera los widgets necesarios.
    /// </summary>
    public void SetupGrid(int rows, int columns)
    {
        if (FixedGridTiles == null || FixedGridRules == null) return;

        // 🔹 Configurar ambos grids
        FixedGridTiles.Rows = rows;
        FixedGridTiles.Columns = columns;

        FixedGridRules.Rows = rows;
        FixedGridRules.Columns = columns;

        // 🔹 Limpiar ambos
        foreach (Node child in FixedGridTiles.GetChildren())
            child.QueueFree();

        foreach (Node child in FixedGridRules.GetChildren())
            child.QueueFree();

        int totalWidgets = rows * columns;

        // 🔹 Crear tiles
        for (int i = 0; i < totalWidgets; i++)
        {
            Control instance = _widgetScene.Instantiate<Control>();
            FixedGridTiles.AddChild(instance);
        }

        // 🔹 Crear rules
        for (int i = 0; i < totalWidgets; i++)
        {
            TileTextureRuleControl instance = _widgetRuleScene.Instantiate<TileTextureRuleControl>();            
            FixedGridRules.AddChild(instance);
            instance.SetData(NeighborCondition.Ignore);
        }

        // 🔹 Forzar refresh
        FixedGridTiles.QueueSort();
        FixedGridRules.QueueSort();
    }
}
