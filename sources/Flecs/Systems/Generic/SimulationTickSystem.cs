using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyTiles;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Generic;

public class SimulationTick
{
    public float Accumulator;
    public float FixedDelta;
    public int TickCount; // 🔥 cuantos ticks ocurrieron este frame
    public int FrameIndex; // 🔥 índice del frame actual, para sistemas que quieran hacer algo cada N frames
}

public class SimulationTickSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsPreUpdate;
    protected override bool MultiThreaded => false; // 🔥 importante
    protected override bool HasQuery => false;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;
        var sim = blackyWorld.simulationTick;        

        float delta = (float)it.DeltaTime();

        sim.Accumulator += delta;
        sim.TickCount = 0;

        int maxTicks = 5;

        while (sim.Accumulator >= sim.FixedDelta && sim.TickCount < maxTicks)
        {
            sim.Accumulator -= sim.FixedDelta;
            sim.TickCount++;
        }
        sim.FrameIndex++;
    }
}