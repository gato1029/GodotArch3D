using Godot;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyEngine.PathFinding;




// ---------------------------------------------------------
// Sub-chunk de holgura: guarda, para cada tile, la distancia
// (en tiles, clamped a byte) al obstáculo más cercano.
// 255 = "sin obstáculo cerca" (libre para cualquier radio razonable).
// 0   = el propio tile está ocupado.
// ---------------------------------------------------------
public class BlackyClearanceSubChunk
{
    public const byte MaxDistance = 255;

    private readonly byte[,] _distance;

    public BlackyClearanceSubChunk(int size)
    {
        _distance = new byte[size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                _distance[x, y] = MaxDistance;
    }

    public byte Get(int x, int y) => _distance[x, y];
    public void Set(int x, int y, byte value) => _distance[x, y] = value;
}

// ---------------------------------------------------------
// Mapa de holgura para pathfinding y chequeos rápidos de
// colisión de terreno. Se deriva de BlackyChunkOccupancyMap
// (u otra fuente de ocupación) y se mantiene actualizado de
// forma incremental: solo recalcula la ventana afectada por
// cada cambio, no el mapa completo.
// ---------------------------------------------------------
public class BlackyClearanceMap : IDisposable
{
    private readonly int _chunkSize;
    private readonly int _maxRadiusTiles; // mayor radio de collider que vas a consultar
    private readonly Func<int, int, bool> _isBlocking; // predicado: ¿este tile bloquea?

    private readonly Dictionary<Vector2I, BlackyClearanceSubChunk> _chunks = new();

    private bool _hasDirty = false;
    private int _dirtyMinX, _dirtyMinY, _dirtyMaxX, _dirtyMaxY;

    private static readonly Vector2I[] Dirs =
    {
        new(1,0), new(-1,0), new(0,1), new(0,-1),
        new(1,1), new(1,-1), new(-1,1), new(-1,-1)
    };

    /// <param name="isBlocking">
    /// Función que decide si un tile bloquea el paso. Normalmente
    /// combina varias capas de BlackyChunkOccupancyMap con OR
    /// (ej.: bloquea si hay edificio O si hay recurso), ya que al
    /// pathfinding no le importa el "quién", solo el "se puede pasar".
    /// </param>
    /// <param name="maxRadiusTiles">
    /// Mayor radio de collider (en tiles) que vas a consultar alguna
    /// vez. Define cuánto se expande la ventana de recálculo por
    /// cada cambio — no necesita ser exacto, un valor holgado
    /// (ej. el radio de tu unidad más grande + 1) es suficiente.
    /// </param>
    public BlackyClearanceMap(Func<int, int, bool> isBlocking, int chunkSize, int maxRadiusTiles)
    {
        _isBlocking = isBlocking;
        _chunkSize = chunkSize;
        _maxRadiusTiles = maxRadiusTiles;
    }

    #region Notificación de cambios

    // Llamar esto desde el OnTileUpdated de BlackyChunkOccupancyMap
    // (o desde donde sea que coloques/quites algo del mapa).
    public void NotifyTileChanged(int worldX, int worldY)
    {
        int margin = _maxRadiusTiles + 1;
        MarkDirty(worldX - margin, worldY - margin, worldX + margin, worldY + margin);
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

    // Llamar UNA vez por frame (o justo antes de pathfindear), no
    // en cada NotifyTileChanged — así varios cambios seguidos
    // (ej. SetArea de un edificio grande) se resuelven en un solo
    // recálculo en vez de uno por tile.
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
                bool blocked = _isBlocking(x, y);
                byte value = blocked ? (byte)0 : BlackyClearanceSubChunk.MaxDistance;

                SetDistance(x, y, value);

                if (blocked)
                    queue.Enqueue(new Vector2I(x, y));
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            byte currentDist = GetDistance(current.X, current.Y);

            foreach (var dir in Dirs)
            {
                int nx = current.X + dir.X;
                int ny = current.Y + dir.Y;

                if (nx < minX || ny < minY || nx > maxX || ny > maxY)
                    continue; // fuera de la ventana: no lo tocamos

                byte candidate = (byte)Math.Min(currentDist + 1, BlackyClearanceSubChunk.MaxDistance);

                if (candidate < GetDistance(nx, ny))
                {
                    SetDistance(nx, ny, candidate);
                    queue.Enqueue(new Vector2I(nx, ny));
                }
            }
        }
    }

    #endregion

    #region Consulta pública

    // O(1): ¿un collider de este radio (en tiles) puede pasar por aquí?
    public bool IsWalkableForRadius(int worldX, int worldY, int radiusTiles)
        => GetDistance(worldX, worldY) > radiusTiles;

    // Distancia cruda al obstáculo más cercano, por si la necesitas
    // para heurísticas (ej. preferir rutas más alejadas de obstáculos).
    public byte GetClearance(int worldX, int worldY)
        => GetDistance(worldX, worldY);

    #endregion

    #region Almacenamiento por chunk

    private byte GetDistance(int worldX, int worldY)
    {
        var (chunkCoord, localX, localY) = GetCoords(worldX, worldY);

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
            return BlackyClearanceSubChunk.MaxDistance; // sin datos aún = asumimos libre

        return chunk.Get(localX, localY);
    }

    private void SetDistance(int worldX, int worldY, byte value)
    {
        var (chunkCoord, localX, localY) = GetCoords(worldX, worldY);

        if (!_chunks.TryGetValue(chunkCoord, out var chunk))
        {
            chunk = new BlackyClearanceSubChunk(_chunkSize);
            _chunks.Add(chunkCoord, chunk);
        }

        chunk.Set(localX, localY, value);
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