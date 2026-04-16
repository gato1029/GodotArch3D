using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

public class SteeringSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    const int MAX_NEIGHBORS = 6;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveColliderComponent>()
          .With<SpatialIDComponent>()
          .With<VelocityComponent>()
          .With<SteeringComponent>()
          .With<MoveResolutorComponent>()
          .Without<SleepTag>();
    }
    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;
        var sim = world.simulationTick;

        // 🔥 AQUI VA (ANTES DE TODO)
        if ((sim.FrameIndex & 1) != 0)
            return;

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var velArray = it.Field<VelocityComponent>(3);
        var steeringArray = it.Field<SteeringComponent>(4);
        var resArray = it.Field<MoveResolutorComponent>(5);

        var dynGrid = world.DynamicHash;

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var sid = ref sidArray[i];
            ref var vel = ref velArray[i];
            ref var steering = ref steeringArray[i];
            ref var res = ref resArray[i];

            Vector2 desiredDir = steering.DesiredDir;

            if (desiredDir == Vector2.Zero)
            {
                vel.desiredVel = Vector2.Zero;
                continue;
            }

            if (desiredDir.LengthSquared() > 1.001f)
                desiredDir = desiredDir.Normalized();

            float radius = steering.SeparationRadius;
            float radiusSq = radius * radius;

            Vector2 avoidance = Vector2.Zero;
            int neighborCount = 0;

            Vector2 posFuture = pos.position + (steering.DesiredDir * vel.MaxSpeed * it.DeltaTime());

            float cx = posFuture.X + col.Offset.X;
            float cy = posFuture.Y + col.Offset.Y;

            var min = FastSpatialHash.WorldToTile(cx - radius, cy - radius);
            var max = FastSpatialHash.WorldToTile(cx + radius, cy + radius);

            // ======================================
            // 🔥 AVOIDANCE + CROWD DETECTION
            // ======================================
            bool done = false;
            for (int tx = min.X; tx <= max.X; tx++)
            {
                for (int ty = min.Y; ty <= max.Y; ty++)
                {
                    int cell = FastSpatialHash.GetHashDirect(tx, ty, dynGrid.TotalCells);
                    int idx = dynGrid.GetHead(cell);

                    while (idx != -1)
                    {
                        int otherSID = dynGrid.GetSpatialID(idx);

                        if (otherSID == sid.Value)
                        {
                            idx = dynGrid.GetNext(idx);
                            continue;
                        }

                        Entity other = dynGrid.GetEntity(idx);
                        ref var otherSid = ref other.GetMut<SpatialIDComponent>();

                        if ((sid.Mask & otherSid.Layer) == 0)
                        {
                            idx = dynGrid.GetNext(idx);
                            continue;
                        }

                        ref var otherPos = ref other.GetMut<PositionComponent>();
                        ref var otherCol = ref other.GetMut<MoveColliderComponent>();

                        float dx = cx - (otherPos.position.X + otherCol.Offset.X);
                        float dy = cy - (otherPos.position.Y + otherCol.Offset.Y);

                        float distSq = dx * dx + dy * dy;

                        if (distSq < 0.0001f)
                        {
                            idx = dynGrid.GetNext(idx);
                            continue;
                        }

                        if (distSq < radiusSq)
                        {
                            float dist = MathF.Sqrt(distSq);
                            float inv = 1f / dist;

                            float nx = dx * inv;
                            float ny = dy * inv;

                            float weight = 1f - (dist / radius);

                            avoidance.X += nx * weight;
                            avoidance.Y += ny * weight;

                            neighborCount++;
                            if (neighborCount >= MAX_NEIGHBORS)
                            {
                                done = true;
                                break;
                            }
                        }

                        idx = dynGrid.GetNext(idx);
                    }
                }
            }

            // ======================================
            // 🔥 CROWD PRESSURE
            // ======================================

            float crowd = MathF.Min(1f, neighborCount / 6f);
            float speedFactor = 1f - (crowd * 0.65f);

            float avoidanceWeight = steering.SeparationWeight;

            // ======================================
            // 🔥 FINAL DIRECTION
            // ======================================

            Vector2 finalDir =
                desiredDir * (1f - crowd) +
                avoidance * avoidanceWeight;

            float lenSq = finalDir.LengthSquared();

            if (lenSq < 0.0001f)
            {
                vel.desiredVel = Vector2.Zero;
                continue;
            }

            finalDir /= MathF.Sqrt(lenSq);

            // ======================================
            // 🔥 STUCK DETECTION (clave RTS)
            // ======================================

            float forwardProgress = finalDir.Dot(desiredDir);

            bool crowded = neighborCount >= 4;
            bool badDirection = forwardProgress < 0.01f;

            bool stuck = crowded || badDirection; // 

            if (stuck)
            {
                vel.desiredVel = Vector2.Zero;
                //res.Blocked = true;
                //res.BlockedTimer += it.DeltaTime();
                continue;
            }

            // ======================================
            // 🔥 APPLY VELOCITY
            // ======================================

            vel.desiredVel = finalDir * vel.MaxSpeed * speedFactor;
        }
    }
}