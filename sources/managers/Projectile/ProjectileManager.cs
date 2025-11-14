using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Projectile;
internal class ProjectileManager:SingletonBase<ProjectileManager>
{
    System.Collections.Generic.Dictionary<int, BulletData> dictionary = new System.Collections.Generic.Dictionary<int, BulletData>();

    public void RegisterData(int id)
    {
        if (!dictionary.ContainsKey(id))
        {
            var data = DataBaseManager.Instance.FindById<BulletData>(id);

            dictionary.Add(id, data);
        }
    }
    public void RegisterData(int id, BulletData data)
    {
        if (!dictionary.ContainsKey(id))
        {

            dictionary.Add(id, data);
        }
    }

    public BulletData GetData(int id)
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
