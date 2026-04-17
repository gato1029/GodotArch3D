using Godot;
using System;

public partial class TileRuleTextureControl : Control
{
    private string widgetPath = "res://sources/KuroTiles/ControlKuroTile.tscn";
    private PackedScene _widgetScene;

    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        // 1. Cargar la escena del widget
        _widgetScene = GD.Load<PackedScene>(widgetPath);
        // Hacer que el contenedor ocupe todo el espacio de este Control
        FixedGridTiles.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        // Ejemplo: Crear una rejilla inicial de 3x3
        SetupGrid(3, 3);
        SpinBoxX.ValueChanged += SpinBoxX_ValueChanged;
        SpinBoxY.ValueChanged += SpinBoxY_ValueChanged;
        
    }

    private void SpinBoxY_ValueChanged(double value)
    {
       SetupGrid((int)SpinBoxY.Value, (int)value);
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
        if (FixedGridTiles == null) return;

        // Actualizar propiedades del contenedor
        FixedGridTiles.Rows = rows;
        FixedGridTiles.Columns = columns;

        // Limpiar widgets anteriores si existen
        foreach (Node child in FixedGridTiles.GetChildren())
        {
            child.QueueFree();
        }

        // Crear nuevos widgets basándose en la cantidad total (filas * columnas)
        int totalWidgets = rows * columns;
        for (int i = 0; i < totalWidgets; i++)
        {
            Control instance = _widgetScene.Instantiate<Control>();
            FixedGridTiles.AddChild(instance);
        }

        // Forzar al contenedor a que se refresque inmediatamente
        FixedGridTiles.Columns = columns; // El setter ya llama a Refresh()
    }
}
