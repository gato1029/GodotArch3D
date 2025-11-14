using Arch.Bus;
using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.Flecs.Components;
using GodotEcsArch.sources.Flecs.Globals;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using RVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Units;
internal class AttackHitSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true; //antes true;
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<AttackPendingComponent>()
            .With<AnimationComponent>()
            .With<CharacterComponent>()
            .With<MeleeAttackComponent>()
            .With<PositionComponent>()
            .With<ColliderComponent>()
            .Without<DeadTag>()
            .Without<DestroyRequestTag>();

    }

    protected override void OnIter(Iter it)
    {
        var atkArray = it.Field<AttackPendingComponent>(0);
        var aniArray = it.Field<AnimationComponent>(1);
        var chaArray = it.Field<CharacterComponent>(2);
        var melleArray = it.Field<MeleeAttackComponent>(3);
        var posArray = it.Field<PositionComponent>(4);
        var colArray = it.Field<ColliderComponent>(5);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var atk = ref atkArray[i];
            ref var ani = ref aniArray[i];
            ref var cha = ref chaArray[i];
            ref var mel = ref melleArray[i];
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];

            if (atk.Active && cha.characterStateType == GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE)
            {
                cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.ATTACK;
                ani.animationComplete = false;   // empezar de nuevo
            }
            if (atk.Active && ani.animationComplete && cha.characterStateType == GodotEcsArch.sources.managers.Characters.CharacterStateType.ATTACK)
            {
                
                //
                // aplicar daño
                if (atk.Target != default && atk.Target.IsAlive() && !atk.Target.Has<DeadTag>() && !atk.Target.Has<DestroyRequestTag>())
                {
                    var attackerPos = pos.position;
                    bool isBuilding = atk.Target.Has<BuildingComponent>();

                    if (isBuilding)
                    {
                        var colliderBuilder = atk.Target.Get<ColliderComponent>().aabb;

                        Rect2 rect2 = new Rect2(pos.position - new Vector2(mel.Range,mel.Range)/2, new Vector2(mel.Range,mel.Range));
                        //rect2.Position = pos.position - rect2.Size / 2 + col.offset;

                        if (rect2.Intersects(colliderBuilder))
                        {
                            if (atk.Target != default && atk.Target.IsAlive() && !atk.Target.Has<DeadTag>() && !atk.Target.Has<DestroyRequestTag>())
                            {
                                // Creamos un evento de daño (temporal)
                                GlobalData.EventsDamage.Enqueue(new DamageEvent
                                {
                                    Source = it.Entity(i),
                                    Target = atk.Target,
                                    Amount = mel.Damage
                                });

                                //atk.Target.Set(new DamagePendingComponent { Amount = mel.Damage, Source = it.Entity(i) });
                            }
                            atk.Active = false;
                        }
                    }
                    else
                    {
                        // Obtener posiciones (si tenés PositionComponent en ambos)
                        
                        var targetPos = atk.Target.Get<PositionComponent>().position;
                        // Dirección hacia el objetivo
                        Vector2 dif = targetPos - attackerPos;
                        float dist = dif.Length();
                        if (dist <= mel.Range)
                        {
                            if (atk.Target != default && atk.Target.IsAlive() && !atk.Target.Has<DeadTag>() && !atk.Target.Has<DestroyRequestTag>())
                            {
                                GlobalData.EventsDamage.Enqueue(new DamageEvent
                                {
                                    Source = it.Entity(i),
                                    Target = atk.Target,
                                    Amount = mel.Damage
                                }); 
                              
                                ////  atk.Target.Mut(ref it).Set(new DamagePendingComponent { Amount = mel.Damage, Source = it.Entity(i) });
                                //atk.Target.Set(new DamagePendingComponent { Amount = mel.Damage, Source = it.Entity(i) });
                            }
                            atk.Active = false;
                        }
                    }                    
                    
                    atk.Target = default;
                    atk.Active = false;
                    cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE;
                    
                }
                else
                {
                    atk.Target = default;
                    atk.Active = false;
                    cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.IDLE;
                }

                
            }
        }
    }
}