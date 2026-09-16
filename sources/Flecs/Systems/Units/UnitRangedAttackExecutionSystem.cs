using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;

namespace GodotEcsArch.sources.Flecs.Systems.Units;

internal class UnitRangedAttackExecutionSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

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
        float deltaTime = it.DeltaTime();

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
                entity.Remove<AttackPendingTag>();
                atp.Target = default;
                atp.Active = false;
                ranged.Timer = 0f;
                continue;
            }

            var targetPos = atp.Target.Get<PositionComponent>();

            // 2. Verificar rango
            float rangeSquared = ranged.Range * ranged.Range;
            float distanceSquared = pos.position.DistanceSquaredTo(targetPos.position);

            if (distanceSquared <= rangeSquared)
            {
                // 3. Controlar la cadencia de fuego (Cooldown)
                ranged.Timer += deltaTime;
                if (ranged.Timer < ranged.Cooldown)
                {
                    continue;
                }

                ranged.Timer -= ranged.Cooldown;

                // 4. Delegar SIEMPRE al hilo principal de forma segura
                Vector2 direction = (targetPos.position - pos.position).Normalized();
                float totalDist = pos.position.DistanceTo(targetPos.position);

                arrowPool.EnqueueSpawn(
                    pos.position,
                    direction * ranged.SpeedProjectile,
                    new ProjectileTargetComponent
                    {
                        Target = atp.Target,
                        Damage = ranged.Damage,
                        idMod = ranged.idMod,
                        idProjectile = ranged.idProjectile,
                        Origin = pos.position,
                        Destination = targetPos.position,
                        TotalDistance = totalDist,
                        isUnit = atp.isUnit
                    }
                );
            }
            else
            {
                entity.Remove<AttackPendingTag>();
                atp.Target = default;
                atp.Active = false;
                ranged.Timer = 0f;
            }
        }
    }
}