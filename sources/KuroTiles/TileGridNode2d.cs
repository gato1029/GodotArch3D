using Godot;
using System;
using System.Collections.Generic;

namespace GodotFlecs.sources.KuroTiles;


public class TilePreviewData
{
    public int idMaterial;
    public int index;
    public int localX;
    public int localY;

    public int atlasX;
    public int atlasY;

    public int width;
    public int height;

    public Texture2D texture;

    public bool isEmpty = false;

    public TilePreviewData() { }

    public TilePreviewData(bool empty)
    {
        isEmpty = empty;
        index = -1;
        idMaterial = -1;
    }
}

public class TileSelectionMatrixData
{
    // matriz local [y,x]
    public TilePreviewData[,] matrix;

    // tamaño local de la matriz
    public int width;
    public int height;

    // celda origen real en atlas
    public Vector2I atlasStartCell;

    // metadata
    public int idMaterial;
    public Vector2I cellSize;

    public bool IsEmpty()
    {
        return matrix == null || width == 0 || height == 0;
    }

    public TilePreviewData Get(int x, int y)
    {
        if (matrix == null) return null;
        if (y < 0 || y >= height) return null;
        if (x < 0 || x >= width) return null;

        return matrix[y, x];
    }

    public List<TilePreviewData> ToFlatList()
    {
        List<TilePreviewData> list = new List<TilePreviewData>();

        if (matrix == null) return list;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (matrix[y, x] != null)
                    list.Add(matrix[y, x]);

        return list;
    }
}
public partial class TileGridNode2d : Node2D
{
    [Export] public Vector2I cellSize = new Vector2I(64, 64);
    [Export] public float lineWidth = 1f;
    [Export] public TextureRect imageTexture;
    [Export] public SpinBox spinBoxX;
    [Export] public SpinBox spinBoxY;
    [Export] public SubViewport subViewport;
    [Export] public Color selectionColor = new Color(1f, 0.3f, 0.3f, 0.5f);

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

    // 🔥 NUEVO EVENTO MATRIZ
    public delegate void EventNotifySelectionMatrix(TileSelectionMatrixData matrix);
    public event EventNotifySelectionMatrix OnNotifySelectionMatrix;

    public enum SelectionMode { SingleArea, MultiTile }
    [Export] public SelectionMode selectionMode = SelectionMode.SingleArea;

    private List<Vector2I> multiSelectedTiles = new List<Vector2I>();

    private int idMaterial;

    private Image cachedAtlasImage;

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
        cachedAtlasImage = texture.GetImage();
        if (imageTexture != null)
        {
            Vector2 textureSize = texture.GetSize();
            imageTexture.Size = textureSize;
            imageTexture.Position = -textureSize / 2f;
        }

