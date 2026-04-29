using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.Flecs.Components;
using System;
using System.Collections.Generic;

namespace GodotEcsArch.sources.BlackyTiles.TilesTexture;

public struct TileDataMod
{
    public int SubTextureId;
    public ushort Index;

    // Cache de renderizado pre-calculado
    public Color BaseUV;
    public Color[] AnimatedUVs;

    // Datos para el sistema de animación ECS
    public bool IsAnimated;
    public float Fps;
    public int FrameCount;

    // Datos para el collider
    public bool HasCollider;
    public FastCollider Collider;
}

public struct TileDataPersisted
{
    public string ModName;
    public ushort Index;
}

public class BlackyTilePalette
{
    private ushort idIncremental = 1;
    private Dictionary<ushort, TileDataMod> palette = new();
    private Dictionary<(int subTextureId, ushort index), ushort> lookup = new();
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

        // Cachear datos de animación
        // Obtener datos del mod (animación y colisión)
        bool existsInMod = AtlasModsManager.Instance.TryGet(modName, indexTexture, out TileTextureData modTileData);

        bool hasAnim = existsInMod && modTileData.indexAnimation != null && modTileData.indexAnimation.Length > 0;

        // Pre-calcular UVs
        Color baseUV = CalculateUVFromId(data.idSubTexture, indexTexture);
        Color[] animatedUVs = null;

        if (hasAnim)
        {
            animatedUVs = new Color[modTileData.indexAnimation.Length];
            for (int i = 0; i < modTileData.indexAnimation.Length; i++)
            {
                animatedUVs[i] = CalculateUVFromId(data.idSubTexture, (ushort)modTileData.indexAnimation[i]);
            }
        }

        // Llenar el struct con cache total
        palette[newId] = new TileDataMod
        {
            SubTextureId = data.idSubTexture,
            Index = indexTexture,
            BaseUV = baseUV,
            AnimatedUVs = animatedUVs,
            IsAnimated = hasAnim,
            Fps = hasAnim ? modTileData.fps : 0,
            FrameCount = hasAnim ? modTileData.indexAnimation.Length : 0,

            // Cache de Colisión
            HasCollider = existsInMod && modTileData.hasCollider,
            Collider = existsInMod ? modTileData.fastCollider : default
        };

        persistedPalette[newId] = new TileDataPersisted { ModName = modName, Index = indexTexture };
        lookup[key] = newId;

        return newId;
    }

    // =========================
    // UV RENDER (ACCESO DIRECTO)
    // =========================
    public Color GetTileUV(ushort tileId, int frameIndex = -1)
    {
        if (!palette.TryGetValue(tileId, out var tile))
            return Colors.Black;

        if (frameIndex < 0 || !tile.IsAnimated)
            return tile.BaseUV;

        return tile.AnimatedUVs[frameIndex]; 
    }

    /// <summary>
    /// Obtiene la data del manager por subTextureId y calcula el UV resultante.
    /// </summary>
    private Color CalculateUVFromId(int subTextureId, ushort localIndex)
    {
        var data = AtlasTexturesModsManager.Instance.GetMaterialTextureBySubId(subTextureId);
        if (data == null) return Colors.Magenta;

        int localColumns = data.widthAtlas / data.divisionPixelAtlasX;
        int row = localIndex / localColumns;
        int column = localIndex % localColumns;

        return new Color(
            data.xInAtlas + (column * data.divisionPixelAtlasX),
            data.yInAtlas + (row * data.divisionPixelAtlasY),
            data.divisionPixelAtlasX,
            data.divisionPixelAtlasY
        );
    }

    /// <summary>
    /// Intenta obtener el FastCollider de un tile. 
    /// Retorna true si el tile existe y tiene un collider asignado.
    /// </summary>
    public bool TryGetCollider(ushort tileId, out FastCollider collider)
    {
        collider = default;
        if (palette.TryGetValue(tileId, out var tile) && tile.HasCollider)
        {
            collider = tile.Collider;
            return true;
        }
        return false;
    }

    // =========================
    // PERSISTENCIA Y RECONSTRUCCIÓN
    // =========================
    public void LoadPersistedPalette(Dictionary<ushort, TileDataPersisted> data)
    {
        persistedPalette = data;
        RebuildPalette();
    }

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
            if (data == null) continue;

            bool existsInMod = AtlasModsManager.Instance.TryGet(persisted.ModName, persisted.Index, out TileTextureData modTileData);
            bool hasAnim = existsInMod && modTileData.indexAnimation != null && modTileData.indexAnimation.Length > 0;

            Color[] animatedUVs = null;
            if (hasAnim)
            {
                animatedUVs = new Color[modTileData.indexAnimation.Length];
                for (int i = 0; i < modTileData.indexAnimation.Length; i++)
                    animatedUVs[i] = CalculateUVFromId(data.idSubTexture, (ushort)modTileData.indexAnimation[i]);
            }

            palette[tileId] = new TileDataMod
            {
                SubTextureId = data.idSubTexture,
                Index = persisted.Index,
                BaseUV = CalculateUVFromId(data.idSubTexture, persisted.Index),
                AnimatedUVs = animatedUVs,
                IsAnimated = hasAnim,
                Fps = hasAnim ? modTileData.fps : 0,
                FrameCount = hasAnim ? modTileData.indexAnimation.Length : 0,
                HasCollider = existsInMod && modTileData.hasCollider,
                Collider = existsInMod ? modTileData.fastCollider : default
            };

            lookup[(data.idSubTexture, persisted.Index)] = tileId;
            if (tileId >= idIncremental) idIncremental = (ushort)(tileId + 1);
        }
    }
    /// <summary>
    /// Retorna el diccionario listo para ser serializado y guardado en disco.
    /// Contiene la relación ID -> ModName e Index local.
    /// </summary>
    public Dictionary<ushort, TileDataPersisted> GetPersistedPalette()
    {
        return persistedPalette;
    }
    // =========================
    // UTILIDADES
    // =========================
    public bool TryGetTile(ushort id, out TileDataMod tile) => palette.TryGetValue(id, out tile);
    public bool Exists(ushort id) => palette.ContainsKey(id);
    public void Clear() { palette.Clear(); lookup.Clear(); persistedPalette.Clear(); idIncremental = 1; }
}
