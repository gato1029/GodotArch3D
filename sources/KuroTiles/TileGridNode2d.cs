using Godot;
using RVO;
using System;
using System.Collections.Generic;

namespace GodotFlecs.sources.KuroTiles;

public partial class TileGridNode2d : Node2D
{
    [Export] public Vector2I cellSize = new Vector2I(64, 64);
    [Export] public float lineWidth = 1f;
    [Export] public TextureRect imageTexture;
    [Export] public SpinBox spinBoxX;
    [Export] public SpinBox spinBoxY;
    [Export] public SubViewport subViewport;
    [Export] public Color selectionColor = new Color(1, 0, 0, 0.3f);

    private Vector2I selectedCell = new Vector2I(-1, -1);
    private bool isDragging = false;
    private Vector2I dragStartCell = new Vector2I(-1, -1);
    private Vector2I dragEndCell = new Vector2I(-1, -1);

    Color lineColor = Colors.Red;

    public delegate void EventNotifySelection(float x, float y, float width, float height);
    public event EventNotifySelection OnNotifySelection;

    public delegate void EventNotifyMultiSelection(List<TileInfoKuro> tiles);
    public event EventNotifyMultiSelection OnNotifyMultiSelection;

    public delegate void EventNotifySelectionIndex(int index);
    public event EventNotifySelectionIndex OnNotifySelectionIndex;

    public delegate void EventNotifyMultiSelectionIndex(List<int> indices);
    public event EventNotifyMultiSelectionIndex OnNotifyMultiSelectionIndex;

    // 🔹 NUEVO: Modo de selección
    public enum SelectionMode { SingleArea, MultiTile }
    [Export] public SelectionMode selectionMode = SelectionMode.SingleArea;

    // 🔹 NUEVO: Lista de tiles seleccionados
    private List<Vector2I> multiSelectedTiles = new List<Vector2I>();

    private int idMaterial;
    public override void _Ready()
    {
        lineColor = Colors.Gray;
        lineColor.A = 0.7f;
        if (spinBoxX != null)
            spinBoxX.ValueChanged += OnSpinBoxXChanged;
        if (spinBoxY != null)
            spinBoxY.ValueChanged += OnSpinBoxYChanged;
    }

    internal void SetTextureEmpty()
    {
        imageTexture.Texture = null;
        QueueRedraw();
    }

    public void SetTexture(Texture2D texture, int id)
    {
        idMaterial = id;    
        imageTexture.Texture = texture;
        if (imageTexture != null)
        {
            Vector2 textureSize = texture.GetSize();
            imageTexture.Size = textureSize;
            imageTexture.Position = -textureSize / 2f;
        }
        QueueRedraw();
    }

    public void SetTexture(TextureRect texture,int id)
    {
        idMaterial = id;
        imageTexture = texture;
        if (imageTexture != null)
        {
            Vector2 textureSize = imageTexture.Size;
            imageTexture.Position = -textureSize / 2f;
        }
        QueueRedraw();
    }

    public void SetSizeCell(int x, int y)
    {
        spinBoxX.Value = x;
        spinBoxY.Value = y;
        cellSize = new Vector2I(x, y);
        QueueRedraw();
    }

    private void OnSpinBoxYChanged(double value)
    {
        cellSize.Y = (int)value;
        QueueRedraw();
    }

    private void OnSpinBoxXChanged(double value)
    {
        cellSize.X = (int)value;
        QueueRedraw();
    }

    bool forced = false;
    public override void _Draw()
    {
        if (imageTexture == null || imageTexture.Texture == null) return;
        Vector2 textureSize = imageTexture.Size;
        Vector2 center = Position;

        int sizeCellsX = Mathf.CeilToInt(textureSize.X / cellSize.X);
        int sizeCellsY = Mathf.CeilToInt(textureSize.Y / cellSize.Y);
        int sizeX = sizeCellsX * cellSize.X;
        int sizeY = sizeCellsY * cellSize.Y;

        int startX = (int)(center.X - sizeX / 2f);
        int startY = (int)(center.Y - sizeY / 2f);
        int endX = (int)(center.X + sizeX / 2f);
        int endY = (int)(center.Y + sizeY / 2f);

        for (int j = 0; j <= sizeCellsY; j++)
        {
            float y = startY + j * cellSize.Y;
            DrawLine(new Vector2(startX, y), new Vector2(endX, y), lineColor, lineWidth);
        }

        for (int i = 0; i <= sizeCellsX; i++)
        {
            float x = startX + i * cellSize.X;
            DrawLine(new Vector2(x, startY), new Vector2(x, endY), lineColor, lineWidth);
        }

        if (isDragging && dragStartCell.X != -1 && dragStartCell.Y != -1)
            PaintSelection();

        if (forced)
        {
            PaintSelection();
            forced = false;
        }

        // 🔹 NUEVO: Dibuja los tiles seleccionados con su número
        DrawMultiSelection();
    }

