using Flecs.NET.Core;
using GodotEcsArch.sources.Flecs.Systems.Generic;
using GodotEcsArch.sources.Flecs.Systems.Units;

namespace GodotEcsArch.sources.GameRuntime.Internal.RTSControler;

using Godot;
using BlackyEngine.Core;
using BlackyEngine.Generic;
using BlackyEngine.PathFinding;
using BlackyEngine.Spatial;
using managers.Mods;
using utils;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;


public class RTSSelectionManager
{
    private readonly BlackyWorld _world;
    private readonly BlackyPathfinder _pathfinder;
    private readonly Query _query;
    private StaticSpatialChunkGrid<Entity>.QueryContext _buildingQueryContext;
    private ushort _playerTeamId = 1; // TODO: Reemplazar por la fuente real del equipo del jugador
    private FormationType _currentFormation = FormationType.Circle;

    private const int SlotsPerRing = 8;
    private const int MaxAttackSlots = 32;
    private const float DefaultUnitRadius = 0.6f;
    private const float SkipFirstWaypointRadius = 1.0f;

    public RTSSelectionManager(BlackyWorld world)
    {
        _world = world;
        _pathfinder = world.State.PathFinder;
        _query = _world.Simulation.Flecs.WorldFlecs.QueryBuilder().With<SelectedTag>().Build();
    }

    // ==========================================
    // 1. SELECCIÓN (Punto y Rectángulo)
    // ==========================================

    public void SelectUnitAtPoint(Vector2 point)
    {
        ClearPreviousSelection();

        float tolerance = 1.0f;
        float minX = point.X - tolerance;
        float minY = point.Y - tolerance;
        float maxX = point.X + tolerance;
        float maxY = point.Y + tolerance;

        Span<int> results = stackalloc int[30];
        ushort teamIgnore = 999;

        int foundCount = _world.State.DynamicHash.QueryNodesInBoxFiltered(minX, minY, maxX, maxY, teamIgnore, results);

        Entity closestEntity = default;
        float minDistanceSq = tolerance * tolerance;

        for (int i = 0; i < foundCount; i++)
        {
            int nodeIdx = results[i];
            Entity entity = _world.State.DynamicHash.GetEntity(nodeIdx);

            if (entity.IsAlive() && entity.Has<PositionComponent>())
            {
                ref var pos = ref entity.GetMut<PositionComponent>();
                float distSq = pos.position.DistanceSquaredTo(point);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closestEntity = entity;
                }
            }
        }

