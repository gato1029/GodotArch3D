using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Multimesh;
using GodotEcsArch.sources.managers.Projectile;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Generic;

internal class ProjectileSpawnSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // 🟢 Hilo principal
    protected override bool HasQuery => false;  

    protected override void BuildQuery(ref QueryBuilder qb)
    {
 
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        // Procesa todas las creaciones pendientes de forma segura
        world.Services.ArrowPool.ProcessPendingSpawns();
        world.Services.ArrowPool.ProcessPendingRecycles();
    }
}
