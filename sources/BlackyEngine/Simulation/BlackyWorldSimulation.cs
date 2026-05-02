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

        Flecs.WorldFlecs.SetCtx(world);       
    }

    public void Update(float delta)
    {
        Flecs.Update(delta);
    }
}
