using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.Flecs.Systems.Generic;

// Steering de hordas: toma la dirección del flow field (FlowFieldFollowSystem),
// comprueba si la celda de destino tiene lugar y avanza; si no, espera y reintenta;
// si falla demasiadas veces seguidas, se asienta como parte de la horda.
public class CrowdSteeringSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    private const float ArrivalBaseRadius = 0.5f;
    private const float ArrivalGrowth = 0.6f;
    private const float RetryInterval = 0.1f;
    private const int MaxAttempts = 4;
    private const float ProbeMargin = 0.2f;

    // Recto, +45°, -45° (en radianes)
    private static readonly float[] ConeOffsets = { 0f, Mathf.DegToRad(60f), -Mathf.DegToRad(60f) };

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()            // 0
          .With<MoveColliderComponent>()        // 1
          .With<SpatialIDComponent>()           // 2
          .With<VelocityComponent>()            // 3
          .With<MoveResolutorComponent>()       // 4
          .With<FlowFieldFollowerComponent>()   // 5
          .With<StateComponent>()               // 6
          .With<SteeringComponent>()            // 7 (para DesiredDir / animación)
          .With<MovementMassiveTag>()
          .Without<StoppedTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;
        var flowFieldManager = world.State.FlowFieldManager;
        if (flowFieldManager == null) return;
        var grid = world.State.GridSparseManager.Grid.DynamicLayer;               // capacidad por celda, crowd
        var terrain = world.State.GridSparseManager;    // terreno/edificios

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var velArray = it.Field<VelocityComponent>(3);
        var resArray = it.Field<MoveResolutorComponent>(4);
        var followerArray = it.Field<FlowFieldFollowerComponent>(5);
        var stateArray = it.Field<StateComponent>(6);
        var steeringArray = it.Field<SteeringComponent>(7);

        float dt = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var sid = ref sidArray[i];
            ref var vel = ref velArray[i];
            ref var res = ref resArray[i];
            ref var follower = ref followerArray[i];
            ref var state = ref stateArray[i];
            ref var steering = ref steeringArray[i];
            var e = it.Entity(i);

            // Sin campo (se está reconstruyendo): esperar sin contar intentos.
            if (follower.Status == 2)
            {
                GoIdle(ref vel, ref state, ref steering);
                continue;
            }

            // En la zona de meta: se asienta.
            if (follower.Status == 1)
            {
                Settle(e, ref vel, ref state, ref steering);
                continue;
            }

            // Dormida entre reintentos.
            if (res.RetryTimer > 0f)
            {
                res.RetryTimer -= dt;
                GoIdle(ref vel, ref state, ref steering);
                continue;
            }

            // Radio de llegada creciente: el primero llega al centro, y cada asentada
            // hace que las siguientes se detengan un poco más lejos (anillos concéntricos).
            int settled = flowFieldManager.GetSettledCount(follower.FlowFieldId);
            float arrivalRadius = ArrivalBaseRadius + MathF.Sqrt(settled / (float)CrowdSettings.Capacity) * ArrivalGrowth;

            if (pos.position.DistanceTo(follower.GoalPos) <= arrivalRadius)
            {
                if (!follower.Counted)
                {
                    follower.Counted = true;
                    flowFieldManager.AddSettled(follower.FlowFieldId);
                }

                Settle(e, ref vel, ref state, ref steering);
                continue;
            }

            Vector2 dir = follower.TargetDir;
            if (dir == Vector2.Zero)
            {
                GoIdle(ref vel, ref state, ref steering);
                continue;
            }

            dir = dir.Normalized();

            int currentIdx = grid.GetIndex(pos.position);
            float probe = col.Radius + ProbeMargin;
            float baseAngle = dir.Angle();

            // Mitad de la horda prefiere esquivar a un lado, la otra al contrario:
            // evita que todas se amontonen del mismo lado.
            bool flip = (sid.Value & 1) != 0;

            bool found = false;
            Vector2 bestDir = Vector2.Zero;

            for (int s = 0; s < ConeOffsets.Length; s++)
            {
                int k = (flip && s > 0) ? ConeOffsets.Length - s : s;

                Vector2 candDir = Vector2.Right.Rotated(baseAngle + ConeOffsets[k]);
                Vector2 candPos = pos.position + candDir * probe;
                int candIdx = grid.GetIndex(candPos);

                if (candIdx != currentIdx)
                {
                    // Terreno: lookup barato, solo al cruzar de celda.
                    if (terrain.IsBlocked(candPos))
                        //break;
                        continue;

                    // Lugar en la celda destino (reserva atómica).
                    if (!grid.TryEnterCell(candIdx, CrowdSettings.Capacity))
                       // break;
                        continue;
                }

                bestDir = candDir;
                found = true;
                break;
            }

            if (found)
            {
                res.ConsecutiveBlocks = 0;
                steering.DesiredDir = bestDir;
                vel.desiredVel = bestDir * vel.MaxSpeed;
                state.stateType = StateType.MOVING;
            }
            else
            {
                res.ConsecutiveBlocks++;
                // Pequeño desfase por unidad para que no reintenten todas en el mismo frame.
                res.RetryTimer = RetryInterval + (sid.Value % 5) * 0.03f;
                GoIdle(ref vel, ref state, ref steering);

                if (res.ConsecutiveBlocks >= MaxAttempts)
                {
                    res.ConsecutiveBlocks = 0;
                    Settle(e, ref vel, ref state, ref steering);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GoIdle(ref VelocityComponent vel, ref StateComponent state, ref SteeringComponent steering)
    {
        vel.desiredVel = Vector2.Zero;
        vel.currentVel = Vector2.Zero; // MovementResolutionSystem no corre en IDLE: sin velocidad fantasma
        steering.DesiredDir = Vector2.Zero;
        state.stateType = StateType.IDLE;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Settle(Entity e, ref VelocityComponent vel, ref StateComponent state, ref SteeringComponent steering)
    {
        GoIdle(ref vel, ref state, ref steering);

        if (!e.Has<StoppedTag>())
            e.Add<StoppedTag>();
    }
}