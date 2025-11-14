using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Multimesh;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
internal class DeathCleanupSystem : FlecsSystemBase
{
    protected override bool MultiThreaded => false;
    protected override ulong Phase => flecs.EcsPostUpdate;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<RenderGPUComponent>()
        .With<ColliderComponent>()
        .With<DeathTimerComponent>()
        .With<CharacterComponent>()        
        .With<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var gpuArray = it.Field<RenderGPUComponent>(0);
        var colArray = it.Field<ColliderComponent>(1);
        var timerArray = it.Field<DeathTimerComponent>(2);
        float dt = it.DeltaTime();
        for (int i = 0; i < it.Count(); i++)
        {            
            ref var timer = ref timerArray[i];
            timer.RemainingTime -= dt;
            if (timer.RemainingTime <= 0f)
            {
                var e = it.Entity(i);
                ref var gpu = ref gpuArray[i];
                ref var col = ref colArray[i];
                MultimeshManager.Instance.FreeInstance(gpu.rid, gpu.instance, gpu.idMaterial);
                CollisionManager.Instance.characterEntitiesFlecs.RemoveCollider(col.idCollider);
                e.Destruct();
            }
        }
    }
}