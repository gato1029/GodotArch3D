using Arch.Core;
using Godot;
using GodotEcsArch.sources.managers.Chunks;
using GodotEcsArch.sources.utils;
using System;
using System.Collections.Generic;

public class OccupancyGrid
{
    private readonly ChunkManagerBase chunkManager = null;

    // Diccionario principal: chunk → celdas
    private readonly Dictionary<Vector2I, bool[,]> chunkCells = new();
    
    private Dictionary<Vector2I, Dictionary<Vector2I, int>> chunksLoad = new();

    public OccupancyGrid(ChunkManagerBase chunkManager)
    {
        this.chunkManager = chunkManager;
        chunkManager.OnChunkLoad += ChunkManager_OnChunkLoad;
        chunkManager.OnChunkUnload += ChunkManager_OnChunkUnload;
    }

    private void ChunkManager_OnChunkUnload(Vector2 obj)
    {
        Vector2I chunkPosition = (Vector2I)obj;

        if (chunksLoad.TryGetValue(chunkPosition, out var chunk))
        {
            foreach (var item in chunk)
                WireShape.Instance.FreeShape(item.Value);

            chunksLoad.Remove(chunkPosition);
        }
    }

    private void ChunkManager_OnChunkLoad(Vector2 obj)
    {
        Vector2I chunkPosition = (Vector2I)obj;

        // Ya está cargado → no repetir shapes
        if (chunksLoad.ContainsKey(chunkPosition))
            return;

        if (!chunkCells.TryGetValue(chunkPosition, out var data))
            return;

        Dictionary<Vector2I, int> idsSquare = new();

        int width = data.GetLength(0);
        int height = data.GetLength(1);

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (data[x, y])
                {
                    Vector2I posTile = new(x, y);
                    Vector2I posGlobal = chunkManager.TilePositionGlobal(chunkPosition, posTile);
                    Vector2 posWorld = TilesHelper.WorldPositionTile(posGlobal);

                    int id = WireShape.Instance.DrawFilledSquare(
                        new Vector2(16, 16), posWorld, -100, Godot.Colors.Red, .5f
                    );

                    idsSquare[posGlobal] = id;
                }
            }
        }

        chunksLoad.Add(chunkPosition, idsSquare);

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


        // --------------------------------------
        // SI NO EXISTE EL CHUNK EN chunksLoad,
        // SE GENERA AHORA MISMO (con shapes)
        // --------------------------------------
        if (!chunksLoad.TryGetValue(chunkPos, out var chunkShapes))
        {
            // Construir shapes de todo el chunk igual que OnChunkLoad
            chunkShapes = new Dictionary<Vector2I, int>();

            var data = chunkCells[chunkPos];
            int width = data.GetLength(0);
            int height = data.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (data[x, y])
                    {
                        Vector2I local = new Vector2I(x, y);
                        Vector2I global = chunkManager.TilePositionGlobal(chunkPos, local);
                        Vector2 worldPos = TilesHelper.WorldPositionTile(global);

                        int id = WireShape.Instance.DrawFilledSquare(
                            new Vector2(16, 16),
                            worldPos,
                            -100,
                            Godot.Colors.Red,
                            .5f
                        );
                        chunkShapes.Add(global, id);
                    }
                }
            }

            // Registrar el chunk ya cargado con shapes
            chunksLoad.Add(chunkPos, chunkShapes);
        }

        // -------------------------------------------------
        // A partir de aquí el chunk SI está cargado (shape)
        // -------------------------------------------------

        if (occupied)
        {
            // Dibujar solo el tile afectado si no estaba dibujado
            if (!chunkShapes.ContainsKey(tilePositionGlobal))
            {
                Vector2 worldPos = TilesHelper.WorldPositionTile(tilePositionGlobal);
                int id = WireShape.Instance.DrawFilledSquare(
                    new Vector2(16, 16),
                    worldPos,
                    -100,
                    Godot.Colors.Red,
                    .5f
                );

                chunkShapes.Add(tilePositionGlobal, id);
            }
        }
        else
        {
            // Liberar tile si estaba dibujado
            if (chunkShapes.TryGetValue(tilePositionGlobal, out int id))
            {
                WireShape.Instance.FreeShape(id);
                chunkShapes.Remove(tilePositionGlobal);
            }
        }
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
    /// Verifica si todas las celdas de un área global están libres.
    /// sizeX, sizeY → dimensiones del objeto.
    /// </summary>
    public bool CanPlace(Vector2I startGlobal,List<KuroTile> tiles)
    {
        foreach (var item in tiles)
        {
            Vector2I pos = new(startGlobal.X + item.x, startGlobal.Y +item.y);
            if (IsOccupied(pos))
                return false;
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
