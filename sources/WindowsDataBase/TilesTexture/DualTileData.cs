
using System.Collections.Generic;


namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;


// Tile dual principal
public class DualTileData
{
    // Tile principal/top
    public int TileIndex { get; set; }

    // ID de textura/atlas
    public string TextureId { get; set; } = "";

    // Tipo de altura
    // 1,2,3...
    public int Height { get; set; }

    // Segmentos inferiores
    // El orden en la lista representa
    // la posición vertical
    public List<DualTilePart> Parts { get; set; } = new();

    public DualTileData(
        int tileIndex,
        string textureId,
        int height
    )
    {
        TileIndex = tileIndex;
        TextureId = textureId;
        Height = height;
    }
}

// Parte inferior del dual tile
public class DualTilePart
{
    public int TileIndex { get; set; }

    public string TextureId { get; set; } = "";

    public DualTilePart(
        int tileIndex,
        string textureId
    )
    {
        TileIndex = tileIndex;
        TextureId = textureId;
    }
}

// Template/configuración de dual tiles
public class DualTileTemplate:IdDataLong
{
    // 16 slots
    // Cada slot almacena variantes por altura
    public Dictionary<int, DualTileData>[] Slots { get; set; }
        = new Dictionary<int, DualTileData>[16];

    public DualTileTemplate()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = new Dictionary<int, DualTileData>();
        }
    }

    // Agrega/reemplaza variante
    public void Add(
        int slot,
        DualTileData data
    )
    {
        Slots[slot][data.Height] = data;
    }

    // Obtiene variante por altura
    public DualTileData Get(
        int slot,
        int height
    )
    {
        return Slots[slot][height];
    }

    // Verifica si existe
    public bool Has(
        int slot,
        int height
    )
    {
        return Slots[slot].ContainsKey(height);
    }
}
