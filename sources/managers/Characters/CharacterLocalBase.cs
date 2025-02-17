using Godot;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Characters;
public class CharacterLocalBase:SingletonBase<CharacterLocalBase>
{
    Dictionary<int,CharacterBaseData> dictionary = new Dictionary<int,CharacterBaseData>();
    protected override void Initialize()
    {
    }

    public void RegisterCharacterBase(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<CharacterBaseData>(id);
            dictionary.Add(id,data);
        }
    }
    public void RegisterCharacterBase(int id, CharacterBaseData data)
    {
        if (!dictionary.ContainsKey(id))
        {            
            dictionary.Add(id, data);
        }
    }

    public CharacterBaseData GetCharacterBaseData(int id)
    {
        if (dictionary.ContainsKey(id))
        {
            return dictionary[id];
        }
        else
        {
            RegisterCharacterBase(id);
            return dictionary[id];
        }        
    }

    protected override void Destroy()
    {
     
    }
}
