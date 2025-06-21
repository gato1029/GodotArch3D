using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;

namespace GodotEcsArch.sources.managers.Characters;
public  class CharacterModelManager: SingletonBase<CharacterModelManager>
{
    public Dictionary<int, CharacterModelBaseData> dictionary;

    protected override void Initialize()
    {
        dictionary = new Dictionary<int, CharacterModelBaseData>();
    }

    public void RegisterCharacterModel(int id, CharacterModelBaseData data)
    {

        if (!dictionary.ContainsKey(id))
        {
            dictionary.Add(id, data);
        }
        else
        {
            if (id == -1)
            {
                dictionary[id] = data;
            }
        }

    }

    public CharacterModelBaseData GetCharacterModel(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            CharacterModelBaseData data = DataBaseManager.Instance.FindById<CharacterModelBaseData>(id);
            RegisterCharacterModel(id, data);
            return data;
        }
        if (dictionary.ContainsKey(id))
        {
            return dictionary[id];
        }
        return null;
    }
}
