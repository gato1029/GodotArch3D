using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Spatial;
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
          .Without<StoppedTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var velArray = it.Field<VelocityComponent>(3);
        var steeringArray = it.Field<SteeringComponent>(4);
        var resArray = it.Field<MoveResolutorComponent>(5);

        var dynGrid = world.State.DynamicHash;

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var sid = ref sidArray[i];
            ref var vel = ref velArray[i];
            ref var steering = ref steeringArray[i];
            ref var res = ref resArray[i];

            if (res.Blocked)
            {
                vel.desiredVel = Vector2.Zero;
                continue;
            }

            // =====================================================
            // DIRECCIÓN DESEADA
            // =====================================================
            Vector2 desiredDir = steering.DesiredDir;
            bool hasDesiredDir = desiredDir != Vector2.Zero;

            if (hasDesiredDir && desiredDir.LengthSquared() > 1.001f)
            {
                desiredDir = desiredDir.Normalized();
            }

            // NOTA: ya NO salimos aquí cuando desiredDir es cero.
            // Una unidad sin dirección (por ejemplo, llegó al disco
            // de destino de un flow field) igual necesita avoidance
            // para separarse de otras que converjan al mismo punto.

            // =====================================================
            // DATOS DE SEPARACIÓN
            // =====================================================
            float radius = steering.SeparationRadius;
            float radiusSq = radius * radius;

            Vector2 avoidance = Vector2.Zero;
            int neighborCount = 0;

            // =====================================================
            // POSICIÓN FUTURA (si no hay desiredDir, usamos la posición actual)
            // =====================================================
            Vector2 posFuture = hasDesiredDir
                ? pos.position + desiredDir * vel.MaxSpeed * it.DeltaTime()
                : pos.position;

            float cx = posFuture.X + col.Offset.X;
            float cy = posFuture.Y + col.Offset.Y;

            var min = FastSpatialHash.WorldToTile(cx - radius, cy - radius);
            var max = FastSpatialHash.WorldToTile(cx + radius, cy + radius);

            // =====================================================
            // AVOIDANCE + CROWD DETECTION
            // =====================================================
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
                                break;
                        }

                        idx = dynGrid.GetNext(idx);
                    }

                    if (neighborCount >= MAX_NEIGHBORS)
                        break;
                }

                if (neighborCount >= MAX_NEIGHBORS)
                    break;
            }

            // =====================================================
            // CROWD
            // =====================================================
            float crowd = MathF.Min(1f, neighborCount / 6f);
            float crowdInfluence = crowd * 0.35f;

            // =====================================================
            // DIRECCIÓN FINAL
            // =====================================================
            float avoidanceWeight = steering.SeparationWeight;
            Vector2 avoidanceLateral = avoidance;

            if (hasDesiredDir)
            {
                float avoidAlongDesired = avoidance.Dot(desiredDir);
                if (avoidAlongDesired < 0f)
                {
                    avoidanceLateral -= desiredDir * avoidAlongDesired;
                }

                if (avoidance.LengthSquared() > 0.0001f)
                {
                    float side = (sid.Value % 2 == 0) ? 1f : -1f;
                    Vector2 perp = new Vector2(-desiredDir.Y, desiredDir.X) * side;
                    avoidanceLateral += perp * 0.15f;
                }
            }
            else if (avoidance.LengthSquared() > 0.0001f)
            {
                // Sin dirección propia: usamos un eje fijo del mundo
                // (no relativo a desiredDir, porque no existe) para
                // seguir rompiendo empates simétricos entre vecinos.
                float side = (sid.Value % 2 == 0) ? 1f : -1f;
                Vector2 worldPerp = new Vector2(0, 1) * side;
                avoidanceLateral += worldPerp * 0.15f;
            }

            Vector2 finalDir = hasDesiredDir
                ? desiredDir * (1f - crowdInfluence) + avoidanceLateral * avoidanceWeight
                : avoidanceLateral * avoidanceWeight; // pura separación, sin avance

            float lenSq = finalDir.LengthSquared();

            if (lenSq < 0.0001f)
            {
                vel.desiredVel = Vector2.Zero;

                // Solo acumulamos BlockedTimer si la unidad REALMENTE
                // quería avanzar a algún lado y no pudo — una unidad
                // sin desiredDir (ya llegó, sin vecinos que la empujen)
                // está correctamente detenida, no "bloqueada".
                if (hasDesiredDir)
                {
                    res.BlockedTimer += it.DeltaTime();
                }

                continue;
            }

            finalDir /= MathF.Sqrt(lenSq);

            // =====================================================
            // DETECCIÓN DE BLOQUEO — solo aplica si había intención
            // real de avanzar hacia algo (hasDesiredDir).
            // =====================================================
            if (hasDesiredDir)
            {
                float forwardProgress = finalDir.Dot(desiredDir);

                bool crowded = neighborCount >= 4;
                bool badDirection = forwardProgress < 0.05f;

                if (crowded && badDirection)
                {
                    res.BlockedTimer += it.DeltaTime();

                    if (res.BlockedTimer >= 0.5f)
                    {
                        res.Blocked = true;
                        vel.desiredVel = Vector2.Zero;
                        continue;
                    }
                }

                if (res.BlockedTimer >= 0.35f)
                {
                    res.Blocked = true;
                    vel.desiredVel = Vector2.Zero;
                    continue;
                }
            }

            // =====================================================
            // VELOCIDAD
            // =====================================================
            float speedFactor = 1f - (crowd * 0.65f);

            vel.desiredVel = finalDir * vel.MaxSpeed * speedFactor;
        }
    }
}