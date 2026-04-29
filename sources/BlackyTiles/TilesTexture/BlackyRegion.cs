using System;
using System.Collections.Generic;
using Godot;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;



public class BlackyRegion
{
    // Identificador único de la región (en coordenadas de región, no de mundo)
    public int X { get; }
    public int Y { get; }

    // La paleta local de esta región
    public BlackyTilePalette Palette { get; private set; }

    // Opcional: Mantener una lista de los chunks cargados en esta región 
    // para facilitar el guardado masivo.
    private readonly HashSet<BlackyChunkCoord> _containedChunks = new();

    public BlackyRegion(int x, int y)
    {
        X = x;
        Y = y;
        Palette = new BlackyTilePalette();
    }

    // ===============================
    // GESTIÓN DE CHUNKS
    // ===============================

    public void RegisterChunk(BlackyChunkCoord coord)
    {
        _containedChunks.Add(coord);
    }

    public void UnregisterChunk(BlackyChunkCoord coord)
    {
        _containedChunks.Remove(coord);
    }

    public IEnumerable<BlackyChunkCoord> GetRegisteredChunks()
    {
        return _containedChunks;
    }

    // ===============================
    // PALETA INTERFACE
    // ===============================

    /// <summary>
    /// Obtiene el ID local (ushort) para un tile basado en su nombre de mod e índice.
    /// </summary>
    public ushort GetOrCreateTile(string modName, ushort indexTexture)
    {
        return Palette.GetOrCreateTile(modName, indexTexture);
    }

    /// <summary>
    /// Devuelve la información de textura (UVs) para un ID local.
    /// </summary>
    public Color GetTileUV(ushort tileId)
    {
        return Palette.GetTileUV(tileId);
    }

    // ===============================
    // PERSISTENCIA
    // ===============================

    /// <summary>
    /// Prepara los datos de la paleta para ser guardados.
    /// </summary>
    public Dictionary<ushort, TileDataPersisted> GetSaveData()
    {
        return Palette.GetPersistedPalette();
    }

    /// <summary>
    /// Carga una paleta existente (usado al cargar la región desde el disco).
    /// </summary>
    public void LoadPalette(Dictionary<ushort, TileDataPersisted> data)
    {
        Palette.LoadPersistedPalette(data);
    }
}

