using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Building;
internal class BuildRangedAttackSystem:FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
        .With<ColliderComponent>()
        .With<BuildingComponent>()
        .With<RangedAttackComponent>() // Componente de ataque a distancia
        .With<TeamComponent>()
        .With<AttackPendingComponent>()
        .With<DirectionComponent>()
        .Without<PlayerInputComponent>()
        .Without<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<ColliderComponent>(1);
        var builArray = it.Field<BuildingComponent>(2);
        var rangedArray = it.Field<RangedAttackComponent>(3);
        var teamArray = it.Field<TeamComponent>(4);
        var attackPendArray = it.Field<AttackPendingComponent>(5);
        var dirArray = it.Field<DirectionComponent>(6);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var bui = ref builArray[i];
            ref var ranged = ref rangedArray[i];
            ref var team = ref teamArray[i];
            ref var atp = ref attackPendArray[i];
            ref var dir = ref dirArray[i];

            if (ranged.Timer > 0f)
            {
                ranged.Timer -= it.DeltaTime();
                continue;
            }

            //int batchId = (col.idCollider) % GlobalData.numBatchColliders;
            //if (batchId != GlobalData.batchIndexColliders) continue;
           
            if (atp.Active)
            {
                continue;
            }

            // Buscar enemigos en rango de ataque a distancia
            var nearby = CollisionManager.Instance.characterEntitiesFlecs.QueryAABBBInCirclePoints(pos.position, ranged.Range,0, 5);

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
                        break;
                    }                                        
                }
            }
        }
    }
}
