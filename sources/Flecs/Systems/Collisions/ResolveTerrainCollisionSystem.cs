using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.managers.Mods;
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

public class ResolveTerrainCollisionSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveColliderComponent>()
          .With<SpatialIDComponent>()
          .With<SteeringComponent>()
          .With<VelocityComponent>()
          .With<UnitTag>()
          .Without<StoppedTag>();
          
    }

    protected override void OnIter(Iter it)
    {

        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;

        var staticSpatialGrid = blackyWorld.State.StaticSpatialTerrain;

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var steeringArray = it.Field<SteeringComponent>(3);
        var velArray = it.Field<VelocityComponent>(4);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var sid = ref sidArray[i];

            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var steering = ref steeringArray[i];
            ref var vel = ref velArray[i];
            Vector2 posFuture = pos.position + (steering.DesiredDir * vel.MaxSpeed * it.DeltaTime()*2f);            
            bool collided = CheckAgainstStaticGrid(posFuture, ref col, staticSpatialGrid);                     
            if (collided)
            {
                steering.DesiredDir = Godot.Vector2.Zero;      
                vel.desiredVel = Vector2.Zero;
            }
        }
    }


[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckAgainstStaticGrid(
    Vector2 pos,
    ref MoveColliderComponent col,
    StaticSpatialGridOptimizedGeneric<ColliderSpriteInstanceData> grid)
    {
        int radius = (int)MathF.Ceiling((col.Radius * 2f) / grid._cellSize);
        bool existCollision = false;
        foreach (var id in grid.QueryNearbyUnique(pos.X, pos.Y, radius))
        {
            if (grid.TryGetValue(id, out var infoSpriteCollider))
            {
                AtlasModsManager.TryGetTileSprite(infoSpriteCollider.TileId, out var tileSprite);
             
                foreach (var fast in tileSprite.fastCollidersBody)
                {
                    FastCollider fastCollider = fast;
                    if (!CollisionMathHelper.CheckCircle(pos.X, pos.Y, col.Radius, col.Offset, infoSpriteCollider.Position.X, infoSpriteCollider.Position.Y, ref fastCollider))
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
        }

        return existCollision;
    }
}
