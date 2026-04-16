using GodotEcsArch.sources.BlackyTiles;
using GodotFlecs.sources.Flecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs;

public class CurrentWorlds: SingletonBase<CurrentWorlds>
{
    public Dictionary<string,BlackyWorld> worlds { get; set; } = new Dictionary<string, BlackyWorld>();

    public BlackyWorld GetWorld(string key)
    {
        if (worlds.TryGetValue(key, out var manager))
        {
            return manager;
        }
        return null;
    }

    public void AddWorld(string key, BlackyWorld manager)
    {
        if (!worlds.ContainsKey(key))
        {
            worlds.Add(key, manager);
        }
    }
    public void RemoveWorld(string key)
    {
        if (worlds.ContainsKey(key))
        {
            worlds.Remove(key);
        }
    }

    public List<BlackyWorld> GetAllWorld()
    {
        return worlds.Values.ToList();
    }

    public void ClearAll()
    {
        worlds.Clear();
    }


}
