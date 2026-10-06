

namespace GodotEcsArch.sources.BlackyEngine.GridSparse;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Godot;

public class BlackyConcurrentDynamicSparseGrid
{
    public readonly float InvCellSize;
    public readonly int GridWidth;
    public readonly int GridHeight;
    public readonly int HalfGridWidth;
    public readonly int HalfGridHeight;

    private readonly int[] _grid;
    private readonly int[] _occupiedIndices;
    private int _occupiedCount = 0;

    public BlackyConcurrentDynamicSparseGrid(float worldWidth, float worldHeight, float minCellSize, int maxOccupiedCells = 150000)
    {
        InvCellSize = 1.0f / minCellSize;
        GridWidth = Mathf.CeilToInt(worldWidth * InvCellSize);
        GridHeight = Mathf.CeilToInt(worldHeight * InvCellSize);

        HalfGridWidth = GridWidth / 2;
        HalfGridHeight = GridHeight / 2;

        _grid = new int[GridWidth * GridHeight];
        _occupiedIndices = new int[maxOccupiedCells];
    }

    public void OccupyCell(in Vector2 worldPos, int value = 1)
    {
        int index = GetIndex(worldPos);
        if (index == -1) return;

        if (Interlocked.CompareExchange(ref _grid[index], value, 0) == 0)
        {
            int slot = Interlocked.Increment(ref _occupiedCount) - 1;
            if (slot < _occupiedIndices.Length)
            {
                _occupiedIndices[slot] = index;
            }
        }
    }

    public void OccupyArea(in Vector2 worldPos, float radius, int value = 1)
    {
        float minWorldX = worldPos.X - radius;
        float maxWorldX = worldPos.X + radius;
        float minWorldY = worldPos.Y - radius;
        float maxWorldY = worldPos.Y + radius;

        int minCx = Math.Max(0, Mathf.FloorToInt(minWorldX * InvCellSize) + HalfGridWidth);
        int maxCx = Math.Min(GridWidth - 1, Mathf.FloorToInt(maxWorldX * InvCellSize) + HalfGridWidth);
        int minCy = Math.Max(0, Mathf.FloorToInt(minWorldY * InvCellSize) + HalfGridHeight);
        int maxCy = Math.Min(GridHeight - 1, Mathf.FloorToInt(maxWorldY * InvCellSize) + HalfGridHeight);

        for (int cy = minCy; cy <= maxCy; cy++)
        {
            int rowOffset = cy * GridWidth;
            for (int cx = minCx; cx <= maxCx; cx++)
            {
                int index = rowOffset + cx;
                if (Interlocked.CompareExchange(ref _grid[index], value, 0) == 0)
                {
                    int slot = Interlocked.Increment(ref _occupiedCount) - 1;
                    if (slot < _occupiedIndices.Length)
                    {
                        _occupiedIndices[slot] = index;
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetIndex(in Vector2 worldPos)
    {
        int cx = Mathf.FloorToInt(worldPos.X * InvCellSize) + HalfGridWidth;
        int cy = Mathf.FloorToInt(worldPos.Y * InvCellSize) + HalfGridHeight;

        if (cx < 0 || cx >= GridWidth || cy < 0 || cy >= GridHeight)
            return -1;

        return cy * GridWidth + cx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlockedIndex(int index) => index == -1 || _grid[index] != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBlocked(in Vector2 worldPos) => IsBlockedIndex(GetIndex(worldPos));

    public void ClearOccupiedOnly()
    {
        for (int i = 0; i < _occupiedCount; i++)
        {
            _grid[_occupiedIndices[i]] = 0;
        }
        _occupiedCount = 0;
    }
}