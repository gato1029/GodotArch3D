using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Terrain;
internal class TerrainManager:SingletonBase<TerrainManager>
{
    System.Collections.Generic.Dictionary<int, TerrainData> dictionary = new System.Collections.Generic.Dictionary<int, TerrainData>();

    public void RegisterData(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<TerrainData>(id);
            dictionary.Add(id, data);
        }
    }
    public void RegisterUpdateData(int id, TerrainData data)
    {
        if (!dictionary.ContainsKey(id))
        {
            dictionary.Add(id, data);
        }
        else
        {
            dictionary[id] = data;
        }
    }

    public TerrainData GetData(int id)
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
    protected override void Initialize()
    {

    }

    protected override void Destroy()
    {

    }

    public bool CheckExist(SpriteData spriteData)
    {
        var atlasA = MaterialManager.Instance.GetAtlasTextureInternal(spriteData);

        foreach (var item in dictionary)
        {
            var atlasB = MaterialManager.Instance.GetAtlasTextureInternal(item.Value.spriteData);
            if (TextureHelper.AreAtlasTexturesFullyEqual(atlasA, atlasB))
            {
                return true;
            }
            
        }
        return false;
    }
}
