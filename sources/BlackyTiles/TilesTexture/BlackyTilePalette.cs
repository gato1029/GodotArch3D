using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public struct TileDataMod
{
    public int AtlasId;     // Runtime (rápido)
    public ushort Index;    // Índice dentro del atlas
}

public struct TileDataPersisted
{
    public string ModName;  // Persistente (seguro)
    public ushort Index;
}

public class BlackyTilePalette
{
    private ushort idIncremental = 1;

    // Runtime
    private Dictionary<ushort, TileDataMod> palette = new();
    private Dictionary<(int atlasId, ushort index), ushort> lookup = new();

    // Persistencia (opcional mantener en memoria)
    private Dictionary<ushort, TileDataPersisted> persistedPalette = new();

    // Cache para acelerar rebuild
    private Dictionary<string, int> modNameToAtlasId = new();

    // =========================
    // 🧱 CREACIÓN / REGISTRO
    // =========================
    public ushort GetOrCreateTile(string modName, ushort indexTexture)
    {
        var data = AtlasTexturesModsManager.Instance.GetMaterialTexture(modName);
        if (data == null)
        {
            GD.PrintErr($"Mod no encontrado: {modName}");
            return 0;
        }

        int atlasId = data.idTextureAtlas;
        var key = (atlasId, indexTexture);

        // 🔁 Ya existe
        if (lookup.TryGetValue(key, out ushort existingId))
            return existingId;

        // 🆕 Crear nuevo
        ushort newId = idIncremental++;

        var runtimeData = new TileDataMod
        {
            AtlasId = atlasId,
            Index = indexTexture
        };

        var persistedData = new TileDataPersisted
        {
            ModName = modName,
            Index = indexTexture
        };

        palette[newId] = runtimeData;
        persistedPalette[newId] = persistedData;
        lookup[key] = newId;

        return newId;
    }

    // =========================
    // 🎨 RENDER
    // =========================
    public Color GetTileUV(ushort tileId)
    {
        if (!palette.TryGetValue(tileId, out var tile))
            return Colors.Black;

        var data = AtlasTexturesModsManager.Instance.GetMaterialTextureByAtlasId(tile.AtlasId);
        if (data == null)
            return Colors.Magenta;

        var offset = new Vector2I(data.xInAtlas, data.yInAtlas);

        int columns = data.widthAtlas / data.divisionPixelAtlasX;

        int row = tile.Index / columns;
        int column = tile.Index % columns;

        int x = column * data.divisionPixelAtlasX;
        int y = row * data.divisionPixelAtlasY;

        int newX = offset.X + x;
        int newY = offset.Y + y;

        return new Color(newX, newY, data.divisionPixelAtlasX, data.divisionPixelAtlasY);
    }

    // =========================
    // 💾 SERIALIZACIÓN
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
    // 🔄 REBUILD (CLAVE)
    // =========================
    private void RebuildPalette()
    {
        palette.Clear();
        lookup.Clear();
        modNameToAtlasId.Clear();

        idIncremental = 1;

        // 🔥 construir cache de mods
        foreach (var mod in AtlasTexturesModsManager.Instance.GetAllMaterialsTextures())
        {
            modNameToAtlasId[mod.idNameMod] = mod.idTextureAtlas;
        }

        foreach (var kv in persistedPalette)
        {
            ushort tileId = kv.Key;
            var persisted = kv.Value;

            if (!modNameToAtlasId.TryGetValue(persisted.ModName, out int atlasId))
            {
                GD.PrintErr($"Mod faltante al reconstruir: {persisted.ModName}");
                continue;
            }

            var runtimeData = new TileDataMod
            {
                AtlasId = atlasId,
                Index = persisted.Index
            };

            palette[tileId] = runtimeData;
            lookup[(atlasId, persisted.Index)] = tileId;

            if (tileId >= idIncremental)
                idIncremental = (ushort)(tileId + 1);
        }
    }

    // =========================
    // 🔍 UTILIDAD
    // =========================
    public bool TryGetTile(ushort id, out TileDataMod tile)
    {
        return palette.TryGetValue(id, out tile);
    }
}
