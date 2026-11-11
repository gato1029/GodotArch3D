using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Units;

internal class AttackingPathTransitionSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<AttackPendingComponent>()
          .With<AttackingPathFollowerTag>()
          .With<PathReferenceComponent>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var posArray = it.Field<PositionComponent>(0);
        var atpArray = it.Field<AttackPendingComponent>(1);
        var pathArray = it.Field<PathReferenceComponent>(2);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var atp = ref atpArray[i];
            ref var pathRef = ref pathArray[i];
            var entity = it.Entity(i);

            // 1. Si el objetivo murió, limpiamos todo
            if (!atp.Target.IsAlive() || atp.Target.Has<DeadTag>())
            {
                world.State.PathRegistryManager.ReleasePath(pathRef.PathId);
                entity.Remove<PathReferenceComponent>();
                entity.Remove<AttackingPathFollowerTag>();
                entity.Remove<AttackPendingComponent>();
                entity.Add<StoppedTag>();
                continue;
            }

            Vector2 targetPos = atp.Target.Get<PositionComponent>().position;
            
            // 2. Comprobación de distancia o línea de visión directa (Ruta Libre)
            // Si la unidad ya está a menos de una distancia prudente (ej. 10 unidades) 
            // O si ya hay línea de visión libre sin obstáculos entre medio:
            float distSqr = pos.position.DistanceSquaredTo(targetPos);
            bool closeEnough = distSqr <= (12.0f * 12.0f); // 12 unidades de rango de transición

            Vector2 tempTarget = targetPos;
            bool hasLineOfSight = CollisionMathHelper.RutaLibre(entity, ref tempTarget, world);

            if (closeEnough || hasLineOfSight)
            {
                // ¡ROMPIENDO EL PATH A*! Ya salvamos los obstáculos globales.
                // Liberamos el path del gestor de memoria
                world.State.PathRegistryManager.ReleasePath(pathRef.PathId);
                entity.Remove<PathReferenceComponent>();

                // Quitamos el tag de seguimiento de path y activamos el Chase inteligente
                entity.Remove<AttackingPathFollowerTag>();
                entity.Add<ChaseTargetTag>(); // <--- ¡Ahora el UnitChaseSystem toma el control total!
            }
        }
    }
}