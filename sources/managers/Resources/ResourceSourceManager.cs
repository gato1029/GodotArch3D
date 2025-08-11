using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Resources;
public class ResourceSourceManager:SingletonBase<ResourceSourceManager>
{
    System.Collections.Generic.Dictionary<int, ResourceSourceData> dictionary = new System.Collections.Generic.Dictionary<int, ResourceSourceData>();

    public void RegisterData(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<ResourceSourceData>(id);

            dictionary.Add(id, data);
        }
    }
    public void RegisterData(int id, ResourceSourceData data)
    {
        if (!dictionary.ContainsKey(id))
        {

            dictionary.Add(id, data);
        }
    }

    public ResourceSourceData GetData(int id)
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
