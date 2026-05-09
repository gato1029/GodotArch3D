
using System.Collections.Generic;


namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;


// Tile dual principal
public class DualTileData
{
    // Tile principal/top
    public DualTilePart MainTile { get; set; }
    
    // Tipo de altura
    // 1,2,3...
    public int Height { get; set; }

    // Segmentos inferiores
    // El orden en la lista representa
    // la posición vertical
    public List<DualTilePart> Parts { get; set; } = new();

    public DualTileData(
        DualTilePart mainTile,
        int height
    )
    {
        this.MainTile = mainTile;        
        Height = height;
    }
}

// Parte inferior del dual tile
public class DualTilePart
{
    public int TileIndex { get; set; }

    public string IdMod { get; set; } = "";    
    public DualTilePart(
        int tileIndex,
        string idMod       

    )
    {
        TileIndex = tileIndex;
        IdMod = idMod;        
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
