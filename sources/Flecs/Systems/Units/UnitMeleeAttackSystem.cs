

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
        .With<Components.CharacterComponent>()
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
        var charArray = it.Field<Components.CharacterComponent>(2);
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
            if (cha.characterStateType == CharacterStateType.EXECUTE_ATTACK)
            {
                if (atp.Active && atp.Target.IsAlive() && !atp.Target.Has<DeadTag>())
                {
                    Vector2 originAttackCenter = pos.position + melle.OffSetRange;
                    var targetPos = atp.Target.Get<PositionComponent>();
                    ushort targetTemplateId = atp.Target.Get<UnitDefinitionComponent>().idTemplate;

                    if (CheckCollisionWithTarget(originAttackCenter, dir.normalized, melle.RangeAttack, targetPos.position, targetTemplateId))
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
                        cha.characterStateType = CharacterStateType.IDLE;
                        atp.Active = false;
                        atp.Target = default;
                    }


                }
                else
                {
                    // si esta muerto libero target
                    cha.characterStateType = CharacterStateType.IDLE;
                    atp.Active = false;
                    atp.Target = default;
                }
                                
            }

            if (melle.Timer <= 0f)
            {
                if (atp.Active && atp.Target.IsAlive() && !atp.Target.Has<DeadTag>())
                {
                    cha.characterStateType = CharacterStateType.ATTACK;
                    melle.Timer = melle.Cooldown;
                }
                else
                {
                    cha.characterStateType = CharacterStateType.IDLE;
                    atp.Active = false;
                    atp.Target = default;
                    ent.Remove<AttackPendingTag>();
                }
            }
        }
    }



    private void SearchEnemy(Entity entity, ref Components.CharacterComponent cha, PositionComponent pos, ref MeleeAttackComponent melle, TeamComponent team, DirectionComponent direction, FastSpatialHash dynGrid, SpatialIDComponent spatialId, ref AttackPendingComponent atp)
    {
        Span<int> neighbors = stackalloc int[8];
        Vector2 originAttackCenter = pos.position + melle.OffSetRange;

        int count = dynGrid.QueryNodesBounded(
            originAttackCenter.X,
            originAttackCenter.Y,
            melle.RangeAttack,
            neighbors
        );
        bool existTarget = false;
        for (int ii = 0; ii < count; ii++)
        {
            int neighborId = neighbors[ii];
            if (spatialId.Value == neighborId) continue;

            var targetEntity = dynGrid.GetEntity(neighborId);
            if (!targetEntity.IsAlive() || targetEntity.Has<DeadTag>()) continue;

            // Filtro rápido de equipo antes de buscar componentes pesados
            var otherTeam = targetEntity.Get<TeamComponent>();
            if (team.TeamId == otherTeam.TeamId) continue;

            var targetPosition = targetEntity.Get<PositionComponent>();
            ushort idTemplate = targetEntity.Get<UnitDefinitionComponent>().idTemplate;

            if (CheckCollisionWithTarget(originAttackCenter, direction.normalized, melle.RangeAttack, targetPosition.position, idTemplate))
            {
                cha.characterStateType = CharacterStateType.ATTACK;
                melle.Timer = melle.Cooldown;
                atp.Target = targetEntity;
                atp.Active = true;
                existTarget = true;
                break;
                
            }
        }
        if (!existTarget)
        {
            cha.characterStateType = CharacterStateType.IDLE;
            atp.Active = false;
            atp.Target = default;
            entity.Remove<AttackPendingTag>();
        }
    }

    private bool CheckCollisionWithTarget(Vector2 origin, Vector2 dirNormalized, float range, Vector2 targetPos, ushort templateId)
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