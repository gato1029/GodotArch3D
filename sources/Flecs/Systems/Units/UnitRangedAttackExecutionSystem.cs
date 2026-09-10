using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Units;

internal class UnitRangedAttackExecutionSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<RangedAttackComponent>()
          .With<AttackPendingComponent>()
          .With<AttackPendingTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var arrowPool = world.Services.ArrowPool;

        var posArray = it.Field<PositionComponent>(0);
        var rangedArray = it.Field<RangedAttackComponent>(1);
        var attackPendingArray = it.Field<AttackPendingComponent>(2);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var ranged = ref rangedArray[i];
            ref var atp = ref attackPendingArray[i];
            var entity = it.Entity(i);

            // 1. Validar si el objetivo actual sigue vivo
            if (!atp.Target.IsAlive() || atp.Target.Has<DeadTag>())
            {
                // El objetivo murió: liberamos para buscar uno nuevo
                entity.Remove<AttackPendingTag>();
                atp.Target = default;
                atp.Active = false;
                continue;
            }

            var targetPos = atp.Target.Get<PositionComponent>();

            // 2. Verificar rango usando el cuadrado de la distancia (sin raíces cuadradas)
            float rangeSquared = ranged.Range * ranged.Range;
            float distanceSquared = pos.position.DistanceSquaredTo(targetPos.position);

            if (distanceSquared <= rangeSquared)
            {
                // El objetivo sigue en rango: DISPARAR DIRECTAMENTE (Sin consultar grid)
                if (arrowPool.TryGetAvailableArrow(out Entity arrowEntity))
                {
                    Vector2 direction = (targetPos.position - pos.position).Normalized();
                    float totalDist = pos.position.DistanceTo(targetPos.position);
                    arrowEntity.Set(new ProjectilePositionComponent { Position = pos.position });
                    arrowEntity.Set(new ProjectileVelocityComponent { Velocity = direction * ranged.SpeedProjectile });
                    arrowEntity.Set(new ProjectileTargetComponent { Target = atp.Target, Damage = ranged.Damage, idMod = ranged.idMod, idProjectile = ranged.idProjectile, Origin =pos.position, Destination = targetPos.position , TotalDistance = totalDist });
                    arrowEntity.Add<ActiveProjectileTag>();
                }
            }
            else
            {
                // El objetivo se salió de rango: liberamos el tag para que el buscador lo reasigne o busque otro
                entity.Remove<AttackPendingTag>();
                atp.Target = default;
                atp.Active = false;
            }
        }
    }
}