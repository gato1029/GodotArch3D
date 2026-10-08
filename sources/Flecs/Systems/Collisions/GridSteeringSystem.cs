using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

public class GridSteeringSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // necesario: coordinación secuencial dentro del frame

    private const float RetryInterval = 0.5f;
    private const int MaxAttempts = 5;
    private const float LookAheadMargin = 0.05f;
    private const float cellSize = 0.5f; // tamaño de celda para el hash espacial

    private static readonly float[] ConeOffsetsDeg = { 0f, 30f, -30f, 60f, -60f };
    private const float AvoidanceHoldTime = 0.2f;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveColliderComponent>()
          .With<SpatialIDComponent>()
          .With<VelocityComponent>()
          .With<SteeringComponent>()
          .With<MoveResolutorComponent>()
          .With<StateComponent>()
          .Without<StoppedTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var gridManager = world.State.GridSparseManager; // solo para terreno estático
        var dynGrid = world.State.DynamicHash;            // fuente real de colisión entre unidades

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var velArray = it.Field<VelocityComponent>(3);
        var steeringArray = it.Field<SteeringComponent>(4);
        var resArray = it.Field<MoveResolutorComponent>(5);
        var stateArray = it.Field<StateComponent>(6);

        float dt = it.DeltaTime();

        Span<int> neighbors = stackalloc int[8];

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var sid = ref sidArray[i];
            ref var vel = ref velArray[i];
            ref var steering = ref steeringArray[i];
            ref var res = ref resArray[i];
            ref var state = ref stateArray[i];
            var e = it.Entity(i);

            if (res.RetryTimer > 0f)
            {
                res.RetryTimer -= dt;                
                res.AvoidanceTimer = 0f;
                GoIdle(ref vel, ref state, ref steering);
                continue;
            }

            if (res.Blocked)
            {                
                res.AvoidanceTimer = 0f;
                GoIdle(ref vel, ref state, ref steering);
                continue;
            }

            Vector2 targetDir = steering.TargetDir;

            if (targetDir == Vector2.Zero)
            {
                res.AvoidanceTimer = 0f;
                GoIdle(ref vel, ref state, ref steering);
                continue;
            }

            targetDir = targetDir.Normalized();

            if (res.AvoidanceTimer > 0f)
            {
                res.AvoidanceTimer -= dt;
            }

            Vector2 currentCenter = pos.position + col.Offset;

            float probeDist = col.Radius + LookAheadMargin;
            float baseAngle = targetDir.Angle();
            bool found = false;
            Vector2 bestDir = Vector2.Zero;
            int chosenIndex = 0;

            int lastIdx = res.LastConeIndex;
            bool inAvoidanceHold = res.AvoidanceTimer > 0f && lastIdx > 0 && lastIdx < ConeOffsetsDeg.Length;
            Vector2I currentCell = ToCell(currentCenter, cellSize);
            for (int step = 0; step < ConeOffsetsDeg.Length; step++)
            {
                int s;

                if (inAvoidanceHold)
                {
                    if (step == 0) s = lastIdx;
                    else if (step == 1) s = 0;
                    else
                    {
                        s = step;
                        if (s == lastIdx || s == 0) continue;
                    }
                }
                else
                {
                    if (step == 0) s = 0;
                    else if (step == 1 && lastIdx > 0 && lastIdx < ConeOffsetsDeg.Length) s = lastIdx;
                    else
                    {
                        s = step;
                        if (s == lastIdx) continue;
                    }
                }

                float angle = baseAngle + Mathf.DegToRad(ConeOffsetsDeg[s]);
                Vector2 candidateDir = Vector2.Right.Rotated(angle);
                Vector2 candidatePos = currentCenter + candidateDir * probeDist;


                Vector2I candidateCell = ToCell(candidatePos, cellSize);

                // Si seguimos dentro de la misma celda que ya ocupamos,
                // no hace falta validar nada — ya estamos físicamente ahí.
                if (candidateCell != currentCell)
                {
                    if (gridManager.IsBlocked(candidatePos))
                    {
                        //break;
                        continue;
                    }

                }

                if (CheckUnitsAtCandidate(candidatePos, col.Radius, sid.Value, neighbors, dynGrid))
                    continue;


                bestDir = candidateDir;
                found = true;
                chosenIndex = s;
                break;
            }

            if (found)
            {
                if (chosenIndex > 0)
                {
                    if (!inAvoidanceHold || chosenIndex != lastIdx)
                    {
                        res.AvoidanceTimer = AvoidanceHoldTime;
                    }
                }
                else
                {
                    res.AvoidanceTimer = 0f;
                }

                res.LastConeIndex = chosenIndex;
                steering.DesiredDir = bestDir;
                vel.desiredVel = bestDir * vel.MaxSpeed;
                res.ConsecutiveBlocks = 0;

                // -------------------------------------------------
                // Actualizamos la posición "intencional" en el hash
                // AHORA, antes de procesar la siguiente unidad de
                // este mismo frame — así la próxima consulta de
                // CheckUnitsAtCandidate ya ve esta nueva posición,
                // evitando que dos unidades elijan el mismo destino
                // en el mismo frame (coordinación secuencial, válida
                // porque el sistema es single-threaded).
                // -------------------------------------------------
                Vector2 predictedNextPos = pos.position + bestDir * vel.MaxSpeed * dt;
                dynGrid.UpdatePosition(sid.Value, predictedNextPos.X,predictedNextPos.Y); // ajusta al método real de tu hash
            }
            else
            {
                res.LastConeIndex = 0;
                res.AvoidanceTimer = 0f;
                steering.DesiredDir = Vector2.Zero;
                vel.desiredVel = Vector2.Zero;
                res.RetryTimer = RetryInterval;
                res.ConsecutiveBlocks++;
                GoIdle(ref vel, ref state, ref steering);

                if (res.ConsecutiveBlocks >= MaxAttempts)
                {
                    res.ConsecutiveBlocks = 0;
                    if (!e.Has<StoppedTag>())
                        e.Add<StoppedTag>();
                }
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void GoIdle(ref VelocityComponent vel, ref StateComponent state, ref SteeringComponent steering)
    {
        vel.desiredVel = Vector2.Zero;
        vel.currentVel = Vector2.Zero; // MovementResolutionSystem no corre en IDLE: evita velocidad fantasma
        steering.DesiredDir = Vector2.Zero;
        state.stateType = StateType.IDLE;
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private bool CheckUnitsAtCandidate(
        Vector2 candidatePos, float radius, int selfSid,
        Span<int> neighbors, FastSpatialHash dynGrid)
    {
        int count = dynGrid.QueryNodesBoundedClosestLayersFiltered(candidatePos.X, candidatePos.Y, radius, 0, neighbors);

        for (int ii = 0; ii < count; ii++)
        {
            int otherSid = dynGrid.GetSpatialID(neighbors[ii]);
            if (otherSid == selfSid) continue;

            Entity other = dynGrid.GetEntity(neighbors[ii]);
            if (!other.IsAlive() || other.Has<DeadTag>()) continue;

            var otherPos = other.Get<PositionComponent>().position;
            var otherCol = other.Get<MoveColliderComponent>();

            float dist = candidatePos.DistanceTo(otherPos + otherCol.Offset);
            if (dist < radius + otherCol.Radius)
                return true;
        }

        return false;
    }

    private static Vector2I ToCell(Vector2 atlasPos, float cellSize)
    {
        return new Vector2I(
            Mathf.FloorToInt(atlasPos.X / cellSize),
            Mathf.FloorToInt(atlasPos.Y / cellSize));
    }
}