    private void PaintSelection()
    {
        int startXCell = Mathf.Min(dragStartCell.X, dragEndCell.X);
        int startYCell = Mathf.Min(dragStartCell.Y, dragEndCell.Y);
        int endXCell = Mathf.Max(dragStartCell.X, dragEndCell.X);
        int endYCell = Mathf.Max(dragStartCell.Y, dragEndCell.Y);

        Vector2 startPos = Position - imageTexture.Size / 2f;

        Rect2 rect = new Rect2(
            startPos + new Vector2(startXCell * cellSize.X, startYCell * cellSize.Y),
            new Vector2((endXCell - startXCell + 1) * cellSize.X, (endYCell - startYCell + 1) * cellSize.Y)
        );

        DrawRect(rect, selectionColor);
    }

    // 🔹 NUEVO
    private void DrawMultiSelection()
    {
        if (multiSelectedTiles.Count == 0) return;
        Vector2 startPos = Position - imageTexture.Size / 2f;

        for (int i = 0; i < multiSelectedTiles.Count; i++)
        {
            Vector2I cell = multiSelectedTiles[i];
            Rect2 rect = new Rect2(
                startPos + new Vector2(cell.X * cellSize.X, cell.Y * cellSize.Y),
                cellSize
            );
            DrawRect(rect, selectionColor);
            DrawString(
                ThemeDB.FallbackFont, // fuente por defecto
                rect.Position + cellSize / 2, // centro del tile
                (i + 1).ToString(),
                HorizontalAlignment.Center,
                -1,
                24, // tamaño fuente
                Colors.White
            );
        }
    }

    public Vector2I GetCellAtPosition(Vector2 mouseGlobalPos)
    {
        if (imageTexture == null) return new Vector2I(-1, -1);
        Vector2 localPos = GetMouseLocalToTexture();
        var cell = SnapToGrid(ClampToTextureRect(localPos));
        return cell;
    }

    private Vector2I SnapToGrid(Vector2 position)
    {
        return new Vector2I(
           (int)(Mathf.Floor(position.X / cellSize.X) * cellSize.X),
           (int)(Mathf.Floor(position.Y / cellSize.Y) * cellSize.Y)
       );
    }

    private Vector2 ClampToTextureRect(Vector2 position)
    {
        Vector2 textureSize = imageTexture.Size;
        return new Vector2(
            Mathf.Clamp(position.X, 0, textureSize.X - 0.001f),
            Mathf.Clamp(position.Y, 0, textureSize.Y - 0.001f)
        );
    }

    private Vector2 GetMouseLocalToTexture()
    {
        Vector2 globalMouse = GetViewport().GetMousePosition();
        return imageTexture.GetGlobalTransformWithCanvas().AffineInverse() * globalMouse;
    }

    private bool IsMouseInsideTexture(Vector2 localPos)
    {
        Vector2 imageSize = imageTexture.Size;
        return localPos.X >= 0 && localPos.X <= imageSize.X &&
               localPos.Y >= 0 && localPos.Y <= imageSize.Y;
    }
    private List<Vector2I> GetCellsInsideArea(Vector2I start, Vector2I end)
    {
        List<Vector2I> cells = new List<Vector2I>();

        int startX = Mathf.Min(start.X, end.X);
        int startY = Mathf.Min(start.Y, end.Y);
        int endX = Mathf.Max(start.X, end.X);
        int endY = Mathf.Max(start.Y, end.Y);

        for (int x = startX; x <= endX; x++)
            for (int y = startY; y <= endY; y++)
                cells.Add(new Vector2I(x, y));

        return cells;
    }

