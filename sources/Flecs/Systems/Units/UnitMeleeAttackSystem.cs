

using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Spatial;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.managers.Characters;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using ImGuiGodot.Internal;
using SadRogue.Primitives.GridViews;
using System;


namespace GodotFlecs.sources.Flecs.Systems.Units;

internal class UnitMeleeAttackSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
        .With<SpatialIDComponent>()
        .With<Components.StateComponent>()
        .With<MeleeAttackComponent>()
        .With<TeamComponent>()
        .With<AttackPendingComponent>()
        .With<DirectionComponent>()
        .With<UnitDefinitionComponent>()
        .With<AttackPendingTag>()
        .Without<PlayerInputComponent>()
        .Without<DeadTag>()
        .Without<DestroyRequestTag>();
    }
    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<BlackyWorld>();
        if (world == null) return;

        var dynGrid = world.State.DynamicHash;

        var posArray = it.Field<PositionComponent>(0);
        var spaArray = it.Field<SpatialIDComponent>(1);
        var charArray = it.Field<Components.StateComponent>(2);
        var melleArray = it.Field<MeleeAttackComponent>(3);
        var teamArray = it.Field<TeamComponent>(4);
        var attackPendArray = it.Field<AttackPendingComponent>(5);
        var dirArray = it.Field<DirectionComponent>(6);
        var unitArray = it.Field<UnitDefinitionComponent>(7);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var spa = ref spaArray[i];
            ref var cha = ref charArray[i];
            ref var melle = ref melleArray[i];
            ref var team = ref teamArray[i];
            ref var atp = ref attackPendArray[i];
            ref var dir = ref dirArray[i];
            ref var unit = ref unitArray[i];
            Entity ent = it.Entity(i);

            // Reducción del cooldown de ataque
            if (melle.Timer > 0f)
            {
                melle.Timer -= it.DeltaTime();
            }

            if (cha.stateType == StateType.EXECUTE_ATTACK)
            {
                if (atp.Active && atp.Target.IsAlive() && !atp.Target.Has<DeadTag>())
                {
                    Vector2 originAttackCenter = pos.position + melle.OffSetRange;
                    var targetPos = atp.Target.Get<PositionComponent>();

                    if (atp.Target.Has<UnitDefinitionComponent>())
                    {
                        ushort targetTemplateId = atp.Target.Get<UnitDefinitionComponent>().idTemplate;
                        if (CheckCollisionWithTargetUnit(originAttackCenter, dir.normalized, melle.RangeAttack, targetPos.position, targetTemplateId))
                        {
                            GlobalData.EventsDamage.Enqueue(new DamageEvent
                            {
                                Source = ent,
                                Target = atp.Target,
                                Amount = melle.Damage
                            });
                            //cha.characterStateType = CharacterStateType.ATTACK;
                        }
                        else
                        {
                            // si no hubo collision quiere decir que no hay objetivo y libero
                            cha.stateType = StateType.IDLE;
                            atp.Active = false;
                            atp.Target = default;
                            ent.Remove<AttackPendingTag>();

                            // El objetivo salió de rango: liberamos el slot reservado
                            AttackSlotHelper.RequestReleaseAttackSlot(ent);
                        }
                    }
                    else
                    //if (atp.Target.Has<BuildingDefinitionComponent>())
                    {
                        ushort targetTemplateId = atp.Target.Get<BuildingDefinitionComponent>().idTemplate;
                        if (CheckCollisionWithTargetBuild(originAttackCenter, dir.normalized, melle.RangeAttack, targetPos.position, targetTemplateId))
                        {
                            GlobalData.EventsDamage.Enqueue(new DamageEvent
                            {
                                Source = ent,
                                Target = atp.Target,
                                Amount = melle.Damage
                            });
                            //cha.characterStateType = CharacterStateType.ATTACK;
                        }
                        else
                        {
                            // si no hubo collision quiere decir que no hay objetivo y libero
                            cha.stateType = StateType.IDLE;
                            atp.Active = false;
                            atp.Target = default;
                            ent.Remove<AttackPendingTag>();

                            // El objetivo salió de rango: liberamos el slot reservado
                            AttackSlotHelper.RequestReleaseAttackSlot(ent);
                        }
                    }
                }
                else
                {
                    // si esta muerto libero target
                    cha.stateType = StateType.IDLE;
                    atp.Active = false;
                    atp.Target = default;
                    ent.Remove<AttackPendingTag>();

                    // El target murió o dejó de existir: liberamos el slot reservado
                    AttackSlotHelper.RequestReleaseAttackSlot(ent);
                }
            }

            if (melle.Timer <= 0f)
            {
                if (atp.Active && atp.Target.IsAlive() && !atp.Target.Has<DeadTag>())
                {
                    cha.stateType = StateType.ATTACK;
                    melle.Timer = melle.Cooldown;
                }
                else
                {
                    cha.stateType = StateType.IDLE;
                    atp.Active = false;
                    atp.Target = default;
                    ent.Remove<AttackPendingTag>();

                    // Ataque cancelado por cooldown sin target válido: liberamos el slot
                    AttackSlotHelper.RequestReleaseAttackSlot(ent);
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