using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.PathFinding;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using SadRogue.Primitives;

namespace GodotEcsArch.sources.Flecs.Systems.Generic;

internal class FlowFieldUnstickSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

    private const float UnstickDelay = 1.0f; // segundos bloqueada antes de reintentar

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<FlowFieldFollowerComponent>()
          .With<MoveResolutorComponent>()
          .With<StoppedTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var resArray = it.Field<MoveResolutorComponent>(1);
        float dt = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var res = ref resArray[i];
            var e = it.Entity(i);

            res.StuckTimer += dt; // necesitas agregar este campo a MoveResolutorComponent

            if (res.StuckTimer >= UnstickDelay)
            {
                res.Blocked = false;
                res.BlockedTimer = 0f;
                res.StuckTimer = 0f;
                e.Remove<StoppedTag>();
            }
        }
    }
}




internal class FlowFieldFollowSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsPreUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<FlowFieldFollowerComponent>()
          .With<MovementMassiveTag>()
          .Without<StoppedTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld = it.World().GetCtx<BlackyWorld>();
        if (blackyWorld == null) return;

        var flowFieldManager = blackyWorld.State.FlowFieldManager;
        if (flowFieldManager == null) return;

        var posArray = it.Field<PositionComponent>(0);
        var followerArray = it.Field<FlowFieldFollowerComponent>(1);

        // Casi toda la horda comparte el mismo campo: cacheamos el último
        // lookup para no consultar el diccionario por cada unidad.
        int lastId = int.MinValue;
        FlowField lastField = null;

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var follower = ref followerArray[i];

            if (follower.FlowFieldId != lastId)
            {
                lastId = follower.FlowFieldId;
                lastField = flowFieldManager.GetField(lastId);
            }

            FlowField field = lastField;

            // Sin campo (invalidado y todavía sin reconstruir): esperar, no asentar.
            if (field == null)
            {
                follower.Status = 2;
                follower.TargetDir = Vector2.Zero;
                continue;
            }

            Vector2 dir = field.SampleInterpolated(pos.position);

            if (dir != Vector2.Zero)
            {
                follower.Status = 0;
                follower.TargetDir = dir;
                continue;
            }

            // Dirección cero: solo ocurre en el GoalTile exacto o en una celda sin salida.
            Vector2I tile = TilesHelper.WorldPositionToTile(pos.position);
            
            //GD.Print($"Zero en {tile} pos={pos.position} walkable={clearanceMap.IsWalkableForRadius(tile.X, tile.Y, radius)} dist={field.DebugDistance(t)} inRegion={field.DebugInRegion(t)}");
            if (field.IsGoalTile(tile))
            {
                follower.Status = 1;
                follower.TargetDir = Vector2.Zero;
                continue;
            }

            Vector2 escape = field.SampleEscape(tile);
            if (escape != Vector2.Zero)
            {
                follower.Status = 0;
                follower.TargetDir = escape;
            }
            else
            {
                follower.Status = 2; // realmente atrapada, sin salida cercana
                follower.TargetDir = Vector2.Zero;
            }
          
        }
    }
}
internal static class CrowdSettings
{
    // Unidades máximas por celda. Con celdas de 1.0 y colliders de radio ~0.13 caben
    // muchas más físicamente; 4 es conservador. Súbelo si la horda queda muy suelta.
    public const int Capacity = 1;
}
internal class CrowdRegisterSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; // TryEnterCell es atómico

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<FlowFieldFollowerComponent>()
          .With<MovementMassiveTag>()
          .Without<DeadTag>();   // SIN excluir StoppedTag: las asentadas también ocupan celda
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var grid = world.State.GridSparseManager.Grid.DynamicLayer;
        var posArray = it.Field<PositionComponent>(0);

        for (int i = 0; i < it.Count(); i++)
        {
            int idx = grid.GetIndex(posArray[i].position);
            grid.TryEnterCell(idx, CrowdSettings.Capacity);
        }
    }
}
