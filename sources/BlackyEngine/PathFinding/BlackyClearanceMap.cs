using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.State.Occupancy;

// ---------------------------------------------------------
// Sub-chunk de holgura: guarda, para cada tile, dos cosas:
// - si está bloqueado (ocupado) directamente
// - la distancia (escalada, ver PrecisionScale) al obstáculo
//   más cercano, usada para las consultas de pathfinding.
// ---------------------------------------------------------
public class BlackyClearanceSubChunk
{
    public const byte MaxDistance = 255;

    private readonly byte[,] _distance;
    private readonly bool[,] _blocked;
    private readonly int[,] _debug; // luego quitarlo

    public BlackyClearanceSubChunk(int size)
    {
        _distance = new byte[size, size];
        _blocked = new bool[size, size];
        _debug = new int[size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                _distance[x, y] = MaxDistance;
                _debug[x, y] = -1;
            }
        }
    }

    public byte GetDistance(int x, int y) => _distance[x, y];
    public void SetDistance(int x, int y, byte value) => _distance[x, y] = value;

    public bool GetBlocked(int x, int y) => _blocked[x, y];
    public void SetBlocked(int x, int y, bool value) => _blocked[x, y] = value;

    public int GetIdDebug(int x, int y) => _debug[x, y];
    public void SetIdDebug(int x, int y, int id) => _debug[x, y] = id;

}

// ---------------------------------------------------------
// Mapa de holgura para pathfinding y chequeos rápidos de
// colisión de terreno. Mantiene su propio estado de bloqueo
// (actualizado vía OnTileChanged, llamado desde donde controles
// la ocupación real) y deriva de ahí un mapa de distancias al
// obstáculo más cercano — reutilizable para cualquier radio de
// collider, sin necesitar un mapa distinto por tamaño de unidad.
//
// Se actualiza de forma incremental: cada cambio solo marca una
// ventana como "sucia"; el recálculo real ocurre al llamar
// FlushDirty(), normalmente una vez por frame.
// ---------------------------------------------------------
public class BlackyClearanceMap : IDisposable
{
    // Escala de precisión: cuántas unidades internas representan
    // 1 tile completo de distancia. Con 4 (cuartos de tile) y un
    // byte (0-255), el rango útil máximo es 255/4 ≈ 63 tiles —
    // de sobra para cualquier radio de collider real.
    private const int PrecisionScale = 2;

    private readonly int _chunkSize;
    private readonly int _maxRadiusTilesScaled; // ya en unidades escaladas

    private readonly Dictionary<Vector2I, BlackyClearanceSubChunk> _chunks = new();

    private bool _hasDirty = false;
    private int _dirtyMinX, _dirtyMinY, _dirtyMaxX, _dirtyMaxY;

    private static readonly Vector2I[] Dirs =
    {
        new(1,0), new(-1,0), new(0,1), new(0,-1),
        new(1,1), new(1,-1), new(-1,1), new(-1,-1)
    };

    private static readonly int OrthogonalStep = PrecisionScale;
    private static readonly int DiagonalStep = Mathf.RoundToInt(PrecisionScale * 1.41421356f);

    /// <param name="chunkSize">Tamaño de cada sub-chunk, en tiles.</param>
    /// <param name="maxRadiusTiles">
    /// Mayor radio de collider (en tiles, puede tener decimales) que
    /// vas a consultar alguna vez. Define cuánto se expande la
    /// ventana de recálculo por cada cambio — un valor holgado
    /// (radio de tu unidad más grande + 1) es suficiente.
    /// </param>
    public BlackyClearanceMap(int chunkSize, float maxRadiusTiles)
    {
        _chunkSize = chunkSize;
        _maxRadiusTilesScaled = Mathf.CeilToInt(maxRadiusTiles * PrecisionScale);
    }

    #region Notificación de cambios

    /// <summary>
    /// Llamar cada vez que un tile pasa a estar ocupado/libre —
    /// puedes llamarlo desde donde controles tú la ocupación
    /// (al colocar/destruir un edificio, árbol, recurso, etc.),
    /// sin depender de ninguna otra estructura.
    /// </summary>
    public void OnTileChanged(int worldX, int worldY, bool isBlocked)
    {
        SetBlockedFlag(worldX, worldY, isBlocked);
        
        int marginTiles = Mathf.CeilToInt((float)_maxRadiusTilesScaled / PrecisionScale) + 1;
        MarkDirty(worldX - marginTiles, worldY - marginTiles, worldX + marginTiles, worldY + marginTiles);
    }

    private void MarkDirty(int minX, int minY, int maxX, int maxY)
    {
        if (!_hasDirty)
        {
            _dirtyMinX = minX; _dirtyMinY = minY;
            _dirtyMaxX = maxX; _dirtyMaxY = maxY;
            _hasDirty = true;
        }
        else
        {
            _dirtyMinX = Math.Min(_dirtyMinX, minX);
            _dirtyMinY = Math.Min(_dirtyMinY, minY);
            _dirtyMaxX = Math.Max(_dirtyMaxX, maxX);
            _dirtyMaxY = Math.Max(_dirtyMaxY, maxY);
        }
    }

    /// <summary>
    /// Recalcula las distancias de la región marcada como sucia.
    /// Llamar UNA vez por frame (o justo antes de pathfindear),
    /// no en cada OnTileChanged — así varios cambios seguidos
    /// (ej. colocar un edificio grande, tile por tile) se resuelven
    /// en un solo recálculo en vez de uno por tile.
    /// </summary>
    public void FlushDirty()
    {
        if (!_hasDirty) return;

        RebuildRegion(_dirtyMinX, _dirtyMinY, _dirtyMaxX, _dirtyMaxY);
        _hasDirty = false;
    }

