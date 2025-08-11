using Godot.Collections;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Resources;
internal class ResourcesGameManager: SingletonBase<ResourcesGameManager>
{
    System.Collections.Generic.Dictionary<int,ResourceData> dictionary = new System.Collections.Generic.Dictionary<int,ResourceData>();

    public void RegisterData(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<ResourceData>(id);
           
            dictionary.Add(id, data);
        }
    }
    public void RegisterData(int id, ResourceData data)
    {        
        if (!dictionary.ContainsKey(id))
        {
        
            dictionary.Add(id, data);
        }
    }

    public ResourceData GetData(int id)
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
}