    private bool clickJustPressed = false;
    private Vector2 clickStartMousePos;
    private const float dragThreshold = 4f; // píxeles de movimiento para considerar drag
    public override void _Input(InputEvent @event)
    {
        // CLICK DERECHO limpia selección (MultiTile)
        if (@event is InputEventMouseButton right && right.ButtonIndex == MouseButton.Right && right.Pressed)
        {
            if (selectionMode == SelectionMode.MultiTile)
                ClearMultiSelection();
        }

        // CLICK IZQUIERDO
        if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
        {
            if (mouse.Pressed)
            {
                Vector2 localMouse = GetMouseLocalToTexture();
                if (!IsMouseInsideTexture(localMouse)) return;

                clickJustPressed = true;
                clickStartMousePos = localMouse;

                Vector2I cell = SnapToGrid(ClampToTextureRect(localMouse)) / cellSize;

                if (selectionMode == SelectionMode.SingleArea)
                {
                    dragStartCell = dragEndCell = cell;
                    isDragging = true;
                }
                else if (selectionMode == SelectionMode.MultiTile)
                {
                    // Prepararnos para posible drag; no asumimos drag hasta que se mueva suficiente
                    dragStartCell = dragEndCell = cell;
                    isDragging = false;
                }

                QueueRedraw();
            }
            else // MOUSE UP
            {
                // Si estábamos haciendo drag en MultiTile, notificamos lista final
                if (isDragging && selectionMode == SelectionMode.MultiTile)
                {
                    OnNotifyMultiSelection?.Invoke(GetSelectedFrames());
                }
                else if (clickJustPressed && selectionMode == SelectionMode.MultiTile)
                {
                    // CLICK SIMPLE en MultiTile -> toggle o añadir si Ctrl
                    Vector2 localMouse = GetMouseLocalToTexture();
                    Vector2I cell = SnapToGrid(ClampToTextureRect(localMouse)) / cellSize;

                    bool isCtrl = Input.IsKeyPressed(Key.Ctrl);
                    if (isCtrl)
                    {
                        // CTRL + CLICK -> siempre agregar (permitir duplicados)
                        multiSelectedTiles.Add(cell);
                    }
                    else
                    {
                        // Click normal -> toggle
                        if (!multiSelectedTiles.Contains(cell))
                            multiSelectedTiles.Add(cell);
                        else
                            multiSelectedTiles.Remove(cell);
                    }

                    OnNotifyMultiSelection?.Invoke(GetSelectedFrames());
                    OnNotifyMultiSelectionIndex?.Invoke(GetSelectedIndices());
                    QueueRedraw();
                }

                // IMPORTANTE: SingleArea notifica siempre al soltar (mantener comportamiento original)
                if (selectionMode == SelectionMode.SingleArea)
                {
                    int startX = Mathf.Min(dragStartCell.X, dragEndCell.X);
                    int startY = Mathf.Min(dragStartCell.Y, dragEndCell.Y);
                    int endX = Mathf.Max(dragStartCell.X, dragEndCell.X);
                    int endY = Mathf.Max(dragStartCell.Y, dragEndCell.Y);

                    float px = startX * cellSize.X;
                    float py = startY * cellSize.Y;
                    float w = (endX - startX + 1) * cellSize.X;
                    float h = (endY - startY + 1) * cellSize.Y;

                    OnNotifySelection?.Invoke(px, py, w, h);
                    // Solo si es 1 tile
                    if (dragStartCell == dragEndCell)
                    {
                        int index = CellToIndex(dragStartCell);
                        OnNotifySelectionIndex?.Invoke(index);
                    }
                }

                clickJustPressed = false;
                isDragging = false;
            }
        }
        // MOUSE MOTION
        else if (@event is InputEventMouseMotion motion)
        {
            if (selectionMode == SelectionMode.MultiTile)
            {
                if (clickJustPressed)
                {
                    Vector2 localMouse = GetMouseLocalToTexture();
                    if (localMouse.DistanceTo(clickStartMousePos) > dragThreshold)
                    {
                        isDragging = true;
                        clickJustPressed = false;
                    }
                }

                if (isDragging)
                {
                    Vector2 localMouse = GetMouseLocalToTexture();
                    if (IsMouseInsideTexture(localMouse))
                    {
                        dragEndCell = SnapToGrid(ClampToTextureRect(localMouse)) / cellSize;

                        // Obtener todas las celdas en el rectángulo y reemplazar selección
                        multiSelectedTiles = GetCellsInsideArea(dragStartCell, dragEndCell);

                        QueueRedraw();
                    }
                }
            }
            else if (selectionMode == SelectionMode.SingleArea && isDragging)
            {
                Vector2 localMouse = GetMouseLocalToTexture();
                if (IsMouseInsideTexture(localMouse))
                {
                    dragEndCell = SnapToGrid(ClampToTextureRect(localMouse)) / cellSize;
                    QueueRedraw();
                }
            }
        }
    }
    public List<int> GetSelectedIndices()
    {
        List<int> indices = new List<int>();

        foreach (var cell in multiSelectedTiles)
            indices.Add(CellToIndex(cell));

        return indices;
    }

