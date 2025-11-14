using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Components;
using System;

namespace GodotFlecs.sources.Flecs.Systems.Units;
internal class UnitRangedAttackSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
        .With<ColliderComponent>()
        .With<CharacterComponent>()
        .With<RangedAttackComponent>() // Componente de ataque a distancia
        .With<TeamComponent>()
        .With<AttackPendingComponent>()
        .With<DirectionComponent>()
        .Without<PlayerInputComponent>()
        .Without<DeadTag>()
         .Without<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<ColliderComponent>(1);
        var charArray = it.Field<CharacterComponent>(2);
        var rangedArray = it.Field<RangedAttackComponent>(3);
        var teamArray = it.Field<TeamComponent>(4);
        var attackPendArray = it.Field<AttackPendingComponent>(5);
        var dirArray = it.Field<DirectionComponent>(6);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var cha = ref charArray[i];
            ref var ranged = ref rangedArray[i];
            ref var team = ref teamArray[i];
            ref var atp = ref attackPendArray[i];
            ref var dir = ref dirArray[i];

            if (ranged.Timer > 0f)
            {
                ranged.Timer -= it.DeltaTime();
                continue; 
            }

            int batchId = (col.idCollider) % GlobalData.numBatchColliders;
            if (batchId != GlobalData.batchIndexColliders) continue;

            if (cha.characterStateType != GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE)
            {
                continue;
            }
            if (atp.Active)
            {
                continue;
            }

            // Buscar enemigos en rango de ataque a distancia
            var nearby = CollisionManager.Instance.characterEntitiesFlecs.QueryAABBBInCirclePoints(pos.position, ranged.Range, col.idCollider,1);
            bool existTarget = false;
            foreach (var target in nearby)
            {
                if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
                {
                    continue; // mismo equipo, ignorar
                }
                else
                {
                    if (target.Owner.IsAlive() && !target.Owner.Has<DeadTag>())
                    {
                        atp.Target = target.Owner;
                        atp.Active = true;
                        ranged.Timer = ranged.Cooldown; // resetear cooldown

                        Vector2 dif = target.Owner.Get<PositionComponent>().position - pos.position;
                        dir.value = dif.Normalized();
                        dir.normalized = new Vector2(Math.Sign(dif.X), Math.Sign(dif.Y));
                        dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.normalized);
                        existTarget = true;
                        break;
                    }
                }
            }
            if (existTarget) continue;
            nearby = CollisionManager.Instance.BuildingsCollidersFlecs.QueryAABBBInCirclePoints(pos.position, ranged.Range, 0, 1);

            foreach (var target in nearby)
            {
                if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
                {
                    continue; // mismo equipo, ignorar
                }
                else
                {
                    if (target.Owner.IsAlive() && !target.Owner.Has<DestroyRequestTag>())
                    {
                        atp.Target = target.Owner;
                        atp.Active = true;
                        ranged.Timer = ranged.Cooldown; // resetear cooldown

                        Vector2 dif = target.Owner.Get<PositionComponent>().position - pos.position;
                        dir.value = dif.Normalized();
                        dir.normalized = new Vector2(Math.Sign(dif.X), Math.Sign(dif.Y));
                        dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.normalized);
                        existTarget = true;
                        break;
                    }
                }
            }

        }
    }
}