using GodotEcsArch.sources.managers.Resources;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Buildings;
internal class BuildingManager : SingletonBase<BuildingManager>
{
    System.Collections.Generic.Dictionary<int, BuildingData> dictionary = new System.Collections.Generic.Dictionary<int, BuildingData>();

    public void RegisterData(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<BuildingData>(id);

            dictionary.Add(id, data);
        }
    }
    public void RegisterData(int id, BuildingData data)
    {
        if (!dictionary.ContainsKey(id))
        {

            dictionary.Add(id, data);
        }
    }

    public BuildingData GetData(int id)
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
