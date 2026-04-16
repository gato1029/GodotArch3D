using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.WindowsDataBase.TileCreator;

public struct TileSpriteDataPair
{
    public ushort group;
    public ushort idSave;
}

internal class ManagerTileSpriteData:SingletonBase<ManagerTileSpriteData>
{
    public Dictionary<long, TileSpriteData> tileSpriteDataById = new Dictionary<long, TileSpriteData>();
    public Dictionary<TileSpriteDataPair, long> keyValuePairs = new Dictionary<TileSpriteDataPair, long>(); 

    public TileSpriteData GetTileSpriteDataById(long id)
    {
        if (tileSpriteDataById.TryGetValue(id, out TileSpriteData data))
        {
            return data;
        }
        return null; // o cualquier valor que indique que no se encontró
    }

    public long GetIdByGroupAndIdSave(ushort group, ushort idSave)
    {
        var key = new TileSpriteDataPair { group = group, idSave = idSave };
        if (keyValuePairs.TryGetValue(key, out long id))
        {
            return id;
        }
        return -1; // o cualquier valor que indique que no se encontró
    }
}
