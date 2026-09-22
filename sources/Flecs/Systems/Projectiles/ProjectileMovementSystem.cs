using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.utils;
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
          .With<RenderGPUComponent>()
          .With<RenderTransformComponent>()
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
        var gpuArray = it.Field<RenderGPUComponent>(3);
        var transformArray = it.Field<RenderTransformComponent>(4);
        float deltaTime = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var vel = ref velArray[i];
            ref var projTarget = ref targetArray[i];
            ref var gpu = ref gpuArray[i];
            ref var transform = ref transformArray[i];
            var arrowEntity = it.Entity(i);


            // 1. Mover flecha
            pos.Position += vel.Velocity * deltaTime;

            float z = CommonAtributes.Calculate(gpu.depthOffset, 10, gpu.layerRender, pos.Position); // debemos usar esto apartir de ahora
            var tt = transform.transform;
            tt.Origin = new Vector3(pos.Position.X + gpu.originOffset.X, pos.Position.Y + gpu.originOffset.Y, z);
            transform.transform = tt;

            RenderingServer.MultimeshInstanceSetTransform(gpu.rid, gpu.instance, transform.transform);

            // 2. Comprobar colisión en tiempo real (si el objetivo sigue vivo)
            if (projTarget.Target.IsAlive() && !projTarget.Target.Has<DeadTag>())
            {
                var targetPos = projTarget.Target.Get<PositionComponent>();
                // if es unidad verifico contra el cuerpo
                var collisionThreshold = projTarget.impactThereshold;

                if (pos.Position.DistanceSquaredTo(targetPos.position) <= (collisionThreshold * collisionThreshold))
                {                        
                    GlobalData.EventsDamage.Enqueue(new DamageEvent
                    {
                        Target = projTarget.Target,
                        Amount = projTarget.Damage,
                    });
                    arrowPool.EnqueueRecycle(arrowEntity); // 🟢 Seguro en multihilo
                    continue;
                }
                             
            }

            // 3. Comprobar si la flecha llegó a su distancia máxima de recorrido sin impactar
            float traveledDistSq = projTarget.Origin.DistanceSquaredTo(pos.Position);
            float totalDistSq = projTarget.TotalDistance * projTarget.TotalDistance;

            if (traveledDistSq >= totalDistSq)
            {
                arrowPool.EnqueueRecycle(arrowEntity); // 🟢 Seguro en multihilo
            }


        }
    }
  
}