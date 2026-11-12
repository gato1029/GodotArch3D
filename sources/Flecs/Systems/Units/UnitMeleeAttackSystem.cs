using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Units;

internal class UnitMeleeAttackSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<StateComponent>()
          .With<MeleeAttackComponent>()
          .With<TeamComponent>()
          .With<AttackPendingComponent>()
          .With<DirectionComponent>()
          .With<UnitDefinitionComponent>()
          .With<AttackPendingTag>()
          .With<StoppedTag>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;
        var stateRegister = world.State.stateChangeRegistry;
        var dynGrid = world.State.DynamicHash;

        var posArray = it.Field<PositionComponent>(0);
        var charArray = it.Field<StateComponent>(1);
        var melleArray = it.Field<MeleeAttackComponent>(2);
        var teamArray = it.Field<TeamComponent>(3);
        var attackPendArray = it.Field<AttackPendingComponent>(4);
        var dirArray = it.Field<DirectionComponent>(5);

        Span<int> localNeighbors = stackalloc int[8];
        float deltaTime = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var cha = ref charArray[i];
            ref var melle = ref melleArray[i];
            ref var team = ref teamArray[i];
            ref var atp = ref attackPendArray[i];
            ref var dir = ref dirArray[i];
            Entity ent = it.Entity(i);

            // 1. Reducción del cooldown global de ataque
            if (melle.Timer > 0f)
            {
                melle.Timer -= deltaTime;
            }

            // 2. Si el objetivo ya no es válido o murió, buscamos un nuevo objetivo cercano (oportunidad local) antes de rendirnos
            if (!atp.Active || !atp.Target.IsAlive() || atp.Target.Has<DeadTag>())
            {
                // Radio de escaneo local basado en el rango de ataque de su arma
                float scanRadius = MathF.Max(melle.RangeAttack * 1.5f, 1.5f);
                int count = dynGrid.QueryNodesBoundedClosestLayersFiltered(
                    pos.position.X, pos.position.Y,
                    scanRadius, team.TeamId, localNeighbors
                );

                bool foundNewTarget = false;
                Entity newTargetEntity = default;

                for (int ii = 0; ii < count; ii++)
                {
                    var neighborId = localNeighbors[ii];
                    var potentialTarget = dynGrid.GetEntity(neighborId);
                    if (!potentialTarget.IsAlive() || potentialTarget.Has<DeadTag>() || potentialTarget == ent) continue;

                    Vector2 targetPos = potentialTarget.Get<PositionComponent>().position;
                    if (pos.position.DistanceSquaredTo(targetPos) <= scanRadius * scanRadius)
                    {
                        newTargetEntity = potentialTarget;
                        foundNewTarget = true;
                        break;
                    }
                }

                if (foundNewTarget)
                {
                    // 🎯 ¡ENCONTRÓ NUEVO OBJETIVO EN LA GRESCA! Reasignamos de inmediato sin ir a IDLE
                    float impactThreshold = newTargetEntity.Has<MoveColliderComponent>()
                        ? newTargetEntity.Get<MoveColliderComponent>().Radius
                        : 0.5f;

                    // Liberamos el slot anterior y nos prepararnos para atacar al nuevo
                    AttackSlotHelper.RequestReleaseAttackSlot(ent);

                    atp.Target = newTargetEntity;
                    atp.Active = true;
                    atp.targetPosition = newTargetEntity.Get<PositionComponent>().position;
                    atp.impactThereshold = impactThreshold;

                    // Reiniciamos el timer de animación para enganchar el siguiente golpe
                    melle.AnimTimer = 0f;
                    melle.Timer = 0f;
                    continue;
                }
                else
                {
                    // ❌ NADIE CERCA: Limpiamos, liberamos el slot y volvemos a IDLE pacíficamente
                    melle.AnimTimer = 0f;
                    melle.Timer = 0;
                    atp.Active = false;
                    atp.Target = default;
                    ent.Remove<AttackPendingTag>();

                    if (cha.stateType == StateType.ATTACK)
                    {
                        cha.stateType = StateType.IDLE;
                        stateRegister.RegisterState(ent.Id.Value, cha.stateType);
                    }

                    AttackSlotHelper.RequestReleaseAttackSlot(ent);
                    continue;
                }
            }

            // 3. Si el cooldown está listo y no estamos atacando todavía, iniciamos el estado ATTACK y el timer de animación
            if (melle.Timer <= 0f && cha.stateType != StateType.ATTACK)
            {
                cha.stateType = StateType.ATTACK;
                melle.AnimTimer = 0f;
                stateRegister.RegisterState(ent.Id.Value, cha.stateType);
            }

            // 4. Si estamos en estado ATTACK, avanzamos el temporizador de la animación
            if (cha.stateType == StateType.ATTACK)
            {
                if (atp.Target.IsAlive())
                {
                    Vector2 targetPos = atp.Target.Get<PositionComponent>().position;
                    Vector2 lookDir = targetPos - pos.position;

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

                        stateRegister.SetDirection(ent.Id.Value, dir.animationDirection);
                    }
                }
                melle.AnimTimer += deltaTime;
                // Cuando se cumple la duración de la animación, evaluamos colisión y aplicamos daño
                if (melle.AnimTimer >= melle.AnimDuration)
                {
                    Vector2 originAttackCenter = pos.position + melle.OffSetRange;
                    var targetPos = atp.Target.Get<PositionComponent>();
                    bool hitSuccess = false;

                    if (atp.Target.Has<UnitDefinitionComponent>())
                    {
                        ushort targetTemplateId = atp.Target.Get<UnitDefinitionComponent>().idTemplate;
                        if (CheckCollisionWithTargetUnit(originAttackCenter, dir.normalized, melle.RangeAttack, targetPos.position, targetTemplateId))
                        {
                            hitSuccess = true;
                        }
                    }
                    else
                    {
                        ushort targetTemplateId = atp.Target.Get<BuildingDefinitionComponent>().idTemplate;
                        if (CheckCollisionWithTargetBuild(originAttackCenter, dir.normalized, melle.RangeAttack, targetPos.position, targetTemplateId))
                        {
                            hitSuccess = true;
                        }
                    }

                    if (hitSuccess)
                    {
                        // 🎯 ÉXITO: El golpe conectó, aplicamos daño.
                        GlobalData.EventsDamage.Enqueue(new DamageEvent
                        {
                            Source = ent,
                            Target = atp.Target,
                            Amount = melle.Damage
                        });

                        melle.AnimTimer = 0f;
                        melle.Timer = 0;
                        cha.stateType = StateType.IDLE;
                        stateRegister.RegisterState(ent.Id.Value, cha.stateType);
                    }
                    else
                    {
                        // ❌ FALLO: El objetivo esquivó o salió del rango al golpear. Limpiamos y liberamos slot.
                        melle.Timer = 0;
                        melle.AnimTimer = 0f;
                        atp.Active = false;
                        atp.Target = default;
                        ent.Remove<AttackPendingTag>();

                        if (cha.stateType == StateType.ATTACK)
                        {
                            cha.stateType = StateType.IDLE;
                            stateRegister.RegisterState(ent.Id.Value, cha.stateType);
                        }

                        AttackSlotHelper.RequestReleaseAttackSlot(ent);
                    }
                }
            }
        }
    }

    private bool CheckCollisionWithTargetBuild(Vector2 origin, Vector2 dirNormalized, float range, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.buildingPalette.GetData(templateId);
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.CheckAttackHalfCircle(
                origin.X, origin.Y,
                dirNormalized.X, dirNormalized.Y,
                range,
                targetPos.X, targetPos.Y,
                ref fast
            ))
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckCollisionWithTargetUnit(Vector2 origin, Vector2 dirNormalized, float range, Vector2 targetPos, ushort templateId)
    {
        var template = BlackyPalletesPersistence.characterPalette.GetData(templateId);
        foreach (var item in template.bodyColliders)
        {
            FastCollider fast = item;
            if (CollisionMathHelper.CheckAttackHalfCircle(
                origin.X, origin.Y,
                dirNormalized.X, dirNormalized.Y,
                range,
                targetPos.X, targetPos.Y,
                ref fast
            ))
            {
                return true;
            }
        }
        return false;
    }
}