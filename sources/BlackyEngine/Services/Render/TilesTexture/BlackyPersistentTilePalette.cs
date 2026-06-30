using Godot;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using GodotFlecs.sources.Flecs.Components;
using MessagePack;
using ProtoBuf;
using System;
using System.Collections.Generic;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;

//public class TileDataMod
//{
//    public string ModName;
//    public int SubTextureId;
//    public ushort Index;

//    // Cache de renderizado pre-calculado
//    public Color BaseUV;
//    public Color[] AnimatedUVs;

//    // Datos para el sistema de animación ECS
//    public bool IsAnimated;
//    public float Fps;
//    public int FrameCount;

//    // Datos para el collider
//    public bool HasCollider;
//    public FastCollider Collider;
//}


[MessagePackObject]
public struct TileDataPersisted
{
    [Key(0)]
    public string ModName { get; set; }

    [Key(1)]
    public ushort TextureIndex { get; set; }
}

public abstract class BlackyTilePaletteBase
{
    protected ushort idIncremental = 1;

    
    //protected Dictionary<ushort, TileDataMod> palette = new();
    //protected Dictionary<(int subTextureId, ushort index), ushort> lookup = new();

    protected Dictionary<ushort, TileSpriteData> paletteSprite = new();
    protected Dictionary<(int subTextureId, ushort index), ushort> lookupSprite = new();

    protected int idIncrementalHard = 1;
    protected Dictionary<long, TileSpriteData> paletteSpriteHard = new();
    protected Dictionary<(int subTextureId, long idTileSprite), int> lookupSpriteHard = new();

    /// <summary>
    /// Crea o retorna un tile ya cacheado.
    /// </summary>
    /// 
    protected int CreateSprite(string modName, long idTileSprite)
    {
        var data = AtlasTexturesModsManager.Instance.GetMaterialTexture(modName);                
        if (data == null)
        {
            GD.PrintErr($"Mod no encontrado: {modName}");
            return 0;
        }

        var key = (data.idSubTexture, idTileSprite);
        if (lookupSpriteHard.TryGetValue(key, out int existingId))
            return existingId;

        TileSpriteData tileSpriteData = AtlasModsManager.Get<TileSpriteData>(modName, idTileSprite);

        int newId = idIncrementalHard++;
        lookupSpriteHard[key] = newId;
        return newId;
    }
    protected ushort CreateTileInternal(string modName, ushort indexTexture)
    {
        var data = AtlasTexturesModsManager.Instance.GetMaterialTexture(modName);

        if (data == null)
        {
            GD.PrintErr($"Mod no encontrado: {modName}");
            return 0;
        }

        var key = (data.idSubTexture, indexTexture);

        if (lookupSprite.TryGetValue(key, out ushort existingId))
            return existingId;

        ushort newId = idIncremental++;

        //// Buscar metadata del tile
        string keyMod = modName + ":" + indexTexture;

        //
        //  bool existsInMod = AtlasModsManager.TryGetTileTexture(modName,keyMod, out TileTextureData modTileData);

        string onlyModName = modName.Split(':')[0];
        
        bool existInMod = AtlasModsManager.TryGetTileSprite(onlyModName, keyMod, indexTexture, out TileSpriteData modTileSpriteData);
        

        if (existInMod)
        {
            paletteSprite[newId] = modTileSpriteData;
        }
        else
        {
            // no existe debemos crear un tile con UVs básicos para no romper el renderizado, pero sin animación ni collider
            Godot.Color baseUV = CalculateUVFromId(data.idSubTexture,indexTexture);

            SpriteData spriteData = new SpriteData
            { 
                uv = baseUV,
                idModMaterial = modName,
                haveCollider = false,
                x = 0, y = 0, scale = 1, 
            };
            paletteSprite[newId] = new TileSpriteData
            {
                tileIndex = indexTexture,
                tileSpriteType = TileSpriteType.DualStatic,
                spriteData = spriteData,
                animationData = null,
                spriteAnimationMultiple = null,
                spriteMultipleAnimationDirection = null,
                tilesOcupancy = null
            };
        }


        lookupSprite[key] = newId;

        return newId;
    }

  
    protected Godot.Color CalculateUVFromId(int subTextureId, ushort localIndex)
    {
        var data = AtlasTexturesModsManager.Instance
            .GetMaterialTextureBySubId(subTextureId);

        if (data == null)
            return Godot.Colors.Magenta;

        int localColumns =
            data.widthAtlas / data.divisionPixelAtlasX;

        int row = localIndex / localColumns;
        int column = localIndex % localColumns;

        return new Godot.Color(
            data.xInAtlas + (column * data.divisionPixelAtlasX),
            data.yInAtlas + (row * data.divisionPixelAtlasY),
            data.divisionPixelAtlasX,
            data.divisionPixelAtlasY
        );
    }

   

