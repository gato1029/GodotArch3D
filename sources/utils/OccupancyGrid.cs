using System;
using System.Collections.Generic;

public class OccupancyGrid
{
    private readonly int width;
    private readonly int height;
    private readonly bool[,] cells; // true = ocupada, false = libre

    public int Width => width;
    public int Height => height;

    public OccupancyGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        cells = new bool[width, height];
    }

    /// <summary>
    /// Verifica si una celda está ocupada.
    /// </summary>
    public bool IsOccupied(int x, int y)
    {
        if (!IsInside(x, y))
            throw new ArgumentOutOfRangeException($"Celda fuera de límites: ({x},{y})");
        return cells[x, y];
    }

    /// <summary>
    /// Marca una celda como ocupada o libre.
    /// </summary>
    public void SetOccupied(int x, int y, bool occupied)
    {
        if (!IsInside(x, y))
            throw new ArgumentOutOfRangeException($"Celda fuera de límites: ({x},{y})");
        cells[x, y] = occupied;
    }

    /// <summary>
    /// Verifica si todas las celdas en un área están libres (ej. para un edificio).
    /// </summary>
    public bool CanPlace(int startX, int startY, int sizeX, int sizeY)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                int checkX = startX + x;
                int checkY = startY + y;
                if (!IsInside(checkX, checkY) || cells[checkX, checkY])
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Marca un área como ocupada (cuando se coloca un edificio).
    /// </summary>
    public void Place(int startX, int startY, int sizeX, int sizeY)
    {
        if (!CanPlace(startX, startY, sizeX, sizeY))
            throw new InvalidOperationException("El área está ocupada o fuera de límites.");

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                cells[startX + x, startY + y] = true;
            }
        }
    }

    /// <summary>
    /// Libera un área ocupada (cuando destruyes un edificio).
    /// </summary>
    public void Clear(int startX, int startY, int sizeX, int sizeY)
    {
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (IsInside(startX + x, startY + y))
                    cells[startX + x, startY + y] = false;
            }
        }
    }

    /// <summary>
    /// Comprueba si una posición está dentro de los límites de la grilla.
    /// </summary>
    private bool IsInside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
