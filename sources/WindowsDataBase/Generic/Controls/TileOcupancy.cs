using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class KuroTile : IEquatable<KuroTile>
{
    public int x { get; set; }
    public int y { get; set; }

    public KuroTile()
    {
     
    }
    public KuroTile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString() => $"({x}, {y})";

    // Implementamos Equals para comparar valores
    public bool Equals(KuroTile other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object? obj)
    {
        return obj is KuroTile other && Equals(other);
    }

    // Implementamos GetHashCode para el HashSet y diccionarios
    public override int GetHashCode()
    {
        unchecked // permite overflow sin excepción
        {
            int hash = 17;
            hash = hash * 31 + x;
            hash = hash * 31 + y;
            return hash;
        }
    }

    // Opcional: operadores == y !=
    public static bool operator ==(KuroTile left, KuroTile right) => left.Equals(right);
    public static bool operator !=(KuroTile left, KuroTile right) => !(left == right);
}

public partial class TileOcupancy : Node2D
{
    public Vector2 tileSize { get; set; } = new Vector2(16, 16);

    [Export] public Color tileColor = Colors.LawnGreen;
    [Export] public Color pathTileColor = Colors.DodgerBlue; // Color distintivo para las celdas de path
    [Export] public Color outlineColor = Colors.Gray;

    [Export] public CheckButton checkEnable;     // Controla si la edición general está activa
    [Export] public CheckButton checkEnablePath; // Alterna entre selectedTiles (inactivo) y selectedTilesPath (activo)

    private HashSet<KuroTile> selectedTiles = new HashSet<KuroTile>();
    private HashSet<KuroTile> selectedTilesPath = new HashSet<KuroTile>();
    private HashSet<KuroTile> blockedTiles = new HashSet<KuroTile>();

    public override void _Ready()
    {
        SetProcessInput(true);
        // Nos suscribimos al evento Toggled del CheckButton de path para repintar automáticamente al cambiar
        if (checkEnablePath != null)
        {
            checkEnablePath.Toggled += OnCheckEnablePathToggled;
        }
    }
    private void OnCheckEnablePathToggled(bool buttonPressed)
    {
        // Forzamos el redibujado instantáneo al cambiar de modo con el CheckButton
        QueueRedraw();
    }
    public override void _Input(InputEvent @event)
    {
        if (checkEnable == null || !checkEnable.ButtonPressed)
        {
            return;
        }

        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            Vector2 svMouse = GetViewport().GetMousePosition();
            Vector2 localPos = GetGlobalTransformWithCanvas().AffineInverse() * svMouse;

            bool isPathMode = checkEnablePath != null && checkEnablePath.ButtonPressed;

            if (mb.ButtonIndex == MouseButton.Left)
            {
                if (isPathMode)
                    SelectTilePath(localPos);
                else
                    SelectTile(localPos);
            }
            else if (mb.ButtonIndex == MouseButton.Right)
            {
                if (isPathMode)
                    UnselectTilePath(localPos);
                else
                    UnselectTile(localPos);
            }
        }
    }

    private KuroTile GetTileFromPosition(Vector2 position)
    {
        float halfX = tileSize.X * 0.5f;
        float halfY = tileSize.Y * 0.5f;

        float nx = (position.X + halfX) / tileSize.X;
        float ny = (position.Y + halfY) / tileSize.Y;

        int tx = Mathf.FloorToInt(nx);
        int ty = -Mathf.FloorToInt(ny);

        return new KuroTile(tx, ty);
    }

    #region Block Methods
    public void BlockTile(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        blockedTiles.Add(tile);
        QueueRedraw();
    }
    public void UnblockTile(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        blockedTiles.Remove(tile);
        QueueRedraw();
    }
    #endregion

    #region Standard Selected Tiles Methods
    public void SelectTile(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        selectedTiles.Add(tile);
        QueueRedraw();
    }

    public void UnselectTile(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        if (blockedTiles.Contains(tile)) return;

        selectedTiles.Remove(tile);
        QueueRedraw();
    }

    private void SelectTile(Vector2 pos)
    {
        KuroTile tile = GetTileFromPosition(pos);
        selectedTiles.Add(tile);
        QueueRedraw();
    }

    private void UnselectTile(Vector2 pos)
    {
        KuroTile tile = GetTileFromPosition(pos);
        if (blockedTiles.Contains(tile)) return;

        selectedTiles.Remove(tile);
        QueueRedraw();
    }

    public List<KuroTile> GetSelectedTiles()
    {
        return selectedTiles.ToList();
    }

    public void SetSelectedTiles(IEnumerable<KuroTile> tiles)
    {
        selectedTiles = new HashSet<KuroTile>(tiles);
        QueueRedraw();
    }
    #endregion

    #region Path Selected Tiles Methods
    public void SelectTilePath(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        selectedTilesPath.Add(tile);
        QueueRedraw();
    }

    public void UnselectTilePath(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        if (blockedTiles.Contains(tile)) return;

        selectedTilesPath.Remove(tile);
        QueueRedraw();
    }

    private void SelectTilePath(Vector2 pos)
    {
        KuroTile tile = GetTileFromPosition(pos);
        selectedTilesPath.Add(tile);
        QueueRedraw();
    }

    private void UnselectTilePath(Vector2 pos)
    {
        KuroTile tile = GetTileFromPosition(pos);
        if (blockedTiles.Contains(tile)) return;

        selectedTilesPath.Remove(tile);
        QueueRedraw();
    }

    public List<KuroTile> GetSelectedTilesPath()
    {
        return selectedTilesPath.ToList();
    }

    public void SetSelectedTilesPath(IEnumerable<KuroTile> tiles)
    {
        selectedTilesPath = new HashSet<KuroTile>(tiles);
        QueueRedraw();
    }
    #endregion

    public override void _Draw()
    {
        DrawTiles();
    }

    private void DrawTiles()
    {
        bool isPathMode = checkEnablePath != null && checkEnablePath.ButtonPressed;

        // Según el estado del CheckButton de path, decidimos qué conjunto visualizar,
        // pero ninguno de los dos pierde sus datos internos.
        var targetCollection = isPathMode ? selectedTilesPath : selectedTiles;
        var currentColor = isPathMode ? pathTileColor : tileColor;

        foreach (var cell in targetCollection)
        {
            Vector2 topLeft = new Vector2(
                cell.x * tileSize.X - tileSize.X / 2f,
                (-cell.y * tileSize.Y - tileSize.Y / 2f)
            );

            Rect2 rect = new Rect2(topLeft, tileSize);

            DrawRect(rect, currentColor, filled: true);
            DrawRect(rect, outlineColor, filled: false);
        }
    }
}