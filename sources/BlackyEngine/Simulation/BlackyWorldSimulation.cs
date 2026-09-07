using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Simulation;

public sealed class BlackyWorldSimulation
{
    public FlecsManager Flecs { get; }
    public SimulationTick Tick { get; }

    public BlackyWorldSimulation(BlackyWorld world)
    {
        Flecs = new FlecsManager(NodeMainHelper.node3DMain);
        Tick = new SimulationTick();
        //Tick.FixedDelta = 1f/ 60; // 60 ticks por segundo
        //Tick.FixedDelta = 1f / 50; // 50 ticks por segundo
        Tick.FixedDelta = 1f / 30; // 30 ticks por segundo
        Flecs.WorldFlecs.SetCtx(world);       
    }

    public void Update(float delta)
    {
        Flecs.Update(delta);
    }
}
