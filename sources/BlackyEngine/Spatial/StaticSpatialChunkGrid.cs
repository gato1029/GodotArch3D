using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace GodotEcsArch.sources.BlackyEngine.Spatial;

/// <summary>
/// Grid espacial para objetos ESTATICOS (recursos, edificios).
///
/// - Cada chunk guarda un mini CSR (cellStart + cellEntities): IDs contiguos por celda,
///   sin punteros ni maxNodes.
/// - Los chunks se crean solo cuando se registra el primer objeto (null = sin coste).
/// - Eliminar = marcar como muerto (O(1)). Cuando un chunk acumula muchos muertos
///   se compacta (se reconstruye UNA vez) y sus IDs se liberan para reutilizarse.
/// - Registrar solo anade el ID a la lista del chunk y lo marca "dirty"; la
///   reconstruccion ocurre una sola vez, en la primera consulta o en Flush().
/// - Para consultar en paralelo: usar un QueryContext por hilo y asegurarse de que
///   no haya chunks dirty. RegisterStaticSafe/FreeColliderSafe dejan limpios los
///   chunks afectados; Flush() limpia todos.
/// - Team por entidad (no por nodo), igual semantica que la clase anterior.
/// - Coordenadas centradas en (0,0), igual que la clase anterior.
/// </summary>
public sealed class StaticSpatialChunkGrid<T>
{
    private const byte FlagAlive = 1;
    private const byte FlagMulti = 2; // la entidad ocupa mas de una celda -> necesita dedupe

    // Un chunk se compacta cuando todos sus miembros estan muertos, o cuando
    // hay al menos MinDeadForCompaction muertos y son >= 1/DeadRatioDivisor del total.
    private const int MinDeadForCompaction = 8;
    private const int DeadRatioDivisor = 4; // 25 %

    private struct CellBounds
    {
        public int MinX, MinY, MaxX, MaxY; // en celdas globales (ya recortadas al mundo)
    }

    private sealed class Chunk
    {
        public readonly int Index;
        public readonly int ChunkX;
        public readonly int ChunkY;

        public readonly int[] CellStart;               // tamano cellsPerChunk + 1
        public int[] CellEntities = Array.Empty<int>(); // IDs contiguos por celda

        public int[] Members = new int[4];             // IDs unicos del chunk (vivos o muertos sin compactar)
        public int MemberCount;
        public int DeadCount;
        public bool Dirty;

        public Chunk(int index, int chunkX, int chunkY, int cellsPerChunk)
        {
            Index = index;
            ChunkX = chunkX;
            ChunkY = chunkY;
            CellStart = new int[cellsPerChunk + 1];
        }
    }

    // ---------------- Geometria ----------------
    public readonly float _cellSize; // se mantiene publico por compatibilidad con la clase anterior

    private readonly int _widthInCells;
    private readonly int _heightInCells;
    private readonly float _originX;
    private readonly float _originY;

    private readonly int _chunkSize;   // celdas por lado del chunk (potencia de 2)
    private readonly int _chunkShift;
    private readonly int _chunkMask;
    private readonly int _cellsPerChunk;
    private readonly int _chunksW;
    private readonly int _chunksH;

    private readonly Chunk?[] _chunks;
    private readonly int[] _scratch; // contadores temporales para reconstruir

    // ---------------- Datos por entidad (crecen bajo demanda) ----------------
    private byte[] _flags;
    private ushort[] _teams;
    private int[] _visited;
    private int[] _pendingRefs;   // chunks que aun contienen a esta entidad ya muerta
    private CellBounds[] _bounds;
    private T[] _values;

    private readonly Stack<int> _freeIds = new();
    private int _nextEntityId = 1; // 0 = invalido
    private int _currentQueryId;

    /// <summary>
    /// Estado scratch de una consulta. Cada hilo debe tener el suyo.
    /// Pasalo a las sobrecargas que lo reciben para consultar en paralelo sin
    /// compartir el contador ni el array de visitados del grid.
    /// </summary>
    public sealed class QueryContext
    {
        internal int QueryId;
        internal int[] Visited = Array.Empty<int>();
    }

    /// <summary>Crea un contexto de consulta para el hilo actual.</summary>
    public QueryContext CreateQueryContext() => new QueryContext();

