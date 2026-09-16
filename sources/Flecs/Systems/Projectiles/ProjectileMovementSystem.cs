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
                // if es unidad verifico contra el cuerpo
                var collisionThreshold = projTarget.Target.Get<MoveColliderComponent>().Radius;

                if (pos.Position.DistanceSquaredTo(targetPos.position) <= (collisionThreshold * collisionThreshold))
                {
                    // si esta dentro del umbral minimo verifico contra su collider del body
                    if (projTarget.isUnit)
                    {
                        ushort idTarget = projTarget.Target.Get<UnitDefinitionComponent>().idTemplate;
                        if (CheckCollisionWithTargetUnit(pos.Position, targetPos.position, idTarget))
                        {
                            GlobalData.EventsDamage.Enqueue(new DamageEvent
                            {
                                Target = projTarget.Target,
                                Amount = projTarget.Damage,
                            });
                            arrowPool.EnqueueRecycle(arrowEntity); // 🟢 Seguro en multihilo
                            continue;
                        }
                        else
                        {                          
                            arrowPool.EnqueueRecycle(arrowEntity); // 🟢 Seguro en multihilo
                            continue;
                        }

                    }
                    else
                    {
                        // el edificio nunca se mueve
                        GlobalData.EventsDamage.Enqueue(new DamageEvent
                        {
                            Target = projTarget.Target,
                            Amount = projTarget.Damage,
                        });
                        arrowPool.EnqueueRecycle(arrowEntity); // 🟢 Seguro en multihilo
                        continue;

                   
                    }

                   
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

    private bool CheckCollisionWithTargetBuild(Vector2 point, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.buildingPalette.GetData(templateId);
  
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.PointCheck(point.X, point.Y, targetPos.X, targetPos.Y, ref fast
            ))
            {
                return true;
            }
        }
        return false;
   
    }

    private bool CheckCollisionWithTargetUnit(Vector2 point, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.characterPalette.GetData(templateId);
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.PointCheck(point.X,point.Y,targetPos.X,targetPos.Y, ref fast
            ))
            {
                return true;
            }
        }
        return false;
    }


}