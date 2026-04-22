using LiteDB;
using System;

namespace GodotEcsArch.sources.managers.Mods;

public class MaterialModData
{
    [BsonId]
    public string idNameMod { get; set; } // modName:idMaterial, este es el nombre del mod como tal tampoco se repite es unico, no importa si existen n modos el nombre no cambiara
    public int idTextureAtlas { get; set; } // esto nunca se repetira son numeros incrementales
    public string pathTextureAtlas { get; set; }
    public int xInAtlas { get; set; }
    public int yInAtlas { get; set; }
    public int widthAtlas { get; set; }
    public int heightAtlas { get; set; }    
    public int divisionPixelAtlasX { get; set; }
    public int divisionPixelAtlasY { get; set; }
    public long timeStamp { get; set; } = 0;

    public void UpdateTimeStamp()
    {
        timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
