using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Maps;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.TileSprite;
internal class TileSpriteSpawnSystem:FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsPreUpdate; // ✅ igual que BatchPrecalSystem
    protected override bool MultiThreaded => false;
    protected override bool HasQuery => false;

    protected override void BuildQuery(ref QueryBuilder qb)
    {

    }

    protected override void OnIter(Iter it)
    {
        // Si no hay nada que spawnear, no hacemos nada
        if (GlobalData.PendingBuildingQueue.Count == 0)
            return;

        var world = it.World();

        // Modo diferido por seguridad (mejor performance en masa)
        world.DeferBegin();

        while (GlobalData.PendingBuildingQueue.Count > 0)
        {
            var (pos, id) = GlobalData.PendingBuildingQueue.Dequeue();
            MapManagerEditor.Instance.CurrentMapLevelData.mapBuildings.AddUpdateTile(pos, id);
        }

        world.DeferEnd();
    }
}
