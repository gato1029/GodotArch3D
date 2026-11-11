using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;

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

        var posArray = it.Field<PositionComponent>(0);
        var charArray = it.Field<StateComponent>(1);
        var melleArray = it.Field<MeleeAttackComponent>(2);
        var attackPendArray = it.Field<AttackPendingComponent>(4);
        var dirArray = it.Field<DirectionComponent>(5);

        float deltaTime = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var cha = ref charArray[i];
            ref var melle = ref melleArray[i];
            ref var atp = ref attackPendArray[i];
            ref var dir = ref dirArray[i];
            Entity ent = it.Entity(i);

            // 1. Reducción del cooldown global de ataque
            if (melle.Timer > 0f)
            {
                melle.Timer -= deltaTime;
            }

            // 2. Si el objetivo ya no es válido o murió, limpiamos, liberamos el slot y volvemos a IDLE
            if (!atp.Active || !atp.Target.IsAlive() || atp.Target.Has<DeadTag>())
            {
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

                // Aquí SÍ liberamos el slot porque el objetivo ya no existe o se canceló el ataque
                AttackSlotHelper.RequestReleaseAttackSlot(ent);
                continue;
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
                    {// 🎯 ÉXITO: El golpe conectó, aplicamos daño.
                        GlobalData.EventsDamage.Enqueue(new DamageEvent
                        {
                            Source = ent,
                            Target = atp.Target,
                            Amount = melle.Damage
                        });
                        // Reiniciamos animación, activamos el cooldown principal y regresamos a IDLE
                        // ⚠️ IMPORTANTE: NO liberamos el slot aquí para que la unidad mantenga su posición en el anillo
                        melle.AnimTimer = 0f;
                        melle.Timer = 0;
                        cha.stateType = StateType.IDLE;
                        stateRegister.RegisterState(ent.Id.Value, cha.stateType);
                    }else
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

                        // Liberamos el slot para que se libere el espacio en el anillo
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