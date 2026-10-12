using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;

namespace GodotEcsArch.sources.BlackyEngine.GridSparse;

public class BlackyConcurrentDynamicSparseGrid
{
    private const float PixelsPerUnit = 0.03125f; // 1 px = 1/32 unidad

    public readonly float CellSize;   // tamaño de celda en unidades del mundo
    public readonly int GridWidth;
    public readonly int GridHeight;
    public readonly int HalfGridWidth;
    public readonly int HalfGridHeight;

    private readonly float _invCell;  // 1 / CellSize (multiplicar es más barato que dividir)
    private readonly int[] _grid;
    private readonly int[] _occupiedIndices;
    private readonly bool _persistent;
    private int _occupiedCount = 0;

    /// <param name="worldWidth">Ancho del mundo en UNIDADES (no en píxeles).</param>
    /// <param name="worldHeight">Alto del mundo en unidades.</param>
    /// <param name="cellSizePixel">Tamaño de celda en píxeles: 16 = 0.5 u, 32 = 1.0 u.</param>
    /// <param name="maxOccupiedCells">Capacidad de la lista de limpieza por frame.</param>
    /// <param name="persistent">
    /// true: capa que NO se limpia por frame (unidades asentadas). No lleva lista
    /// de limpieza, y las celdas se liberan explícitamente con ReleaseCell.
    /// </param>
    public BlackyConcurrentDynamicSparseGrid(
        float worldWidth,
        float worldHeight,
        int cellSizePixel,
        int maxOccupiedCells = 150000,
        bool persistent = false)
    {
        CellSize = PixelsPerUnit * cellSizePixel;
        _invCell = CellSize;
        _persistent = persistent;
        int ws = 16 / cellSizePixel;
        GridWidth = Mathf.CeilToInt(worldWidth * ws);
        GridHeight = Mathf.CeilToInt(worldHeight * ws);

        HalfGridWidth = GridWidth / 2;
        HalfGridHeight = GridHeight / 2;

        _grid = new int[GridWidth * GridHeight];
        _occupiedIndices = persistent ? Array.Empty<int>() : new int[maxOccupiedCells];
    }

    // =========================================================================
    // COORDENADAS
    // =========================================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndex(in Vector2 worldPos)
    {
        int cx = Mathf.FloorToInt(worldPos.X / _invCell) + HalfGridWidth;
        int cy = Mathf.FloorToInt(worldPos.Y / _invCell) + HalfGridHeight;

        if (cx < 0 || cx >= GridWidth || cy < 0 || cy >= GridHeight)
            return -1;

        return cy * GridWidth + cx;
    }

    // =========================================================================
    // MODO "OCUPADO / LIBRE" (terreno, unidades individuales)
    // Cualquier valor != 0 cuenta como bloqueado.
    // =========================================================================

    public void OccupyCell(in Vector2 worldPos, int value = 1)
    {
        int index = GetIndex(worldPos);
        if (index == -1) return;

        if (Interlocked.CompareExchange(ref _grid[index], value, 0) == 0)
            RegisterOccupied(index);
    }

    public void OccupyArea(in Vector2 worldPos, float radius, int value = 1)
    {
        int minCx = Math.Max(0, Mathf.FloorToInt((worldPos.X - radius) / _invCell) + HalfGridWidth);
        int maxCx = Math.Min(GridWidth - 1, Mathf.FloorToInt((worldPos.X + radius) / _invCell) + HalfGridWidth);
        int minCy = Math.Max(0, Mathf.FloorToInt((worldPos.Y - radius) / _invCell) + HalfGridHeight);
        int maxCy = Math.Min(GridHeight - 1, Mathf.FloorToInt((worldPos.Y + radius) / _invCell) + HalfGridHeight);

        for (int cy = minCy; cy <= maxCy; cy++)
        {
            int rowOffset = cy * GridWidth;
            for (int cx = minCx; cx <= maxCx; cx++)
            {
                int index = rowOffset + cx;
                if (Interlocked.CompareExchange(ref _grid[index], value, 0) == 0)
                    RegisterOccupied(index);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlockedIndex(int index) => index == -1 || _grid[index] != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked(in Vector2 worldPos) => IsBlockedIndex(GetIndex(worldPos));

    // =========================================================================
    // MODO "CAPACIDAD" (hordas). El valor de la celda es un contador.
    // No mezclar con OccupyCell/IsBlocked en la misma instancia.
    // =========================================================================

    /// <summary>
    /// Intenta entrar a la celda. Devuelve true si había lugar (count &lt;= capacity).
    /// Seguro entre hilos. capacity = 1 equivale a exclusividad estricta.
    /// </summary>
    public bool TryEnterCell(int index, int capacity)
    {
        if (index < 0) return false;

        int n = Interlocked.Increment(ref _grid[index]);

        if (n > capacity)
        {
            Interlocked.Decrement(ref _grid[index]);
            return false;
        }

        if (n == 1) RegisterOccupied(index);
        return true;
    }

    public bool TryEnterCell(Vector2 worldPos, int capacity)
    {
        int index = GetIndex(worldPos);
        return TryEnterCell(index, capacity);        
    }

    /// <summary>
    /// Libera un lugar de la celda. Úsalo cuando una unidad se va de una celda
    /// que había reservado, o en la capa persistente al reactivarse/morir.
    /// </summary>
    public void ReleaseCell(int index)
    {
        if (index < 0) return;

        int n = Interlocked.Decrement(ref _grid[index]);
        if (n < 0) Interlocked.Increment(ref _grid[index]); // protección ante doble liberación
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCount(int index) => index < 0 ? int.MaxValue : _grid[index];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsFull(int index, int capacity) => index < 0 || _grid[index] >= capacity;

    // =========================================================================
    // LIMPIEZA
    // =========================================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RegisterOccupied(int index)
    {
        if (_persistent) return; // la capa persistente no se limpia por frame

        int slot = Interlocked.Increment(ref _occupiedCount) - 1;
        if (slot < _occupiedIndices.Length)
            _occupiedIndices[slot] = index;
    }

    /// <summary>
    /// Limpia las celdas ocupadas desde el último frame. Llamar una vez por frame,
    /// desde un sistema single-thread y ANTES del steering.
    /// </summary>
    public void ClearOccupiedOnly()
    {
        if (_persistent) return;

        if (_occupiedCount > _occupiedIndices.Length)
        {
            // Desborde de la lista: no sabemos cuáles celdas quedaron, limpiamos todo.
            // Si esto aparece seguido, sube maxOccupiedCells.
            Array.Clear(_grid);
        }
        else
        {
            for (int i = 0; i < _occupiedCount; i++)
                _grid[_occupiedIndices[i]] = 0;
        }

        _occupiedCount = 0;
    }
}