        QueueRedraw();
    }

    public void SetTexture(TextureRect texture, int id)
    {
        idMaterial = id;
        imageTexture = texture;
        cachedAtlasImage = ((Texture2D)texture.Texture).GetImage();
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

        DrawRect(rect, new Color(1f, 1f, 1f, 0.15f));
        DrawRect(rect, new Color(1f, 0.2f, 0.2f, 1f), false, 2f);
    }

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

            DrawRect(rect, new Color(1f, 1f, 1f, 0.15f));
            DrawRect(rect, new Color(1f, 0.2f, 0.2f, 1f), false, 2f);

            DrawString(
                ThemeDB.FallbackFont,
                rect.Position + cellSize / 2,
                (i + 1).ToString(),
                HorizontalAlignment.Center,
                -1,
                24,
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

    // 🔥 NUEVO CONSTRUCTOR MATRIZ
    private TileSelectionMatrixData BuildSelectionMatrix(Vector2I start, Vector2I end)
    {
        TileSelectionMatrixData data = new TileSelectionMatrixData();

        int startX = Mathf.Min(start.X, end.X);
        int startY = Mathf.Min(start.Y, end.Y);
        int endX = Mathf.Max(start.X, end.X);
        int endY = Mathf.Max(start.Y, end.Y);

        int width = endX - startX + 1;
        int height = endY - startY + 1;

        data.width = width;
        data.height = height;
        data.atlasStartCell = new Vector2I(startX, startY);
        data.idMaterial = idMaterial;
        data.cellSize = cellSize;

        data.matrix = new TilePreviewData[width, height];

        Texture2D baseTexture = (Texture2D)imageTexture.Texture;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2I atlasCell = new Vector2I(startX + x, startY + y);

                int px = atlasCell.X * cellSize.X;
                int py = atlasCell.Y * cellSize.Y;

                AtlasTexture atlasTex = new AtlasTexture
                {
                    Atlas = baseTexture,
                    Region = new Rect2(px, py, cellSize.X, cellSize.Y)
                };
                Rect2 tileRegion = new Rect2(px, py, cellSize.X, cellSize.Y);
                bool empty = IsTileRegionEmpty(baseTexture, tileRegion);
                TilePreviewData preview = new TilePreviewData
                {
                    idMaterial = idMaterial,
                    index = CellToIndex(atlasCell),

                    localX = x,
                    localY = y,

                    atlasX = px,
                    atlasY = py,

                    width = cellSize.X,
                    height = cellSize.Y,

                    texture = atlasTex,
                    isEmpty = empty
                };

                data.matrix[x, y] = preview;
            }
        }

        return data;
    }

    private bool clickJustPressed = false;
    private Vector2 clickStartMousePos;
    private const float dragThreshold = 4f;

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton right && right.ButtonIndex == MouseButton.Right && right.Pressed)
        {
            if (selectionMode == SelectionMode.MultiTile)
                ClearMultiSelection();
        }

        if (@event is InputEventMouseButton mouse && mouse.ButtonIndex == MouseButton.Left)
        {
            if (mouse.Pressed)
            {
                Vector2 localMouse = GetMouseLocalToTexture();
                if (!IsMouseInsideTexture(localMouse)) return;

                clickJustPressed = true;
                clickStartMousePos = localMouse;

                Vector2I cell = SnapToGrid(ClampToTextureRect(localMouse)) / cellSize;

                dragStartCell = dragEndCell = cell;

                if (selectionMode == SelectionMode.SingleArea)
                    isDragging = true;
                else
                    isDragging = false;

                QueueRedraw();
            }
            else
            {
                if (isDragging && selectionMode == SelectionMode.MultiTile)
                {
                    OnNotifyMultiSelection?.Invoke(GetSelectedFrames());
                    OnNotifyMultiSelectionIndex?.Invoke(GetSelectedIndices());
                }
                else if (clickJustPressed && selectionMode == SelectionMode.MultiTile)
                {
                    Vector2 localMouse = GetMouseLocalToTexture();
                    Vector2I cell = SnapToGrid(ClampToTextureRect(localMouse)) / cellSize;

                    bool isCtrl = Input.IsKeyPressed(Key.Ctrl);

                    if (isCtrl)
                        multiSelectedTiles.Add(cell);
                    else
                    {
                        if (!multiSelectedTiles.Contains(cell))
                            multiSelectedTiles.Add(cell);
                        else
                            multiSelectedTiles.Remove(cell);
                    }

                    OnNotifyMultiSelection?.Invoke(GetSelectedFrames());
                    OnNotifyMultiSelectionIndex?.Invoke(GetSelectedIndices());
                    QueueRedraw();
                }

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

                    // 🔥 NUEVA MATRIZ LOCAL
                    OnNotifySelectionMatrix?.Invoke(BuildSelectionMatrix(dragStartCell, dragEndCell));

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
            int px = cell.X * cellSize.X;
            int py = cell.Y * cellSize.Y;

            AtlasTexture atlasTex = new AtlasTexture
            {
                Atlas = baseTexture,
                Region = new Rect2(px, py, cellSize.X, cellSize.Y)
            };

            TileInfoKuro tile = new TileInfoKuro
            {
                idMaterial = this.idMaterial,
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
        return cell.Y * cols + cell.X + 1;
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
        multiSelectedTiles.Clear();

        Vector2I cell = IndexToCell(index);

        dragStartCell = cell;
        dragEndCell = cell;

        float px = cell.X * cellSize.X;
        float py = cell.Y * cellSize.Y;
        float w = cellSize.X;
        float h = cellSize.Y;

        OnNotifySelection?.Invoke(px, py, w, h);
        OnNotifySelectionIndex?.Invoke(index);
        OnNotifySelectionMatrix?.Invoke(BuildSelectionMatrix(cell, cell));

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

    public Vector2 GetGlobalPositionFromIndex(int index)
    {
        if (imageTexture == null || imageTexture.Texture == null)
            return Vector2.Zero;

        Vector2I cell = IndexToCell(index);

        Vector2 gridLocalPos = new Vector2(
            cell.X * cellSize.X + cellSize.X / 2f,
            cell.Y * cellSize.Y + cellSize.Y / 2f
        );

        return ToGlobal(gridLocalPos - imageTexture.Size / 2f);
    }
    private bool IsTileRegionEmpty(Texture2D texture, Rect2 region)
    {
        if (cachedAtlasImage == null)
            return true;

        Image img = cachedAtlasImage;
        if (img == null)
            return true;

     //   img.Lock();

        int startX = Mathf.Clamp((int)region.Position.X, 0, img.GetWidth() - 1);
        int startY = Mathf.Clamp((int)region.Position.Y, 0, img.GetHeight() - 1);

        int endX = Mathf.Clamp((int)(region.Position.X + region.Size.X), 0, img.GetWidth());
        int endY = Mathf.Clamp((int)(region.Position.Y + region.Size.Y), 0, img.GetHeight());

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                Color c = img.GetPixel(x, y);

                if (c.A > 0.01f)
                {
                 //   img.Unlock();
                    return false;
                }
            }
        }

     //   img.Unlock();
        return true;
    }
}