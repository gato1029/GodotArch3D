using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public struct KuroTile : IEquatable<KuroTile>
{
    public int x;
    public int y;

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
    [Export] public Color outlineColor = Colors.Gray;

    private HashSet<KuroTile> selectedTiles = new HashSet<KuroTile>();
    private HashSet<KuroTile> blockedTiles = new HashSet<KuroTile>();
    public override void _Ready()
    {
        SetProcessInput(true);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed)
        {
            Vector2 svMouse = GetViewport().GetMousePosition();
            Vector2 localPos = GetGlobalTransformWithCanvas().AffineInverse() * svMouse;

            if (mb.ButtonIndex == MouseButton.Left)
                SelectTile(localPos);
            else if (mb.ButtonIndex == MouseButton.Right)
                UnselectTile(localPos);
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
    public void SelectTile(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        selectedTiles.Add(tile);
        QueueRedraw();
    }
    public void UnselectTile(int x, int y)
    {
        KuroTile tile = new KuroTile(x, y);
        if (blockedTiles.Contains(tile))
        {
            // Tile bloqueado, no eliminar
            return;
        }
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
        if (blockedTiles.Contains(tile))
        {
            // Tile bloqueado, no eliminar
            return;
        }
        selectedTiles.Remove(tile);
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawSelectedTiles();
    }

    private void DrawSelectedTiles()
    {
        foreach (var cell in selectedTiles)
        {
            Vector2 topLeft = new Vector2(
                cell.x * tileSize.X - tileSize.X / 2f,
                (-cell.y * tileSize.Y - tileSize.Y / 2f)
            );

            Rect2 rect = new Rect2(topLeft, tileSize);

            DrawRect(rect, tileColor, filled: true);
            DrawRect(rect, outlineColor, filled: false);
        }
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
}
