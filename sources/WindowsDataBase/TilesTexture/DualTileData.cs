
using GodotEcsArch.sources.managers.Mods;
using LiteDB;
using System.Collections.Generic;


namespace GodotEcsArch.sources.WindowsDataBase.TilesTexture;


// Tile dual principal
public class DualTileData
{    
    public int Height { get; set; }

    // Segmentos inferiores
    // El orden en la lista representa
    // la posición vertical
    public List<DualTilePart> Parts { get; set; } = new();

    public DualTilePart GetPart(int index)
    {
        return Parts[index];
    }
    public DualTilePart CreateDualTilePart()
    {
        var part = new DualTilePart(); // Inicializar con valores predeterminados
        Parts.Add(part);
        return part;
    }
    public void AddPart(DualTilePart part)
    {
        Parts.Add(part);
    }

    public void RemovePart(DualTilePart part)
    {
        Parts.Remove(part);
    }
    public DualTileData()
    {

    }
    public DualTileData(     
        int height
    )
    {        
        Height = height;
    }
}

// Parte inferior del dual tile
public class DualTilePart
{
    public int TileIndex { get; set; }

    public string IdMod { get; set; } = "";    

    public DualTilePart() { }
    public DualTilePart(
        int tileIndex,
        string idMod       

    )
    {
        TileIndex = tileIndex;
        IdMod = idMod;        
    }
}

public class DualTileSlot
{
    public int Slot { get; set; } // 0-15    
    public Dictionary<int, DualTileData> Data { get; set; } = new(); // altura -> DualTileData, para almacenar variantes por altura, altura 0 es el generico, sin variantes es decir aplica para todas las alturas
    public DualTileSlot(int slot)
    {
        Slot = slot;        
        CreateData(0); // Crear el dato genérico para este slot
    }
    public DualTileSlot()
    {
        
    }
    public void RemoveData(int height)
    {
        Data.Remove(height);
    }
    public IEnumerable<DualTileData> GetAllDualTileData()
    {
        return Data.Values;
    }
    public DualTileData CreateDataNextHeight()
    {
        int nextHeight = 1; // Comenzar desde 1, ya que 0 es el genérico
        while (Data.ContainsKey(nextHeight))
        {
            nextHeight++;
        }
        return CreateData(nextHeight);
    }
    public DualTileData CreateData(int height)
    {
        var data = new DualTileData(height);
        Data[height] = data;
        return data;
    }
    public DualTileData GetGeneric()
    {
        if (Data.TryGetValue(0, out var data))
        {
            return data;
        }
        return null;
    }
    public DualTileData GetData(int height)
    {
        return Data[height];
    }
    public bool HasData(int height)
    {
        return Data.ContainsKey(height);
    }

    public void AddData(DualTileData data)
    {
        Data[data.Height] = data;
    }
    public bool HasVariant()
    {
        if ( Data.Count>1 ) return true;
        return false;
    }
}
// Template/configuración de dual tiles
public class DualTileTemplate:IdDataLong
{
    // 16 slots
    // Cada slot almacena variantes por altura
    public DualTileSlot[] Slots { get; set; }
        = new DualTileSlot[16];

    public DualTileTemplate()
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = new DualTileSlot(i);
        }
    }

    [BsonCtor]
    public DualTileTemplate(DualTileSlot[] slots) 
    {
        var  data =slots[1].GetGeneric(); // Asegurar que el slot 0 tenga un dato genérico
        var  part = data.GetPart(0);
        var temp = AtlasModsManager.GetAtlasTexture(part.IdMod, part.TileIndex, out bool isAnimated, out TileTextureData tileTextureData);
        if (!isAnimated)
        {
            textureVisual = temp[0];            
        }
    }
    public DualTileSlot GetSlot(int slot)
    {
        return Slots[slot];
    }
    public DualTileSlot[] GetSlots()
    {
        return Slots;
    }
}
