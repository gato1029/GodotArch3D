using System;
using System.Collections.Generic;
using Godot;
using GodotEcsArch.sources.BlackyTiles;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;



public class BlackyRegion
{
    // Identificador único de la región (en coordenadas de región, no de mundo)
    public int X { get; }
    public int Y { get; }

    // La paleta local de esta región
    public BlackyPersistentTilePalette Palette { get; private set; }
    public BlackyRuntimeTilePalette RuntimeTilePalette { get; private set; }

    // Opcional: Mantener una lista de los chunks cargados en esta región 
    // para facilitar el guardado masivo.
    private readonly HashSet<BlackyChunkCoord> _containedChunks = new();
    private readonly HashSet<BlackyChunkCoord> _dirtyChunks = new();
    public BlackyRegion(int x, int y)
    {
        X = x;
        Y = y;
        Palette = new BlackyPersistentTilePalette();
        RuntimeTilePalette = new BlackyRuntimeTilePalette();
    }
    public void MarkChunkDirty(
    BlackyChunkCoord coord)
    {
        _dirtyChunks.Add(coord);
    }

    public IEnumerable<BlackyChunkCoord>
        GetDirtyChunks()
    {
        return _dirtyChunks;
    }

    public void ClearDirtyChunks()
    {
        _dirtyChunks.Clear();
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
    public ushort GetOrCreateTile(string modName, ushort indexTexture, bool isPersistent=true)
    {
        if (isPersistent)
        {
            return Palette.GetOrCreateTile(modName, indexTexture);
        }
        else
        {
            return RuntimeTilePalette.GetOrCreateTile(modName, indexTexture);
        }
    }

    

    /// <summary>
    /// Devuelve la información de textura (UVs) para un ID local.
    /// </summary>
    //public Color GetTileUV(ushort tileId, bool isPersistent = true)
    //{
    //    if (isPersistent)
    //    {
    //        return Palette.GetTileUV(tileId);
    //    }
    //    else
    //    {
    //        return RuntimeTilePalette.GetTileUV(tileId);
    //    }        
    //}

    public void TryGetTileDataMod(ushort tileId, out TileSpriteData tileDataMod, bool isPersistent = true)
    {
        if (isPersistent)
        {
            Palette.TryGetTileDataMod(tileId, out tileDataMod);
        }
        else
        {
            RuntimeTilePalette.TryGetTileDataMod(tileId, out tileDataMod);
        }
    }

    // ===============================
    // PERSISTENCIA
    // ===============================

    /// <summary>
    /// Prepara los datos de la paleta para ser guardados.
    /// </summary>
    public Dictionary<ushort, TileDataPersisted> GetSavePaletteData()
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

