using Godot;
using GodotEcsArch.sources.BlackyEngine.State.Occupancy;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;

// ---------------------------------------------------------
// Una entrada de caché: guarda tanto los parámetros con los que
// se construyó el campo (para poder reconstruirlo si se invalida)
// como el FlowField calculado en sí. Field puede quedar null
// temporalmente si está marcado sucio y aún no se hizo Flush.
// ---------------------------------------------------------
public class FlowFieldEntry
{
    public int Id;
    public Vector2I OriginTile;
    public Vector2I GoalTile;
    public Vector2I RegionMin;
    public Vector2I RegionMax;
    public float RadiusTiles;
    public FlowField Field;
}

// ---------------------------------------------------------
// Administra campos de flujo identificados por int id. Varias
// unidades/oleadas con orígenes y destinos parecidos pueden
// compartir el mismo id en vez de generar un campo por cada
// combinación exacta de origen/destino.
//
// Igual que BlackyClearanceMap: los cambios de ocupación solo
// MARCAN qué ids quedaron sucios (barato, O(entradas) por
// notificación); el recálculo real ocurre al llamar FlushDirty(),
// no en cada notificación.
// ---------------------------------------------------------
public class BlackyFlowFieldManager
{
    private readonly BlackyClearanceMap _clearanceMap;
    private readonly Dictionary<int, FlowFieldEntry> _entries = new();
    private readonly HashSet<int> _dirtyIds = new();

    private int _nextId = 1;

    public BlackyFlowFieldManager(BlackyClearanceMap clearanceMap)
    {
        _clearanceMap = clearanceMap;
    }

    #region Creación

    /// <summary>
    /// Construye un campo nuevo y lo registra con un id propio.
    /// Devuelve -1 si el destino no es transitable.
    /// </summary>
    public int CreateField(
        Vector2I originTile,
        Vector2I goalTile,
        Vector2I regionMin,
        Vector2I regionMax,
        float radiusTiles)
    {
        var field = FlowField.Build(_clearanceMap, goalTile, regionMin, regionMax, radiusTiles);
        if (field == null)
            return -1;

        int id = _nextId++;

        _entries[id] = new FlowFieldEntry
        {
            Id = id,
            OriginTile = originTile,
            GoalTile = goalTile,
            RegionMin = regionMin,
            RegionMax = regionMax,
            RadiusTiles = radiusTiles,
            Field = field
        };

        return id;
    }

    #endregion

    #region Búsqueda de campo existente

    /// <summary>
    /// Busca, entre los campos ya cacheados, el más cercano cuyo
    /// destino coincida (dentro de 'maxGoalDistanceTiles') con
    /// 'destinyTile' y cuya región cubra tanto el origen como el
    /// destino pedidos. Devuelve -1 si no hay ninguno reutilizable
    /// — en ese caso, el llamador debe usar CreateField.
    /// </summary>
    public int FindNearestFieldId(Vector2I originTile, Vector2I destinyTile, float maxGoalDistanceTiles = 2f)
    {
        int bestId = -1;
        float bestGoalDistSq = maxGoalDistanceTiles * maxGoalDistanceTiles;

        foreach (var entry in _entries.Values)
        {
            if (entry.Field == null) continue; // sucio, pendiente de reconstruir

            if (!Contains(entry, originTile)) continue;
            if (!Contains(entry, destinyTile)) continue;

            Vector2I diff = entry.GoalTile - destinyTile;
            float goalDistSq = diff.X * diff.X + diff.Y * diff.Y;

            if (goalDistSq <= bestGoalDistSq)
            {
                bestGoalDistSq = goalDistSq;
                bestId = entry.Id;
            }
        }

        return bestId;
    }

    #endregion

    #region Consulta

    public FlowField GetField(int id)
        => _entries.TryGetValue(id, out var entry) ? entry.Field : null;

    public FlowFieldEntry GetEntry(int id)
        => _entries.TryGetValue(id, out var entry) ? entry : null;

    #endregion

    #region Invalidación (marcar) y reconstrucción (flush)

    /// <summary>
    /// Llamar desde el mismo punto que notifica cambios de
    /// ocupación (BlackyPathfinder.NotifyTileOccupancyChanged).
    /// Solo MARCA qué ids quedaron afectados — no recalcula nada
    /// todavía. Barato: recorre entradas, sin tocar Dijkstra.
    /// </summary>
    public void MarkTileChanged(int worldX, int worldY)
    {
        var tile = new Vector2I(worldX, worldY);

        foreach (var entry in _entries.Values)
        {
            if (Contains(entry, tile))
            {
                _dirtyIds.Add(entry.Id);
            }
        }
    }

    /// <summary>
    /// Recalcula todos los campos marcados como sucios. Llamar
    /// UNA vez por frame (o antes de consultar campos), igual que
    /// BlackyClearanceMap.FlushDirty(). Mientras un id está sucio
    /// y no se hizo flush, GetField(id) devuelve el valor viejo
    /// (Field no se pone en null hasta el propio recálculo, para
    /// que las unidades no se queden sin dirección un frame entero
    /// solo por estar en cola).
    /// </summary>
    public void FlushDirty()
    {
        if (_dirtyIds.Count == 0) return;

        foreach (var id in _dirtyIds)
        {
            if (!_entries.TryGetValue(id, out var entry))
                continue;

            entry.Field = FlowField.Build(
                _clearanceMap,
                entry.GoalTile,
                entry.RegionMin,
                entry.RegionMax,
                entry.RadiusTiles);

            // Si el destino dejó de ser transitable, Field queda null:
            // GetField(id) devolverá null y FlowFieldFollowSystem
            // pondrá DesiredDir en cero para esos seguidores.
        }

        _dirtyIds.Clear();
    }

    #endregion

    public void RemoveField(int id)
    {
        _entries.Remove(id);
        _dirtyIds.Remove(id);
    }

    public void Clear()
    {
        _entries.Clear();
        _dirtyIds.Clear();
    }

    private static bool Contains(FlowFieldEntry entry, Vector2I tile)
        => tile.X >= entry.RegionMin.X && tile.X <= entry.RegionMax.X &&
           tile.Y >= entry.RegionMin.Y && tile.Y <= entry.RegionMax.Y;
}