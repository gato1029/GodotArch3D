using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles;
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
using CharacterComponent = GodotFlecs.sources.Flecs.Components.CharacterComponent;

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
            .With<CharacterComponent>()
            .With<UnitTag>()
            .Without<SleepTag>();
    }
    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;

        var sim = blackyWorld.simulationTick;

        var posArray = it.Field<PositionComponent>(0);
        var velArray = it.Field<VelocityComponent>(1);
        var moveArray = it.Field<MoveResolutorComponent>(2);
        var chaArray = it.Field<CharacterComponent>(3);

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

            // 🎨 movimiento suave
            pos.position += vel.desiredVel * dt;
            pos.height = 4;
            bool isMoving = vel.desiredVel.LengthSquared() > 0.0002f;

            if (!isMoving)
            {
                move.Blocked = true;
                cha.characterStateType = CharacterStateType.IDLE;

                // 🔥 timer determinista sin loop
                move.BlockedTimer += totalTickTime;
            }
            else
            {
                move.Blocked = false;
                move.BlockedTimer = 0f;
            }
        }
    }
}