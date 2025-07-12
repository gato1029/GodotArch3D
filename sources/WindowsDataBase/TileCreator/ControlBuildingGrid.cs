using Godot;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using System;
using System.Collections.Generic;

public partial class ControlBuildingGrid : MarginContainer
{
    [Export] public Vector2I gridSize = new Vector2I(10, 10); // tamaño en celdas del grid
    [Export] public int baseScale = 2;
    [Export] public Texture2D baseTexture;
    [Export] public int cellSize = 16;

    private int cellInternal;
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private bool _isSelecting = false;

    public Vector2 sizeReal;
    private ShaderMaterial _shaderMaterial;

    public delegate void EventNotifySelection(float x, float y, float width, float height);
    public event EventNotifySelection OnNotifySelection;

    private Vector2I _startCell;
    private Vector2I _endCell;

    public List<BuildingTilePosition> SelectedBuildings { get; private set; } = new();
    private HashSet<Vector2I> _selectedSet = new();

    private Control TileOverlayContainer;
    private ColorRect DeselectionOverlay;
    private Vector2I? _centerCell = null;
    private ColorRect CenterOverlay;
    public override void _Ready()
    {
        InitializeUI();
        ShaderMaterial tempo = GD.Load<ShaderMaterial>("res://resources/Material/MaterialGridCanvas.tres");
        _shaderMaterial = (ShaderMaterial)tempo.Duplicate();
        GridBase.Material = _shaderMaterial;
        Selection.Modulate = new Color(1, 1, 1, 0.5f);
        Selection.Visible = false;

        SpinBoxCellSize.ValueChanged += SpinBoxCellSize_ValueChanged;
        // Crear contenedor para overlays sobre el grid
        TileOverlayContainer = new Control
        {
            Name = "TileOverlayContainer",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = GridBase.Size
        };
        GridBase.AddChild(TileOverlayContainer);

        DeselectionOverlay = new ColorRect
        {
            Color = new Color(1, 0, 0, 0.4f), // rojo transparente
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        GridBase.AddChild(DeselectionOverlay);

        CenterOverlay = new ColorRect
        {
            Color = new Color(1, 1, 0, 0.6f), // amarillo
            Size = new Vector2(cellInternal, cellInternal),
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        GridBase.AddChild(CenterOverlay);

        if (baseTexture != null)
        {
            SetTexture(baseTexture);
        }
        this.MouseFilter = Control.MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.Click;
        GridBase.FocusMode = FocusModeEnum.Click;
        GridBase.GuiInput += OnGridGuiInput;

        SpinBoxGridSizeX.ValueChanged += SpinBoxGridSizeX_ValueChanged;
        SpinBoxGridSizeY.ValueChanged+= SpinBoxGridSizeX_ValueChanged;
    }
    public void SetCenter(Vector2I cell)
    {
        _centerCell = cell;
        CenterOverlay.Position = cell * cellInternal;
        CenterOverlay.Size = new Vector2(cellInternal, cellInternal);
        CenterOverlay.Visible = true;
    }
    public BuildingPosition GetBuildingPosition()
    {
        var buildingPosition = new BuildingPosition
        {
            buildingTilePositions = SelectedBuildings,
            sizeGridX = (int)SpinBoxGridSizeX.Value,
            sizeGridY = (int)SpinBoxGridSizeY.Value,
            centerX = _centerCell?.X ?? 0,
            centerY = _centerCell?.Y ?? 0
        };
        return buildingPosition;
    }
    public void SetBuildingPosition(BuildingPosition buildingPosition)
    {
        if (buildingPosition != null)
        {
            ClearSelection();
            SpinBoxGridSizeX.Value = buildingPosition.sizeGridX;
            SpinBoxGridSizeY.Value = buildingPosition.sizeGridY;
            SetGridSize(new Vector2I(buildingPosition.sizeGridX, buildingPosition.sizeGridY));

            foreach (var tile in buildingPosition.buildingTilePositions)
            {
                var pos = new Vector2I(tile.X, tile.Y);
                if (_selectedSet.Add(pos))
                {
                    SelectedBuildings.Add(new BuildingTilePosition(pos.X, pos.Y));
                    ShowTileOverlay(pos);
                }
            }

            SetCenter(new Vector2I(buildingPosition.centerX, buildingPosition.centerY));
        }
    }
    private void SpinBoxGridSizeX_ValueChanged(double value)
    {
        SetGridSize(new Vector2I((int)SpinBoxGridSizeX.Value, (int)SpinBoxGridSizeY.Value));
    }

    public void SetGridSize(Vector2I newGridSize)
    {
        gridSize = newGridSize;

        // Calcular nuevo tamaño en píxeles
        Vector2 gridPixelSize = new Vector2(gridSize.X * cellInternal, gridSize.Y * cellInternal);

        // Ajustar dimensiones visuales
        GridBase.CustomMinimumSize = gridPixelSize;
        GridBase.Size = gridPixelSize;

        TileOverlayContainer.Size = gridPixelSize;

        CenterContainerBase.CustomMinimumSize = gridPixelSize;
        CenterContainerBase.Size = gridPixelSize;

        // Actualizar el shader
        _shaderMaterial?.SetShaderParameter("size", gridPixelSize);

        // Ocultar selección y overlay si estaban activos fuera del nuevo rango
        Selection.Visible = false;
        DeselectionOverlay.Visible = false;

        // Limpiar selección existente que pueda estar fuera del nuevo grid
        ClearSelection();
    }
    private void OnGridGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            GrabFocus(); // Esto asegura que el ControlBuildingGrid gane el foco
        }
    }

