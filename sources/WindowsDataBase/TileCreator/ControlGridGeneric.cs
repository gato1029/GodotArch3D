using Godot;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

[Tool]
public partial class ControlGridGeneric : MarginContainer
{
    [Export] public Vector2I gridSize = new Vector2I(10, 10); // tamaño en celdas del grid
    [Export] public int baseScale = 2;
   
    [Export] public int cellSize = 16;

    private int cellInternal;



    public Vector2 sizeReal;
    private ShaderMaterial _shaderMaterial;



    private Vector2I _startCell;
    private Vector2I _endCell;




    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        ShaderMaterial tempo = GD.Load<ShaderMaterial>("res://resources/Material/MaterialGridCanvas.tres");
        _shaderMaterial = (ShaderMaterial)tempo.Duplicate();
        GridBase.Material = _shaderMaterial;

      
        cellInternal = cellSize * baseScale;
        _shaderMaterial.SetShaderParameter("grid_size", cellInternal);

        SpinBoxCellSize.ValueChanged += SpinBoxCellSize_ValueChanged;
       

        this.MouseFilter = Control.MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.Click;
        GridBase.FocusMode = FocusModeEnum.Click;
        //GridBase.GuiInput += OnGridGuiInput;

        SpinBoxGridSizeX.ValueChanged += SpinBoxGridSizeX_ValueChanged;
        SpinBoxGridSizeY.ValueChanged += SpinBoxGridSizeX_ValueChanged;

        SetGridSize(gridSize);
    }
    private void SpinBoxGridSizeX_ValueChanged(double value)
    {
        SetGridSize(new Vector2I((int)SpinBoxGridSizeX.Value, (int)SpinBoxGridSizeY.Value));
    }
    private void SpinBoxCellSize_ValueChanged(double value)
    {
        cellSize = (int)value;
        cellInternal = cellSize * baseScale;
        _shaderMaterial.SetShaderParameter("grid_size", cellInternal);
    }
    public void SetScale(int scale)
    {
        baseScale = scale;
        cellInternal = cellSize * baseScale;
   


        // El grid tiene un tamaño definido por gridSize
        Vector2 gridPixelSize = new Vector2(gridSize.X * cellInternal, gridSize.Y * cellInternal);

        GridBase.CustomMinimumSize = gridPixelSize;
        GridBase.Size = gridPixelSize;

       

        // Centrar ambos
        CenterContainerBase.CustomMinimumSize = gridPixelSize;
        CenterContainerBase.Size = gridPixelSize;

        _shaderMaterial.SetShaderParameter("size", gridPixelSize);
        _shaderMaterial.SetShaderParameter("grid_size", cellInternal);
   
    }
    private void Zoom(int dir)
    {
        if (dir == 0)
            SetScale(Mathf.Min(6, baseScale + 1));
        else
            SetScale(Mathf.Max(1, baseScale - 1));
    }

    public void SetGridSize(Vector2I newGridSize)
    {
        gridSize = newGridSize;

        // Calcular nuevo tamaño en píxeles
        Vector2 gridPixelSize = new Vector2(gridSize.X * cellInternal, gridSize.Y * cellInternal);

        // Ajustar dimensiones visuales
        GridBase.CustomMinimumSize = gridPixelSize;
        GridBase.Size = gridPixelSize;

       

        CenterContainerBase.CustomMinimumSize = gridPixelSize;
        CenterContainerBase.Size = gridPixelSize;

        // Actualizar el shader
        _shaderMaterial?.SetShaderParameter("size", gridPixelSize);
        _shaderMaterial.SetShaderParameter("grid_size", cellInternal);
    }

}
