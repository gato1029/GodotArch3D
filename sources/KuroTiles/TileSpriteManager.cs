using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Group;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.KuroTiles;

internal class TileSpriteManager : SingletonBase<TileSpriteManager>
{
    System.Collections.Generic.Dictionary<long, TileSpriteData> dictionary = new System.Collections.Generic.Dictionary<long, TileSpriteData>();

    public void RegisterData(long id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<TileSpriteData>(id);

            dictionary.Add(id, data);
        }
    }
    public void RegisterData(long id, TileSpriteData data)
    {
        if (!dictionary.ContainsKey(id))
        {

            dictionary.Add(id, data);
        }
    }

    public TileSpriteData GetData(long id)
    {
        if (dictionary.ContainsKey(id))
        {
            return dictionary[id];
        }
        else
        {
            RegisterData(id);
            return dictionary[id];
        }
    }
}
