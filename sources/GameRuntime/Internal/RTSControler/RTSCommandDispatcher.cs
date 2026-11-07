using System;
using System.Collections.Generic;
using System.Linq;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.PathFinding;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs.Components;

namespace GodotEcsArch.sources.GameRuntime.Internal.RTSControler;

public class RTSCommandDispatcher
{
    private readonly BlackyWorld _world;
    private readonly BlackyPathfinder _pathfinder;

    public RTSCommandDispatcher(BlackyWorld world)
    {
        _world = world;
        _pathfinder = world.State.PathFinder;
    }

    public void IssueTargetCommand(Vector2 targetPosition)
    {
        // 1. Obtenemos la selección actual directamente del SelectionContext (Ultra rápido)
        var selectedEntities = SelectionContext.CurrentSelectedEntities;
        if (selectedEntities == null || selectedEntities.Count == 0) return;

        // 2. Evaluamos si hay un enemigo bajo el cursor
        Entity enemyTarget = FindEnemyAtPoint(targetPosition);

        if (enemyTarget.IsAlive())
        {
            CommandUnitsToAttack(selectedEntities, enemyTarget);
        }
        else if (Input.IsKeyPressed(Key.Shift))
        {
            CommandUnitsToMoveFlowField(selectedEntities, targetPosition);
        }
        else
        {
            CommandUnitsToMove(selectedEntities, targetPosition);
        }
    }
    private FormationType _currentFormation = FormationType.Circle;
    private const float DefaultUnitRadius = 0.6f; // o el valor que ya usas como default
    private void CommandUnitsToMove(IReadOnlyList<Entity> selectedEntities, Vector2 targetPosition)
    {
         // ---------------------------------------------------------
        // 1. Centro del grupo
        // ---------------------------------------------------------
        Vector2 groupCenter = Vector2.Zero;

        foreach (var entity in selectedEntities)
        {
            groupCenter += entity.Get<PositionComponent>().position;
        }
        groupCenter /= selectedEntities.Count;

        // ---------------------------------------------------------
        // 2. Dirección del movimiento
        // ---------------------------------------------------------
        Vector2 toTarget = targetPosition - groupCenter;
        float totalMoveDistance = toTarget.Length();

        if (totalMoveDistance < 0.001f)
            return;

        Vector2 movementDirection = toTarget / totalMoveDistance;

        // ---------------------------------------------------------
        // 3. Crear formación
        // ---------------------------------------------------------
        float spacing = .8f; // antes 1.5f

        var slots = FormationHelper.GenerateSlots(_currentFormation, selectedEntities.Count, spacing);

        // ---------------------------------------------------------
        // 4. Asignar cada unidad a un slot
        // ---------------------------------------------------------
        var assignments = FormationHelper.AssignNearestSlots(
            selectedEntities.ToList(),
            slots,
            groupCenter,
            movementDirection);

        if (assignments.Count == 0)
            return;

        // ---------------------------------------------------------
        // 5. Punto de salida de la formación


        Vector2 pathStart = groupCenter;

        Vector2I originTile = TilesHelper.WorldPositionToTile(pathStart);

        if (!_pathfinder.IsWalkableForRadius(originTile, DefaultUnitRadius))
        {
            Vector2I? nearestWalkable = _pathfinder.FindNearestWalkableTile(originTile, DefaultUnitRadius, searchRadiusTiles: 1);

            if (nearestWalkable.HasValue)
            {
                originTile = nearestWalkable.Value;
            }
            else
            {
                pathStart = groupCenter;
                originTile = TilesHelper.WorldPositionToTile(pathStart);
            }
        }

        // ---------------------------------------------------------
        // 6. Calcular A* (UNA sola vez para todo el grupo)
        // ---------------------------------------------------------
        Vector2I destiny = TilesHelper.WorldPositionToTile(targetPosition);

        var points = _pathfinder.FindPathWorld(originTile, destiny);

        if (points == null || points.Count == 0)
            return;

        // ---------------------------------------------------------
        // 7. Registrar camino compartido
        // ---------------------------------------------------------
        int pathId = _world.State.PathRegistryManager.RegisterPath(points.ToArray());
        int pathLength = points.Count;

        int unitsCommanded = 0;

        // ---------------------------------------------------------
        // 8. Asignar path + formación a cada unidad
        // ---------------------------------------------------------
        foreach (var assignment in assignments)
        {
            Entity entity = assignment.Key;
            Vector2 formationOffset = assignment.Value;

            if (!entity.IsAlive())
                continue;
            if (entity.Has<MovementMassiveTag>())
                entity.Remove<MovementMassiveTag>();

            if (entity.Has<FlowFieldFollowerComponent>())
            {
                var old = entity.Get<FlowFieldFollowerComponent>();
                if (old.Counted)
                {
                    _world.State.FlowFieldManager.RemoveSettled(old.FlowFieldId);
                }                
                entity.Remove<FlowFieldFollowerComponent>();
            }
                
            // ---------------------------------------------
            // Liberar path anterior
            // ---------------------------------------------
            if (entity.Has<PathReferenceComponent>())
            {
                var oldPath = entity.Get<PathReferenceComponent>();
                if (oldPath.PathId != pathId)
                {
                    _world.State.PathRegistryManager.ReleasePath(oldPath.PathId);
                }                
            }
            
            // ---------------------------------------------
            Vector2 entityPos = entity.Get<PositionComponent>().position;

  
       
            // Proyectamos la posición de la unidad "sin su offset" sobre el path
            // original: equivale a proyectar sobre el path desplazado por su offset.
            int startIndex = FindStartWaypointIndex(pathId, pathLength, entityPos - formationOffset);

            Vector2 waypoint = _world.State.PathRegistryManager.GetWaypoint(pathId, startIndex);
            Vector2 firstTarget = waypoint + formationOffset;

            // ---------------------------------------------
            // Validar ruta ANTES de comprometer una referencia
            // al path — evita fuga de referencias y unidades
            // huérfanas sin PathReferenceComponent.
            // ---------------------------------------------
            if (CollisionMathHelper.RutaLibre(entity, ref firstTarget, _world))
            {
                _world.State.PathRegistryManager.AddReference(pathId);

                entity.Set(
                    new PathReferenceComponent(
                        pathId,
                        startIndex + 1, // el waypoint startIndex ya lo consumimos como MoveTarget
                        formationOffset));

                // ---------------------------------------------
                // Asignar MoveTarget
                // ---------------------------------------------
                if (entity.Has<MoveTargetComponent>())
                {
                    ref var target = ref entity.GetMut<MoveTargetComponent>();
                    target.Value = firstTarget;
                }
                else
                {
                    entity.Set(new MoveTargetComponent
                    {
                        Value = firstTarget
                    });
                }

                // ---------------------------------------------
                // Desbloquear unidad
                // ---------------------------------------------
                if (entity.Has<StoppedTag>())
                    entity.Remove<StoppedTag>();

                ref var resolutor = ref entity.GetMut<MoveResolutorComponent>();

                resolutor.Blocked = false;
                resolutor.BlockedTimer = 0f;
                resolutor.LastConeIndex = 0;
                resolutor.AvoidanceTimer = 0f;
                resolutor.ConsecutiveBlocks = 0;
                resolutor.RetryTimer = 0f;

                unitsCommanded++;
            }
            else
            {
                // ---------------------------------------------
                // La ruta hacia el waypoint no está libre: la
                // unidad se queda detenida donde está, sin
                // asignarle este path (no consumió referencia).
                //
                // Queda sin PathReferenceComponent, así que la
                // próxima orden de movimiento la tomará de cero
                // con normalidad. Mientras tanto, la marcamos
                // como detenida para que SteeringSystem/
                // MoveSeparationSystem no intenten moverla.
                // ---------------------------------------------
                if (entity.Has<MoveResolutorComponent>())
                {
                    ref var resolutor = ref entity.GetMut<MoveResolutorComponent>();
                    resolutor.Blocked = true;
                }

                if (!entity.Has<StoppedTag>())
                    entity.Add<StoppedTag>();
            }
        }

        // ---------------------------------------------------------
        // 9. Si nadie pudo usar el path, liberarlo
        // ---------------------------------------------------------
        if (unitsCommanded == 0)
        {
            _world.State.PathRegistryManager.ReleasePath(pathId);
            return;
        }
        GD.Print($"Moviendo {selectedEntities.Count} unidades hacia {targetPosition}");
    }
    private const float SkipFirstWaypointRadius = 1.0f;
    private int FindStartWaypointIndex(int pathId, int pathLength, Vector2 localPos)
    {
        if (pathLength <= 1)
            return 0;

        var registry = _world.State.PathRegistryManager;

        float bestDistSq = float.MaxValue;
        int bestTarget = 0;

        for (int i = 0; i < pathLength - 1; i++)
        {
            Vector2 a = registry.GetWaypoint(pathId, i);
            Vector2 b = registry.GetWaypoint(pathId, i + 1);
            Vector2 ab = b - a;
            float lenSq = ab.LengthSquared();

            float t = lenSq > 0.0001f
                ? Mathf.Clamp((localPos - a).Dot(ab) / lenSq, 0f, 1f)
                : 0f;

            Vector2 closest = a + ab * t;
            float distSq = localPos.DistanceSquaredTo(closest);

            if (distSq < bestDistSq - 0.0001f)
            {
                bestDistSq = distSq;
                bestTarget = (t <= 0f) ? i : i + 1;
            }
        }

        // Si la unidad está antes del inicio del path pero ya cerca de wp0,
        // ir a wp0 solo para girar de inmediato es un desvío inútil.
        if (bestTarget == 0)
        {
            Vector2 wp0 = registry.GetWaypoint(pathId, 0);

            if (localPos.DistanceSquaredTo(wp0) < SkipFirstWaypointRadius * SkipFirstWaypointRadius)
                return 1;
        }

        return bestTarget;
    }


    private void CommandUnitsToAttack(IReadOnlyList<Entity> units, Entity target)
    {
        GD.Print($"Atacando a entidad {target.Id} con {units.Count} unidades");
    }

    private void CommandUnitsToMoveFlowField(IReadOnlyList<Entity> units, Vector2 targetPosition)
    {
        GD.Print($"Desplazando horda de {units.Count} unidades hacia {targetPosition}");
    }

    private Entity FindEnemyAtPoint(Vector2 point)
    {
        // Lógica de búsqueda en Spatial Hash de unidades/edificios enemigos
        return default;
    }
}