using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

public class GridSteeringSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

    private const float RetryInterval = 0.3f;
    private const int MaxAttempts = 5;
    // Margen adicional de anticipación (Lookahead) más allá del radio físico
    private const float LookAheadMargin = 0.2f;
    // Offsets del cono, en orden de preferencia: primero recto,
    // luego alternando hacia los costados. ±30° = cono total de 60°.
    // Nunca incluye ángulos hacia atrás, así que "retroceder" nunca
    // es una opción posible — queda bloqueado por diseño del cono.
    private static readonly float[] ConeOffsetsDeg = { 0f, 30f, -30f, 60f, -60f };
    // Tiempo (en segundos) que mantendrá la dirección de esquiva antes de intentar reorientarse al objetivo
    private const float AvoidanceHoldTime = 2f;
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

        var gridManager = world.State.GridSparseManager;
        var claimedCells = world.State.GridSparseManager.ClaimedCellsThisFrame;

        const float cellSize = 0.5f; // debe coincidir con dynamicCellSize del grid

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var velArray = it.Field<VelocityComponent>(3);
        var steeringArray = it.Field<SteeringComponent>(4);
        var resArray = it.Field<MoveResolutorComponent>(5);

        float dt = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var sid = ref sidArray[i];
            ref var vel = ref velArray[i];
            ref var steering = ref steeringArray[i];
            ref var res = ref resArray[i];
            var e = it.Entity(i);
 // -------------------------------------------------
            // Cooldown entre intentos: no revalidamos cada frame.
            // -------------------------------------------------
            if (res.RetryTimer > 0f)
            {
                res.RetryTimer -= dt;
                vel.desiredVel = Vector2.Zero;
                res.Blocked = false;
                res.AvoidanceTimer = 0f;
                continue;
            }

            if (res.Blocked)
            {
                vel.desiredVel = Vector2.Zero;
                res.AvoidanceTimer = 0f;
                continue;
            }

            Vector2 targetDir = steering.TargetDir;

            if (targetDir == Vector2.Zero)
            {
                steering.DesiredDir = Vector2.Zero;
                vel.desiredVel = Vector2.Zero;
                res.AvoidanceTimer = 0f;
                continue;
            }

            targetDir = targetDir.Normalized();

            // Consumo del temporizador de fijación de esquiva
            if (res.AvoidanceTimer > 0f)
            {
                res.AvoidanceTimer -= dt;
            }

            Vector2 currentCenter = pos.position + col.Offset;
            Vector2 currentAtlasPos = gridManager.MainWorldLocalToAtlasPos(currentCenter);
            Vector2I currentCell = ToCell(currentAtlasPos, cellSize);

            float probeDist = 0.5f + LookAheadMargin;
            float baseAngle = targetDir.Angle();
            bool found = false;
            Vector2 bestDir = Vector2.Zero;
            int chosenIndex = 0;

            int lastIdx = res.LastConeIndex;

            // ¿Estamos dentro del periodo de compromiso de esquiva?
            bool inAvoidanceHold = res.AvoidanceTimer > 0f && lastIdx > 0 && lastIdx < ConeOffsetsDeg.Length;

            for (int step = 0; step < ConeOffsetsDeg.Length; step++)
            {
                int s;

                if (inAvoidanceHold)
                {
                    // MIENTRAS DURE EL TIMER: Mantiene el ángulo de esquiva (lastIdx)
                    // para darle tiempo a la unidad de avanzar y superar físicamente el obstáculo.
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
                    // CUANDO EL TIMER EXPIRA: Vuelve a intentar apuntar directo al objetivo (0°)
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
                Vector2 candidateProbePos = currentCenter + candidateDir * probeDist;

                if (gridManager.IsBlocked(candidateProbePos))
                    continue;

                Vector2I candidateCell = ToCell(candidateProbePos, cellSize);

                if (candidateCell == currentCell || claimedCells.TryAdd(candidateCell, sid.Value))
                {
                    bestDir = candidateDir;
                    found = true;
                    chosenIndex = s;
                    break;
                }
            }

            if (found)
            {
                if (chosenIndex > 0)
                {
                    // Solo iniciamos un nuevo ciclo de temporizador si no estábamos ya esquivando o si cambió de ángulo
                    if (!inAvoidanceHold || chosenIndex != lastIdx)
                    {
                        res.AvoidanceTimer = AvoidanceHoldTime;
                    }
                }
                else
                {
                    // Volvió con éxito a 0°: reseteamos la evasión
                    res.AvoidanceTimer = 0f;
                }

                res.LastConeIndex = chosenIndex;
                steering.DesiredDir = bestDir;
                vel.desiredVel = bestDir * vel.MaxSpeed;
                res.ConsecutiveBlocks = 0;
            }
            else
            {
                res.LastConeIndex = 0;
                res.AvoidanceTimer = 0f;
                steering.DesiredDir = Vector2.Zero;
                vel.desiredVel = Vector2.Zero;
                res.RetryTimer = RetryInterval;
                res.ConsecutiveBlocks++;
                res.Blocked = true;

                if (res.ConsecutiveBlocks >= MaxAttempts)
                {
                    res.ConsecutiveBlocks = 0;
                    if (!e.Has<StoppedTag>())
                        e.Add<StoppedTag>();
                }
            }
        }
    }

    private static Vector2I ToCell(Vector2 atlasPos, float cellSize)
    {
        return new Vector2I(
            Mathf.FloorToInt(atlasPos.X / cellSize),
            Mathf.FloorToInt(atlasPos.Y / cellSize));
    }
}