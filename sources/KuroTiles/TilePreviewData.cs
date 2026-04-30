using Godot;

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
