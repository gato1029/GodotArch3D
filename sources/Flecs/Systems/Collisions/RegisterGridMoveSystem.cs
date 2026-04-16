using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyTiles;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Collisions;

public class RegisterGridMoveSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // luego lo mejoramos

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<FastColliderComponent>()
          .With<UnitTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var grid = world.GridMove;

        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<FastColliderComponent>(1);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var p = ref posArray[i];
            ref var col = ref colArray[i];

            float x = p.position.X + col.OffsetX;
            float y = p.position.Y + col.OffsetY;

            // 🔥 clave: usar AABB optimizado
            grid.SetWorldRectFast(
                x,
                y,col.Width,
                col.Height
            );
        }
    }
}