using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using RVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems;
internal class RvoDeltaSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<VelocityComponent>()
         .With<RvoAgentIdComponent>()
         .With<CharacterComponent>()
         .With<MoveResolutorComponent>()
         .With<UseRvoTag>();
    }

    protected override void OnIter(Iter it)
    {


        var velArray = it.Field<VelocityComponent>(0);
        var idArray = it.Field<RvoAgentIdComponent>(1);

        // 3️⃣ Leer velocidades resultantes
        for (int i = 0; i < it.Count(); i++)
        {
            ref var vel = ref velArray[i];
            ref var id = ref idArray[i];
            if (vel.prefVel == Godot.Vector2.Zero)
            {
                //Simulator.Instance.setAgentMaxSpeed(id.Id, 0);
                //Simulator.Instance.setAgentSleep(id.Id, true);
            }
            else
            {
                Simulator.Instance.setAgentPrefVelocity(id.Id, vel.prefVel);
            }

        }


    }

}


internal class RvoDeltaSimpleSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RvoDeltaTag>();
    }

    protected override void OnIter(Iter it)
    {
        float dt = it.DeltaTime();   
        Simulator.Instance.setTimeStep(dt);
        Simulator.Instance.doStep();
        
    }
}
