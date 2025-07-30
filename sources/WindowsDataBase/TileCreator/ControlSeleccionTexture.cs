using Godot;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;

[Tool]
public partial class ControlSeleccionTexture : MarginContainer
{
    [Export] public int baseScale = 2;
    [Export] public Texture2D baseTexture;
    [Export] public int cellSize = 8;

    private int cellInternal = 8;
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private bool _isSelecting = false;

    public Vector2 sizeReal;
    private ShaderMaterial _shaderMaterial;

    public delegate void EventNotifySelection(float x, float y, float width, float height);
    public event EventNotifySelection OnNotifySelection;

    // Accesos a nodos internos
    private Vector2I _startCell;
    private Vector2I _endCell;
    public override void _Ready()
    {
        InitializeUI(); // Insertado por el generador de UI
        ShaderMaterial tempo = GD.Load<ShaderMaterial>("res://resources/Material/MaterialGridCanvas.tres");
        _shaderMaterial = (ShaderMaterial)tempo.Duplicate();
        GridBase.Material = _shaderMaterial;

        Selection.Modulate = new Color(1, 1, 1, 0.5f);
        Selection.Visible = false;
        SpinBoxCellSize.ValueChanged += SpinBoxCellSize_ValueChanged;
        if (baseTexture != null)
        {
            SetTexture(baseTexture);
            cellInternal = cellSize * baseScale;
        }
        MouseEntered += ControlSeleccionTexture_MouseEntered;
    }

    private void ControlSeleccionTexture_MouseEntered()
    {
        CenterContainerBase.GrabFocus();
    }

    private void SpinBoxCellSize_ValueChanged(double value)
    {
        cellSize = (int)value;
        cellInternal = cellSize * baseScale;
        _shaderMaterial.SetShaderParameter("grid_size", cellSize * baseScale);
    }

