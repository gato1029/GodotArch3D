using GodotEcsArch.sources.BlackyEngine.Core;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.Core;

public sealed class BlackyWorldRegistry
{
    private static readonly BlackyWorldRegistry _instance = new();
    public static BlackyWorldRegistry Instance => _instance;

    private readonly Dictionary<string, BlackyWorld> worlds = new();

    public BlackyWorld ActiveWorld { get; private set; }

    private BlackyWorldRegistry() { }

    public void AddWorld(string id, BlackyWorld world, bool setActive = false)
    {
        worlds[id] = world;

        if (setActive || ActiveWorld == null)
            ActiveWorld = world;
    }

    public BlackyWorld GetWorld(string id)
    {
        return worlds.TryGetValue(id, out var world) ? world : null;
    }

    public bool TryGetWorld(string id, out BlackyWorld world)
    {
        return worlds.TryGetValue(id, out world);
    }

    public IEnumerable<BlackyWorld> GetAllWorlds()
    {
        return worlds.Values;
    }

    public void RemoveWorld(string id)
    {
        if (worlds.ContainsKey(id))
        {
            if (ActiveWorld == worlds[id])
                ActiveWorld = null;

            worlds[id].Dispose();
            worlds.Remove(id);
        }
    }

    public void SetActiveWorld(string id)
    {
        if (worlds.TryGetValue(id, out var world))
            ActiveWorld = world;
    }
}