using Arch.Bus;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using SadRogue.Primitives.GridViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Units;
internal class UnitMeleeAttackSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; 
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
        .With<ColliderComponent>()
        .With<CharacterComponent>()
        .With<MeleeAttackComponent>()
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
        var melleArray = it.Field<MeleeAttackComponent>(3);
        var teamArray = it.Field<TeamComponent>(4);
        var attackPendArray = it.Field<AttackPendingComponent>(5);
        var dirArray = it.Field<DirectionComponent>(6);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];            
            ref var col = ref colArray[i];
            ref var cha = ref charArray[i];
            ref var melle = ref melleArray[i];
            ref var team = ref teamArray[i];
            ref var atp = ref attackPendArray[i];
            ref var dir = ref dirArray[i];

            if (melle.Timer > 0f)
            {
                melle.Timer -= it.DeltaTime();
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
            // cooldown control
           

            // obtener posibles enemigos cercanos usando spatial hash
            var nearby = CollisionManager.Instance.characterEntitiesFlecs.QueryCirclePoints(pos.position, melle.Range, col.idCollider);
            bool existTarget = false;
            foreach (var target in nearby)
            {

                if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
                {
                    continue; // mismo equipo, ignorar
                }
                else
                {
                    if (target.Owner!=default && target.Owner.IsAlive() && !target.Owner.Has<DeadTag>())
                    {
                        atp.Target = target.Owner;
                        atp.Active = true;

                        melle.Timer = melle.Cooldown; // resetear cooldown



                        Vector2 dif = target.Owner.Get<PositionComponent>().position - pos.position;
                        dir.value = dif.Normalized();
                        dir.normalized = new Vector2(Math.Sign(dif.X), Math.Sign(dif.Y));
                        dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.normalized);

                        break;
                    }
                }
            }
            // para buscar estructuras cercanas
            if (existTarget) continue;
            nearby = CollisionManager.Instance.BuildingsCollidersFlecs.QueryBruteShape( melle.Range, pos.position, 0);
            
            foreach (var target in nearby)
            {
                if (target.Owner.Get<TeamComponent>().TeamId == team.TeamId)
                {
                    continue; // mismo equipo, ignorar
                }
                else
                {
                    if (target.Owner != default && target.Owner.IsAlive() && !target.Owner.Has<DestroyRequestTag>())
                    {
                        atp.Target = target.Owner;
                        atp.Active = true;

                        melle.Timer = melle.Cooldown; // resetear cooldown

                        Vector2 dif = target.Owner.Get<PositionComponent>().position - pos.position;
                        dir.value = dif.Normalized();
                        dir.normalized = new Vector2(Math.Sign(dif.X), Math.Sign(dif.Y));
                        dir.animationDirection = CommonOperations.GetDirectionAnimationLeftRight(dir.normalized);
                        break;
                    }
                }
            }
        }
    }


}
