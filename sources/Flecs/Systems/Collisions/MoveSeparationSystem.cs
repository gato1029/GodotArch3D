using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

// Sistema de diagnóstico temporal, corre al final de todo
internal class DebugOverlapCheckSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate; // última fase
    protected override bool MultiThreaded => false;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveColliderComponent>()
          .With<SpatialIDComponent>()
          .With<VelocityComponent>()
          .With<MoveResolutorComponent>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var velArray = it.Field<VelocityComponent>(3);
        var resArray = it.Field<MoveResolutorComponent>(4);

        for (int i = 0; i < it.Count(); i++)
        {
            for (int j = i + 1; j < it.Count(); j++)
            {
                float dist = posArray[i].position.DistanceTo(posArray[j].position);
                float minDist = colArray[i].Radius + colArray[j].Radius;

                if (dist < minDist * 0.8f) // solapamiento notorio, no solo rozando
                {
                    var eI = it.Entity(i);
                    var eJ = it.Entity(j);

                    GD.Print($"[OVERLAP] A(id={sidArray[i].Value}, stopped={eI.Has<StoppedTag>()}, blocked={resArray[i].Blocked}, currentVel={velArray[i].currentVel}, desiredVel={velArray[i].desiredVel}) " +
                              $"B(id={sidArray[j].Value}, stopped={eJ.Has<StoppedTag>()}, blocked={resArray[j].Blocked}, currentVel={velArray[j].currentVel}, desiredVel={velArray[j].desiredVel}) dist={dist} minDist={minDist}");
                }
            }
        }
    }
}

