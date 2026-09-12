using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyEngine.Core;

namespace GodotFlecs.sources.Flecs.Systems.Generic;

internal class ResourceCreationSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // 🟢 Se ejecuta seguro en el hilo principal
    protected override bool HasQuery => false;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        // Cada servicio procesa su propia cola respetando su propio presupuesto por frame
        world.Services.ResourcePainter.ProcessPendingCommands();
        world.Services.ResourcePainter.ProcessPendingRemovals();
    }
}