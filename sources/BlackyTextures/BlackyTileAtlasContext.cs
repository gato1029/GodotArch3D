using Godot;
using System;

namespace GodotEcsArch.sources.BlackyTextures;

public class BlackyTileAtlasContext
{
    public int IdMaterial { get; private set; } = -1;

    public Texture2D AtlasTexture { get; private set; }
    public Image CachedImage { get; private set; }

    public Vector2I CellSize { get; private set; } = new Vector2I(64, 64);

    public int Columns { get; private set; }
    public int Rows { get; private set; }

    public Vector2 TextureSize => AtlasTexture != null ? AtlasTexture.GetSize() : Vector2.Zero;

    public bool HasTexture => AtlasTexture != null;

    public void SetTexture(Texture2D texture, int idMaterial)
    {
        AtlasTexture = texture;
        IdMaterial = idMaterial;

        CachedImage = texture != null ? texture.GetImage() : null;

        RecalculateGridMetrics();
    }

    public void SetCellSize(int x, int y)
    {
        CellSize = new Vector2I(x, y);
        RecalculateGridMetrics();
    }

    private void RecalculateGridMetrics()
    {
        if (AtlasTexture == null)
        {
            Columns = 0;
            Rows = 0;
            return;
        }

        Vector2 size = AtlasTexture.GetSize();

        Columns = Mathf.CeilToInt(size.X / CellSize.X);
        Rows = Mathf.CeilToInt(size.Y / CellSize.Y);
    }

    public int CellToIndex(Vector2I cell)
    {
        return cell.Y * Columns + cell.X + 1;
    }

    public Vector2I IndexToCell(int index)
    {
        index -= 1;

        int x = index % Columns;
        int y = index / Columns;

        return new Vector2I(x, y);
    }

    public bool IsValidCell(Vector2I cell)
    {
        return cell.X >= 0 && cell.Y >= 0 &&
                cell.X < Columns && cell.Y < Rows;
    }

    public bool IsTileEmpty(Rect2 region)
    {
        if (CachedImage == null)
            return true;

        int startX = Mathf.Clamp((int)region.Position.X, 0, CachedImage.GetWidth() - 1);
        int startY = Mathf.Clamp((int)region.Position.Y, 0, CachedImage.GetHeight() - 1);

        int endX = Mathf.Clamp((int)(region.Position.X + region.Size.X), 0, CachedImage.GetWidth());
        int endY = Mathf.Clamp((int)(region.Position.Y + region.Size.Y), 0, CachedImage.GetHeight());

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                if (CachedImage.GetPixel(x, y).A > 0.01f)
                    return false;
            }
        }

        return true;
    }

    public AtlasTexture BuildAtlasTile(Vector2I cell)
    {
        if (AtlasTexture == null)
            return null;

        int px = cell.X * CellSize.X;
        int py = cell.Y * CellSize.Y;

        return new AtlasTexture
        {
            Atlas = AtlasTexture,
            Region = new Rect2(px, py, CellSize.X, CellSize.Y)
        };
    }

    public Rect2 GetTileRegion(Vector2I cell)
    {
        return new Rect2(
            cell.X * CellSize.X,
            cell.Y * CellSize.Y,
            CellSize.X,
            CellSize.Y
        );
    }
    public void Clear()
    {
        AtlasTexture = null;
        //TextureSize = Vector2.Zero;
        IdMaterial = -1;
        Columns = 0;
        Rows = 0;
    }
}