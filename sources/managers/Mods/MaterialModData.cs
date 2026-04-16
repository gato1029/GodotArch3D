using LiteDB;
using System;

namespace GodotEcsArch.sources.managers.Mods;

public class MaterialModData
{
    [BsonId]
    public string idNameMod { get; set; } // modName:idMaterial           
    public int idTextureAtlas { get; set; }
    public string pathTextureAtlas { get; set; }
    public int xInAtlas { get; set; }
    public int yInAtlas { get; set; }
    public long timeStamp { get; set; } = 0;

    public void UpdateTimeStamp()
    {
        timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
