

namespace GodotEcsArch.sources.BlackyEngine.GridSparse;

using System;
using System.Runtime.CompilerServices;
using Godot;

public class BlackyStaticGridLayer
{
    public readonly float InvCellSize;
    public readonly int GridWidth;
    public readonly int GridHeight;
    public readonly int HalfGridWidth;
    public readonly int HalfGridHeight;

    private readonly byte[] _grid;

    public BlackyStaticGridLayer(float worldWidth, float worldHeight, float minCellSizePixel)
    {
        InvCellSize = 0.5f;
        GridWidth = Mathf.CeilToInt(worldWidth );
        GridHeight = Mathf.CeilToInt(worldHeight);

        HalfGridWidth = GridWidth / 2;
        HalfGridHeight = GridHeight / 2;

        _grid = new byte[GridWidth * GridHeight];
    }

    /// 
    /// Limpia un área rectangular de la grilla estática.
    /// 
    public void ClearRegion(Vector2 worldMin, Vector2 worldSize)
    {
        int minCx = Math.Max(0, Mathf.FloorToInt(worldMin.X * InvCellSize) + HalfGridWidth);
        int maxCx = Math.Min(GridWidth - 1, Mathf.FloorToInt((worldMin.X + worldSize.X) * InvCellSize) + HalfGridWidth);
        int minCy = Math.Max(0, Mathf.FloorToInt(worldMin.Y * InvCellSize) + HalfGridHeight);
        int maxCy = Math.Min(GridHeight - 1, Mathf.FloorToInt((worldMin.Y + worldSize.Y) * InvCellSize) + HalfGridHeight);

        for (int cy = minCy; cy < maxCy; cy++)
        {
            int rowOffset = cy * GridWidth;
            for (int cx = minCx; cx < maxCx; cx++)
            {
                _grid[rowOffset + cx] = 0;
            }
        }
    }

    /// 
    /// Carga la matriz del mapa base de un mini-mundo sobre el atlas.
    /// 
    public void LoadRegionData(Vector2 worldMinOffset, byte[,] obstacleMatrix)
    {
        int cols = obstacleMatrix.GetLength(0);
        int rows = obstacleMatrix.GetLength(1);

        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                byte tileValue = obstacleMatrix[x, y];
                if (tileValue != 0)
                {
                    Vector2 tileWorldPos = worldMinOffset + new Vector2(x, y);
                    int index = GetIndex(tileWorldPos);
                    if (index != -1)
                    {
                        _grid[index] = tileValue;
                    }
                }
            }
        }
    }

    /// 
    /// Coloca un objeto estático o edificio (torres, rocas, murallas) en coordenadas del atlas.
    /// 
    public void PlaceStaticObject(Vector2 worldCenterPos, Vector2 worldSize, byte obstacleType = 1)
    {
        ModifyBuildingFootprint(worldCenterPos, worldSize, obstacleType);
    }

    /// 
    /// Remueve un objeto estático o edificio destruido.
    /// 
    public void RemoveStaticObject(Vector2 worldCenterPos, Vector2 worldSize)
    {
        ModifyBuildingFootprint(worldCenterPos, worldSize, value: 0);
    }

    private void ModifyBuildingFootprint(Vector2 worldCenterPos, Vector2 worldSize, byte value)
    {
        Vector2 halfSize = worldSize * 0.5f;

        float minWorldX = worldCenterPos.X - halfSize.X;
        float maxWorldX = worldCenterPos.X + halfSize.X;
        float minWorldY = worldCenterPos.Y - halfSize.Y;
        float maxWorldY = worldCenterPos.Y + halfSize.Y;

        int minCx = Math.Max(0, Mathf.FloorToInt(minWorldX * InvCellSize) + HalfGridWidth);
        int maxCx = Math.Min(GridWidth - 1, Mathf.FloorToInt(maxWorldX * InvCellSize) + HalfGridWidth);
        int minCy = Math.Max(0, Mathf.FloorToInt(minWorldY * InvCellSize) + HalfGridHeight);
        int maxCy = Math.Min(GridHeight - 1, Mathf.FloorToInt(maxWorldY * InvCellSize) + HalfGridHeight);

        for (int cy = minCy; cy <= maxCy; cy++)
        {
            int rowOffset = cy * GridWidth;
            for (int cx = minCx; cx <= maxCx; cx++)
            {
                _grid[rowOffset + cx] = value;
            }
        }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PlaceStaticObject(Vector2I cellPosition, byte obstacleType = 1)
    {
        int index = GetCellIndex(cellPosition);

        if (index != -1)
            _grid[index] = obstacleType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveStaticObject(Vector2I cellPosition)
    {
        int index = GetCellIndex(cellPosition);

        if (index != -1)
            _grid[index] = 0;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCellIndex(in Vector2I cellPosition)
    {
        int cx = cellPosition.X + HalfGridWidth;
        int cy = cellPosition.Y + HalfGridHeight;

        if (cx < 0 || cx >= GridWidth || cy < 0 || cy >= GridHeight)
            return -1;

        return cy * GridWidth + cx;
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
}
