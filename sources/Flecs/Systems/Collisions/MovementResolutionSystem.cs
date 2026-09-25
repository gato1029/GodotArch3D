
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateComponent = GodotFlecs.sources.Flecs.Components.StateComponent;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

public class MovementResolutionSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
            .With<VelocityComponent>() // 🔥 ahora necesario
            .With<MoveResolutorComponent>()
            .With<StateComponent>()
            .With<UnitTag>()
            .Without<StoppedTag>();
    }
    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        var _pathRegistry = blackyWorld.State.PathRegistryManager;
        if (blackyWorld == null) return;

        var sim = blackyWorld.Simulation.Tick;

        var posArray = it.Field<PositionComponent>(0);
        var velArray = it.Field<VelocityComponent>(1);
        var moveArray = it.Field<MoveResolutorComponent>(2);
        var chaArray = it.Field<StateComponent>(3);

        float dt = (float)it.DeltaTime(); // 🎨 movimiento
        float tick = sim?.FixedDelta ?? 0f;
        int tickCount = sim?.TickCount ?? 0;

        // 🔥 acumulación directa
        float totalTickTime = tick * tickCount;

        for (int i = 0; i < it.Count(); i++)
        {
            Entity e = it.Entity(i);

            ref var pos = ref posArray[i];
            ref var vel = ref velArray[i];
            ref var move = ref moveArray[i];
            ref var cha = ref chaArray[i];
            if (move.Blocked)
            {
              
                cha.stateType = StateType.IDLE;              
                vel.desiredVel = Vector2.Zero;                
                if (e.Has<PathReferenceComponent>())
                {
                    var path = e.Get<PathReferenceComponent>();
                    int pathIdToRelease = path.PathId;
                    _pathRegistry.EnqueueReleasePath(pathIdToRelease);

                    // Limpiamos componentes de ruta
                    e.Remove<PathReferenceComponent>();
                }

                e.Add<StoppedTag>();
                continue;
            }
            if (cha.stateType != StateType.MOVING) //solo mover si el estado es MOVING
            {
                continue;
            }

            bool isMoving = vel.desiredVel.LengthSquared() > 0.0002f;

            if (!isMoving)
            {                
                cha.stateType = StateType.IDLE; 
            }
            else
            {   // 🎨 movimiento suave
                pos.position += vel.desiredVel * dt; 
                pos.tilePosition = TilesHelper.WorldPositionToTile(pos.position);
                pos.height = blackyWorld.Services.HeightMapWorld.GetTopHeight(pos.tilePosition);
                move.Blocked = false;                
            }
        }
    }
}