    private void SpinBoxCellSize_ValueChanged(double value)
    {
        cellSize = (int)value;
        cellInternal = cellSize * baseScale;
        _shaderMaterial.SetShaderParameter("grid_size", cellInternal);
    }
    public override void _Input(InputEvent @event)
    {
        if (!GetGlobalRect().HasPoint(GetGlobalMousePosition()))
            return;

        if (GetViewport().GuiGetFocusOwner() != null &&
            GetViewport().GuiGetFocusOwner() != this &&
            !IsAncestorOf(GetViewport().GuiGetFocusOwner()))
            return;

        Vector2 localMouse = GetMouseLocalToGrid();

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (Input.IsKeyPressed(Key.Ctrl))
            {
                if (mouseEvent.ButtonIndex == MouseButton.WheelUp) { Zoom(0); return; }
                if (mouseEvent.ButtonIndex == MouseButton.WheelDown) { Zoom(1); return; }
            }

            if (mouseEvent.ButtonIndex == MouseButton.Middle && IsMouseInsideGrid(localMouse))
            {
                Vector2I cell = (Vector2I)(SnapToGrid(ClampToGridRect(localMouse)) / cellInternal);
                SetCenter(cell);
            }

            // Mouse Pressed
            if (mouseEvent.Pressed)
            {                
                if (mouseEvent.ButtonIndex == MouseButton.Left || mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    if (IsMouseInsideGrid(localMouse))
                    {
                        _startPosition = SnapToGrid(ClampToGridRect(localMouse));
                        _startCell = (Vector2I)(_startPosition / cellInternal);
                        _endPosition = _startPosition;
                        _endCell = _startCell;

                        _isSelecting = true;

                        if (mouseEvent.ButtonIndex == MouseButton.Left)
                        {
                            Selection.Size = Vector2.Zero;
                            Selection.Position = _startPosition;
                            Selection.Visible = true;
                            UpdateSelectionFromCells();
                        }
                        else if (mouseEvent.ButtonIndex == MouseButton.Right)
                        {
                            DeselectionOverlay.Visible = true;
                            UpdateDeselectionOverlay();
                        }
                    }
                }
            }
            else if (_isSelecting)
            {
                _isSelecting = false;

                if (IsMouseInsideGrid(localMouse))
                {
                    _endPosition = SnapToGrid(ClampToGridRect(localMouse));
                    _endCell = (Vector2I)(_endPosition / cellInternal);

                    if (mouseEvent.ButtonIndex == MouseButton.Left)
                    {
                        UpdateSelectionFromCells();

                        if (_startPosition == _endPosition)
                        {
                            Selection.Position = _startPosition;
                            Selection.Size = new Vector2(cellInternal, cellInternal);
                            Selection.Visible = true;

                            OnNotifySelection?.Invoke(
                                _startPosition.X / baseScale,
                                _startPosition.Y / baseScale,
                                cellInternal / baseScale,
                                cellInternal / baseScale
                            );

                            AddBuilding(_startCell);
                        }
                        else
                        {
                            ProcessSelection();
                        }
                        // Ocultar el rectángulo de selección después de marcar
                        Selection.Visible = false;
                    }
                    else if (mouseEvent.ButtonIndex == MouseButton.Right)
                    {
                        ProcessDeselection();
                        DeselectionOverlay.Visible = false;
                    }
                }
                else
                {
                    Selection.Visible = false;
                    DeselectionOverlay.Visible = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion && _isSelecting)
        {
            if (IsMouseInsideGrid(localMouse))
            {
                _endPosition = SnapToGrid(ClampToGridRect(localMouse));
                _endCell = (Vector2I)(_endPosition / cellInternal);

                if (Input.IsMouseButtonPressed(MouseButton.Left))
                {
                    UpdateSelectionFromCells();
                }
                else if (Input.IsMouseButtonPressed(MouseButton.Right))
                {
                    UpdateDeselectionOverlay();
                }
            }
        }
    }

    private void UpdateDeselectionOverlay()
    {
        int minX = Mathf.Min(_startCell.X, _endCell.X);
        int minY = Mathf.Min(_startCell.Y, _endCell.Y);
        int maxX = Mathf.Max(_startCell.X, _endCell.X);
        int maxY = Mathf.Max(_startCell.Y, _endCell.Y);

        DeselectionOverlay.Position = new Vector2(minX, minY) * cellInternal;
        DeselectionOverlay.Size = new Vector2((maxX - minX + 1), (maxY - minY + 1)) * cellInternal;
        DeselectionOverlay.Visible = true;
    }

    private void ProcessDeselection()
    {
        int minX = Mathf.Min(_startCell.X, _endCell.X);
        int minY = Mathf.Min(_startCell.Y, _endCell.Y);
        int maxX = Mathf.Max(_startCell.X, _endCell.X);
        int maxY = Mathf.Max(_startCell.Y, _endCell.Y);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                RemoveBuilding(new Vector2I(x, y));
            }
        }