    public int AllocatedChunks
    {
        get
        {
            int n = 0;
            for (int i = 0; i < _chunks.Length; i++)
                if (_chunks[i] != null) n++;
            return n;
        }
    }

    public StaticSpatialChunkGrid(
        int worldWidth,
        int worldHeight,
        float cellSize,
        int initialEntityCapacity = 1024,
        int chunkSizeInCells = 16)
    {
        if (worldWidth <= 0) throw new ArgumentOutOfRangeException(nameof(worldWidth));
        if (worldHeight <= 0) throw new ArgumentOutOfRangeException(nameof(worldHeight));
        if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));
        if (chunkSizeInCells <= 0 || (chunkSizeInCells & (chunkSizeInCells - 1)) != 0)
            throw new ArgumentException("chunkSizeInCells debe ser potencia de 2", nameof(chunkSizeInCells));

        _cellSize = cellSize;
        _originX = worldWidth * 0.5f;
        _originY = worldHeight * 0.5f;

        _widthInCells = (int)MathF.Ceiling(worldWidth / cellSize);
        _heightInCells = (int)MathF.Ceiling(worldHeight / cellSize);

        _chunkSize = chunkSizeInCells;
        _chunkShift = BitOperations.Log2((uint)chunkSizeInCells);
        _chunkMask = chunkSizeInCells - 1;
        _cellsPerChunk = chunkSizeInCells * chunkSizeInCells;

        _chunksW = (_widthInCells + chunkSizeInCells - 1) >> _chunkShift;
        _chunksH = (_heightInCells + chunkSizeInCells - 1) >> _chunkShift;

        _chunks = new Chunk?[_chunksW * _chunksH];
        _scratch = new int[_cellsPerChunk];

        int cap = Math.Max(initialEntityCapacity, 16) + 1;
        _flags = new byte[cap];
        _teams = new ushort[cap];
        _visited = new int[cap];
        _pendingRefs = new int[cap];
        _bounds = new CellBounds[cap];
        _values = new T[cap];
    }

    // ============================================================
    // IDS / CAPACIDAD
    // ============================================================

    private int GetNewEntityId()
    {
        int id = _freeIds.Count > 0 ? _freeIds.Pop() : _nextEntityId++;
        EnsureEntityCapacity(id);
        return id;
    }

    private void EnsureEntityCapacity(int id)
    {
        if (id < _flags.Length) return;

        int newLen = Math.Max(id + 1, _flags.Length * 2);
        Array.Resize(ref _flags, newLen);
        Array.Resize(ref _teams, newLen);
        Array.Resize(ref _visited, newLen);
        Array.Resize(ref _pendingRefs, newLen);
        Array.Resize(ref _bounds, newLen);
        Array.Resize(ref _values, newLen);
    }

    // ============================================================
    // CONVERSIONES
    // ============================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WorldToCell(float worldX, float worldY, out int cx, out int cy)
    {
        cx = (int)MathF.Floor((worldX + _originX) / _cellSize);
        cy = (int)MathF.Floor((worldY + _originY) / _cellSize);
    }

    // ============================================================
    // REGISTER
    // ============================================================

    /// <summary>Registra un objeto estatico y devuelve su ID. No reconstruye nada todavia.</summary>
    public int RegisterStatic(
        T value,
        float minX, float minY, float maxX, float maxY,
        ushort team = 0)
    {
        int id = GetNewEntityId();

        WorldToCell(minX, minY, out int x0, out int y0);
        WorldToCell(maxX, maxY, out int x1, out int y1);

        x0 = Math.Max(0, x0);
        y0 = Math.Max(0, y0);
        x1 = Math.Min(_widthInCells - 1, x1);
        y1 = Math.Min(_heightInCells - 1, y1);

        _values[id] = value;
        _teams[id] = team;
        _visited[id] = 0;
        _pendingRefs[id] = 0;

        // Fuera del mundo: queda registrado (TryGetValue funciona) pero sin celdas.
        if (x1 < x0 || y1 < y0)
        {
            _bounds[id] = new CellBounds { MinX = 1, MinY = 1, MaxX = 0, MaxY = 0 };
            _flags[id] = FlagAlive;
            return id;
        }

        _bounds[id] = new CellBounds { MinX = x0, MinY = y0, MaxX = x1, MaxY = y1 };

        bool multi = x0 != x1 || y0 != y1;
        _flags[id] = (byte)(FlagAlive | (multi ? FlagMulti : 0));

        int cx0 = x0 >> _chunkShift, cx1 = x1 >> _chunkShift;
        int cy0 = y0 >> _chunkShift, cy1 = y1 >> _chunkShift;

        for (int cy = cy0; cy <= cy1; cy++)
        {
            for (int cx = cx0; cx <= cx1; cx++)
            {
                int ci = cx + cy * _chunksW;
                Chunk c = _chunks[ci] ??= new Chunk(ci, cx, cy, _cellsPerChunk);
                AddMember(c, id);
            }
        }

        return id;
    }

    private static void AddMember(Chunk c, int id)
    {
        if (c.MemberCount == c.Members.Length)
            Array.Resize(ref c.Members, c.Members.Length * 2);

        c.Members[c.MemberCount++] = id;
        c.Dirty = true;
    }

    // ============================================================
    // REMOVE (marcar como muerto)
    // ============================================================

    /// <summary>O(1) + una vuelta por los chunks que ocupaba. No reconstruye (salvo umbral, de forma diferida).</summary>
    public void FreeCollider(int id)
    {
        if (id <= 0 || id >= _flags.Length || (_flags[id] & FlagAlive) == 0)
            return;

        _flags[id] = 0;
        _values[id] = default!;

        CellBounds b = _bounds[id];
        int refs = 0;

        if (b.MaxX >= b.MinX && b.MaxY >= b.MinY)
        {
            int cx0 = b.MinX >> _chunkShift, cx1 = b.MaxX >> _chunkShift;
            int cy0 = b.MinY >> _chunkShift, cy1 = b.MaxY >> _chunkShift;

            for (int cy = cy0; cy <= cy1; cy++)
            {
                for (int cx = cx0; cx <= cx1; cx++)
                {
                    Chunk? c = _chunks[cx + cy * _chunksW];
                    if (c == null) continue;

                    refs++;
                    c.DeadCount++;

                    if (!c.Dirty && ShouldCompact(c))
                        c.Dirty = true; // se compacta en la proxima consulta o Flush()
                }
            }
        }

        if (refs == 0)
            _freeIds.Push(id);          // nadie lo referencia: reutilizable ya
        else
            _pendingRefs[id] = refs;    // se libera cuando todos sus chunks se compacten
    }

    /// <summary>Compatibilidad con la API anterior (los limites ya no hacen falta).</summary>
    public void UnregisterStatic(int id, float minX, float minY, float maxX, float maxY)
        => FreeCollider(id);

    // ============================================================
    // REGISTER / FREE INMEDIATOS
    // Dejan los chunks afectados reconstruidos para poder consultar en paralelo.
    // No llamar mientras haya consultas ejecutandose: son operaciones de escritura.
    // ============================================================

    /// <summary>
    /// Registra y reconstruye en el acto los chunks afectados.
    /// Al volver, esos chunks quedan limpios y listos para consultas paralelas.
    /// </summary>
    public int RegisterStaticSafe(
        T value,
        float minX, float minY, float maxX, float maxY,
        ushort team = 0)
    {
        int id = RegisterStatic(value, minX, minY, maxX, maxY, team);
        RebuildChunksForWorldBounds(minX, minY, maxX, maxY, onlyDirty: true);
        return id;
    }

    /// <summary>
    /// Elimina y compacta en el acto los chunks afectados.
    /// Al volver, el ID queda liberado (si correspondia) y esos chunks quedan limpios.
    /// </summary>
    public void FreeColliderSafe(int id)
    {
        if (!IsAlive(id))
            return;

        CellBounds bounds = _bounds[id];
        FreeCollider(id);

        if (bounds.MaxX >= bounds.MinX && bounds.MaxY >= bounds.MinY)
            RebuildChunksForCellBounds(bounds.MinX, bounds.MinY, bounds.MaxX, bounds.MaxY, onlyDirty: false);
    }

    private void RebuildChunksForWorldBounds(
        float minX, float minY, float maxX, float maxY, bool onlyDirty)
    {
        WorldToCell(minX, minY, out int x0, out int y0);
        WorldToCell(maxX, maxY, out int x1, out int y1);

        x0 = Math.Max(0, x0);
        y0 = Math.Max(0, y0);
        x1 = Math.Min(_widthInCells - 1, x1);
        y1 = Math.Min(_heightInCells - 1, y1);

        if (x1 < x0 || y1 < y0)
            return;

        RebuildChunksForCellBounds(x0, y0, x1, y1, onlyDirty);
    }

    private void RebuildChunksForCellBounds(
        int minX, int minY, int maxX, int maxY, bool onlyDirty)
    {
        int cx0 = minX >> _chunkShift, cx1 = maxX >> _chunkShift;
        int cy0 = minY >> _chunkShift, cy1 = maxY >> _chunkShift;

        for (int cy = cy0; cy <= cy1; cy++)
        {
            for (int cx = cx0; cx <= cx1; cx++)
            {
                Chunk? chunk = _chunks[cx + cy * _chunksW];
                if (chunk == null)
                    continue;

                if (onlyDirty && !chunk.Dirty)
                    continue;

                RebuildChunk(chunk);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldCompact(Chunk c)
    {
        return c.DeadCount == c.MemberCount ||
               (c.DeadCount >= MinDeadForCompaction &&
                c.DeadCount * DeadRatioDivisor >= c.MemberCount);
    }

    // ============================================================
    // BUILD / FLUSH / COMPACT
    // ============================================================

    /// <summary>Reconstruye todos los chunks sucios. Llamar tras una carga masiva para evitar picos en la primera consulta.</summary>
    public void Flush()
    {
        for (int i = 0; i < _chunks.Length; i++)
        {
            Chunk? c = _chunks[i];
            if (c != null && c.Dirty)
                RebuildChunk(c);
        }
    }

    /// <summary>Fuerza la compactacion de cualquier chunk con muertos (libera memoria e IDs).</summary>
    public void Compact()
    {
        for (int i = 0; i < _chunks.Length; i++)
        {
            Chunk? c = _chunks[i];
            if (c != null && (c.Dirty || c.DeadCount > 0))
                RebuildChunk(c);
        }
    }

    /// <summary>Devuelve false si el chunk quedo vacio y fue liberado.</summary>
    private bool RebuildChunk(Chunk c)
    {
        c.Dirty = false;

        // 1) Compactar miembros: descartar muertos y liberar sus referencias.
        int[] members = c.Members;
        int alive = 0;

        for (int i = 0; i < c.MemberCount; i++)
        {
            int id = members[i];

            if ((_flags[id] & FlagAlive) != 0)
                members[alive++] = id;
            else if (--_pendingRefs[id] == 0)
                _freeIds.Push(id);
        }

        c.MemberCount = alive;
        c.DeadCount = 0;

        if (alive == 0)
        {
            _chunks[c.Index] = null; // chunk vacio: memoria devuelta
            return false;
        }

        int shift = _chunkShift;
        int baseX = c.ChunkX << shift;
        int baseY = c.ChunkY << shift;
        int lastX = baseX + _chunkSize - 1;
        int lastY = baseY + _chunkSize - 1;

        int[] start = c.CellStart;
        CellBounds[] bounds = _bounds;

        // 2) Contar entidades por celda (en start[cell + 1]).
        Array.Clear(start, 0, start.Length);

        for (int i = 0; i < alive; i++)
        {
            ref CellBounds b = ref bounds[members[i]];

            int x0 = Math.Max(b.MinX, baseX) - baseX;
            int x1 = Math.Min(b.MaxX, lastX) - baseX;
            int y0 = Math.Max(b.MinY, baseY) - baseY;
            int y1 = Math.Min(b.MaxY, lastY) - baseY;

            for (int y = y0; y <= y1; y++)
            {
                int row = (y << shift) + 1;
                for (int x = x0; x <= x1; x++)
                    start[row + x]++;
            }
        }

        // 3) Sumas acumuladas -> cellStart.
        int cells = _cellsPerChunk;
        for (int i = 0; i < cells; i++)
            start[i + 1] += start[i];

        int total = start[cells];

        // 4) Buffer de entradas (reutiliza si cabe y no esta enorme).
        int[] entries = c.CellEntities;
        if (entries.Length < total || entries.Length > total * 4 + 64)
            entries = c.CellEntities = new int[total + (total >> 2) + 8];

        // 5) Rellenar.
        Array.Copy(start, _scratch, cells);
        int[] cursor = _scratch;

        for (int i = 0; i < alive; i++)
        {
            int id = members[i];
            ref CellBounds b = ref bounds[id];

            int x0 = Math.Max(b.MinX, baseX) - baseX;
            int x1 = Math.Min(b.MaxX, lastX) - baseX;
            int y0 = Math.Max(b.MinY, baseY) - baseY;
            int y1 = Math.Min(b.MaxY, lastY) - baseY;

            for (int y = y0; y <= y1; y++)
            {
                int row = y << shift;
                for (int x = x0; x <= x1; x++)
                    entries[cursor[row + x]++] = id;
            }
        }

        return true;
    }

    // ============================================================
    // ACCESO A CELDAS (nucleo de las consultas)
    // ============================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryGetCell(int cx, int cy, bool allowRebuild, out int[] entities, out int start, out int end)
    {
        entities = null!;
        start = 0;
        end = 0;

        if ((uint)cx >= (uint)_widthInCells || (uint)cy >= (uint)_heightInCells)
            return false;

        Chunk? c = _chunks[(cx >> _chunkShift) + (cy >> _chunkShift) * _chunksW];
        if (c == null)
            return false;

        if (c.Dirty)
        {
            if (!allowRebuild)
                throw new InvalidOperationException(
                    "Chunk dirty durante una consulta paralela. " +
                    "Llama a Flush() o usa RegisterStaticSafe/FreeColliderSafe antes de consultar en paralelo.");

            if (!RebuildChunk(c))
                return false;
        }

        int local = (cx & _chunkMask) + ((cy & _chunkMask) << _chunkShift);
        start = c.CellStart[local];
        end = c.CellStart[local + 1];

        if (start == end)
            return false;

        entities = c.CellEntities;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NextQueryId()
    {
        if (++_currentQueryId == int.MaxValue)
        {
            Array.Fill(_visited, 0);
            _currentQueryId = 1;
        }
        return _currentQueryId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int NextQueryId(QueryContext context)
    {
        if (context.Visited.Length < _flags.Length)
            Array.Resize(ref context.Visited, _flags.Length);

        if (++context.QueryId == int.MaxValue)
        {
            Array.Clear(context.Visited, 0, context.Visited.Length);
            context.QueryId = 1;
        }

        return context.QueryId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int MaxUsefulRadius(int cx, int cy)
    {
        return Math.Max(
            Math.Max(cx, _widthInCells - 1 - cx),
            Math.Max(cy, _heightInCells - 1 - cy));
    }

    // ============================================================
    // QUERY NON ALLOC (camino rapido, por anillos desde el centro)
    // ============================================================

    /// <summary>
    /// Devuelve IDs unicos ordenados por anillos desde la celda central hacia afuera.
    /// Sin asignaciones. Para en cuanto results se llena.
    /// No es thread-safe: usa el scratch interno del grid. Para consultas en paralelo
    /// usa la sobrecarga que recibe un QueryContext.
    /// </summary>
    public int QueryNearbyUniqueNonAlloc(
        float worldX, float worldY, int radius,
        Span<int> results,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (radius < 0 || results.Length == 0) return 0;

        return QueryNearbyUniqueCore(
            worldX, worldY, radius, results,
            NextQueryId(), _visited,
            teamToIgnore, allowRebuild: true);
    }

    /// <summary>
    /// Igual que la anterior, pero usando un contexto por hilo. Es la version segura
    /// para consultas en paralelo, siempre que no haya escrituras concurrentes.
    /// </summary>
    public int QueryNearbyUniqueNonAlloc(
        float worldX, float worldY, int radius,
        Span<int> results,
        QueryContext context,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (radius < 0 || results.Length == 0) return 0;

        return QueryNearbyUniqueCore(
            worldX, worldY, radius, results,
            NextQueryId(context), context.Visited,
            teamToIgnore, allowRebuild: false);
    }

    private int QueryNearbyUniqueCore(
        float worldX, float worldY, int radius,
        Span<int> results,
        int qid, int[] visited,
        ushort teamToIgnore, bool allowRebuild)
    {
        WorldToCell(worldX, worldY, out int cx, out int cy);
        radius = Math.Min(radius, MaxUsefulRadius(cx, cy));

        int count = 0;

        if (CollectCell(cx, cy, teamToIgnore, qid, visited, allowRebuild, results, ref count)) return count;

        for (int r = 1; r <= radius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                if (CollectCell(cx + dx, cy - r, teamToIgnore, qid, visited, allowRebuild, results, ref count)) return count;
                if (CollectCell(cx + dx, cy + r, teamToIgnore, qid, visited, allowRebuild, results, ref count)) return count;
            }
            for (int dy = -r + 1; dy <= r - 1; dy++)
            {
                if (CollectCell(cx - r, cy + dy, teamToIgnore, qid, visited, allowRebuild, results, ref count)) return count;
                if (CollectCell(cx + r, cy + dy, teamToIgnore, qid, visited, allowRebuild, results, ref count)) return count;
            }
        }

        return count;
    }

    /// <summary>
    /// Consulta por rectangulo. No es thread-safe: usa la sobrecarga con QueryContext
    /// para consultas en paralelo.
    /// </summary>
    public int QueryRectNonAlloc(
        float minX, float minY, float maxX, float maxY,
        Span<int> results,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (results.Length == 0) return 0;

        return QueryRectCore(
            minX, minY, maxX, maxY, results,
            NextQueryId(), _visited,
            teamToIgnore, allowRebuild: true);
    }

    /// <summary>Version segura para consultas en paralelo usando un contexto por hilo.</summary>
    public int QueryRectNonAlloc(
        float minX, float minY, float maxX, float maxY,
        Span<int> results,
        QueryContext context,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (results.Length == 0) return 0;

        return QueryRectCore(
            minX, minY, maxX, maxY, results,
            NextQueryId(context), context.Visited,
            teamToIgnore, allowRebuild: false);
    }

    private int QueryRectCore(
        float minX, float minY, float maxX, float maxY,
        Span<int> results,
        int qid, int[] visited,
        ushort teamToIgnore, bool allowRebuild)
    {
        WorldToCell(minX, minY, out int x0, out int y0);
        WorldToCell(maxX, maxY, out int x1, out int y1);

        x0 = Math.Max(0, x0);
        y0 = Math.Max(0, y0);
        x1 = Math.Min(_widthInCells - 1, x1);
        y1 = Math.Min(_heightInCells - 1, y1);

        if (x1 < x0 || y1 < y0) return 0;

        int count = 0;

        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                if (CollectCell(x, y, teamToIgnore, qid, visited, allowRebuild, results, ref count))
                    return count;

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CollectCell(
        int cx, int cy,
        ushort teamToIgnore,
        int qid,
        int[] visited,
        bool allowRebuild,
        Span<int> results,
        ref int count)
    {
        if (!TryGetCell(cx, cy, allowRebuild, out int[] entities, out int start, out int end))
            return false;

        byte[] flags = _flags;
        ushort[] teams = _teams;

        for (int i = start; i < end; i++)
        {
            int id = entities[i];
            byte f = flags[id];

            if ((f & FlagAlive) == 0) continue;      // muerto sin compactar
            if (teams[id] == teamToIgnore) continue; // filtro de equipo

            // Solo las entidades multi-celda necesitan dedupe.
            if ((f & FlagMulti) != 0)
            {
                if (visited[id] == qid) continue;
                visited[id] = qid;
            }

            results[count++] = id;
            if (count == results.Length) return true;
        }

        return false;
    }

    // ============================================================
    // QUERY IEnumerable (comodas, pero asignan; no usar en codigo caliente)
    // ============================================================

    /// <summary>
    /// No es thread-safe: usa el scratch interno del grid. Para consultas en paralelo
    /// usa la sobrecarga que recibe un QueryContext.
    /// </summary>
    public IEnumerable<int> QueryNearbyUnique(
        float worldX, float worldY, int radius,
        ushort teamToIgnore = ushort.MaxValue)
    {
        return QueryNearbyUniqueIterator(
            worldX, worldY, radius,
            NextQueryId(), _visited,
            teamToIgnore, allowRebuild: true);
    }

    /// <summary>Version con contexto por hilo para consultas en paralelo.</summary>
    public IEnumerable<int> QueryNearbyUnique(
        float worldX, float worldY, int radius,
        QueryContext context,
        ushort teamToIgnore = ushort.MaxValue)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        return QueryNearbyUniqueIterator(
            worldX, worldY, radius,
            NextQueryId(context), context.Visited,
            teamToIgnore, allowRebuild: false);
    }

    private IEnumerable<int> QueryNearbyUniqueIterator(
        float worldX, float worldY, int radius,
        int qid, int[] visited,
        ushort teamToIgnore, bool allowRebuild)
    {
        if (radius < 0) yield break;

        WorldToCell(worldX, worldY, out int cx, out int cy);
        radius = Math.Min(radius, MaxUsefulRadius(cx, cy));

        for (int r = 0; r <= radius; r++)
        {
            int cellsInRing = r == 0 ? 1 : 8 * r;

            for (int i = 0; i < cellsInRing; i++)
            {
                RingOffset(r, i, out int dx, out int dy);

                if (!TryGetCell(cx + dx, cy + dy, allowRebuild, out int[] entities, out int start, out int end))
                    continue;

                for (int k = start; k < end; k++)
                {
                    int id = entities[k];
                    byte f = _flags[id];

                    if ((f & FlagAlive) == 0 || _teams[id] == teamToIgnore) continue;

                    if ((f & FlagMulti) != 0)
                    {
                        if (visited[id] == qid) continue;
                        visited[id] = qid;
                    }

                    yield return id;
                }
            }
        }
    }

    public IEnumerable<T> QueryNearbyValues(
        float worldX, float worldY, int radius,
        ushort teamToIgnore = ushort.MaxValue)
    {
        foreach (int id in QueryNearbyUnique(worldX, worldY, radius, teamToIgnore))
        {
            if (TryGetValue(id, out T value))
                yield return value;
        }
    }

    public IEnumerable<T> QueryNearbyValues(
        float worldX, float worldY, int radius,
        QueryContext context,
        ushort teamToIgnore = ushort.MaxValue)
    {
        foreach (int id in QueryNearbyUnique(worldX, worldY, radius, context, teamToIgnore))
        {
            if (TryGetValue(id, out T value))
                yield return value;
        }
    }

    // Posicion i (0..8r-1) dentro del anillo r, como offset respecto al centro.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void RingOffset(int r, int i, out int dx, out int dy)
    {
        int row = 2 * r + 1;

        if (i < row) { dx = -r + i; dy = -r; return; }   // borde superior
        i -= row;

        if (i < row) { dx = -r + i; dy = r; return; }    // borde inferior
        i -= row;

        int side = 2 * r - 1;
        if (i < side) { dx = -r; dy = -r + 1 + i; return; } // lado izquierdo
        i -= side;

        dx = r; dy = -r + 1 + i;                            // lado derecho
    }

    // ============================================================
    // GET VALUE
    // ============================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(int id, out T value)
    {
        if (id <= 0 || id >= _flags.Length || (_flags[id] & FlagAlive) == 0)
        {
            value = default!;
            return false;
        }

        value = _values[id];
        return true;
    }

    public bool IsAlive(int id)
        => id > 0 && id < _flags.Length && (_flags[id] & FlagAlive) != 0;

    public ushort GetTeam(int id)
        => id > 0 && id < _teams.Length ? _teams[id] : (ushort)0;

    // ============================================================
    // CLEAR
    // ============================================================

    public void Clear()
    {
        Array.Clear(_chunks);
        Array.Clear(_flags);
        Array.Clear(_teams);
        Array.Clear(_visited);
        Array.Clear(_pendingRefs);
        Array.Clear(_bounds);
        Array.Clear(_values);

        _freeIds.Clear();
        _nextEntityId = 1;
        _currentQueryId = 0;
    }
}