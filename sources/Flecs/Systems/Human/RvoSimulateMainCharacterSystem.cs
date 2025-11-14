using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using GodotFlecs.sources.Flecs.Systems.Human;
using RVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Human;
internal class RvoSimulateMainCharacterSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // debe ser single-thread
    const float BlockedThreshold = 0.8f;     // velocidad mínima considerada “quieto”
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<VelocityComponent>()
            .With<RvoAgentIdComponent>()
            .With<MoveResolutorComponent>()
            .With<CharacterComponent>()
            .With<PlayerInputComponent>();  // <-- Solo procesamos agentes que tienen target
    }

    protected override void OnIter(Iter it)
    {
        var velArray = it.Field<VelocityComponent>(0);
        var idArray = it.Field<RvoAgentIdComponent>(1);
        var extraArray = it.Field<MoveResolutorComponent>(2);
        var chaArray = it.Field<CharacterComponent>(3);

        // 3️⃣ Leer velocidades resultantes
        for (int i = 0; i < it.Count(); i++)
        {
            ref var vel = ref velArray[i];
            ref var extraMov = ref extraArray[i];
            ref var id = ref idArray[i];
            ref var cha = ref chaArray[i];

            vel.Value = Simulator.Instance.getAgentVelocity(id.Id);
            extraMov.positionFuture = Simulator.Instance.getAgentPosition(id.Id);           
        }
    }
}