public class MoveSeparationSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    // Distancia fija de retroceso al chocar (unidades del mundo).
    private const float BackoffDistance = 0.12f;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveColliderComponent>()
          .With<SpatialIDComponent>()
          .With<MoveResolutorComponent>()
          .With<SteeringComponent>()
          .With<VelocityComponent>()
          .With<UnitDefinitionComponent>()
          .With<StateComponent>()
          .With<MoveTargetComponent>()
          .Without<StoppedTag>();
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var resArray = it.Field<MoveResolutorComponent>(3);
        var steeringArray = it.Field<SteeringComponent>(4);
        var velArray = it.Field<VelocityComponent>(5);
        var unitArray = it.Field<UnitDefinitionComponent>(6);
        var stateArray = it.Field<StateComponent>(7);

        var staGridBuilding = blackyWorld.State.StaticSpatialBuildings;
        var staResourceGrid = blackyWorld.State.StaticSpatialResources;
        var dynGrid = blackyWorld.State.DynamicHash;
        Span<int> neighbors = stackalloc int[8];

        const float RetryInterval = 0.2f;     // tiempo mínimo entre reintentos
        const int MaxConsecutiveBlocks = 10;  // ~2s de intentos antes de rendirse del todo

        float dt = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var sid = ref sidArray[i];
            ref var res = ref resArray[i];
            ref var steering = ref steeringArray[i];
            ref var vel = ref velArray[i];
            ref var state = ref stateArray[i];

            if (res.Blocked)
                continue;

            if (steering.DesiredDir.LengthSquared() < 0.01f)
                continue;

            // -------------------------------------------------
            // Cooldown entre intentos: no revalidamos cada frame,
            // damos un respiro para que la congestión cambie antes
            // de volver a intentar.
            // -------------------------------------------------
            res.RetryTimer -= dt;
            if (res.RetryTimer > 0f)
            {
                vel.desiredVel = Vector2.Zero; // mientras espera, no avanza
                continue;
            }

            // Usamos la velocidad REAL (suavizada), no MaxSpeed,
            // para que coincida con lo que MovementResolutionSystem
            // va a aplicar realmente.
            Vector2 posFuture = pos.position + (vel.desiredVel * dt);

            bool hitStatic = CheckAgainstStaticGrid(ref posFuture, ref col, staGridBuilding)
                           || CheckAgainstStaticGridResources(ref posFuture, ref col, staResourceGrid);

            if (hitStatic)
            {
                vel.desiredVel = Vector2.Zero;
                res.Blocked = true; // estático: sigue siendo bloqueo duro, no cambia
                continue;
            }

            bool hitUnit = CheckUnits(posFuture, col.Radius, col, neighbors, sid, dynGrid, out bool collidedWithSettled);

            if (hitUnit)
            {
                vel.desiredVel = Vector2.Zero;

                if (collidedWithSettled)
                {
                    vel.desiredVel = Vector2.Zero;
                    vel.currentVel = Vector2.Zero; // <- también aquí

                    ResolveOverlapOnSettle(ref pos, ref col, sid, dynGrid);

                    if (!it.Entity(i).Has<StoppedTag>())
                    {
                        it.Entity(i).Add<StoppedTag>();
                    }
                }
                else
                {
                    // Choque contra alguien que TODAVÍA se está moviendo:
                    // esperamos el cooldown normal, sin asentarnos todavía
                    // (puede que se mueva y el camino se libere).
                    res.RetryTimer = RetryInterval;
                    res.ConsecutiveBlocks++;

                    if (res.ConsecutiveBlocks >= MaxConsecutiveBlocks)
                    {
                        if (!it.Entity(i).Has<StoppedTag>())
                            it.Entity(i).Add<StoppedTag>();

                        res.ConsecutiveBlocks = 0;
                    }
                }
            }
            else
            {
                res.ConsecutiveBlocks = 0;
            }
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckUnits(
      Vector2 pos, float radius, MoveColliderComponent moveCollider,
      Span<int> neighbors, SpatialIDComponent spatial, FastSpatialHash grid,
      out bool collidedWithSettled)
    {
        collidedWithSettled = false;

        var count = grid.QueryNodesBoundedClosestLayersFiltered(pos.X, pos.Y, radius, 0, neighbors);

        for (int ii = 0; ii < count; ii++)
        {
            int otherSid = grid.GetSpatialID(neighbors[ii]);
            Entity currentEntity = grid.GetEntity(neighbors[ii]);

            if (spatial.Value == otherSid) continue;

            if (currentEntity.IsAlive() && !currentEntity.Has<DeadTag>())
            {
                var currentPos = currentEntity.Get<PositionComponent>().position;
                var currentCol = currentEntity.Get<MoveColliderComponent>();

                if (CollisionMathHelper.CheckCircle(pos.X, pos.Y, moveCollider.Radius, moveCollider.Offset, currentPos.X, currentPos.Y, currentCol.Radius, currentCol.Offset))
                {
                    if (currentEntity.Has<StoppedTag>())
                    {
                        collidedWithSettled = true;
                    }
                    return true;
                }
            }
        }
        return false;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckAgainstStaticGridResources(ref Vector2 pos, ref MoveColliderComponent col, StaticSpatialGridOptimizedGeneric<Entity> grid)
    {
        var colUnit = new FastCollider
        {
            Shape = ShapeType.Circle,
            Width = col.Radius,
            Height = col.Radius,
            Offset = new Vector2(col.Offset.X, col.Offset.Y)
        };

        int radius = (int)MathF.Ceiling((col.Radius * 2f) / grid._cellSize);
        bool existCollision = false;
        var list = grid.QueryNearbyUnique(pos.X, pos.Y, radius);

        foreach (var id in list)
        {
            if (grid.TryGetValue(id, out Entity other))
            {
                if (!other.IsAlive()) continue;

                TileSpriteData sprite = null;
                if (other.Has<ResourceDefinitionComponent>())
                {
                    int idTemplate = other.Get<ResourceDefinitionComponent>().idSpriteTemplate;
                    AtlasModsManager.TryGetTileSprite(idTemplate, out sprite);
                }

                // 🛡️ Si no hay sprite o colisionadores definidos, saltar esta entidad de forma segura
                if (sprite == null || sprite.fastCollidersBody == null) continue;

                var posOther = other.Get<PositionComponent>();

                foreach (var shape in sprite.fastCollidersBody)
                {
                    var shapeInternal = shape;
                    if (CollisionMathHelper.Check(pos.X, pos.Y, ref colUnit, posOther.position.X, posOther.position.Y, ref shapeInternal))
                    {
                        existCollision = true;
                        break;
                    }
                }
            }

            if (existCollision)
            {
                break;
            }
        }

        return existCollision;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckAgainstStaticGrid(ref Vector2 pos, ref MoveColliderComponent col, StaticSpatialGridOptimizedGeneric<Entity> grid)
    {
        var colUnit = new FastCollider
        {
            Shape = ShapeType.Circle,
            Width = col.Radius,
            Height = col.Radius,
            Offset = new Vector2(col.Offset.X, col.Offset.Y)
        };

        int radius = (int)MathF.Ceiling((col.Radius * 2f) / grid._cellSize);
        bool existCollision = false;

        foreach (var id in grid.QueryNearbyUnique(pos.X, pos.Y, radius))
        {
            if (grid.TryGetValue(id, out Entity other))
            {
                if (!other.IsAlive()) continue;

                TileSpriteData sprite = null;
                if (other.Has<BuildingDefinitionComponent>())
                {
                    int idTemplate = other.Get<BuildingDefinitionComponent>().idSpriteTemplateNormal;
                    AtlasModsManager.TryGetTileSprite(idTemplate, out sprite);
                }

                // 🛡️ Si no hay sprite o colisionadores definidos, saltar esta entidad de forma segura
                if (sprite == null || sprite.fastCollidersBody == null) continue;

                var posOther = other.Get<PositionComponent>();

                foreach (var shape in sprite.fastCollidersBody)
                {
                    var shapeInternal = shape;
                    if (CollisionMathHelper.Check(
                            pos.X, pos.Y, ref colUnit,
                            posOther.position.X, posOther.position.Y, ref shapeInternal))
                    {
                        existCollision = true;
                        break;
                    }
                }
            }

            if (existCollision)
            {
                break;
            }
        }

        return existCollision;
    }
}