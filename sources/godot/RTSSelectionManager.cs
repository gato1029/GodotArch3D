
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generic;
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
    public static RTSSelectionManager Instance { get; private set; }

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
        query = _world.Simulation.Flecs.WorldFlecs.QueryBuilder().With<SelectedTag>().Build();
    }

    public override void _Ready()
    {
        Instance = this;
        GD.Print("Selector Cargado");
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_world == null) return;

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
        // 1. FASE DE LECTURA: Recopilamos las entidades seleccionadas en una lista temporal
        List<Entity> selectedEntities = new();

        query.Each((Entity entity) =>
            {
                if (entity.IsAlive())
                {
                    selectedEntities.Add(entity);
                }
            });

        int unitsCommanded = 0;

        // 2. FASE DE ACCIÓN: Modificamos las entidades de forma 100% segura fuera del Each
        foreach (var entity in selectedEntities)
        {
            if (entity.IsAlive())
            {
                // Actualizar o añadir destino (cambio estructural o de mutación)
                if (entity.Has<MoveTargetComponent>())
                {
                    ref var target = ref entity.GetMut<MoveTargetComponent>();
                    target.Value = targetPosition;
                }
                else
                {
                    entity.Set(new MoveTargetComponent { Value = targetPosition });
                }

                // Quitar el tag de detenido
                if (entity.Has<StoppedTag>())
                {
                    entity.Remove<StoppedTag>();
                    ref var res = ref entity.GetMut<MoveResolutorComponent>();
                    res.Blocked = false;
                }

                unitsCommanded++;
            }
        }

        if (unitsCommanded > 0)
        {
            GD.Print($"Moviendo {unitsCommanded} unidades a {targetPosition}");
        }
    }

    // El dibujo visual se mantiene en coordenadas de pantalla
    public override void _Draw()
    {
        if (!_isDragging) return;

        Rect2 rect = CreateRect(_dragStartScreen, _dragCurrentScreen);

        Color fillColor = new Color(0, 1, 0, 0.15f); // Verde translúcido
        Color borderColor = new Color(0, 1, 0, 0.8f); // Borde verde sólido

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
        //Si no mantienes Shift presionado, limpiamos las selecciones previas
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
                    ref var  selector =  ref ent.GetMut<RenderSelectionGPUComponent>();
                    if (selector.rid != default && selector.instance!=-1)
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
                        if (CollisionMathHelper.CheckCircle(positionComp.position.X, positionComp.position.Y, moveCollider.Radius, moveCollider.Offset, centerPosition.X, centerPosition.Y, ref collider))
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