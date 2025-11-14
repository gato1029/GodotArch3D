using Flecs.NET.Bindings;
using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotFlecs.sources.Flecs.Systems.Generic;
internal class ProjectileMoveSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<ProjectileComponent>()
          .Without<DestroyRequestTag>();         

    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var projArray = it.Field<ProjectileComponent>(1);
        float dt = it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var proj = ref projArray[i];

            // === GUIADOS (HOMING) ===
            if (proj.Homing)
            {
                // Verificamos que el target siga vivo y tenga posición
                if (proj.Target.IsAlive() && !proj.Target.Has<DestroyRequestTag>()&& !proj.Target.Has<DeadTag>())
                {
                    // Posición actual del objetivo
                    var targetPos = proj.Target.Get<PositionComponent>().position;

                    // Dirección hacia el objetivo
                    var newDir = (targetPos - pos.position).Normalized();

                    // Suavizado opcional (evita giros bruscos)
                    proj.Direction = proj.Direction.Lerp(newDir, dt * 5f).Normalized();

                    // Movimiento
                    pos.position += proj.Direction * proj.Speed * dt;

                    // Chequeo de impacto
                    float dist = (targetPos - pos.position).Length();

                    // Radio de impacto configurable (por ejemplo 0.4–0.6f)
                    if (dist <= 0.03f)
                    {
                        GlobalData.EventsDamage.Enqueue(new DamageEvent
                        {
                            Source = it.Entity(i),
                            Target = proj.Target,
                            Amount = proj.Damage
                        });
                        //// Aplica daño al objetivo real
                        //ApplyDamage(it.World(), proj.Target, proj.Damage);

                        // Marca el proyectil para destruir
                        it.Entity(i).Add<DestroyRequestTag>();
                        continue;
                    }
                }
                else
                {
                    // Si el target murió o no tiene posición, el proyectil se destruye
                    it.Entity(i).Add<DestroyRequestTag>();
                    continue;
                }

                // Reducción de vida del proyectil
                proj.LifeTime -= dt;
                if (proj.LifeTime <= 0f)
                {
                    it.Entity(i).Add<DestroyRequestTag>();
                    continue;
                }

            }
            // === LINEALES ===
            else
            {
                if (proj.Target.IsAlive() && !proj.Target.Has<DestroyRequestTag>() && !proj.Target.Has<DeadTag>())
                {
                    var prevPos = pos.position;
                    pos.position += proj.Direction * proj.Speed * dt;

                    // Vector desde la posición previa al objetivo
                    var toTargetBefore = proj.TargetPosition - prevPos;
                    // Vector desde la posición actual al objetivo
                    var toTargetNow = proj.TargetPosition - pos.position;

                    // Si la dirección del vector cambió de signo (se pasó)
                    if (toTargetNow.Dot(toTargetBefore) <= 0f)
                    {
                        pos.position = proj.TargetPosition; // lo "clampemos" exacto                       
                       
                        var targetPosNow = proj.Target.Get<PositionComponent>().position;
                        float dist = (targetPosNow - pos.position).Length();

                        // tolerancia: tamaño del proyectil o radio del objetivo
                        if (dist <= 0.03f) // ajustá según tu escala
                        {
                            GlobalData.EventsDamage.Enqueue(new DamageEvent
                            {
                                Source = it.Entity(i),
                                Target = proj.Target,
                                Amount = proj.Damage
                            });
                            //ApplyDamage(it.World(),proj.Target, proj.Damage);

                        }                        
                        // destruir el proyectil siempre (impacte o no)
                        it.Entity(i).Add<DestroyRequestTag>();
                        continue;
                    }
                }
                else
                {
                    // Si el target murió o no tiene posición, el proyectil se destruye
                    it.Entity(i).Add<DestroyRequestTag>();
                    continue;
                }
            }
        }
    }
    private void ApplyDamage(World world, Entity target, int damage)
    {
        if (!target.IsAlive() && !target.Has<DestroyRequestTag>() && !target.Has<DeadTag>())
            return;

        int dmgAmount = damage;           
        target.Set(new DamagePendingComponent { Amount = dmgAmount });
        
    }
}