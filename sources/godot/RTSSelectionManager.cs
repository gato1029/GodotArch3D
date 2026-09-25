using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generic;
using GodotEcsArch.sources.BlackyEngine.PathFinding;
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
        CommandSelectedUnitsToMove(targetWorldPos);
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
        Vector2 movementDirection = targetPosition - groupCenter;

        if (movementDirection.LengthSquared() < 0.001f)
            return;

        movementDirection = movementDirection.Normalized();

        // ---------------------------------------------------------
        // 3. Crear formación
        // ---------------------------------------------------------
        float spacing = .9f; // antes 1.5f

        var slots = FormationHelper.GenerateGridSlots(selectedEntities.Count, spacing);

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
        // ---------------------------------------------------------
        float formationLeadDistance = spacing * 3.0f;

        Vector2 pathStart = groupCenter + movementDirection * formationLeadDistance;

        // ---------------------------------------------------------
        // 6. Calcular A* (UNA sola vez para todo el grupo)
        // ---------------------------------------------------------
        Vector2I origin = TilesHelper.WorldPositionToTile(pathStart);
        Vector2I destiny = TilesHelper.WorldPositionToTile(targetPosition);

        var points = _pathfinder.FindPathWorld(origin, destiny);

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
                _world.State.PathRegistryManager.ReleasePath(oldPath.PathId);
            }

            // ---------------------------------------------
            // Referencia al nuevo path
            // ---------------------------------------------
            _world.State.PathRegistryManager.AddReference(pathId);

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

            unitsCommanded++;
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