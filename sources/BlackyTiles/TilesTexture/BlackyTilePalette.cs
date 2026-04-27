using Godot;
using GodotEcsArch.sources.managers.Mods;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public struct TileDataMod
{
    public int SubTextureId;   // runtime handle global único
    public ushort Index;       // índice local dentro de la subtextura
}

public struct TileDataPersisted
{
    public string ModName;     // identidad persistente estable
    public ushort Index;
}

public class BlackyTilePalette
{
    private ushort idIncremental = 1;

    // =========================
    // Runtime palette
    // =========================
    private Dictionary<ushort, TileDataMod> palette = new();

    // evita duplicados runtime
    private Dictionary<(int subTextureId, ushort index), ushort> lookup = new();

    // =========================
    // Persistencia
    // =========================
    private Dictionary<ushort, TileDataPersisted> persistedPalette = new();

    // =========================
    // CREACIÓN / REGISTRO
    // =========================
    public ushort GetOrCreateTile(string modName, ushort indexTexture)
    {
        var data = AtlasTexturesModsManager.Instance.GetMaterialTexture(modName);

        if (data == null)
        {
            GD.PrintErr($"Mod no encontrado: {modName}");
            return 0;
        }

        var key = (data.idSubTexture, indexTexture);

        if (lookup.TryGetValue(key, out ushort existingId))
            return existingId;

        ushort newId = idIncremental++;

        palette[newId] = new TileDataMod
        {
            SubTextureId = data.idSubTexture,
            Index = indexTexture
        };

        persistedPalette[newId] = new TileDataPersisted
        {
            ModName = modName,
            Index = indexTexture
        };

        lookup[key] = newId;

        return newId;
    }

    // =========================
    // UV RENDER
    // =========================
    public Color GetTileUV(ushort tileId)
    {
        if (!palette.TryGetValue(tileId, out var tile))
            return Colors.Black;

        var data = AtlasTexturesModsManager.Instance.GetMaterialTextureBySubId(tile.SubTextureId);

        if (data == null)
            return Colors.Magenta;

        int localColumns = data.widthAtlas / data.divisionPixelAtlasX;

        int row = tile.Index / localColumns;
        int column = tile.Index % localColumns;

        int x = data.xInAtlas + (column * data.divisionPixelAtlasX);
        int y = data.yInAtlas + (row * data.divisionPixelAtlasY);

        return new Color(
            x,
            y,
            data.divisionPixelAtlasX,
            data.divisionPixelAtlasY
        );
    }

    // =========================
    // SERIALIZACIÓN
    // =========================
    public Dictionary<ushort, TileDataPersisted> GetPersistedPalette()
    {
        return persistedPalette;
    }

    public void LoadPersistedPalette(Dictionary<ushort, TileDataPersisted> data)
    {
        persistedPalette = data;
        RebuildPalette();
    }

    // =========================
    // REBUILD RUNTIME
    // =========================
    private void RebuildPalette()
    {
        palette.Clear();
        lookup.Clear();
        idIncremental = 1;

        foreach (var kv in persistedPalette)
        {
            ushort tileId = kv.Key;
            var persisted = kv.Value;

            var data = AtlasTexturesModsManager.Instance.GetMaterialTexture(persisted.ModName);

            if (data == null)
            {
                GD.PrintErr($"Mod faltante al reconstruir: {persisted.ModName}");
                continue;
            }

            palette[tileId] = new TileDataMod
            {
                SubTextureId = data.idSubTexture,
                Index = persisted.Index
            };

            lookup[(data.idSubTexture, persisted.Index)] = tileId;

            if (tileId >= idIncremental)
                idIncremental = (ushort)(tileId + 1);
        }
    }

    // =========================
    // UTILIDAD
    // =========================
    public bool TryGetTile(ushort id, out TileDataMod tile)
    {
        return palette.TryGetValue(id, out tile);
    }

    public bool Exists(ushort id)
    {
        return palette.ContainsKey(id);
    }

    public void Clear()
    {
        palette.Clear();
        lookup.Clear();
        persistedPalette.Clear();
        idIncremental = 1;
    }
}