        OnNotifySelection?.Invoke(
            minX * cellInternal / baseScale,
            minY * cellInternal / baseScale,
            (maxX - minX + 1) * cellInternal / baseScale,
            (maxY - minY + 1) * cellInternal / baseScale
        );
    }
    private void RemoveBuilding(Vector2I tile)
    {
        if (_selectedSet.Remove(tile))
        {
            SelectedBuildings.RemoveAll(b => b.X == tile.X && b.Y == tile.Y);

            foreach (Node child in TileOverlayContainer.GetChildren())
            {
                if (child is TileOverlay overlay && overlay.TilePosition == tile)
                {
                    overlay.QueueFree();
                    break;
                }
            }
            OnNotifyChangued?.Invoke(this);
        }
    }
    private void Zoom(int dir)
    {
        if (dir == 0)
            SetScale(Mathf.Min(6, baseScale + 1));
        else
            SetScale(Mathf.Max(1, baseScale - 1));
    }

    private Vector2 GetMouseLocalToGrid()
    {
        Vector2 globalMouse = GetViewport().GetMousePosition();
        return GridBase.GetGlobalTransformWithCanvas().AffineInverse() * globalMouse;
    }

    private bool IsMouseInsideGrid(Vector2 localPos)
    {
        Vector2 gridSize = GridBase.Size;
        return localPos.X >= 0 && localPos.X <= gridSize.X &&
               localPos.Y >= 0 && localPos.Y <= gridSize.Y;
    }

    private Vector2 ClampToGridRect(Vector2 position)
    {
        Vector2 gridSize = GridBase.Size;
        float clampedX = Mathf.Clamp(position.X, 0, gridSize.X - 0.001f);
        float clampedY = Mathf.Clamp(position.Y, 0, gridSize.Y - 0.001f);
        return new Vector2(clampedX, clampedY);
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Floor(position.X / cellInternal) * cellInternal,
            Mathf.Floor(position.Y / cellInternal) * cellInternal
        );
    }

    private void UpdateSelectionFromCells()
    {
        int minX = Mathf.Min(_startCell.X, _endCell.X);
        int minY = Mathf.Min(_startCell.Y, _endCell.Y);
        int maxX = Mathf.Max(_startCell.X, _endCell.X);
        int maxY = Mathf.Max(_startCell.Y, _endCell.Y);

        Selection.Position = new Vector2(minX, minY) * cellInternal;
        Selection.Size = new Vector2((maxX - minX + 1), (maxY - minY + 1)) * cellInternal;
    }

    private void ProcessSelection()
    {
        int minX = Mathf.Min(_startCell.X, _endCell.X);
        int minY = Mathf.Min(_startCell.Y, _endCell.Y);
        int maxX = Mathf.Max(_startCell.X, _endCell.X);
        int maxY = Mathf.Max(_startCell.Y, _endCell.Y);

        int pixelX = minX * cellInternal;
        int pixelY = minY * cellInternal;
        int pixelWidth = (maxX - minX + 1) * cellInternal;
        int pixelHeight = (maxY - minY + 1) * cellInternal;

        OnNotifySelection?.Invoke(pixelX / baseScale, pixelY / baseScale, pixelWidth / baseScale, pixelHeight / baseScale);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                AddBuilding(new Vector2I(x, y));
            }
        }
    }

    private void AddBuilding(Vector2I tile)
    {
        if (_selectedSet.Add(tile))
        {
            SelectedBuildings.Add(new BuildingTilePosition(tile.X, tile.Y));
            ShowTileOverlay(tile);
            OnNotifyChangued?.Invoke(this);
        }
    }

    private void ShowTileOverlay(Vector2I tilePos)
    {
        var overlay = new TileOverlay
        {
            TilePosition = tilePos,
            Color = new Color(0, 1, 0, 0.3f),
            Size = new Vector2(cellInternal, cellInternal),
            Position = tilePos * cellInternal,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };

        TileOverlayContainer.AddChild(overlay);
    }

    public void SetScaleTexture(float scale)
    {
        Vector2 textureSize = baseTexture?.GetSize() ?? Vector2.Zero;
        Vector2 sizeTexture = (textureSize * baseScale) * scale;
        // Ajustar la textura (centrada)        
       

        
            
            Vector2I suggestedGridSize = new Vector2I(
                Mathf.CeilToInt((textureSize.X * scale) / cellSize),
                Mathf.CeilToInt((textureSize.Y * scale) / cellSize)
            );
        SpinBoxGridSizeX.Value = suggestedGridSize.X;
        SpinBoxGridSizeY.Value = suggestedGridSize.Y;
        SetGridSize(suggestedGridSize);
            SetScale(baseScale);
        TextureDraw.Texture = baseTexture;
        TextureDraw.CustomMinimumSize = sizeTexture;
        TextureDraw.Size = sizeTexture;
        //TextureDraw.Scale = new Vector2( scale,scale);
    }

    public void SetOffsetTexture(float offsetX, float offsetY)
    {
           
        TextureDraw.Position = new Vector2(offsetX,offsetY);
        //TextureDraw.Scale = new Vector2( scale,scale);
    }
    public void SetScale(int scale)
    {
        baseScale = scale;
        cellInternal = cellSize * baseScale;
        Vector2 textureSize = baseTexture?.GetSize() ?? Vector2.Zero;
        sizeReal = textureSize * baseScale;

        // Ajustar la textura (centrada)
        TextureDraw.Texture = baseTexture;
        TextureDraw.CustomMinimumSize = sizeReal;
        TextureDraw.Size = sizeReal;
        
        // El grid tiene un tamaño definido por gridSize
        Vector2 gridPixelSize = new Vector2(gridSize.X * cellInternal, gridSize.Y * cellInternal);

        GridBase.CustomMinimumSize = gridPixelSize;
        GridBase.Size = gridPixelSize;

        // El overlay de selección también debe ajustarse
        TileOverlayContainer.Size = gridPixelSize;

        // Centrar ambos
        CenterContainerBase.CustomMinimumSize = gridPixelSize;
        CenterContainerBase.Size = gridPixelSize;

        _shaderMaterial.SetShaderParameter("size", gridPixelSize);
        _shaderMaterial.SetShaderParameter("grid_size", cellInternal);
        UpdateAllOverlays();
        UpdateSelectionFromCells();
    }
    private void UpdateAllOverlays()
    {
        foreach (Node child in TileOverlayContainer.GetChildren())
        {
            if (child is TileOverlay overlay)
            {
                overlay.Position = overlay.TilePosition * cellInternal;
                overlay.Size = new Vector2(cellInternal, cellInternal);
            }
        }

    }
    public void SetTexture(Texture2D texture)
    {
        baseTexture = texture;        
        if (texture != null)
        {
            Vector2 textureSize = texture.GetSize();
            Vector2I suggestedGridSize = new Vector2I(
                Mathf.CeilToInt(textureSize.X / cellSize),
                Mathf.CeilToInt(textureSize.Y / cellSize)
            );
            SpinBoxGridSizeX.Value = suggestedGridSize.X;
            SpinBoxGridSizeY.Value = suggestedGridSize.Y;
            SetGridSize(suggestedGridSize);
            SetScale(baseScale);
        }
        else
        {
            GD.PrintErr("Texture is not assigned.");
        }
    }

    public void ClearSelection()
    {
        SelectedBuildings.Clear();
        _selectedSet.Clear();

        foreach (Node child in TileOverlayContainer.GetChildren())
            child.QueueFree();
        OnNotifyChangued?.Invoke(this);
    }
}
public partial class TileOverlay : ColorRect
{
    public Vector2I TilePosition; // <- celda lógica
}

