using Flecs.NET.Bindings;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotFlecs.sources.Flecs.Components;
using GodotFlecs.sources.Flecs.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.Flecs.Systems.Generic;


internal class PathManagerSystem : FlecsSystemBase
{
    protected override ulong Phase => flecs.EcsOnUpdate;
    protected override bool MultiThreaded => false; // por el momento por la liberacion del path
    protected override void BuildQuery(ref QueryBuilder qb)
    {
        qb.With<PathReferenceComponent>()
          .Without<MoveTargetComponent>() // Solo actúa si la unidad ya llegó al waypoint anterior y no tiene objetivo activo
          .Without<DeadTag>();
    }

    protected override void OnIter(Iter it)
    {
        var blackyWorld =  it.World().GetCtx<BlackyWorld>();

        var pathRegistry =  blackyWorld.State.PathRegistryManager;

        var pathRefArray =  it.Field<PathReferenceComponent>(0);

        for (int i = 0; i < it.Count(); i++)
        {
            ref var pathRef = ref pathRefArray[i];

            var entity = it.Entity(i);


            // =====================================================
            // ¿Todavía quedan waypoints?
            // =====================================================

            int pathLength = pathRegistry.GetPathLength( pathRef.PathId);

            if (pathRef.CurrentIndex < pathLength)
            {
                // -------------------------------------------------
                // Obtener waypoint
                // -------------------------------------------------

                Vector2 waypoint =
                    pathRegistry.GetWaypoint(
                        pathRef.PathId,
                        pathRef.CurrentIndex
                    );


                // -------------------------------------------------
                // Aplicar formación
                // -------------------------------------------------

                Vector2 nextTarget =
                    waypoint +
                    pathRef.FormationOffset;


                // -------------------------------------------------
                // Crear objetivo de movimiento
                // -------------------------------------------------

                entity.Set(
                    new MoveTargetComponent
                    {
                        Value = nextTarget
                    }
                );


                // -------------------------------------------------
                // La unidad deja de estar detenida
                // -------------------------------------------------

                if (entity.Has<StoppedTag>())
                {
                    entity.Remove<StoppedTag>();
                }


                // -------------------------------------------------
                // Avanzar índice
                // -------------------------------------------------

                pathRef.CurrentIndex++;
            }
            else
            {
                // =================================================
                // FIN DEL PATH
                // =================================================

                int pathId =
                    pathRef.PathId;


                // Liberamos la referencia de esta unidad
                pathRegistry.ReleasePath(
                    pathId
                );


                // Quitamos la referencia al path
                entity.Remove<PathReferenceComponent>();


                // La unidad queda detenida
                entity.Add<StoppedTag>();
            }
        }
    }
}
