
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
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

public class MoveSeparationSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    [ThreadStatic] static int[] _visitedDynamic;
    [ThreadStatic] static int[] _visitedStatic;
    [ThreadStatic] private static int _stamp;

    private const int MAX_SIDS_Static = 200_000;
    private const int MAX_SIDS_Dinamic = 20_000;

    // 🔥 ajustes finos
    private const float MIN_PENETRATION = 0.01f;
    private const float BLOCK_THRESHOLD = 0.02f;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveColliderComponent>()
          .With<SpatialIDComponent>()
          .With<MoveResolutorComponent>()
          .With<SteeringComponent>()
          .With<VelocityComponent>()
          .Without<SleepTag>();
          
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;
        var sim = blackyWorld.simulationTick;
        // 🔥 AQUI VA (ANTES DE TODO)
        bool shouldUpdate = (sim.FrameIndex & 1) == 0;

        if (_visitedDynamic == null)
        {
            _visitedDynamic = new int[MAX_SIDS_Dinamic];
            _visitedStatic = new int[MAX_SIDS_Static];
            _stamp = 1;
        }

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var resArray = it.Field<MoveResolutorComponent>(3);
        var steeringArray = it.Field<SteeringComponent>(4);
        var velArray = it.Field<VelocityComponent>(5);

        var dynGrid = blackyWorld.DynamicHash;
        var staGrid = blackyWorld.StaticSpatial;

        for (int i = 0; i < it.Count(); i++)
        {

            _stamp++;
            if (_stamp == int.MaxValue)
            {
                Array.Fill(_visitedStatic, 0);
                Array.Fill(_visitedDynamic, 0);
                _stamp = 1;
            }

            if (!shouldUpdate)
            {
                // 🔥 mantener velocidad anterior (no recalcular)
                continue;
            }

            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var sid = ref sidArray[i];
            ref var res = ref resArray[i];
            ref var steering = ref steeringArray[i];
            ref var vel = ref velArray[i];

            if (steering.DesiredDir.LengthSquared() < 0.0001f)
                continue;

            Vector2 posFuture = pos.position + (steering.DesiredDir*vel.MaxSpeed* it.DeltaTime());

            float cx = posFuture.X + col.Offset.X;
            float cy = posFuture.Y + col.Offset.Y;

            var min = FastSpatialHash.WorldToTile(cx - col.Radius , cy - col.Radius );
            var max = FastSpatialHash.WorldToTile(cx + col.Radius , cy + col.Radius );
                

            bool existCollision = false;
            for (int tx = min.X; tx <= max.X; tx++)
            {
                for (int ty = min.Y; ty <= max.Y; ty++)
                {
                    var dyn = CheckAgainstGrid(it.Entity(i), ref posFuture, ref col, ref sid, tx, ty, dynGrid, false);                    
                    if (dyn)
                    {
                        existCollision = true;
                        break;
                    }
                    
                }
                if (existCollision)
                {
                    break;
                }
            }
            if (!existCollision)
            {
                // 🔥 DETECCIÓN DE COLISIÓN CON ENTIDADES ESTÁTICAS (paredes, árboles, etc)
                var sta = CheckAgainstStaticGrid(ref posFuture, ref col, staGrid);
                existCollision = sta;
            }
            if (existCollision)
            {
                steering.DesiredDir = Vector2.Zero; // Detener el movimiento

            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private  bool  CheckAgainstStaticGrid(
    ref Vector2 pos,
    ref MoveColliderComponent col,
    StaticSpatialGridOptimized grid)
    {


        float cx = pos.X + col.Offset.X;
        float cy = pos.Y + col.Offset.Y;

        // 🔹 collider temporal (como ya hacías)
        var colUnit = new FastCollider
        {
            Shape = ShapeType.Circle,
            Width = col.Radius,
            Height = col.Radius,
            Offset = new Vector2(col.Offset.X, col.Offset.Y)
        };

        // 🔹 calcular radio dinámico (IMPORTANTE)
        int radius = (int)MathF.Ceiling((col.Radius * 2f) / grid._cellSize);
        bool existCollision = false;
        foreach (var id in grid.QueryNearbyUnique(pos.X, pos.Y, radius))
        {
            var other = grid.GetEntity(id);
            if (!other.HasValue || !other.Value.IsAlive()) continue;

            var bodyCollider = other.Value.Get<BodyColliderComponent>();
            var posOther = other.Value.Get<PositionComponent>();

            foreach (var shape in bodyCollider.Shapes)
            {
                var shapeInternal = shape;
                if (!CollisionMathHelper.Check(
                        pos.X, pos.Y, ref colUnit,
                        posOther.position.X, posOther.position.Y, ref shapeInternal))
                {

                    continue;
                }
                else
                {
                    existCollision = true;
                    break;
                }
            }
        }

        return existCollision;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private  bool  CheckAgainstGrid(
        Entity e,
        ref Vector2 pos,
        ref MoveColliderComponent col,
        ref SpatialIDComponent sid,
        int cx,
        int cy,
        FastSpatialHash grid,
        bool isStatic = false)
    {
        int cell = FastSpatialHash.GetHashDirect(cx, cy, grid.TotalCells);
        int currentIdx = grid.GetHead(cell);

      

        float cxSelf = pos.X + col.Offset.X;
        float cySelf = pos.Y + col.Offset.Y;
        bool existCollision = false;
        while (currentIdx != -1)
        {
            int otherSID = grid.GetSpatialID(currentIdx);
            var visited = isStatic ? _visitedStatic : _visitedDynamic;

            if (visited[otherSID] == _stamp)
            {
                currentIdx = grid.GetNext(currentIdx);
                continue;
            }

            visited[otherSID] = _stamp;

            if (!isStatic && otherSID == sid.Value)
            {
                currentIdx = grid.GetNext(currentIdx);
                continue;
            }

            Entity other = grid.GetEntity(currentIdx);
            ref var otherSid = ref other.GetMut<SpatialIDComponent>();

            if ((sid.Mask & otherSid.Layer) != 0)
            {
                ref var otherPos = ref other.GetMut<PositionComponent>();
                ref var otherCol = ref other.GetMut<MoveColliderComponent>();

                float cxOther = otherPos.position.X + otherCol.Offset.X;
                float cyOther = otherPos.position.Y + otherCol.Offset.Y;

                float dx = cxSelf - cxOther;
                float dy = cySelf - cyOther;

                float distSq = dx * dx + dy * dy;
                float minDist = col.Radius + otherCol.Radius;

                if (distSq < minDist * minDist)
                {
                    existCollision = true;
                    break;
              
                }
            }

            currentIdx = grid.GetNext(currentIdx);
        }

        return  existCollision;
    }
}