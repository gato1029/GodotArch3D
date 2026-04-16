using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GodotFlecs.sources.Flecs.Systems.Units;
public class UnitEnemySearchSystem: FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => true;
    const int GROUP_COUNT = 2; // 🔥 ajusta según cantidad de unidades
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PositionComponent>()
          .With<SpatialIDComponent>()
          .With<TeamComponent>()
          .With<CharacterComponent>()
          .With<EnemySearchComponent>()
          .With<AttackPendingComponent>()
          .With<MoveResolutorComponent>()
          .Without<MoveTargetComponent>()
          .Without<PlayerInputComponent>()
          .Without<DeadTag>()
          .Without<DestroyRequestTag>();
    }
    protected override void OnIter(Iter it)
    {
        var world = it.World().GetCtx<GodotEcsArch.sources.BlackyTiles.BlackyWorld>();
        if (world == null) return;

        var sim = world.simulationTick;
        if (sim == null || sim.TickCount == 0) return;

        var dynGrid = world.DynamicHash;

        var posArray = it.Field<PositionComponent>(0);
        var spatialArray = it.Field<SpatialIDComponent>(1);
        var teamArray = it.Field<TeamComponent>(2);
        var charArray = it.Field<CharacterComponent>(3);
        var searchArray = it.Field<EnemySearchComponent>(4);
        var atpArray = it.Field<AttackPendingComponent>(5);
        var moveResolutorArray = it.Field<MoveResolutorComponent>(6);

        Span<int> neighbors = stackalloc int[8];

        // 🔥 tiempo acumulado correcto
        float totalTime = sim.TickCount * sim.FixedDelta;

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pos = ref posArray[i];
            ref var spatial = ref spatialArray[i];
            ref var team = ref teamArray[i];
            ref var search = ref searchArray[i];
            ref var moveResolutor = ref moveResolutorArray[i];

            var e = it.Entity(i);

            // 🔥 acumular tiempo correctamente
            search.Timer += totalTime;

            if (search.Timer >= search.Interval)
            {
                // 🔥 cuántas veces "debió" ejecutarse
                int times = (int)(search.Timer / search.Interval);

                // 🔥 conservar excedente (CLAVE)
                search.Timer -= times * search.Interval;
                // 🔥 👇 AQUI VA EL STAGGERING 👇
                int group = spatial.Value & (GROUP_COUNT - 1);
                int frame = sim.FrameIndex & (GROUP_COUNT - 1);

                if (group != frame)
                    continue;

                // 🔥 SOLO UNA QUERY (optimización crítica)
                int count = dynGrid.QueryNodesBounded(
                    pos.position.X,
                    pos.position.Y,
                    search.Radius,
                    neighbors
                );

                for (int ii = 0; ii < count; ii++)
                {
                    var targetEntity = dynGrid.GetEntity(neighbors[ii]);

                    if (spatial.Value == neighbors[ii]) continue;

                    var otherTeam = targetEntity.Get<TeamComponent>();
                    if (team.TeamId == otherTeam.TeamId) continue;

                    if (targetEntity.IsAlive() && !targetEntity.Has<DeadTag>())
                    {
                        moveResolutor.Blocked = false;
                        moveResolutor.BlockedTimer = 0f;

                        Vector2 targetPos = targetEntity.Get<PositionComponent>().position;

                        e.Set(new MoveTargetComponent(targetPos));
                        e.Remove<SleepTag>();
                        break;
                    }
                }
            }
        }
    }
    //protected override void OnIter(Iter it)
    //{
    //    var world = it.World().GetCtx<GodotEcsArch.sources.BlackyTiles.BlackyWorld>();
    //    if (world == null) return;

    //    var dynGrid = world.DynamicHash;

    //    var posArray = it.Field<PositionComponent>(0);
    //    var spatialArray = it.Field<SpatialIDComponent>(1);
    //    var teamArray = it.Field<TeamComponent>(2);
    //    var charArray = it.Field<CharacterComponent>(3);
    //    var searchArray = it.Field<EnemySearchComponent>(4);
    //    var atpArray = it.Field<AttackPendingComponent>(5);
    //    var moveResolutorArray = it.Field<MoveResolutorComponent>(6);



    //    Span<int> neighbors = stackalloc int[8]; // solo quiero 8 vecinos
    //    float delta = (float)it.DeltaTime();
    //    for (int i = 0; i < it.Count(); i++)
    //    {
    //        ref var pos = ref posArray[i];
    //        ref var spatial = ref spatialArray[i];
    //        ref var team = ref teamArray[i];
    //        ref var cha = ref charArray[i];
    //        ref var search = ref searchArray[i];
    //        ref var atp = ref atpArray[i];
    //        ref var moveResolutor = ref moveResolutorArray[i];
    //        var e = it.Entity(i);

    //        //if (atp.Active)
    //        //{
    //        //    continue;
    //        //}
    //        // Actualizar temporizador
    //        search.Timer = search.Timer+delta;
    //        if (search.Timer >= search.Interval)
    //        {
    //            search.Timer -= search.Interval;
    //            bool existTarget = false;
    //            int count = dynGrid.QueryNodesBounded(pos.position.X, pos.position.Y, search.Radius, neighbors);

    //            for (int ii = 0; ii < count; ii++)
    //            {
    //                var targetEntity = dynGrid.GetEntity(neighbors[ii]);
    //                var otherSpatial = targetEntity.Get<SpatialIDComponent>();
    //                var otherTeam = targetEntity.Get<TeamComponent>();
    //                if (spatial.Value == neighbors[ii]) // Ignorar a sí mismo
    //                {
    //                    continue;
    //                }
    //                if (team.TeamId == otherTeam.TeamId) // si es del mismo equipo, ignorar
    //                {
    //                    continue;
    //                }
    //                if (targetEntity.IsAlive() && !targetEntity.Has<DeadTag>()) // si el enemigo está vivo y no tiene la etiqueta de muerto
    //                {
    //                    moveResolutor.Blocked = false; // desbloquear movimiento si estaba bloqueado
    //                    moveResolutor.BlockedTimer = 0f; // resetear temporizador de bloqueo
    //                                                     // Asignar destino al enemigo encontrado
    //                    Vector2 targetPos = targetEntity.Get<PositionComponent>().position;
    //                    e.Set(new MoveTargetComponent(targetPos));
    //                    e.Remove<SleepTag>(); // despertar al enemigo si estaba dormido
    //                    existTarget = true;
    //                    break;
    //                }
    //            }                                
    //        }





    //        //if (existTarget) continue;
    //        //nearby = CollisionManager.Instance.BuildingsCollidersFlecs.QueryAABBBInCirclePoints(pos.position, search.Radius, 0, 1);

    //        //foreach (var target in nearby)
    //        //{
    //        //    var targetEntity = target.Owner;
    //        //    if (targetEntity.Get<TeamComponent>().TeamId == team.TeamId)
    //        //    {
    //        //        continue; // mismo equipo, ignorar
    //        //    }
    //        //    else
    //        //    {
    //        //        if (target.Owner.IsAlive() && !target.Owner.Has<DestroyRequestTag>())
    //        //        {
    //        //            // Asignar destino al enemigo encontrado
    //        //            Vector2 targetPos = targetEntity.Get<PositionComponent>().position;
    //        //            cha.characterStateType = GodotEcsArch.sources.managers.Characters.CharacterStateType.MOVING;
    //        //            e.Set(new MoveTargetComponent(targetPos));
    //        //            e.Set(new MoveResolutorComponent(false, 0, pos.position,0,0));
    //        //            existTarget = true;
    //        //            break;
    //        //        }
    //        //    }
    //        //}
    //    }
    //}
}
