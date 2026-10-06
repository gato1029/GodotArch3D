using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

public class SteeringSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

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

            // =====================================================
            // DATOS DE SEPARACIÓN
            // =====================================================
            float radius = steering.SeparationRadius;
            float radiusSq = radius * radius;

            Vector2 avoidance = Vector2.Zero;
            int neighborCount = 0;

            // =====================================================
            // POSICIÓN FUTURA
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

                            //if (neighborCount >= MAX_NEIGHBORS)
                            //    break;
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
                float side = (sid.Value % 2 == 0) ? 1f : -1f;
                Vector2 worldPerp = new Vector2(0, 1) * side;
                avoidanceLateral += worldPerp * 0.15f;
            }

            Vector2 finalDir = hasDesiredDir
                ? desiredDir * (1f - crowdInfluence) + avoidanceLateral * avoidanceWeight
                : avoidanceLateral * avoidanceWeight;

            float lenSq = finalDir.LengthSquared();

            if (lenSq < 0.0001f)
            {
                vel.desiredVel = Vector2.Zero;

                if (hasDesiredDir)
                {
                    res.BlockedTimer += it.DeltaTime();
                }

                continue;
            }

            finalDir /= MathF.Sqrt(lenSq);

            // =====================================================
            // DETECCIÓN DE BLOQUEO / ASENTAMIENTO EN HORDA
            // =====================================================
            if (hasDesiredDir)
            {
                float forwardProgress = finalDir.Dot(desiredDir);

                bool crowded = neighborCount >= 4;
                bool badDirection = forwardProgress < 0.05f;

                if (crowded && badDirection)
                {
                    res.BlockedTimer += it.DeltaTime();

                    // -------------------------------------------------
                    // Lleva un rato sostenido sin progresar, rodeada de
                    // gente: la ASENTAMOS definitivamente como parte de
                    // la horda, en vez de bloquearla temporalmente (eso
                    // generaba el titileo). Cubre tanto colisión directa
                    // como el caso de "orbita sin avanzar nunca" que solo
                    // el avoidance puede detectar.
                    // -------------------------------------------------
                    if (res.BlockedTimer >= 0.6f)
                    {
                        vel.desiredVel = Vector2.Zero;
                        vel.currentVel = Vector2.Zero; // <- frenar también la velocidad real

                        // Corrección de último momento: si al asentarnos quedamos
                        // solapados con un vecino, nos separamos ANTES de congelar
                        // la posición para siempre.
                        ResolveOverlapOnSettle(ref pos, ref col, sid, dynGrid);

                        if (!it.Entity(i).Has<StoppedTag>())
                            it.Entity(i).Add<StoppedTag>();

                        continue;
                    }
                }
                else
                {
                    // Progresando bien: decaimiento gradual, no reseteo
                    // instantáneo — evita que fluctuaciones puntuales
                    // borren todo el progreso acumulado hacia el asentamiento.
                    res.BlockedTimer = MathF.Max(0f, res.BlockedTimer - it.DeltaTime() * 0.5f);
                }
            }

            // =====================================================
            // VELOCIDAD
            // =====================================================
            float speedFactor = 1f - (crowd * 0.65f);

            vel.desiredVel = finalDir * vel.MaxSpeed * speedFactor;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResolveOverlapOnSettle(
    ref PositionComponent pos,
    ref MoveColliderComponent col,
    SpatialIDComponent sid,
    FastSpatialHash dynGrid)
    {
        Span<int> neighbors = stackalloc int[8];
        var count = dynGrid.QueryNodesBoundedClosestLayersFiltered(pos.position.X, pos.position.Y, col.Radius * 2f, 0, neighbors);

        for (int ii = 0; ii < count; ii++)
        {
            int otherSid = dynGrid.GetSpatialID(neighbors[ii]);
            if (sid.Value == otherSid) continue;

            Entity other = dynGrid.GetEntity(neighbors[ii]);
            if (!other.IsAlive() || other.Has<DeadTag>()) continue;

            ref var otherPos = ref other.GetMut<PositionComponent>();
            ref var otherCol = ref other.GetMut<MoveColliderComponent>();

            float dx = pos.position.X - otherPos.position.X;
            float dy = pos.position.Y - otherPos.position.Y;
            float distSq = dx * dx + dy * dy;
            float minDist = col.Radius + otherCol.Radius;

            if (distSq < minDist * minDist && distSq > 0.0001f)
            {
                float dist = MathF.Sqrt(distSq);
                float overlap = minDist - dist;
                float inv = 1f / dist;

                // Empujamos SOLO a esta unidad (la que se está asentando
                // recién ahora) — la otra puede ya estar asentada/congelada,
                // así que no la movemos a ella para no reabrir otro solape.
                pos.position += new Vector2(dx * inv, dy * inv) * overlap;
            }
        }
    }

}