    #endregion

    #region Recálculo (BFS multi-fuente, acotado a la ventana sucia)

    private void RebuildRegion(int minX, int minY, int maxX, int maxY)
    {
        var queue = new Queue<Vector2I>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                bool blocked = GetBlockedFlag(x, y);
                byte value = blocked ? (byte)0 : BlackyClearanceSubChunk.MaxDistance;

                SetDistanceInternal(x, y, value);
                
                if (blocked)
                    queue.Enqueue(new Vector2I(x, y));
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            byte currentDist = GetDistanceInternal(current.X, current.Y);

            foreach (var dir in Dirs)
            {
                int nx = current.X + dir.X;
                int ny = current.Y + dir.Y;

                if (nx < minX || ny < minY || nx > maxX || ny > maxY)
                    continue; // fuera de la ventana: no lo tocamos

                bool isDiagonal = dir.X != 0 && dir.Y != 0;
                int step = isDiagonal ? DiagonalStep : OrthogonalStep;

                byte candidate = (byte)Math.Min(currentDist + step, BlackyClearanceSubChunk.MaxDistance);

                if (candidate < GetDistanceInternal(nx, ny))
                {
                    SetDistanceInternal(nx, ny, candidate);
                   
                    queue.Enqueue(new Vector2I(nx, ny));
                }
            }
        }
    }

    #endregion

    #region Consulta pública

    /// <summary>
    /// ¿Un collider de este radio (en tiles, puede tener decimales)
    /// puede pasar por aquí? O(1): un lookup directo, sin recalcular
    /// nada — apto para llamarse miles de veces por cada A*.
    /// </summary>
    public bool IsWalkableForRadius(int worldX, int worldY, float radiusTiles)
    {
        int radiusScaled = Mathf.CeilToInt(radiusTiles * PrecisionScale);
        return GetDistanceInternal(worldX, worldY) > radiusScaled;
    }

    /// <summary>
    /// Distancia cruda (en tiles reales, ya desescalada) al obstáculo
    /// más cercano — útil para heurísticas, no para el chequeo normal.
    /// </summary>
    public float GetClearanceTiles(int worldX, int worldY)
        => (float)GetDistanceInternal(worldX, worldY) / PrecisionScale;

    #endregion

    #region Almacenamiento por chunk — distancia

    private byte GetDistanceInternal(int worldX, int worldY)
    {
        var (chunkCoord, localX, localY) = GetCoords(worldX, worldY);

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
            return BlackyClearanceSubChunk.MaxDistance; // sin datos aún = asumimos libre

        return chunk.GetDistance(localX, localY);
    }

    private void SetDistanceInternal(int worldX, int worldY, byte value)
    {
        var chunk = GetOrCreateChunk(worldX, worldY, out int localX, out int localY);
        chunk.SetDistance(localX, localY, value);
        
        DebugDistance(chunk, worldX, worldY, localX, localY, value);

    }
    private void DebugDistance(BlackyClearanceSubChunk chunk, int worldX,int worldY, int localX, int localY,  int value)
    {
        Vector2 pos = TilesHelper.TilePositionToWorldPosition(worldX, worldY);
        int id = chunk.GetIdDebug(localX, localY);
        if (id==-1)
        {
            id =BlackyWorldContext.Simulation.DebugText.DrawText(new Vector3(pos.X, pos.Y, 10), value.ToString());
            chunk.SetIdDebug(localX,localY,id);
        }
        else
        {
            BlackyWorldContext.Simulation.DebugText.UpdateText(id,value.ToString());
        }
        
    }

    #endregion

    #region Almacenamiento por chunk — flag de bloqueo

    private bool GetBlockedFlag(int worldX, int worldY)
    {
        var (chunkCoord, localX, localY) = GetCoords(worldX, worldY);

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
            return false; // sin datos aún = asumimos libre

        return chunk.GetBlocked(localX, localY);
    }

    private void SetBlockedFlag(int worldX, int worldY, bool value)
    {
        var chunk = GetOrCreateChunk(worldX, worldY, out int localX, out int localY);
        chunk.SetBlocked(localX, localY, value);
    }

    #endregion

    #region Coordinate Math

    private BlackyClearanceSubChunk GetOrCreateChunk(int worldX, int worldY, out int localX, out int localY)
    {
        var (chunkCoord, lx, ly) = GetCoords(worldX, worldY);
        localX = lx;
        localY = ly;

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
        {
            chunk = new BlackyClearanceSubChunk(_chunkSize);
            _chunks.Add(chunkCoord, chunk);
        }

        return chunk;
    }

    private (Vector2I chunkCoord, int localX, int localY) GetCoords(int worldX, int worldY)
    {
        int chunkX = WorldToChunk(worldX);
        int chunkY = WorldToChunk(worldY);

        int localX = WorldToLocal(worldX);
        int localY = WorldToLocal(worldY);

        return (new Vector2I(chunkX, chunkY), localX, localY);
    }

    private int WorldToChunk(int coord)
        => (int)MathF.Floor((float)coord / _chunkSize);

    private int WorldToLocal(int coord)
    {
        int local = coord % _chunkSize;
        return local < 0 ? local + _chunkSize : local;
    }

    #endregion

    public void Dispose()
    {
        _chunks.Clear();
    }
}