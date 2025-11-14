using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Building;
internal class BuildDamageApplySystem:FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; 
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<HealthComponent>()
          .With<DamagePendingComponent>()
          .With<BuildingComponent>()
          .Without<DestroyRequestTag>()
          //.Write<DeathTimerComponent>()
          //.Write<DamagePendingComponent>()
          //.Write<DestroyRequestTag>()
          .Write(Ecs.Wildcard);


    }

    protected override void OnIter(Iter it)
    {
        var hpArray = it.Field<HealthComponent>(0);
        var dmgArray = it.Field<DamagePendingComponent>(1);
        
     
        for (int i = 0; i < it.Count(); i++)
        {
            ref var hp = ref hpArray[i];
            ref var dmg = ref dmgArray[i];           
            hp.value -= dmg.Amount;
            Entity e = it.Entity(i);
            if (e == default || !e.IsAlive())
            {
                int muerto = 0;
            }
            if (hp.value <= 0)
            {
                hp.value = 0;         
                e.Add<DestroyRequestTag>();
                e.Set(new DeathTimerComponent { RemainingTime = 0f }); 
            }
            // Eliminar el componente temporal
            e.Remove<DamagePendingComponent>();
        }
 
    }
}