    public override void _Input(InputEvent @event)
    {
        if (GetGlobalRect().HasPoint(GetGlobalMousePosition()) && (HasFocus() || CenterContainerBase.HasFocus()|| ContainerSelectionTexture.HasFocus()))
        {
            // === ZOOM CON CTRL + SCROLL ===
            if (@event is InputEventMouseButton mouseEventScroll)
            {
                if (Input.IsKeyPressed(Key.Ctrl) && GetGlobalRect().HasPoint(GetGlobalMousePosition()))
                {
                    if (mouseEventScroll.ButtonIndex == MouseButton.WheelUp)
                    {
                        Zoom(0);
                        return;
                    }
                    else if (mouseEventScroll.ButtonIndex == MouseButton.WheelDown)
                    {
                        Zoom(1);
                        return;
                    }
                }
            }
            Vector2 localMouse = GetMouseLocalToTexture();

            if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    if (IsMouseInsideTexture(localMouse))
                    {
                        // Limpieza previa
                        Selection.Size = Vector2.Zero;

                        _startPosition = SnapToGrid(ClampToTextureRect(localMouse));

                        _startCell = (Vector2I)(_startPosition / cellInternal);
                        _endPosition = _startPosition;
                        _endCell = _startCell;
                        _isSelecting = true;
                        Selection.Visible = true;
                        UpdateSelectionFromCells();
                    }
                }
                else if (_isSelecting)
                {
                    _isSelecting = false;
                    if (IsMouseInsideTexture(localMouse))
                    {
                        _endPosition = SnapToGrid(ClampToTextureRect(localMouse));
                        UpdateSelectionFromCells();

                        // Clic sin arrastrar (misma celda)
                        if (_startPosition == _endPosition)
                        {


                            // Mostrar visualmente el cuadrante seleccionado
                            Selection.Position = _startPosition;
                            Selection.Size = new Vector2(cellInternal, cellInternal);
                            Selection.Visible = true;

                            OnNotifySelection?.Invoke(
                                _startPosition.X / baseScale,
                                _startPosition.Y / baseScale,
                                cellInternal / baseScale,
                                cellInternal / baseScale
                            );         
                        }
                        else
                        {
                            ProcessSelection(); // Selección normal
                        }
                    }
                    else
                    {
                        Selection.Visible = false; // Cancelar selección fuera del área
                    }
                }
            }
            else if (@event is InputEventMouseMotion && _isSelecting)
            {
                if (IsMouseInsideTexture(localMouse))
                {
                    _endPosition = SnapToGrid(ClampToTextureRect(localMouse));

                    _endCell = ((Vector2I)(_endPosition / cellInternal));
                    UpdateSelectionFromCells();
                }
            }

        }

    }

    private void Zoom(int v)
    {
        if (v==0)
        {
            SetScale(Mathf.Min(6, baseScale + 1));
        }
        if (v==1)
        {
            SetScale(Mathf.Max(1, baseScale - 1));
        }
    }

    private Vector2 GetMouseLocalToTexture()
    {
       // return TextureDraw.GetLocalMousePosition();
        // Convertir mouse global a coordenadas relativas a TextureDraw
        Vector2 globalMouse = GetViewport().GetMousePosition();
        return TextureDraw.GetGlobalTransformWithCanvas().AffineInverse() * globalMouse;
    }

    private bool IsMouseInsideTexture(Vector2 localPos)
    {

        Vector2 imageSize = TextureDraw.Size;
        return localPos.X >= 0 && localPos.X <= imageSize.X &&
               localPos.Y >= 0 && localPos.Y <= imageSize.Y;
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
    private void UpdateSelectionRect()
    {
        Vector2 textureSize = TextureDraw.Size ;

        var minX = Mathf.Min(_startPosition.X, _endPosition.X);
        var minY = Mathf.Min(_startPosition.Y, _endPosition.Y);
        var maxX = Mathf.Max(_startPosition.X, _endPosition.X);
        var maxY = Mathf.Max(_startPosition.Y, _endPosition.Y);

        float width = Mathf.Max(cellInternal, maxX - minX);
        float height = Mathf.Max(cellInternal, maxY - minY);

        Selection.Position = new Vector2(minX, minY);
        Selection.Size = new Vector2(width, height);
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

        if (pixelWidth > 0 && pixelHeight > 0)
        {
            OnNotifySelection?.Invoke(pixelX / baseScale, pixelY / baseScale, pixelWidth / baseScale, pixelHeight / baseScale);
          
        }

    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        var vect = new Vector2(
           Mathf.Floor(position.X / cellInternal) * cellInternal,
           Mathf.Floor(position.Y / cellInternal) * cellInternal
       );

       return vect; 
    }

    private Vector2 ClampToTextureRect(Vector2 position)
    {
        Vector2 textureSize = TextureDraw.Size ;

        float clampedX = Mathf.Clamp(position.X, 0, textureSize.X - 0.001f);
        float clampedY = Mathf.Clamp(position.Y, 0, textureSize.Y - 0.001f);
        
        return new Vector2(clampedX, clampedY);
    }

    public void SetScale(int scale)
    {
        baseScale = scale;
        cellInternal = cellSize * baseScale;
        if (baseTexture != null)
        {
            sizeReal = baseTexture.GetSize() * baseScale;
            CenterContainerBase.CustomMinimumSize = sizeReal;
            CenterContainerBase.Size = sizeReal;            
            TextureDraw.CustomMinimumSize = sizeReal;
            GridBase.CustomMinimumSize = sizeReal;

            _shaderMaterial.SetShaderParameter("size", sizeReal);
            _shaderMaterial.SetShaderParameter("grid_size", cellSize * baseScale);
        }
        else
        {
            GD.PrintErr("Texture is not assigned.");
        }
    }

    public void SetTexture(Texture2D texture, MaterialData materialData =null)
    {
        if (materialData!=null)
        {
            SpinBoxCellSize.Value = materialData.divisionPixelX;
        }
        baseTexture = texture;

        if (texture != null)
        {
            sizeReal = texture.GetSize() * baseScale;
            CenterContainerBase.CustomMinimumSize = sizeReal;
            CenterContainerBase.Size = sizeReal;
            TextureDraw.Texture = texture;
            TextureDraw.CustomMinimumSize = sizeReal;
            GridBase.CustomMinimumSize = sizeReal;
            cellInternal = cellSize * baseScale;
            _shaderMaterial.SetShaderParameter("size", sizeReal);
            _shaderMaterial.SetShaderParameter("grid_size", cellSize * baseScale);
        }
        else
        {
            GD.PrintErr("Texture is not assigned.");
        }
    }
}
