using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Units;
internal class UnitEnemySearchSystem: FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;

    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<ColliderComponent>()
          .With<TeamComponent>()
          .With<CharacterComponent>()
          .With<EnemySearchComponent>()
          .With<AttackPendingComponent>()
          .Without<MoveTargetComponent>()
          .Without<PlayerInputComponent>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>();
    }

    protected override void OnIter(Iter it)
    {
        var posArray = it.Field<PositionComponent>(0);
        var colArray = it.Field<ColliderComponent>(1);
        var teamArray = it.Field<TeamComponent>(2);
        var charArray = it.Field<CharacterComponent>(3);
        var searchArray = it.Field<EnemySearchComponent>(4);
        var atpArray = it.Field<AttackPendingComponent>(5);

        float delta = (float)it.DeltaTime();

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var col = ref colArray[i];
            ref var team = ref teamArray[i];
            ref var cha = ref charArray[i];
            ref var search = ref searchArray[i];
            ref var atp = ref atpArray[i];
            var e = it.Entity(i);

            if (atp.Active)
            {
                continue;
            }
            // Actualizar temporizador
            search.Timer -= delta;
            if (search.Timer > 0f)
                continue;
            search.Timer = search.Interval; // reiniciar cooldown de búsqueda

            // Buscar enemigos cercanos usando el CollisionManager
            var nearby = CollisionManager.Instance.characterEntitiesFlecs.QueryAABBBInCirclePoints(
                pos.position,
                search.Radius,
                col.idCollider,
                1 // Cantidad de contactos a retornar
            );
            bool existTarget = false;
            foreach (var target in nearby)
            {
                var targetEntity = target.Owner;

                // Ignorar aliados
                if (targetEntity.Get<TeamComponent>().TeamId == team.TeamId)
                    continue;
                if (target.Owner.IsAlive() && !target.Owner.Has<DeadTag>())
                {
                    // Asignar destino al enemigo encontrado
                    Vector2 targetPos = targetEntity.Get<PositionComponent>().position;
                    cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.MOVING;
                    e.Set(new MoveTargetComponent(targetPos));
                    e.Set(new MoveResolutorComponent(false, 0, pos.position));
                    existTarget = true;

                    break;
                }
            }
            if (existTarget) continue;
            nearby = CollisionManager.Instance.BuildingsCollidersFlecs.QueryAABBBInCirclePoints(pos.position, search.Radius, 0, 1);

            foreach (var target in nearby)
            {
                var targetEntity = target.Owner;
                if (targetEntity.Get<TeamComponent>().TeamId == team.TeamId)
                {
                    continue; // mismo equipo, ignorar
                }
                else
                {
                    if (target.Owner.IsAlive() && !target.Owner.Has<DestroyRequestTag>())
                    {
                        // Asignar destino al enemigo encontrado
                        Vector2 targetPos = targetEntity.Get<PositionComponent>().position;
                        cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.MOVING;
                        e.Set(new MoveTargetComponent(targetPos));
                        e.Set(new MoveResolutorComponent(false, 0, pos.position));
                        existTarget = true;
                        break;
                    }
                }
            }
        }
    }
}
