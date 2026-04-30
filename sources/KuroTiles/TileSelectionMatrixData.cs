using Godot;
using System.Collections.Generic;

namespace GodotFlecs.sources.KuroTiles;

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
