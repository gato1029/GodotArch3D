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

        var sim = blackyWorld.Simulation.Tick;

        // 🔥 Staggering: la mitad de las unidades se evalúa en frames
        // pares, la otra mitad en impares (repartido por sid.Value).
        // Reduce el costo de este sistema a la mitad por frame.
        bool evenFrame = (sim.FrameIndex & 1) == 0;

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

            Vector2 posFuture = pos.position + (steering.DesiredDir * vel.MaxSpeed * it.DeltaTime());

            // -------------------------------------------------
            // Contra ESTÁTICOS (edificios, recursos): bloqueo duro.
            // No cambian de posición, así que si hay colisión ahora,
            // va a seguir habiéndola — tiene sentido frenar del todo
            // hasta una orden nueva o el mecanismo de Unstick.
            // -------------------------------------------------
            bool hitStatic = CheckAgainstStaticGrid(ref posFuture, ref col, staGridBuilding)
                           || CheckAgainstStaticGridResources(ref posFuture, ref col, staResourceGrid);

            if (hitStatic)
            {
                vel.desiredVel = Vector2.Zero;
                res.Blocked = true;
                continue;
            }

            // -------------------------------------------------
            // Contra UNIDADES dinámicas: NO bloqueamos duro. Solo
            // evitamos avanzar ESTE frame si el paso siguiente choca
            // contra alguien que también se está moviendo. El próximo
            // frame, SteeringSystem recalcula avoidance con la nueva
            // posición de todos — si el otro ya se movió, el camino
            // puede estar libre sin que nadie tenga que "desbloquear"
            // nada explícitamente.
            // -------------------------------------------------
            bool hitUnit = CheckUnits(posFuture, col.Radius, col, neighbors, sid, dynGrid);

            if (hitUnit)
            {
                vel.desiredVel = Vector2.Zero; 
                //res.Blocked = true;
                // SIN res.Blocked = true aquí — solo detenemos este
                // frame puntual, sin entrar en el estado "congelada".
            }

        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckUnits(Vector2 pos, float radius, MoveColliderComponent moveCollider, Span<int> neighbors, SpatialIDComponent spatial, FastSpatialHash grid)
    {
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