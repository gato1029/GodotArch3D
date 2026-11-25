using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyEngine.Spatial;

namespace GodotEcsArch.sources.GameRuntime.Internal.RTSControler;

using Godot;
using BlackyEngine.Core;
using BlackyEngine.Generic;
using BlackyEngine.PathFinding;
using managers.Mods;
using utils;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;

public enum SelectionKind : byte
{
    None = 0,
    Units = 1,
    Building = 2,
    Resource = 3
}

public class RTSSelectionManager
{
    private readonly BlackyWorld _world;
    private readonly BlackyPathfinder _pathfinder;
    private readonly Query _query;

    public SelectionKind CurrentSelectionKind { get; private set; } = SelectionKind.None;

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

        float tolerance = 1f;
        float minX = point.X - tolerance;
        float minY = point.Y - tolerance;
        float maxX = point.X + tolerance;
        float maxY = point.Y + tolerance;

        ushort teamIgnore = 999;

        // 1) Prioridad 1: Unidades dinámicas
        Span<int> unitResults = stackalloc int[30];
        int foundUnits = _world.State.DynamicHash.QueryNodesInBoxFiltered(
            minX, minY, maxX, maxY, teamIgnore, unitResults);

        Entity closestUnit = default;
        float minUnitDistSq = tolerance * tolerance;

        for (int i = 0; i < foundUnits; i++)
        {
            Entity entity = _world.State.DynamicHash.GetEntity(unitResults[i]);
            if (entity.IsAlive() && !entity.Has<DeadTag>() && entity.Has<PositionComponent>())
            {
                ref var pos = ref entity.GetMut<PositionComponent>();
                float distSq = pos.position.DistanceSquaredTo(point);

                if (distSq < minUnitDistSq)
                {
                    minUnitDistSq = distSq;
                    closestUnit = entity;
                }
            }
        }

        if (closestUnit.IsAlive())
        {
            SelectSingleEntity(closestUnit, SelectionKind.Units);
            GD.Print($"Unidad seleccionada por Spatial Hash (clic): {closestUnit.Id}");
            return;
        }
         tolerance = 0f;
         minX = point.X - tolerance;
         minY = point.Y - tolerance;
         maxX = point.X + tolerance;
         maxY = point.Y + tolerance;
        // 2) Prioridad 2: Edificios estáticos
        Entity? closestBuilding = FindClosestStaticEntity(
            _world.State.StaticSpatialBuildings, point, minX, minY, maxX, maxY, teamIgnore);

        if (closestBuilding.HasValue && closestBuilding.Value.IsAlive())
        {
            SelectSingleEntity(closestBuilding.Value, SelectionKind.Building);
            GD.Print($"Edificio seleccionado por StaticSpatialBuildings (clic): {closestBuilding.Value.Id}");
            return;
        }

        // 3) Prioridad 3: Recursos estáticos
        Entity? closestResource = FindClosestStaticEntity(
            _world.State.StaticSpatialResources, point, minX, minY, maxX, maxY, teamIgnore);

        if (closestResource.HasValue && closestResource.Value.IsAlive())
        {
            SelectSingleEntity(closestResource.Value, SelectionKind.Resource);
            GD.Print($"Recurso seleccionado por StaticSpatialResources (clic): {closestResource.Value.Id}");
            return;
        }

        CurrentSelectionKind = SelectionKind.None;
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

        int foundCount = _world.State.DynamicHash.QueryNodesInBoxFiltered(
            minX, minY, maxX, maxY, teamIgnore, results);
        int selectedCount = 0;

        for (int i = 0; i < foundCount; i++)
        {
            Entity entity = _world.State.DynamicHash.GetEntity(results[i]);

            if (entity.IsAlive() && !entity.Has<DeadTag>() &&
                !entity.Has<SelectedTag>() && entity.Has<UnitDefinitionComponent>())
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
                    SelectionContext.AddSelection(entity);
                    BlackyManagerSelector.CreateSelector(entity);
                    selectedCount++;
                }
            }
        }

        if (selectedCount > 0)
        {
            CurrentSelectionKind = SelectionKind.Units;
            GD.Print($"Seleccionadas {selectedCount} unidades con el Spatial Hash.");
            return;
        }

        // Si no había unidades en el rectángulo, permitir seleccionar 1 edificio o 1 recurso cercano al centro
        Entity? fallbackBuilding = FindClosestStaticEntity(
            _world.State.StaticSpatialBuildings, centerPosition, minX, minY, maxX, maxY, teamIgnore);

        if (fallbackBuilding.HasValue && fallbackBuilding.Value.IsAlive())
        {
            SelectSingleEntity(fallbackBuilding.Value, SelectionKind.Building);
            GD.Print($"Edificio seleccionado en rectángulo (sin unidades): {fallbackBuilding.Value.Id}");
            return;
        }

        Entity? fallbackResource = FindClosestStaticEntity(
            _world.State.StaticSpatialResources, centerPosition, minX, minY, maxX, maxY, teamIgnore);

        if (fallbackResource.HasValue && fallbackResource.Value.IsAlive())
        {
            SelectSingleEntity(fallbackResource.Value, SelectionKind.Resource);
            GD.Print($"Recurso seleccionado en rectángulo (sin unidades): {fallbackResource.Value.Id}");
            return;
        }

        CurrentSelectionKind = SelectionKind.None;
    }

    // ==========================================
    // 2. HELPERS DE SELECCIÓN ESTÁTICA
    // ==========================================

    private static Entity? FindClosestStaticEntity(
        StaticSpatialChunkGrid<Entity>? grid,
        Vector2 referencePoint,
        float minX, float minY, float maxX, float maxY,
        ushort teamIgnore)
    {
        if (grid == null)
            return null;

        Span<int> gridIds = stackalloc int[32];
        int count = grid.QueryRectNonAlloc(minX, minY, maxX, maxY, gridIds, teamIgnore);

        Entity bestEntity = default;
        float bestDistSq =0.25f;

        for (int i = 0; i < count; i++)
        {
            if (!grid.TryGetValue(gridIds[i], out Entity entity))
                continue;

            if (!entity.IsAlive() || entity.Has<DeadTag>())
                continue;

            float distSq = float.MaxValue;
            if (entity.Has<PositionComponent>())
            {
                ref readonly var pos = ref entity.Get<PositionComponent>();
                distSq = pos.position.DistanceSquaredTo(referencePoint);
            }
            else
            {
                distSq = i;
            }

            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                bestEntity = entity;
            }
        }

        return bestEntity.IsAlive() ? bestEntity : null;
    }

    private void SelectSingleEntity(Entity entity, SelectionKind kind)
    {
        if (entity.Has<SelectedTag>())
            return;

        BlackyManagerSelector.CreateSelector(entity);
        entity.Add<SelectedTag>();
        SelectionContext.SetSelection(entity);
        CurrentSelectionKind = kind;
    }

    // ==========================================
    // 3. LIMPIEZA DE SELECCIÓN
    // ==========================================

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

                    if (ent.Has<RenderSelectionGPUComponent>())
                    {
                        ref var selector = ref ent.GetMut<RenderSelectionGPUComponent>();
                        if (selector.gpuIndex != default && selector.instance != -1)
                        {
                            AtlasTexturesModsManager.Instance.DrivenRenderManager .FreeInstance(selector.gpuIndex, selector.instance);
                            selector.gpuIndex = default;
                            selector.instance = -1;
                        }
                    }
                }
            }

            SelectionContext.Clear();
            CurrentSelectionKind = SelectionKind.None;
        }
    }
}