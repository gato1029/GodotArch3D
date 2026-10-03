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
    public int GoalSpreadRadius;
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

    // Para campos persistentes de IA: un único id por tile de objetivo,
    // reutilizado siempre que exista (sin tolerancia de distancia, el
    // objetivo es fijo).
    private readonly Dictionary<Vector2I, int> _persistentFieldsByGoal = new();

    private int _nextId = 1;

    public BlackyFlowFieldManager(BlackyClearanceMap clearanceMap)
    {
        _clearanceMap = clearanceMap;
    }

    #region Creación — uso general (jugador, campos efímeros)

    /// <summary>
    /// Construye un campo nuevo y lo registra con un id propio.
    /// Devuelve -1 si el destino (ni su disco de siembra) es transitable.
    /// </summary>
    public int CreateField(
        Vector2I originTile,
        Vector2I goalTile,
        Vector2I regionMin,
        Vector2I regionMax,
        float radiusTiles,
        int goalSpreadRadius = 0)
    {
        var field = FlowField.Build(_clearanceMap, goalTile, regionMin, regionMax, radiusTiles, goalSpreadRadius);
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
            GoalSpreadRadius = goalSpreadRadius,
            Field = field
        };

        return id;
    }

    #endregion

    #region Búsqueda de campo existente (jugador)

    /// <summary>
    /// Busca, entre los campos cacheados, el más cercano cuyo destino
    /// coincida con 'destinyTile', cuya región cubra origen y destino,
    /// y cuyo GoalSpreadRadius alcance para 'requiredSpreadRadius'
    /// (si el campo existente tiene un disco más chico del necesario,
    /// no se reutiliza, para no apretar al grupo nuevo en un espacio
    /// pensado para menos unidades).
    /// </summary>
    public int FindNearestFieldId(
        Vector2I originTile,
        Vector2I destinyTile,
        int requiredSpreadRadius,
        float maxGoalDistanceTiles = 2f)
    {
        int bestId = -1;
        float bestGoalDistSq = maxGoalDistanceTiles * maxGoalDistanceTiles;

        foreach (var entry in _entries.Values)
        {
            if (entry.Field == null) continue; // sucio, pendiente de reconstruir
            if (entry.GoalSpreadRadius < requiredSpreadRadius) continue;

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

    #region Campos persistentes (IA / oleadas)

    /// <summary>
    /// Para objetivos fijos y duraderos (la base del jugador, un
    /// edificio clave): un único campo por tile de destino exacto,
    /// reutilizado indefinidamente por cualquier cantidad de unidades,
    /// sin importar cuántas oleadas lo usen. Se recalcula solo vía
    /// MarkTileChanged/FlushDirty si el terreno cambia.
    /// </summary>
    public int GetOrCreatePersistentField(
        Vector2I goalTile,
        Vector2I regionMin,
        Vector2I regionMax,
        float radiusTiles,
        int goalSpreadRadius = 3)
    {
        if (_persistentFieldsByGoal.TryGetValue(goalTile, out int existingId) && _entries.ContainsKey(existingId))
            return existingId;

        int id = CreateField(default, goalTile, regionMin, regionMax, radiusTiles, goalSpreadRadius);

        if (id != -1)
            _persistentFieldsByGoal[goalTile] = id;

        return id;
    }

    #endregion

    #region Consulta

    public FlowField GetField(int id)
        => _entries.TryGetValue(id, out var entry) ? entry.Field : null;

    public FlowFieldEntry GetEntry(int id)
        => _entries.TryGetValue(id, out var entry) ? entry : null;

    #endregion

    #region Invalidación (marcar) y reconstrucción (flush)

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
                entry.RadiusTiles,
                entry.GoalSpreadRadius);
        }

        _dirtyIds.Clear();
    }

    #endregion

    public void RemoveField(int id)
    {
        if (_entries.TryGetValue(id, out var entry))
        {
            // Si era un campo persistente, limpiamos también ese índice
            if (_persistentFieldsByGoal.TryGetValue(entry.GoalTile, out int persistentId) && persistentId == id)
                _persistentFieldsByGoal.Remove(entry.GoalTile);
        }

        _entries.Remove(id);
        _dirtyIds.Remove(id);
    }

    public void Clear()
    {
        _entries.Clear();
        _dirtyIds.Clear();
        _persistentFieldsByGoal.Clear();
    }

    private static bool Contains(FlowFieldEntry entry, Vector2I tile)
        => tile.X >= entry.RegionMin.X && tile.X <= entry.RegionMax.X &&
           tile.Y >= entry.RegionMin.Y && tile.Y <= entry.RegionMax.Y;
}