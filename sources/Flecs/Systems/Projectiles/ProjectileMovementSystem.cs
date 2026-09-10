using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Services.Spawn;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Projectiles;

internal class ProjectileMovementSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<ProjectilePositionComponent>()
          .With<ProjectileVelocityComponent>()
          .With<ProjectileTargetComponent>()
          .With<ActiveProjectileTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var arrowPool = world.Services.ArrowPool;
        var posArray = it.Field<ProjectilePositionComponent>(0);
        var velArray = it.Field<ProjectileVelocityComponent>(1);
        var targetArray = it.Field<ProjectileTargetComponent>(2);
        float deltaTime = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var vel = ref velArray[i];
            ref var projTarget = ref targetArray[i];
            var arrowEntity = it.Entity(i);


            // 1. Mover flecha
            pos.Position += vel.Velocity * deltaTime;

            // 2. Comprobar colisión en tiempo real (si el objetivo sigue vivo)
            if (projTarget.Target.IsAlive() && !projTarget.Target.Has<DeadTag>())
            {
                var targetPos = projTarget.Target.Get<PositionComponent>();
                var collisionThreshold = projTarget.Target.Get<MoveColliderComponent>().Radius;

                if (pos.Position.DistanceSquaredTo(targetPos.position) <= (collisionThreshold * collisionThreshold))
                {
                    GlobalData.EventsDamage.Enqueue(new DamageEvent
                    {
                        Target = projTarget.Target,
                        Amount = projTarget.Damage
                    });

                    RecycleArrow(arrowEntity, arrowPool);
                    continue;
                }
            }

            // 3. Comprobar si la flecha llegó a su distancia máxima de recorrido sin impactar
            float traveledDistSq = projTarget.Origin.DistanceSquaredTo(pos.Position);
            float totalDistSq = projTarget.TotalDistance * projTarget.TotalDistance;

            if (traveledDistSq >= totalDistSq)
            {
                RecycleArrow(arrowEntity, arrowPool);
            }


        }
    }
    private void RecycleArrow(Entity arrowEntity, ArrowPoolService arrowPool)
    {
        arrowEntity.Remove<ActiveProjectileTag>();
        arrowEntity.Remove<ProjectileInitializedTag>();
        arrowPool.ReturnArrow(arrowEntity);
    }
}