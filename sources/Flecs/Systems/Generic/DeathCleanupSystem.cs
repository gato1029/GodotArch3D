using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
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
        .With<SpatialIDComponent>()
        .With<DeathTimerComponent>()
        .With<CharacterComponent>()        
        .With<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var dynGrid = world.State.DynamicHash;

        var gpuArray = it.Field<RenderGPUComponent>(0);
        var spatialArray = it.Field<SpatialIDComponent>(1);
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
                ref var spatial = ref spatialArray[i];
                AtlasTexturesModsManager.Instance.FreeInstance(gpu.rid, gpu.instance);
                dynGrid.UnregisterDirect(spatial.Value);
                e.Destruct();
                Entity entity = it.Entity(i);
                world.Tick.TotalUnits--;
                world.Tick.UpdateGroupCount();
                if (entity.Has<RvoAgentDebugComponent>())
                {
                    var agentDebug= entity.Get<RvoAgentDebugComponent>();
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeMove);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeBody);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeRadiusAttack);
                    CollisionShapeDraw.Instance.FreeDraw(agentDebug.idShapeRadiusRangeSearch);                    
                }
            }
        }
    }
}