    internal void SetSizeLineGrid(float zoom)
    {
        lineWidth = Mathf.Clamp(1f / zoom, 1f, 20f);
        forced = true;
        QueueRedraw();
    }

    internal void SetSelection(float x, float y, float width, float height)
    {
        dragStartCell = new Vector2I((int)(x / cellSize.X), (int)(y / cellSize.Y));
        dragEndCell = new Vector2I((int)((x + width - 1) / cellSize.X), (int)((y + height - 1) / cellSize.Y));
        forced = true;
        QueueRedraw();
    }

    // 🔹 NUEVO: limpiar selección múltiple
    public void ClearMultiSelection()
    {
        multiSelectedTiles.Clear();
        QueueRedraw();
    }
    public void SetSelection(List<TileInfoKuro> tiles)
    {
        if (tiles == null || tiles.Count == 0)
        {
            multiSelectedTiles.Clear();
            QueueRedraw();
            return;
        }

        selectionMode = SelectionMode.MultiTile;
        multiSelectedTiles.Clear();

        foreach (var tile in tiles)
        {
            // Convertir coordenadas de pixel a celda
            int cellX = tile.x / cellSize.X;
            int cellY = tile.y / cellSize.Y;
            Vector2I cell = new Vector2I(cellX, cellY);

            if (!multiSelectedTiles.Contains(cell))
                multiSelectedTiles.Add(cell);
        }

        QueueRedraw();
    }
    public List<TileInfoKuro> GetSelectedFrames()
    {
        List<TileInfoKuro> selectedFrames = new List<TileInfoKuro>();

        if (imageTexture == null || imageTexture.Texture == null)
            return selectedFrames;

        Texture2D baseTexture = (Texture2D)imageTexture.Texture;

        foreach (var cell in multiSelectedTiles)
        {
            // Calcula posición en píxeles dentro del atlas
            int px = cell.X * cellSize.X;
            int py = cell.Y * cellSize.Y;

            // Crea un AtlasTexture que recorta esa región
            AtlasTexture atlasTex = new AtlasTexture
            {
                Atlas = baseTexture,
                Region = new Rect2(px, py, cellSize.X, cellSize.Y)
            };

            // Arma la estructura final
            TileInfoKuro tile = new TileInfoKuro
            {
                idMaterial = this.idMaterial , 
                x = px,
                y = py,
                width = cellSize.X,
                height = cellSize.Y,
                texture = atlasTex
            };

            selectedFrames.Add(tile);
        }

        return selectedFrames;
    }

    private int GetColumns()
    {
        if (imageTexture == null) return 0;
        return Mathf.CeilToInt(imageTexture.Size.X / cellSize.X);
    }
    private int CellToIndex(Vector2I cell)
    {
        int cols = GetColumns();
        return cell.Y * cols + cell.X + 1 ; // +1 para q
    }
    private Vector2I IndexToCell(int index)
    {
        int cols = GetColumns();
        index -= 1;

        int x = index % cols;
        int y = index / cols;

        return new Vector2I(x, y);
    }
    public void SetSelection(int index)
    {
        if (imageTexture == null || imageTexture.Texture == null)
            return;

        selectionMode = SelectionMode.SingleArea;

        // Convertir índice → celda
        Vector2I cell = IndexToCell(index);

        // Limpiar multi selección por seguridad
        multiSelectedTiles.Clear();

        dragStartCell = cell;
        dragEndCell = cell;

        // Notificar selección en píxeles (como ya haces)
        float px = cell.X * cellSize.X;
        float py = cell.Y * cellSize.Y;
        float w = cellSize.X;
        float h = cellSize.Y;

        OnNotifySelection?.Invoke(px, py, w, h);

        // 🔹 NUEVO: notificar índice
        OnNotifySelectionIndex?.Invoke(index);

        forced = true;
        QueueRedraw();
    }
    public void SetSelection(List<int> indices)
    {
        selectionMode = SelectionMode.MultiTile;
        multiSelectedTiles.Clear();

        foreach (var index in indices)
        {
            Vector2I cell = IndexToCell(index);

            if (!multiSelectedTiles.Contains(cell))
                multiSelectedTiles.Add(cell);
        }

        QueueRedraw();
    }
}
