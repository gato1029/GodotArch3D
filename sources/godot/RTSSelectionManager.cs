
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generic;
using GodotEcsArch.sources.BlackyEngine.PathFinding;
using GodotEcsArch.sources.Flecs.Systems.Units;
using GodotEcsArch.sources.managers;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.godot;

public partial class RTSSelectionManager : Node2D
{
    private static BlackyWorld _world;
    private static BlackyPathfinder _pathfinder;
    public static RTSSelectionManager Instance { get; private set; }

    private FormationType _currentFormation = FormationType.Circle;
    // TODO: reemplazar por la fuente real del team del jugador
    private ushort _playerTeamId = 2;

    // --- NUEVO: Flag para encender/apagar el sistema ---
    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            if (!_isEnabled)
            {
                // Si lo apagamos mientras estábamos arrastrando, cancelamos el estado visual
                if (_isDragging)
                {
                    _isDragging = false;
                    QueueRedraw();
                }
            }
        }
    }

    // 1. Variables para el control VISUAL (Pantalla / UI)
    private bool _isDragging = false;
    private Vector2 _dragStartScreen = Vector2.Zero;
    private Vector2 _dragCurrentScreen = Vector2.Zero;

    // 2. Variables para la LÓGICA DEL JUEGO (Mundo Real / Spatial Hash)
    private Vector2 _dragStartWorld = Vector2.Zero;
    private Vector2 _dragCurrentWorld = Vector2.Zero;

    // Distancia mínima en píxeles para considerar que es un arrastre (evita falsos drag con clics rápidos)
    private const float DragThreshold = 6.0f;

    Query query;

    public void SetWorld(BlackyWorld world)
    {
        _world = world;
        _pathfinder = world.State.PathFinder;
        query = _world.Simulation.Flecs.WorldFlecs.QueryBuilder().With<SelectedTag>().Build();
    }

    public override void _Ready()
    {
        Instance = this;
        GD.Print("Selector Cargado");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_world == null || !_isEnabled) return;

        // 1. Clic Derecho: Dar orden de movimiento
        if (IsRightMouseButtonPressed(@event))
        {
            HandleRightClick();
            return;
        }

        // 2. Clic Izquierdo Presionado: Iniciar selección
        if (IsLeftMouseButtonPressed(@event, out bool isPressed))
        {
            if (isPressed)
            {
                HandleLeftPress();
            }
            else
            {
                HandleLeftRelease();
            }
            return;
        }

        // 3. Movimiento del Ratón: Actualizar cuadro de selección si se está arrastrando
        if (@event is InputEventMouseMotion && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            HandleMouseMotion();
        }
    }

    // --- MÉTODOS AUXILIARES DE ENTRADA ---

    private bool IsRightMouseButtonPressed(InputEvent @event)
    {
        return @event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Right && mb.Pressed;
    }

    private bool IsLeftMouseButtonPressed(InputEvent @event, out bool pressed)
    {
        pressed = false;
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            pressed = mb.Pressed;
            return true;
        }
        return false;
    }

    private void HandleRightClick()
    {
        Vector2 targetWorldPos = PositionsManager.Instance.positionMouseCamera;

        Entity clickedEnemy = FindEnemyEntityAtPoint(targetWorldPos);

        if (clickedEnemy.IsAlive())
        {
            CommandSelectedUnitsToAttack(clickedEnemy);
        }
        else if (Input.IsKeyPressed(Key.Shift)) // tecla de prueba temporal
        {
            CommandSelectedUnitsToMoveFlowField(targetWorldPos);
        }
        else
        {
            CommandSelectedUnitsToMove(targetWorldPos);
        }
    }
    private const int SlotsPerRing = 8;
    private const int MaxAttackSlots = 32;

    private void CommandSelectedUnitsToAttack(Entity targetEnemy)
    {
        if (!targetEnemy.IsAlive() || targetEnemy.Has<DeadTag>())
            return;

        List<Entity> selectedEntities = new();

        query.Each((Entity entity) =>
        {
            if (entity.IsAlive())
                selectedEntities.Add(entity);
        });

        if (selectedEntities.Count == 0)
            return;

        bool isTargetUnit = targetEnemy.Has<UnitDefinitionComponent>();
        Vector2 targetPos = targetEnemy.Get<PositionComponent>().position;

        ushort idTarget = isTargetUnit
            ? targetEnemy.Get<UnitDefinitionComponent>().idTemplate
            : targetEnemy.Get<BuildingDefinitionComponent>().idTemplate;

        int unitsCommanded = 0;

        // ---------------------------------------------------------
        // 1. Separar: unidades ya en rango (atacan directo, sin
        //    movimiento) vs las que necesitan desplazarse.
        // ---------------------------------------------------------
        List<Entity> unitsNeedingMove = new();

        foreach (var entity in selectedEntities)
        {
            if (!entity.IsAlive()) continue;
            if (!entity.Has<MeleeAttackComponent>()) continue;
            if (entity == targetEnemy) continue;

            // Liberar path de movimiento libre anterior, en cualquier caso
            if (entity.Has<PathReferenceComponent>())
            {
                var oldPath = entity.Get<PathReferenceComponent>();
                _world.State.PathRegistryManager.ReleasePath(oldPath.PathId);
                entity.Remove<PathReferenceComponent>();
            }

            ref var pos = ref entity.GetMut<PositionComponent>();
            ref var melle = ref entity.GetMut<MeleeAttackComponent>();

            Vector2 dirNormalized = entity.Has<DirectionComponent>()
                ? entity.Get<DirectionComponent>().normalized
                : Vector2.Right;

            Vector2 originAttackCenter = pos.position + melle.OffSetRange;

            bool inRange = isTargetUnit
                ? CombatCollisionHelper.CheckCollisionWithTargetUnit(originAttackCenter, dirNormalized, melle.RangeAttack, targetPos, idTarget)
                : CombatCollisionHelper.CheckCollisionWithTargetBuild(originAttackCenter, dirNormalized, melle.RangeAttack, targetPos, idTarget);

            if (inRange)
            {
                AttackSlotHelper.ReleaseAttackSlot(entity);

                entity.Set(new AttackPendingComponent(true, targetEnemy, isTargetUnit, targetPos, 0));
                entity.Add<AttackPendingTag>();

                unitsCommanded++;
            }
            else
            {
                unitsNeedingMove.Add(entity);
            }
        }

        if (unitsNeedingMove.Count == 0)
        {
            GD.Print($"{unitsCommanded} unidades atacando a la entidad {targetEnemy.Id}");
            return;
        }

        // ---------------------------------------------------------
        // 2. UN SOLO A* compartido para todo el grupo que necesita
        //    moverse (igual principio que CommandSelectedUnitsToMove).
        // ---------------------------------------------------------
        Vector2 groupCenter = Vector2.Zero;

        foreach (var entity in unitsNeedingMove)
            groupCenter += entity.Get<PositionComponent>().position;

        groupCenter /= unitsNeedingMove.Count;

        Vector2 movementDirection = targetPos - groupCenter;
        movementDirection = movementDirection.LengthSquared() < 0.001f
            ? Vector2.Right
            : movementDirection.Normalized();

        Vector2I origin = TilesHelper.WorldPositionToTile(groupCenter);
        Vector2I destiny = TilesHelper.WorldPositionToTile(targetPos);

        var points = _pathfinder.FindPathWorld(origin, destiny);

        if (points == null || points.Count == 0)
        {
            // No hay ruta hacia el objetivo: liberamos cualquier slot
            // que estas unidades ya tuvieran y las dejamos detenidas.
            foreach (var entity in unitsNeedingMove)
            {
                AttackSlotHelper.ReleaseAttackSlot(entity);

                if (entity.Has<MoveResolutorComponent>())
                {
                    ref var resolutor = ref entity.GetMut<MoveResolutorComponent>();
                    resolutor.Blocked = true;
                }

                if (!entity.Has<StoppedTag>())
                    entity.Add<StoppedTag>();
            }

            GD.Print($"{unitsCommanded} unidades atacando (sin ruta para el resto del grupo)");
            return;
        }

        int pathId = _world.State.PathRegistryManager.RegisterPath(points.ToArray());
        int pathLength = points.Count;

        int movedCount = 0;

        // ---------------------------------------------------------
        // 3. Asignar slot de anillo (offset) + path compartido a
        //    cada unidad que necesita moverse.
        // ---------------------------------------------------------
        foreach (var entity in unitsNeedingMove)
        {
            if (!entity.IsAlive()) continue;

            ref var moveResolutor = ref entity.GetMut<MoveResolutorComponent>();
            ref var melle = ref entity.GetMut<MeleeAttackComponent>();

            // -----------------------------------------------------
            // Reservar/reutilizar slot de aproximación alrededor
            // del objetivo (evita que todas converjan al mismo punto)
            // -----------------------------------------------------
            int slotIndex;

            if (entity.Has<AttackSlotComponent>())
            {
                var mySlot = entity.Get<AttackSlotComponent>();

                if (mySlot.Target == targetEnemy && mySlot.Target.IsAlive())
                {
                    slotIndex = mySlot.SlotIndex;
                }
                else
                {
                    AttackSlotHelper.ReleaseAttackSlot(entity);
                    slotIndex = AttackSlotHelper.AcquireAttackSlot(entity, targetEnemy, MaxAttackSlots);
                }
            }
            else
            {
                slotIndex = AttackSlotHelper.AcquireAttackSlot(entity, targetEnemy, MaxAttackSlots);
            }

            if (slotIndex == -1)
            {
                continue; // no se pudo reservar slot, saltar esta unidad
            }

            int ring = slotIndex / SlotsPerRing;
            int slotInRing = slotIndex % SlotsPerRing;

            float angle = slotInRing * (MathF.Tau / SlotsPerRing);
            float ringRadius = melle.RangeAttack * 0.85f + ring * (melle.RangeAttack * 0.5f);

            Vector2 slotDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 slotOffset = slotDir * ringRadius; // offset relativo al target, no al grupo

            // -----------------------------------------------------
            // Elegir el waypoint inicial que no quede "detrás" de
            // la unidad (mismo criterio que en CommandSelectedUnitsToMove)
            // -----------------------------------------------------
            Vector2 entityPos = entity.Get<PositionComponent>().position;

            int startIndex = 0;
            Vector2 waypoint = _world.State.PathRegistryManager.GetWaypoint(pathId, 0);
            Vector2 firstTarget = waypoint + slotOffset;

            while (startIndex < pathLength - 1)
            {
                Vector2 toWaypoint = firstTarget - entityPos;

                if (toWaypoint.Dot(movementDirection) >= 0f || toWaypoint.LengthSquared() < 0.01f)
                    break;

                startIndex++;
                waypoint = _world.State.PathRegistryManager.GetWaypoint(pathId, startIndex);
                firstTarget = waypoint + slotOffset;
            }

            _world.State.PathRegistryManager.AddReference(pathId);

            entity.Set(new PathReferenceComponent(pathId, startIndex + 1, slotOffset));

            if (entity.Has<MoveTargetComponent>())
            {
                ref var target = ref entity.GetMut<MoveTargetComponent>();
                target.Value = firstTarget;
            }
            else
            {
                entity.Set(new MoveTargetComponent { Value = firstTarget });
            }

            if (entity.Has<StoppedTag>())
                entity.Remove<StoppedTag>();

            moveResolutor.Blocked = false;
            moveResolutor.BlockedTimer = 0f;

            entity.Set(new AttackPendingComponent(false, targetEnemy, isTargetUnit, targetPos, 0));
           // entity.Add<AttackPendingTag>();

            unitsCommanded++;
            movedCount++;
        }

        // ---------------------------------------------------------
        // 4. Si nadie del grupo en movimiento pudo usar el path,
        //    liberarlo (evita fuga de referencias).
        // ---------------------------------------------------------
        if (movedCount == 0)
        {
            _world.State.PathRegistryManager.ReleasePath(pathId);
        }

        GD.Print($"{unitsCommanded} unidades atacando a la entidad {targetEnemy.Id}");
    }
    private void HandleLeftPress()
    {
        _isDragging = false;
        _dragStartScreen = GetGlobalMousePosition();
        _dragStartWorld = PositionsManager.Instance.positionMouseCamera;

        _dragCurrentScreen = _dragStartScreen;
        _dragCurrentWorld = _dragStartWorld;
    }

    private void HandleLeftRelease()
    {
        Vector2 mouseUpScreen = GetGlobalMousePosition();
        float distanceMoved = _dragStartScreen.DistanceTo(mouseUpScreen);

        if (_isDragging || distanceMoved > DragThreshold)
        {
            // Cuadro de selección (Box Selection)
            Rect2 worldRect = CreateRect(_dragStartWorld, _dragCurrentWorld);
            SelectUnitsInRect(worldRect);
        }
        else
        {
            // Clic simple
            SelectUnitAtPoint(_dragStartWorld);
        }

        _isDragging = false;
        QueueRedraw(); // Limpia el rectángulo visual
    }

    private void HandleMouseMotion()
    {
        if (_dragStartScreen.DistanceTo(GetGlobalMousePosition()) > DragThreshold)
        {
            _isDragging = true;
            _dragCurrentScreen = GetGlobalMousePosition();
            _dragCurrentWorld = PositionsManager.Instance.positionMouseCamera;

            QueueRedraw(); // Fuerza a redibujar el rectángulo en pantalla
        }
    }
    private const float DefaultUnitRadius = 0.6f; // o el valor que ya usas como default
    private void CommandSelectedUnitsToMove(Vector2 targetPosition)
    {
        List<Entity> selectedEntities = new();

        query.Each((Entity entity) =>
        {
            if (entity.IsAlive())
                selectedEntities.Add(entity);
        });

        if (selectedEntities.Count == 0)
            return;

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
        float spacing = .9f; // antes 1.5f

        var slots = FormationHelper.GenerateSlots(_currentFormation, selectedEntities.Count, spacing);

        // ---------------------------------------------------------
        // 4. Asignar cada unidad a un slot
        // ---------------------------------------------------------
        var assignments = FormationHelper.AssignNearestSlots(
            selectedEntities,
            slots,
            groupCenter,
            movementDirection);

        if (assignments.Count == 0)
            return;

        // ---------------------------------------------------------
        // 5. Punto de salida de la formación
        //
        // No adelantamos más de lo que realmente se va a mover el
        // grupo — si el destino está más cerca que el adelanto de
        // formación, el A* terminaría calculando una ruta desde un
        // punto más lejos del destino que la propia unidad, generando
        // un "rodeo" artificial en vez de un movimiento corto directo.
        // ---------------------------------------------------------
        float formationLeadDistance = MathF.Min(spacing * 3.0f, totalMoveDistance * 0.5f);

        Vector2 pathStart = groupCenter + movementDirection * formationLeadDistance;

        // ---------------------------------------------------------
        // 5b. Validar que el punto adelantado sea un origen válido
        // para el A*. Como 'pathStart' es un punto matemático (no la
        // posición real de ninguna unidad), puede caer sobre un
        // obstáculo o zona sin holgura — si eso pasa, el A* falla
        // desde el primer tile. Buscamos el tile transitable más
        // cercano; si no aparece ninguno cerca, usamos el groupCenter
        // original sin adelanto como último recurso.
        // ---------------------------------------------------------
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
                // Sin nada transitable cerca del punto adelantado: caemos
                // de vuelta al groupCenter real, sin adelanto.
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
            // Elegir el waypoint inicial de ESTA unidad
            //
            // Recorremos el path desde el índice 0 y nos
            // quedamos con el primer waypoint que no quede
            // "detrás" de la unidad respecto a la dirección
            // de movimiento del grupo. Evita el retroceso
            // cuando la unidad ya iba adelantada.
            // ---------------------------------------------
            Vector2 entityPos = entity.Get<PositionComponent>().position;

            int startIndex = 0;
            Vector2 waypoint = _world.State.PathRegistryManager.GetWaypoint(pathId, 0);
            Vector2 firstTarget = waypoint + formationOffset;

            while (startIndex < pathLength - 1)
            {
                Vector2 toWaypoint = firstTarget - entityPos;

                // Si el waypoint está delante (o es ambiguo por estar muy cerca), lo aceptamos
                if (toWaypoint.Dot(movementDirection) >= 0f || toWaypoint.LengthSquared() < 0.01f)
                    break;

                startIndex++;
                waypoint = _world.State.PathRegistryManager.GetWaypoint(pathId, startIndex);
                firstTarget = waypoint + formationOffset;
            }

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

        GD.Print($"Moviendo {unitsCommanded} unidades a {targetPosition}");
    }
    private int ComputeGoalSpreadRadius(int unitCount)
    {
        // Área necesaria ≈ unitCount * (spacing como unidad de tile).
        // radius = sqrt(área / π), con mínimo 1 y techo razonable.
        int radius = Mathf.CeilToInt(MathF.Sqrt(unitCount) * 0.6f);
        return Mathf.Clamp(radius, 1, 6);
    }
    private void CommandSelectedUnitsToMoveFlowField(Vector2 targetPosition)
    {
        List<Entity> selectedEntities = new();

        query.Each((Entity entity) =>
        {
            if (entity.IsAlive())
                selectedEntities.Add(entity);
        });

        if (selectedEntities.Count == 0)
            return;

        // ---------------------------------------------------------
        // 1. Centro del grupo (solo para calcular la región del campo,
        // no se usa offset de formación aquí)
        // ---------------------------------------------------------
        Vector2 groupCenter = Vector2.Zero;

        foreach (var entity in selectedEntities)
        {
            groupCenter += entity.Get<PositionComponent>().position;
        }

        groupCenter /= selectedEntities.Count;

        // ---------------------------------------------------------
        // 2. Tiles de origen/destino
        // ---------------------------------------------------------
        Vector2I originTile = TilesHelper.WorldPositionToTile(groupCenter);
        Vector2I destinyTile = TilesHelper.WorldPositionToTile(targetPosition);

        // ---------------------------------------------------------
        // 3. Región del campo: caja que envuelve origen y destino,
        // más un margen para permitir rodeos de obstáculos.
        // ---------------------------------------------------------
        const int RegionMargin = 10; // tiles extra alrededor de la caja justa

        Vector2I regionMin = new(
            Mathf.Min(originTile.X, destinyTile.X) - RegionMargin,
            Mathf.Min(originTile.Y, destinyTile.Y) - RegionMargin);

        Vector2I regionMax = new(
            Mathf.Max(originTile.X, destinyTile.X) + RegionMargin,
            Mathf.Max(originTile.Y, destinyTile.Y) + RegionMargin);

        // ---------------------------------------------------------
        // 4. Reutilizar un campo existente si ya hay uno cercano,
        // o crear uno nuevo.
        // ---------------------------------------------------------
        var flowFieldManager = _world.State.FlowFieldManager;

        int requiredSpreadRadius = 1; //ComputeGoalSpreadRadius(selectedEntities.Count);

        int fieldId = flowFieldManager.FindNearestFieldId(originTile, destinyTile, requiredSpreadRadius);

        if (fieldId == -1)
        {
            fieldId = flowFieldManager.CreateField(
                originTile, destinyTile, regionMin, regionMax,
                DefaultUnitRadius, requiredSpreadRadius);
        }

        if (fieldId == -1)
        {
            // Destino no transitable: no se pudo construir el campo.
            GD.Print("No se pudo calcular un flow field hacia ese destino.");
            return;
        }

        // ---------------------------------------------------------
        // 5. Asignar el seguidor a cada unidad, liberando cualquier
        // path de A* que tuviera antes (son flujos de movimiento
        // excluyentes, ver Without cruzados en las queries).
        // ---------------------------------------------------------
        int unitsCommanded = 0;

        foreach (var entity in selectedEntities)
        {
            if (!entity.IsAlive())
                continue;

            // Libera path de A* anterior si tenía (cambia de modo de movimiento)
            if (entity.Has<PathReferenceComponent>())
            {
                var oldPath = entity.Get<PathReferenceComponent>();
                _world.State.PathRegistryManager.ReleasePath(oldPath.PathId);
                entity.Remove<PathReferenceComponent>();
            }

            entity.Set(new FlowFieldFollowerComponent(fieldId,false));

            if (entity.Has<MoveResolutorComponent>())
            {
                ref var resolutor = ref entity.GetMut<MoveResolutorComponent>();
                resolutor.Blocked = false;
                resolutor.BlockedTimer = 0f;
            }

            if (entity.Has<StoppedTag>())
                entity.Remove<StoppedTag>();

            unitsCommanded++;
        }

        GD.Print($"[FlowField] Moviendo {unitsCommanded} unidades a {targetPosition} (field id={fieldId})");
    }
    private Entity FindEnemyEntityAtPoint(Vector2 point)
    {
        float tolerance = .5f;
        float minX = point.X - tolerance;
        float minY = point.Y - tolerance;
        float maxX = point.X + tolerance;
        float maxY = point.Y + tolerance;

        // -------------------------------------------------
        // 1. Buscar unidades enemigas (dynGrid)
        // -------------------------------------------------
        Span<int> results = stackalloc int[30];

        int foundCount = _world.State.DynamicHash.QueryNodesInBoxFiltered(minX, minY, maxX, maxY, _playerTeamId, results);

        Entity closestEntity = default;
        float minDistanceSq = tolerance * tolerance;

        for (int i = 0; i < foundCount; i++)
        {
            int nodeIdx = results[i];
            Entity entity = _world.State.DynamicHash.GetEntity(nodeIdx);

            if (entity.IsAlive() && !entity.Has<DeadTag>() && entity.Has<PositionComponent>())
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

        if (closestEntity.IsAlive())
            return closestEntity;

        // -------------------------------------------------
        // 2. Si no había unidad, buscar edificios enemigos (staticGrid)
        // -------------------------------------------------
        var staticGrid = _world.State.StaticSpatialBuildings;
        int radius = (int)MathF.Ceiling((tolerance * 2f) / staticGrid._cellSize);

        foreach (var id in staticGrid.QueryNearbyUnique(point.X, point.Y, radius, _playerTeamId))
        {
            if (staticGrid.TryGetValue(id, out Entity buildingEntity))
            {
                if (!buildingEntity.IsAlive() || buildingEntity.Has<DeadTag>()) continue;

                var buildingPos = buildingEntity.Get<PositionComponent>().position;
                float distSq = buildingPos.DistanceSquaredTo(point);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closestEntity = buildingEntity;
                }
            }
        }

        return closestEntity;
    }
    
    // El dibujo visual se mantiene en coordenadas de pantalla
    public override void _Draw()
    {
        if (!_isDragging) return;

        Rect2 rect = CreateRect(_dragStartScreen, _dragCurrentScreen);

        Color fillColor = new Color(0, 1, 0, 0.15f);   // Verde translúcido
        Color borderColor = new Color(0, 1, 0, 0.8f);  // Borde verde sólido

        DrawRect(rect, fillColor, true);
        DrawRect(rect, borderColor, false, 2.0f);
    }

    private Rect2 CreateRect(Vector2 pos1, Vector2 pos2)
    {
        Vector2 min = new Vector2(Mathf.Min(pos1.X, pos2.X), Mathf.Min(pos1.Y, pos2.Y));
        Vector2 size = new Vector2(Mathf.Abs(pos1.X - pos2.X), Mathf.Abs(pos1.Y - pos2.Y));
        return new Rect2(min, size);
    }

    // --- LÓGICA DE SELECCIÓN CON FLECS ---

    private void ClearPreviousSelection()
    {
        // Si no mantienes Shift presionado, limpiamos las selecciones previas
        if (!Input.IsKeyPressed(Key.Shift))
        {
            // 1. Creamos una lista temporal para almacenar las entidades encontradas
            List<Entity> entitiesToDeselect = new();

            // 2. Iteramos de forma ultra-performante por cada entidad encontrada
            query.Each((Entity ent) =>
            {
                if (ent.Has<SelectedTag>())
                {
                    entitiesToDeselect.Add(ent);
                }
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

    private void SelectUnitAtPoint(Vector2 point)
    {
        // 1. Limpiamos selección previa (si no hay Shift presionado)
        ClearPreviousSelection();

        // 2. Definimos un pequeño margen de tolerancia alrededor del clic (ej. 1 unidades)
        float tolerance = 1.0f;
        float minX = point.X - tolerance;
        float minY = point.Y - tolerance;
        float maxX = point.X + tolerance;
        float maxY = point.Y + tolerance;

        // Preparamos un Span temporal pequeño, ya que al hacer clic solo esperamos tocar unas pocas unidades
        Span<int> results = stackalloc int[30];

        // Valor para no filtrar por equipo (o pon el ID de tu equipo si aplica)
        ushort teamIgnore = 999;

        // 3. Consultamos al Spatial Hash solo en esa pequeña zona
        int foundCount = _world.State.DynamicHash.QueryNodesInBoxFiltered(minX, minY, maxX, maxY, teamIgnore, results);

        Entity closestEntity = default;
        float minDistanceSq = tolerance * tolerance; // Usamos distancia al cuadrado para evitar raíces cuadradas costosas

        // 4. Buscamos cuál de los resultados dentro del rango es el más cercano al centro exacto del clic
        for (int i = 0; i < foundCount; i++)
        {
            int nodeIdx = results[i];
            Entity entity = _world.State.DynamicHash.GetEntity(nodeIdx);

            if (entity.IsAlive() && entity.Has<PositionComponent>())
            {
                ref var pos = ref entity.GetMut<PositionComponent>(); // O tu forma de obtener la posición
                float distSq = pos.position.DistanceSquaredTo(point);

                if (distSq < minDistanceSq)
                {
                    minDistanceSq = distSq;
                    closestEntity = entity;
                }
            }
        }

        // 5. Si encontramos la entidad más cercana, le añadimos el tag de selección
        if (closestEntity.IsAlive())
        {
            if (!closestEntity.Has<SelectedTag>())
            {
                BlackyManagerSelector.CreateSelector(closestEntity);
                closestEntity.Add<SelectedTag>();
                GD.Print($"Unidad seleccionada por Spatial Hash (clic): {closestEntity.Id}");
            }
        }
    }

    private void SelectUnitsInRect(Rect2 rect)
    {
        ClearPreviousSelection();

        // Obtenemos los límites del rectángulo en coordenadas del mundo
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

        // Preparamos un Span temporal para recibir los resultados (ej. máximo 250 unidades a la vez)
        Span<int> results = stackalloc int[250];

        // Suponiendo que tu equipo de jugador es, por ejemplo, Team1 (0x10000 o el ID que uses)
        ushort teamIgnore = 999; // Pon un valor que no filtre nada, o el equipo enemigo si solo seleccionas los tuyos

        // ¡Consulta ultrarrápida al Spatial Hash!
        int foundCount = _world.State.DynamicHash.QueryNodesInBoxFiltered(minX, minY, maxX, maxY, teamIgnore, results);

        int selectedCount = 0;
        for (int i = 0; i < foundCount; i++)
        {
            int nodeIdx = results[i];
            Entity entity = _world.State.DynamicHash.GetEntity(nodeIdx);

            // aqui debemos verificar si realmente esta dentro del collider
            if (entity.IsAlive())
            {
                if (!entity.Has<SelectedTag>())
                {
                    if (entity.Has<UnitDefinitionComponent>())
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
            }
        }

        GD.Print($"Seleccionadas {selectedCount} unidades con el Spatial Hash.");
    }
}