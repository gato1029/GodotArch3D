using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Characters;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GodotEcsArch.sources.Flecs.Systems.Units;

internal class MoveTargetAttackMelleSystem:FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<MoveResolutorComponent>()
          .With<AttackPendingComponent>()
          .With<MeleeAttackComponent>()
          .With<MoveColliderComponent>()
          .With<MoveTargetComponent>()          
          .Without<AttackPendingTag>()
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);          
        var resolutorArray = it.Field<MoveResolutorComponent>(1);
        var attackpendingArray = it.Field<AttackPendingComponent>(2);
        var theresholdArray = it.Field<MeleeAttackComponent>(3);
        var moveArray = it.Field<MoveColliderComponent>(4);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
                          
            ref var resolutor = ref resolutorArray[i];
            ref var attackPending = ref attackpendingArray[i];
            ref var melle = ref theresholdArray[i];
            ref var move = ref moveArray[i];
                //if (it.Entity(i).Has<StoppedTag>() && attackPending.Active)
                //{                 
                //    it.Entity(i).Add<AttackPendingTag>();
                //    continue;
                //}


            Vector2 toTarget = attackPending.targetPosition - (pos.position+move.Offset);
            float distSq = toTarget.LengthSquared();

            float umbralLlegada = melle.RangeAttack;// thereshold.radius;
            if (distSq <= umbralLlegada)//0.05f) // Umbral de llegada
            {
              
                if (attackPending.Active)
                {
                    it.Entity(i).Add<AttackPendingTag>();
                }
                if (it.Entity(i).Has<MoveTargetComponent>())
                {
                    it.Entity(i).Remove<MoveTargetComponent>();
                }                
                it.Entity(i).Add<StoppedTag>();
                //steering.DesiredDir = Vector2.Zero;
                resolutor.BlockedTimer = 0;
                resolutor.Blocked = true;
                continue;
            }
        }
    }
}
