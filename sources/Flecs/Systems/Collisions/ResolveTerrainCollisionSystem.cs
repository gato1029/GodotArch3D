using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyTiles;
using GodotEcsArch.sources.managers.Collision;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
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
          .With<UnitTag>()
          .Without<SleepTag>();
          
    }

    protected override void OnIter(Iter it)
    {

        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<MoveColliderComponent>(1);
        var sidArray = it.Field<SpatialIDComponent>(2);
        var steeringArray = it.Field<SteeringComponent>(3);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var sid = ref sidArray[i];

            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var steering = ref steeringArray[i];

            bool collided = false;

            float cx = pos.position.X + col.Offset.X;
            float cy = pos.position.Y + col.Offset.Y;

         
            var tilePositionMin = TilesHelper.WorldToTile(cx - col.Radius * 0.5f, cy - col.Radius * 0.5f);
            var tilePositionMax = TilesHelper.WorldToTile(cx + col.Radius * 0.5f, cy + col.Radius * 0.5f);

            // 2️⃣ Bucle de Tiles
            for (int tx = tilePositionMin.X; tx <= tilePositionMax.X; tx++)
            {
                for (int ty = tilePositionMin.Y; ty <= tilePositionMax.Y; ty++)
                {
                    var height = blackyWorld.Heights.GetTopHeight(tx, ty);
                    var tile = blackyWorld.Terrain.GetTileTop(tx, ty, height);

                    if (tile.CollisionId == 0) continue;

                    int currentRecipeIdx = tile.CollisionId;
                    while (currentRecipeIdx != -1)
                    {
                        ref readonly var recipe = ref TerrainCollisionLibrary.Get(currentRecipeIdx);

                        if ((sid.Mask & recipe.Layer) != 0)
                        {
                            var tileCol = new FastCollider
                            {
                                Shape = recipe.Shape,
                                Width = recipe.Width,
                                Height = recipe.Height,
                                Offset = new Godot.Vector2( recipe.OffsetX, recipe.OffsetY)
                            };

                            var colUnit = new FastCollider
                            {
                                Shape = ShapeType.Circle,
                                Width = col.Radius,
                                Height = col.Radius,
                                Offset = new Godot.Vector2(col.Offset.X, col.Offset.Y)
                            };
                            var posTile =TilesHelper.WorldPositionTile(new Godot.Vector2I(tx, ty));

                            if (CollisionMathHelper.Check(pos.position.X, pos.position.Y, ref colUnit,
                                                        posTile.X, posTile.Y, ref tileCol))
                            {
                                collided = true;
                                break; // Salir del while de recetas
                            }
                        }
                        currentRecipeIdx = recipe.NextIndex;
                    }
                    if (collided) break; // Salir del for de ty
                }
                if (collided) break; // Salir del for de tx
            }
            
            if (collided) // no hay collision
            {
                steering.DesiredDir = Godot.Vector2.Zero;                
            }

        }
    }
}

