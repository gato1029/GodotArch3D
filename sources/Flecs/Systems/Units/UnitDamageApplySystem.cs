using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Units;
internal class UnitDamageApplySystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; // debe ser single-thread
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<HealthComponent>()
          .With<DamagePendingComponent>()
          .With<CharacterComponent>()
          .Without<DeadTag>()
          .Write(Ecs.Wildcard);
    }

    protected override void OnIter(Iter it)
    {
        var hpArray = it.Field<HealthComponent>(0);
        var dmgArray = it.Field<DamagePendingComponent>(1);
        var chaArray = it.Field<CharacterComponent>(2);
        for (int i = 0; i < it.Count(); i++)
        {
            ref var hp = ref hpArray[i];
            ref var dmg = ref dmgArray[i];
            ref var cha = ref chaArray[i];

            hp.value -= dmg.Amount;
            cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.TAKE_HIT;
            if (hp.value <= 0)
            {
                hp.value = 0;
                cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.DIE;
                it.Entity(i).Add<DeadTag>();
                it.Entity(i).Set(new DeathTimerComponent { RemainingTime = 2f }); // ⏱ 2 segundos, por ejemplo
            }                                               
            // Eliminar el componente temporal
            it.Entity(i).Remove<DamagePendingComponent>();
        }
    }
}