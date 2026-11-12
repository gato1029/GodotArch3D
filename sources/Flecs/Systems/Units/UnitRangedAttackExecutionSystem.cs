using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Units;

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
          .With<StoppedTag>()
          .Without<DeadTag>();
        // Nota: NO exigimos DirectionComponent en la query global para permitir que las torretas sin dirección también disparen.
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var arrowPool = world.Services.ArrowPool;
        var stateRegister = world.State.stateChangeRegistry;
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
                // 🔄 ROTACIÓN / DIRECCIÓN OPCIONAL: Si la entidad tiene DirectionComponent (ej. arqueros), la orientamos hacia el objetivo
                if (entity.Has<DirectionComponent>())
                {
                    ref var dir = ref entity.GetMut<DirectionComponent>();
                    Vector2 lookDir = targetPos.position - pos.position;

                    if (lookDir != Vector2.Zero)
                    {
                        dir.value = lookDir.Normalized();
                        dir.normalized = new Vector2(
                            MathF.Abs(lookDir.X) < 0.01f ? 0 : MathF.Sign(lookDir.X),
                            MathF.Abs(lookDir.Y) < 0.01f ? 0 : MathF.Sign(lookDir.Y)
                        );

                        switch (dir.directionType)
                        {
                            case DirectionAnimationType.DOS:
                                dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.value);
                                break;
                            case DirectionAnimationType.CUATRO:
                                dir.animationDirection = CommonOperations.GetDirectionAnimation(dir.value);
                                break;
                            case DirectionAnimationType.OCHO:
                                dir.animationDirection = CommonOperations.GetDirectionAnimationEight(dir.value);
                                break;
                        }

                        stateRegister.SetDirection(entity.Id.Value, dir.animationDirection);
                    }
                }

                // 3. Controlar la cadencia de fuego (Cooldown)
                ranged.Timer += deltaTime;
                if (ranged.Timer < ranged.Cooldown)
                {
                    continue;
                }

                ranged.Timer -= ranged.Cooldown;

                // 4. Delegar el disparo del proyectil
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
                        impactThereshold = atp.impactThereshold
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