        if (closestEntity.IsAlive() && !closestEntity.Has<SelectedTag>())
        {
            BlackyManagerSelector.CreateSelector(closestEntity);
            closestEntity.Add<SelectedTag>();
            GD.Print($"Unidad seleccionada por Spatial Hash (clic): {closestEntity.Id}");
        }
    }

    public void SelectUnitsInRect(Rect2 rect)
    {
        ClearPreviousSelection();

        float minX = rect.Position.X;
        float minY = rect.Position.Y;
        float maxX = rect.Position.X + rect.Size.X;
        float maxY = rect.Position.Y + rect.Size.Y;

        Vector2 centerPosition = rect.GetCenter();

        FastCollider collider = new FastCollider
        {
            Shape = ShapeType.Rect,
            Height = rect.Size.Y,
            Width = rect.Size.X,
            Offset = Vector2.Zero
        };

        Span<int> results = stackalloc int[250];
        ushort teamIgnore = 999;

        int foundCount = _world.State.DynamicHash.QueryNodesInBoxFiltered(minX, minY, maxX, maxY, teamIgnore, results);
        int selectedCount = 0;

        for (int i = 0; i < foundCount; i++)
        {
            int nodeIdx = results[i];
            Entity entity = _world.State.DynamicHash.GetEntity(nodeIdx);

            if (entity.IsAlive() && !entity.Has<SelectedTag>() && entity.Has<UnitDefinitionComponent>())
            {
                var moveCollider = entity.Get<MoveColliderComponent>();
                var positionComp = entity.Get<PositionComponent>();

                if (CollisionMathHelper.CheckCircle(
                    positionComp.position.X,
                    positionComp.position.Y,
                    moveCollider.Radius,
                    moveCollider.Offset,
                    centerPosition.X,
                    centerPosition.Y,
                    ref collider))
                {
                    entity.Add<SelectedTag>();
                    BlackyManagerSelector.CreateSelector(entity);
                    selectedCount++;
                }
            }
        }

        GD.Print($"Seleccionadas {selectedCount} unidades con el Spatial Hash.");
    }

    private void ClearPreviousSelection()
    {
        if (!Input.IsKeyPressed(Key.Shift))
        {
            List<Entity> entitiesToDeselect = new();

            _query.Each((Entity ent) =>
            {
                if (ent.Has<SelectedTag>()) entitiesToDeselect.Add(ent);
            });

            foreach (var ent in entitiesToDeselect)
            {
                if (ent.IsAlive() && !ent.Has<DeadTag>())
                {
                    ent.Remove<SelectedTag>();

                    ref var selector = ref ent.GetMut<RenderSelectionGPUComponent>();
                    if (selector.rid != default && selector.instance != -1)
                    {
                        AtlasTexturesModsManager.Instance.FreeInstance(selector.rid, selector.instance);
                        selector.rid = default;
                        selector.instance = -1;
                    }
                }
            }
        }
    }

    // ==========================================
    // 2. COMANDOS (Mover, Atacar, FlowField)
    // ==========================================

    public void HandleRightClick(Vector2 targetWorldPos)
    {
        Entity clickedEnemy = FindEnemyEntityAtPoint(targetWorldPos);

        if (clickedEnemy.IsAlive())
        {
            CommandSelectedUnitsToAttack(clickedEnemy);
        }
        else if (Input.IsKeyPressed(Key.Shift))
        {
            CommandSelectedUnitsToMoveFlowField(targetWorldPos);
        }
        else
        {
            CommandSelectedUnitsToMove(targetWorldPos);
        }
    }

  private void CommandSelectedUnitsToMove(Vector2 targetPosition)
    {
        List<Entity> selectedEntities = new();
        _query.Each((Entity entity) => { if (entity.IsAlive()) selectedEntities.Add(entity); });
        if (selectedEntities.Count == 0) return;

        Vector2 groupCenter = Vector2.Zero;
        foreach (var entity in selectedEntities) groupCenter += entity.Get<PositionComponent>().position;
        groupCenter /= selectedEntities.Count;

        Vector2 toTarget = targetPosition - groupCenter;
        float totalMoveDistance = toTarget.Length();
        if (totalMoveDistance < 0.001f) return;

        Vector2 movementDirection = toTarget / totalMoveDistance;
        float spacing = 0.8f;

        var slots = FormationHelper.GenerateSlots(_currentFormation, selectedEntities.Count, spacing);
        var assignments = FormationHelper.AssignNearestSlots(selectedEntities, slots, groupCenter, movementDirection);
        if (assignments.Count == 0) return;

        Vector2 pathStart = groupCenter;
        Vector2I originTile = TilesHelper.WorldPositionToTile(pathStart);

        if (!_pathfinder.IsWalkableForRadius(originTile, DefaultUnitRadius))
        {
            Vector2I? nearestWalkable = _pathfinder.FindNearestWalkableTile(originTile, DefaultUnitRadius, searchRadiusTiles: 1);
            originTile = nearestWalkable ?? originTile;
        }

        Vector2I destiny = TilesHelper.WorldPositionToTile(targetPosition);
        var points = _pathfinder.FindPathWorld(originTile, destiny);
        if (points == null || points.Count == 0) return;

        int pathId = _world.State.PathRegistryManager.RegisterPath(points.ToArray());
        int pathLength = points.Count;
        int unitsCommanded = 0;

        foreach (var assignment in assignments)
        {
            Entity entity = assignment.Key;
            Vector2 formationOffset = assignment.Value;

            if (!entity.IsAlive()) continue;
            
            // Si estaba atacando o persiguiendo, limpiamos esos estados de combate
            if (entity.Has<ChaseTargetTag>()) entity.Remove<ChaseTargetTag>();
            if (entity.Has<AttackPendingTag>()) entity.Remove<AttackPendingTag>();
            if (entity.Has<AttackingPathFollowerTag>()) entity.Remove<AttackingPathFollowerTag>();
            if (entity.Has<AttackPendingComponent>())
            {
                ref var atp = ref entity.GetMut<AttackPendingComponent>();
                atp.Active = false;
                atp.Target = default;
            }
            AttackSlotHelper.RequestReleaseAttackSlot(entity);

            if (entity.Has<MovementMassiveTag>()) entity.Remove<MovementMassiveTag>();

            if (entity.Has<FlowFieldFollowerComponent>())
            {
                var old = entity.Get<FlowFieldFollowerComponent>();
                if (old.Counted) _world.State.FlowFieldManager.RemoveSettled(old.FlowFieldId);
                entity.Remove<FlowFieldFollowerComponent>();
            }

            if (entity.Has<PathReferenceComponent>())
            {
                var oldPath = entity.Get<PathReferenceComponent>();
                if (oldPath.PathId != pathId) _world.State.PathRegistryManager.ReleasePath(oldPath.PathId);
            }

            Vector2 entityPos = entity.Get<PositionComponent>().position;
            int startIndex = FindStartWaypointIndex(pathId, pathLength, entityPos - formationOffset);

            Vector2 waypoint = _world.State.PathRegistryManager.GetWaypoint(pathId, startIndex);
            Vector2 firstTarget = waypoint + formationOffset;

            if (CollisionMathHelper.RutaLibre(entity, ref firstTarget, _world))
            {
                _world.State.PathRegistryManager.AddReference(pathId);
                entity.Set(new PathReferenceComponent(pathId, startIndex + 1, formationOffset));

                if (entity.Has<MoveTargetComponent>())
                    entity.GetMut<MoveTargetComponent>().Value = firstTarget;
                else
                    entity.Set(new MoveTargetComponent { Value = firstTarget });

                if (entity.Has<StoppedTag>()) entity.Remove<StoppedTag>();

                ref var resolutor = ref entity.GetMut<MoveResolutorComponent>();
                resolutor.Blocked = false;
                resolutor.BlockedTimer = 0f;
                unitsCommanded++;
            }
            else
            {
                if (entity.Has<MoveResolutorComponent>()) entity.GetMut<MoveResolutorComponent>().Blocked = true;
                if (!entity.Has<StoppedTag>()) entity.Add<StoppedTag>();
            }
        }

        if (unitsCommanded == 0) _world.State.PathRegistryManager.ReleasePath(pathId);
        else GD.Print($"Moviendo {unitsCommanded} unidades a {targetPosition}");
    }

  private void CommandSelectedUnitsToAttack(Entity targetEnemy)
    {
        if (!targetEnemy.IsAlive() || targetEnemy.Has<DeadTag>()) return;

        List<Entity> selectedEntities = new();
        _query.Each((Entity entity) => { if (entity.IsAlive()) selectedEntities.Add(entity); });
        if (selectedEntities.Count == 0) return;

        bool isTargetUnit = targetEnemy.Has<UnitDefinitionComponent>();
        Vector2 targetPos = targetEnemy.Get<PositionComponent>().position;
        ushort idTarget = isTargetUnit ? targetEnemy.Get<UnitDefinitionComponent>().idTemplate : targetEnemy.Get<BuildingDefinitionComponent>().idTemplate;

        int unitsCommanded = 0;

        foreach (var entity in selectedEntities)
        {
            if (!entity.IsAlive() || entity == targetEnemy)
            {
                continue;
            }

            // Si la unidad estaba siguiendo una ruta o path anterior, lo liberamos
            if (entity.Has<PathReferenceComponent>())
            {
                var oldPath = entity.Get<PathReferenceComponent>();
                _world.State.PathRegistryManager.ReleasePath(oldPath.PathId);
                entity.Remove<PathReferenceComponent>();
            }

            // Limpiamos tags de navegación previa por si acaso
            if (entity.Has<AttackingPathFollowerTag>()) entity.Remove<AttackingPathFollowerTag>();

            ref var pos = ref entity.GetMut<PositionComponent>();
            Vector2 dirNormalized = entity.Has<DirectionComponent>() ? entity.Get<DirectionComponent>().normalized : Vector2.Right;

            // Determinamos si es Melee o Rango para verificar si ya está en rango de ataque
            bool inRange = false;
            float impactThreshold = isTargetUnit && targetEnemy.Has<MoveColliderComponent>() 
                ? targetEnemy.Get<MoveColliderComponent>().Radius 
                : 0.5f;

            if (entity.Has<MeleeAttackComponent>())
            {
                ref var melee = ref entity.GetMut<MeleeAttackComponent>();
                Vector2 originAttackCenter = pos.position + melee.OffSetRange;

                inRange = isTargetUnit
                    ? CombatCollisionHelper.CheckCollisionWithTargetUnit(originAttackCenter, dirNormalized, melee.RangeAttack, targetPos, idTarget)
                    : CombatCollisionHelper.CheckCollisionWithTargetBuild(originAttackCenter, dirNormalized, melee.RangeAttack, targetPos, idTarget);
            }
            else if (entity.Has<RangedAttackComponent>())
            {
                ref var ranged = ref entity.GetMut<RangedAttackComponent>();
                float distSqr = pos.position.DistanceSquaredTo(targetPos);
                float rangeSqr = ranged.Range * ranged.Range;
                inRange = (distSqr <= rangeSqr);
            }

            // ==========================================
            // EVALUACIÓN: ¿Está en rango, persecución directa o necesita A*?
            // ==========================================
            if (inRange)
            {
                // 1. YA ESTÁ EN RANGO: Ataca de inmediato
                AttackSlotHelper.ReleaseAttackSlot(entity);
                entity.Set(new AttackPendingComponent(true, targetEnemy, isTargetUnit, targetPos, impactThreshold));
                entity.Add<AttackPendingTag>();
                
                if (entity.Has<ChaseTargetTag>()) entity.Remove<ChaseTargetTag>();
                if (!entity.Has<StoppedTag>()) entity.Add<StoppedTag>();
                if (entity.Has<MoveTargetComponent>()) entity.Remove<MoveTargetComponent>();

                unitsCommanded++;
            }
            else
            {
                // 2. ESTÁ LEJOS: Verificamos si tiene permitido perseguir
                bool canChase;
                if (entity.Has<EnemySearchComponent>())
                {
                    canChase = entity.Get<EnemySearchComponent>().EnableChase;
                }
                else
                {
                    bool isMobileUnit = entity.Has<UnitDefinitionComponent>() || entity.Has<MoveColliderComponent>();
                    bool isBuilding = entity.Has<BuildingDefinitionComponent>();
                    canChase = isMobileUnit && !isBuilding;
                }

                if (!canChase)
                {
                    // Unidades estáticas / Torres: No persiguen, asignan blanco directo de contraataque
                    AssignDirectAttackTarget(entity, targetEnemy, isTargetUnit, targetPos, impactThreshold);
                    unitsCommanded++;
                    continue;
                }

                // 3. UNIDAD MÓVIL LEJANA: Comprobamos si hay línea de visión directa (Ruta Libre)
                Vector2 tempTargetPos = targetPos;
                bool hasLineOfSight = CollisionMathHelper.RutaLibre(entity, ref tempTargetPos, _world);

                if (hasLineOfSight)
                {
                    // CASO A: Lejos pero con línea recta despejada -> Chase directo
                    entity.Set(new AttackPendingComponent(
                        Active: true,
                        Target: targetEnemy,
                        isUnit: isTargetUnit,
                        targetPosition: targetPos,
                        impactThereshold: impactThreshold
                    ));

                    entity.Add<ChaseTargetTag>();
                    if (entity.Has<StoppedTag>()) entity.Remove<StoppedTag>();
                    unitsCommanded++;
                }
                else
                {
                    // CASO B: Hay obstáculos en medio -> ¡Usamos A* global para rodearlos!
                    Vector2 entityPos = pos.position;
                    Vector2I originTile = TilesHelper.WorldPositionToTile(entityPos);
                    Vector2I destinyTile = TilesHelper.WorldPositionToTile(targetPos);

                    var points = _pathfinder.FindPathWorld(originTile, destinyTile);
                    if (points != null && points.Count > 0)
                    {
                        int pathId = _world.State.PathRegistryManager.RegisterPath(points.ToArray());
                        int pathLength = points.Count;

                        _world.State.PathRegistryManager.AddReference(pathId);
                        
                        entity.Set(new PathReferenceComponent(pathId, 1, Vector2.Zero));
                        
                        Vector2 firstWaypoint = _world.State.PathRegistryManager.GetWaypoint(pathId, 0);
                        entity.Set(new MoveTargetComponent { Value = firstWaypoint });

                        entity.Set(new AttackPendingComponent(
                            Active: true,
                            Target: targetEnemy,
                            isUnit: isTargetUnit,
                            targetPosition: targetPos,
                            impactThereshold: impactThreshold
                        ));
                        
                        entity.Add<AttackingPathFollowerTag>(); // Seguido por el AttackingPathTransitionSystem

                        if (entity.Has<StoppedTag>()) entity.Remove<StoppedTag>();
                        unitsCommanded++;
                    }
                    else
                    {
                        // Fallback: Si el A* no encuentra ruta, intentamos persecución directa por vectores
                        entity.Set(new AttackPendingComponent(
                            Active: true,
                            Target: targetEnemy,
                            isUnit: isTargetUnit,
                            targetPosition: targetPos,
                            impactThereshold: impactThreshold
                        ));

                        entity.Add<ChaseTargetTag>();
                        if (entity.Has<StoppedTag>()) entity.Remove<StoppedTag>();
                        unitsCommanded++;
                    }
                }
            }
        }

        GD.Print($"{unitsCommanded} unidades ordenadas para atacar a la entidad {targetEnemy.Id}");
    }
    /// <summary>
    /// Asigna un objetivo de ataque directo a una unidad o estructura estática,
    /// obligándola a disparar/atacar sin activar la persecución (Hold Ground).
    /// </summary>
    public  void AssignDirectAttackTarget( Entity entity, Entity targetEntity, bool isUnit, Vector2 targetPos, float impactThreshold)
    {
        if (!entity.IsAlive() || !targetEntity.IsAlive()) return;

        // 1. Aseguramos que la unidad esté detenida (no se mueva)
        entity.Add<StoppedTag>();

        // 2. Si tenía un componente de movimiento previo, lo removemos para que no intente caminar
        if (entity.Has<MoveTargetComponent>())
        {
            entity.Remove<MoveTargetComponent>();
        }

        // 3. Limpiamos cualquier tag de persecución por si venía de otro estado
        if (entity.Has<ChaseTargetTag>())
        {
            entity.Remove<ChaseTargetTag>();
        }

        // 4. Inyectamos el AttackPendingComponent con los datos del blanco
        entity.Set(new AttackPendingComponent(
            Active: true,
            Target: targetEntity,
            isUnit: isUnit,
            targetPosition: targetPos,
            impactThereshold: impactThreshold
        ));

        // 5. Activamos el tag de ataque para que los sistemas de Melee o Rango lo procesen de inmediato
        if (!entity.Has<AttackPendingTag>())
        {
            entity.Add<AttackPendingTag>();
        }
    }
    private void CommandSelectedUnitsToMoveFlowField(Vector2 targetPosition)
    {
        List<Entity> selectedEntities = new();
        _query.Each((Entity entity) => { if (entity.IsAlive()) selectedEntities.Add(entity); });
        if (selectedEntities.Count == 0) return;

        Vector2 groupCenter = Vector2.Zero;
        foreach (var entity in selectedEntities) groupCenter += entity.Get<PositionComponent>().position;
        groupCenter /= selectedEntities.Count;

        Vector2I originTile = TilesHelper.WorldPositionToTile(groupCenter);
        Vector2I destinyTile = TilesHelper.WorldPositionToTile(targetPosition);
        const int RegionMargin = 20;

        Vector2I regionMin = new(Mathf.Min(originTile.X, destinyTile.X) - RegionMargin, Mathf.Min(originTile.Y, destinyTile.Y) - RegionMargin);
        Vector2I regionMax = new(Mathf.Max(originTile.X, destinyTile.X) + RegionMargin, Mathf.Max(originTile.Y, destinyTile.Y) + RegionMargin);

        var flowFieldManager = _world.State.FlowFieldManager;
        int requiredSpreadRadius = Mathf.Clamp(Mathf.CeilToInt(MathF.Sqrt(selectedEntities.Count / (float)CrowdSettings.Capacity)), 1, 6);

        int fieldId = flowFieldManager.FindNearestFieldId(originTile, destinyTile, requiredSpreadRadius);
        if (fieldId == -1)
        {
            fieldId = flowFieldManager.CreateField(originTile, destinyTile, regionMin, regionMax, DefaultUnitRadius, requiredSpreadRadius);
        }

        if (fieldId == -1) return;

        int unitsCommanded = 0;
        foreach (var entity in selectedEntities)
        {
            if (!entity.IsAlive()) continue;

            if (entity.Has<PathReferenceComponent>())
            {
                _world.State.PathRegistryManager.ReleasePath(entity.Get<PathReferenceComponent>().PathId);
                entity.Remove<PathReferenceComponent>();
            }
            if (entity.Has<MoveTargetComponent>()) entity.Remove<MoveTargetComponent>();
            if (!entity.Has<MovementMassiveTag>()) entity.Add<MovementMassiveTag>();

            if (entity.Has<FlowFieldFollowerComponent>())
            {
                var old = entity.Get<FlowFieldFollowerComponent>();
                if (old.Counted) flowFieldManager.RemoveSettled(old.FlowFieldId);
            }

            entity.Set(new FlowFieldFollowerComponent(fieldId, Vector2.Zero, 0, targetPosition, false));
            if (entity.Has<StoppedTag>()) entity.Remove<StoppedTag>();
            unitsCommanded++;
        }

        GD.Print($"[FlowField] Moviendo {unitsCommanded} unidades a {targetPosition}");
    }

    private Entity FindEnemyEntityAtPoint(Vector2 point)
    {
        float tolerance = 1f;
        Span<int> results = stackalloc int[30];
        int foundCount = _world.State.DynamicHash.QueryNodesInBoxFiltered(point.X - tolerance, point.Y - tolerance, point.X + tolerance, point.Y + tolerance, _playerTeamId, results);

        Entity closestEntity = default;
        float minDistanceSq = tolerance * tolerance;

        for (int i = 0; i < foundCount; i++)
        {
            Entity entity = _world.State.DynamicHash.GetEntity(results[i]);
            if (entity.IsAlive() && !entity.Has<DeadTag>() && entity.Has<PositionComponent>())
            {
                float distSq = entity.Get<PositionComponent>().position.DistanceSquaredTo(point);
                if (distSq < minDistanceSq) { minDistanceSq = distSq; closestEntity = entity; }
            }
        }

        if (closestEntity.IsAlive()) return closestEntity;

        var staticGrid = _world.State.StaticSpatialBuildings;
        var buildingContext = _buildingQueryContext ??= new StaticSpatialChunkGrid<Entity>.QueryContext();
        int radius = (int)MathF.Ceiling((tolerance * 2f) / staticGrid._cellSize);

        foreach (var id in staticGrid.QueryNearbyUnique(point.X, point.Y, radius, buildingContext, _playerTeamId))
        {
            if (staticGrid.TryGetValue(id, out Entity buildingEntity))
            {
                if (!buildingEntity.IsAlive() || buildingEntity.Has<DeadTag>()) continue;
                float distSq = buildingEntity.Get<PositionComponent>().position.DistanceSquaredTo(point);
                if (distSq < minDistanceSq) { minDistanceSq = distSq; closestEntity = buildingEntity; }
            }
        }

        return closestEntity;
    }

    private int FindStartWaypointIndex(int pathId, int pathLength, Vector2 localPos)
    {
        if (pathLength <= 1) return 0;
        var registry = _world.State.PathRegistryManager;
        float bestDistSq = float.MaxValue;
        int bestTarget = 0;

        for (int i = 0; i < pathLength - 1; i++)
        {
            Vector2 a = registry.GetWaypoint(pathId, i);
            Vector2 b = registry.GetWaypoint(pathId, i + 1);
            Vector2 ab = b - a;
            float lenSq = ab.LengthSquared();
            float t = lenSq > 0.0001f ? Mathf.Clamp((localPos - a).Dot(ab) / lenSq, 0f, 1f) : 0f;
            Vector2 closest = a + ab * t;
            float distSq = localPos.DistanceSquaredTo(closest);

            if (distSq < bestDistSq - 0.0001f) { bestDistSq = distSq; bestTarget = (t <= 0f) ? i : i + 1; }
        }

        if (bestTarget == 0 && localPos.DistanceSquaredTo(registry.GetWaypoint(pathId, 0)) < SkipFirstWaypointRadius * SkipFirstWaypointRadius)
            return 1;

        return bestTarget;
    }
}