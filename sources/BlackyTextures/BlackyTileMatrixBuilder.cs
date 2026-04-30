using Godot;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTextures;

public static class BlackyTileMatrixBuilder
{
    public static TileSelectionMatrixData Build(
        BlackyTileAtlasContext context,
        BlackyAtlasOccupancyMap occupancy,
        Vector2I start,
        Vector2I end)
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
        data.idMaterial = context.IdMaterial;
        data.cellSize = context.CellSize;

        // se mantiene TU convención exacta
        data.matrix = new TilePreviewData[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2I atlasCell = new Vector2I(startX + x, startY + y);

                Rect2 region = context.GetTileRegion(atlasCell);

                TilePreviewData preview = new TilePreviewData
                {
                    idMaterial = context.IdMaterial,
                    index = context.CellToIndex(atlasCell),

                    localX = x,
                    localY = y,

                    atlasX = (int)region.Position.X,
                    atlasY = (int)region.Position.Y,

                    width = context.CellSize.X,
                    height = context.CellSize.Y,

                    texture = context.BuildAtlasTile(atlasCell),
                    isEmpty = occupancy.IsEmpty(atlasCell.X, atlasCell.Y)
                };

                // mantenemos tu convención matrix[x,y]
                data.matrix[x, y] = preview;
            }
        }

        return data;
    }
}
