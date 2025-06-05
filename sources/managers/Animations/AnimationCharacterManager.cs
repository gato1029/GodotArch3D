using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;

namespace GodotEcsArch.sources.managers.Animations;
internal class AnimationCharacterManager : SingletonBase<AnimationCharacterManager>
{
    Dictionary<int, AnimationCharacterBaseData> dictionary = new Dictionary<int, AnimationCharacterBaseData>();
    protected override void Initialize()
    {
    }

    public void RegisterCharacterBase(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<AnimationCharacterBaseData>(id);
            dictionary.Add(id, data);
        }
    }
    public void RegisterCharacterBase(int id, AnimationCharacterBaseData data)
    {
        if (!dictionary.ContainsKey(id))
        {
            dictionary.Add(id, data);
        }
    }

    public AnimationCharacterBaseData GetCharacterBaseData(int id)
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
}