    public bool TryGetTileDataMod(
        ushort id,
        out TileSpriteData tile)
    {
        return paletteSprite.TryGetValue(id, out tile);
    }

    public bool Exists(ushort id)
    {
        return paletteSprite.ContainsKey(id);
    }

    public virtual void Clear()
    {
        paletteSprite.Clear();
        lookupSprite.Clear();
        idIncremental = 1;
    }
}


public class BlackyRuntimeTilePalette : BlackyTilePaletteBase
{
    /// <summary>
    /// Obtiene o crea un tile runtime.
    /// NO persistente.
    /// </summary>
    public ushort GetOrCreateTile(
        string modName,
        ushort indexTexture)
    {
        return CreateTileInternal(
            modName,
            indexTexture);
    }
}
public class BlackyPersistentTilePalette: BlackyTilePaletteBase
{
    
    private Dictionary<ushort, TileDataPersisted> persistedPalette = new();

    /// <summary>
    /// Obtiene o crea un tile persistente.
    /// </summary>
    public ushort GetOrCreateTile(
        string modName,
        ushort indexTexture)
    {
        ushort id = CreateTileInternal(
            modName,
            indexTexture);

        if (id == 0)
            return 0;

        if (!persistedPalette.ContainsKey(id))
        {
            persistedPalette[id] = new TileDataPersisted
            {
                ModName = modName,
                TextureIndex = indexTexture
            };
        }

        return id;
    }

    /// <summary>
    /// Data serializable.
    /// </summary>
    public Dictionary<ushort, TileDataPersisted>
        GetPersistedPalette()
    {
        return persistedPalette;
    }

    /// <summary>
    /// Reconstruye palette desde save.
    /// </summary>
    public void LoadPersistedPalette(
        Dictionary<ushort, TileDataPersisted> data)
    {
        persistedPalette = data;
        RebuildPalette();
    }

    /// <summary>
    /// Reconstrucción runtime desde persistencia.
    /// </summary>
    private void RebuildPalette()
    {
        paletteSprite.Clear();
        lookupSprite.Clear();

        idIncremental = 1;

        foreach (var kv in persistedPalette)
        {
            ushort tileId = kv.Key;
            TileDataPersisted persisted = kv.Value;

            var atlasData =
                AtlasTexturesModsManager.Instance
                .GetMaterialTexture(persisted.ModName);

            if (atlasData == null)
                continue;

            string keyMod = persisted.ModName + ":" + persisted.TextureIndex;

            string onlyModName = persisted.ModName.Split(':')[0];
            bool existInMod = AtlasModsManager.TryGetTileSprite(onlyModName, keyMod, persisted.TextureIndex, out TileSpriteData modTileSpriteData);



            if (existInMod)
            {
                paletteSprite[tileId] = modTileSpriteData;
            }
            else
            {
                // no existe debemos crear un tile con UVs básicos para no romper el renderizado, pero sin animación ni collider
                Godot.Color baseUV = CalculateUVFromId(atlasData.idSubTexture, persisted.TextureIndex);

                SpriteData spriteData = new SpriteData
                {
                    uv = baseUV,
                    idModMaterial = persisted.ModName,
                    haveCollider = false,
                    x = 0,
                    y = 0,
                    scale = 1,
                };
                paletteSprite[tileId] = new TileSpriteData
                {
                    tileIndex = persisted.TextureIndex,
                    tileSpriteType = TileSpriteType.DualStatic,
                    spriteData = spriteData,
                    animationData = null,
                    spriteAnimationMultiple = null,
                    spriteMultipleAnimationDirection = null,
                    tilesOcupancy = null,                    
                };
            }
         
            lookupSprite[(atlasData.idSubTexture, persisted.TextureIndex)] = tileId;

            if (tileId >= idIncremental)
                idIncremental = (ushort)(tileId + 1);
        }
    }

    public override void Clear()
    {
        base.Clear();
        persistedPalette.Clear();
    }
 
}
