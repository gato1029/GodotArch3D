using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Group;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.KuroTiles;
internal class GroupManager:SingletonBase<GroupManager>
{
    System.Collections.Generic.Dictionary<int, GroupData> dictionary = new System.Collections.Generic.Dictionary<int, GroupData>();

    public void RegisterData(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<GroupData>(id);

            dictionary.Add(id, data);
        }
    }
    public void RegisterData(int id, GroupData data)
    {
        if (!dictionary.ContainsKey(id))
        {

            dictionary.Add(id, data);
        }
    }

    public GroupData GetData(int id)
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
