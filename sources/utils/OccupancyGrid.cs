using Godot;
using GodotEcsArch.sources.managers.Chunks;
using System;
using System.Collections.Generic;

public class OccupancyGrid
{
    private readonly ChunkManagerBase chunkManager = null;

    // Diccionario principal: chunk → celdas
    private readonly Dictionary<Vector2I, bool[,]> chunkCells = new();

    public OccupancyGrid(ChunkManagerBase chunkManager)
    {
        this.chunkManager = chunkManager;
    }

    // ---------------------------------------------------------
    // ------------------ FUNCIONES PRINCIPALES ----------------
    // ---------------------------------------------------------

    /// <summary>
    /// Verifica si una celda global está ocupada.
    /// </summary>
    public bool IsOccupied(Vector2I tilePositionGlobal)
    {
        
        var chunkPos = chunkManager.ChunkPosition(tilePositionGlobal);
        var localPos = chunkManager.TilePositionInChunk(chunkPos, tilePositionGlobal);

        if (!chunkCells.TryGetValue(chunkPos, out var chunk))
            return false; // Si no existe el chunk, está libre (vacío por defecto)

        return chunk[localPos.X, localPos.Y];
    }

    /// <summary>
    /// Marca una celda global como ocupada o libre.
    /// </summary>
    public void SetOccupied(Vector2I tilePositionGlobal, bool occupied)
    {
        var chunkPos = chunkManager.ChunkPosition(tilePositionGlobal);
        var localPos = chunkManager.TilePositionInChunk(chunkPos, tilePositionGlobal);

        // Crear chunk si no existe
        EnsureChunk(chunkPos);

        chunkCells[chunkPos][localPos.X, localPos.Y] = occupied;
    }

    /// <summary>
    /// Verifica si todas las celdas de un área global están libres.
    /// sizeX, sizeY → dimensiones del objeto.
    /// </summary>
    public bool CanPlace(Vector2I startGlobal, int sizeX, int sizeY)
    {
        for (int dx = 0; dx < sizeX; dx++)
        {
            for (int dy = 0; dy < sizeY; dy++)
            {
                Vector2I pos = new(startGlobal.X + dx, startGlobal.Y + dy);
                if (IsOccupied(pos))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Marca un área global como ocupada.
    /// </summary>
    public void Place(Vector2I startGlobal, int sizeX, int sizeY)
    {
        if (!CanPlace(startGlobal, sizeX, sizeY))
            throw new InvalidOperationException("El área está ocupada.");

        for (int dx = 0; dx < sizeX; dx++)
        {
            for (int dy = 0; dy < sizeY; dy++)
            {
                SetOccupied(new Vector2I(startGlobal.X + dx, startGlobal.Y + dy), true);
            }
        }
    }

    /// <summary>
    /// Libera un área global.
    /// </summary>
    public void Clear(Vector2I startGlobal, int sizeX, int sizeY)
    {
        for (int dx = 0; dx < sizeX; dx++)
        {
            for (int dy = 0; dy < sizeY; dy++)
            {
                SetOccupied(new Vector2I(startGlobal.X + dx, startGlobal.Y + dy), false);
            }
        }
    }

    // ---------------------------------------------------------
    // ---------------------- CHUNKING -------------------------
    // ---------------------------------------------------------

    /// <summary>
    /// Asegura que un chunk existe.
    /// </summary>
    private void EnsureChunk(Vector2I chunkPos)
    {
        if (!chunkCells.ContainsKey(chunkPos))
        {            
            chunkCells[chunkPos] = new bool[chunkManager.chunkDimencion.X, chunkManager.chunkDimencion.Y];
        }
    }

    /// <summary>
    /// Devuelve el array bool[,] del chunk. (Lo crea si no existe)
    /// </summary>
    public bool[,] GetChunk(Vector2I chunkPos)
    {
        EnsureChunk(chunkPos);
        return chunkCells[chunkPos];
    }
}
