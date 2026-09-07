
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

            //if (input.attackPressed && character.characterStateType != CharacterStateType.EXECUTE_ATTACK)
            //{
            //    input.isAttack = true;
            //    character.characterStateType = CharacterStateType.ATTACK;
            //    return true;
            //}
            if (cha.characterStateType == CharacterStateType.EXECUTE_ATTACK)
            {
                if (atp.Active) // tiene objetivo
                {
                    if (atp.Target.IsAlive() && !atp.Target.Has<DeadTag>()) // verifica que no este marcado para morir
                    {
                        GlobalData.EventsDamage.Enqueue(new DamageEvent
                        {
                            Source = ent,
                            Target = atp.Target,
                            Amount = melle.Damage
                        });
                    }                    
                    atp.Active = false; // liberamos objetivo
                    atp.Target = default;
                }
            }
            if (melle.Timer > 0f)
            {
                melle.Timer -= it.DeltaTime();                
            }
            else
            {                                
                ExecuteAttack(ent,ref cha, pos, ref melle, team, dir, dynGrid, spa, ref atp);                
            }
            //attack.Timer = attack.Cooldown; // inicia cooldown
            

            //    if (melle.Timer > 0f)
            //    {
            //        melle.Timer -= it.DeltaTime();
            //        continue;
            //    }

            //    int batchId = (col.idCollider) % GlobalData.numBatchColliders;
            //    if (batchId != GlobalData.batchIndexColliders) continue;

            //    if (cha.characterStateType != GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE)
            //    {
            //        continue;
            //    }
            //    if (atp.Active)
            //    {
            //        continue;
            //    }
            //    // cooldown control


            //    // obtener posibles enemigos cercanos usando spatial hash
            //    var nearby = CollisionManager.Instance.characterEntitiesFlecs.QueryCirclePoints(pos.position, melle.RangeAttack, col.idCollider);
            //    bool existTarget = false;
            //    foreach (var target in nearby)
            //    {

            //        if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
            //        {
            //            continue; // mismo equipo, ignorar
            //        }
            //        else
            //        {
            //            if (target.Owner != default && target.Owner.IsAlive() && !target.Owner.Has<DeadTag>())
            //            {
            //                atp.Target = target.Owner;
            //                atp.Active = true;

            //                melle.Timer = melle.Cooldown; // resetear cooldown



            //                Vector2 dif = target.Owner.Get<PositionComponent>().position - pos.position;
            //                dir.value = dif.Normalized();
            //                dir.normalized = new Vector2(Math.Sign(dif.X), Math.Sign(dif.Y));
            //                dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.normalized);

            //                break;
            //            }
            //        }
            //    }
            //    // para buscar estructuras cercanas
            //    if (existTarget) continue;
            //    nearby = CollisionManager.Instance.BuildingsCollidersFlecs.QueryBruteShape(melle.RangeAttack, pos.position, 0);

            //    foreach (var target in nearby)
            //    {
            //        if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
            //        {
            //            continue; // mismo equipo, ignorar
            //        }
            //        else
            //        {
            //            if (target.Owner != default && target.Owner.IsAlive() && !target.Owner.Has<DestroyRequestTag>())
            //            {
            //                atp.Target = target.Owner;
            //                atp.Active = true;

            //                melle.Timer = melle.Cooldown; // resetear cooldown

            //                Vector2 dif = target.Owner.Get<PositionComponent>().position - pos.position;
            //                dir.value = dif.Normalized();
            //                dir.normalized = new Vector2(Math.Sign(dif.X), Math.Sign(dif.Y));
            //                dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.normalized);
            //                break;
            //            }
            //        }
            //    }
        }
    }

    private void ExecuteAttack(Entity entity, ref Components.CharacterComponent cha, PositionComponent pos, ref MeleeAttackComponent melle, TeamComponent team, DirectionComponent direction, FastSpatialHash dynGrid, SpatialIDComponent spatialId, ref AttackPendingComponent atp)
    {
        Span<int> neighbors = stackalloc int[8];

        Vector2 originAttackCenter = pos.position + melle.OffSetRange;
        // 🔥 SOLO UNA QUERY (optimización crítica)
        int count = dynGrid.QueryNodesBounded(
            originAttackCenter.X,
            originAttackCenter.Y,
            melle.RangeAttack,
            neighbors
        );

        for (int ii = 0; ii < count; ii++)
        {
            var targetEntity = dynGrid.GetEntity(neighbors[ii]);

            if (spatialId.Value == neighbors[ii]) continue; // no nos atacamos a nosotros mismos

            var otherTeam = targetEntity.Get<TeamComponent>();
            if (team.TeamId == otherTeam.TeamId) continue; // mismo equipo, ignorar

            if (targetEntity.IsAlive() && !targetEntity.Has<DeadTag>())
            {
                var targetPosition = targetEntity.Get<PositionComponent>();
                ushort idTemplate = targetEntity.Get<UnitDefinitionComponent>().idTemplate;
                var template = BlackyPalletesPersistence.characterPalette.GetData(idTemplate);



                foreach (var item in template.bodyColliders)
                {
                    FastCollider fast = item;
                    if (CollisionMathHelper.CheckAttackHalfCircle(
                        originAttackCenter.X,
                        originAttackCenter.Y,
                        direction.normalized.X,
                        direction.normalized.Y,
                        melle.RangeAttack,
                        targetPosition.position.X,
                        targetPosition.position.Y,
                        ref fast
                    ))
                    {
                        cha.characterStateType = CharacterStateType.ATTACK;
                        melle.Timer = melle.Cooldown;
                        atp.Target = targetEntity;
                        atp.Active = true;
                        break;
                    }
                }



            }
        }


    }
}