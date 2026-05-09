using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using System;
using System.Collections;
using System.Collections.Generic;


namespace GodotEcsArch.sources.managers.Mods;


public class AtlasModsManager : SingletonBase<AtlasModsManager>
{
    // =========================================================
    // ATLASES (ID -> DATA)
    // =========================================================

    private readonly AtlasMods<int, MaterialData> materiales = new();
    private readonly AtlasMods<int, ResourceSourceData> fuenteRecursos = new();
    private readonly AtlasMods<int, TileSpriteData> spriteData = new();
    private readonly AtlasMods<int, AutoTileSpriteData> autoSpriteData = new();
    private readonly AtlasMods<int, TerrainData> terrainData = new();
    private readonly AtlasMods<int, TerrainDataTransition> terrainDataTransicion = new();

    private readonly AtlasMods<int, BuildingData> buildingData = new();
    private readonly AtlasMods<int, BulletData> bulletData = new();
    private readonly AtlasMods<int, CharacterModelBaseData> characterData = new();

    // string-key atlas
    private readonly AtlasMods<string, TileTextureData> tilesTextureData = new();

    // =========================================================
    // INDEXES (RELACIONES / QUERIES)
    // =========================================================

    private readonly Dictionary<byte, MultiIndex<int, TileTextureData>> _tilesByMaterialIndex = new();

    // =========================================================
    // TYPE REGISTRY (interno seguro)
    // =========================================================

    private readonly Dictionary<Type, object> _atlases = new();

    private void RegisterAtlas<T>(AtlasMods<int, T> atlas) where T : class
    {
        _atlases[typeof(T)] = atlas;
    }

    private void RegisterAtlas<T>(AtlasMods<string, T> atlas) where T : class
    {
        _atlases[typeof(T)] = atlas;
    }
    private AtlasMods<int, T> GetAtlas<T>() where T : class
    {
        return _atlases.TryGetValue(typeof(T), out var atlas)
            ? atlas as AtlasMods<int, T>
            : null;
    }

    // =========================================================
    // MOD RESOLUTION (SIN CACHE, SIMPLE Y CORRECTO)
    // =========================================================

    private bool TryGetModId(string modName, out byte modId)
    {
        var id = TableMods.Instance.ObtenerId(modName);

        if (id == null)
        {
            modId = default;
            return false;
        }

        modId = id.Value;
        return true;
    }

    // =========================================================
    // SINGLETON INIT
    // =========================================================

    protected override void Initialize()
    {
    
    }

    // =========================================================
    // 🟢 API PRINCIPAL (NORMAL - modName)
    // =========================================================

    public static T Get<T>(string modName, int id) where T : class
        => Instance.InternalGet<T, int>(modName, id);

    public static bool TryGet<T>(string modName, int id, out T value) where T : class
        => Instance.InternalTryGet<T, int>(modName, id, out value);

    private T InternalGet<T, TKey>(string modName, TKey id)
        where T : class where TKey : notnull
    {
        if (!TryGetModId(modName, out var modId))
            return null;

        var atlas = GetAtlas<T>() as AtlasMods<TKey, T>;
        return atlas?.Get(modId, id);
    }

    private bool InternalTryGet<T, TKey>(string modName, TKey id, out T value)
        where T : class where TKey : notnull
    {
        value = null;

        if (!TryGetModId(modName, out var modId))
            return false;

        var atlas = GetAtlas<T>() as AtlasMods<TKey, T>;

        return atlas != null && atlas.TryGet(modId, id, out value);
    }

    public static IEnumerable<T> GetAll<T>(string modName) where T : class
    => Instance.InternalGetAll<T>(modName);

    private IEnumerable<T> InternalGetAll<T>(string modName) where T : class
    {
        if (!TryGetModId(modName, out var modId))
            yield break;

        var atlas = GetAtlas<T>();

        if (atlas == null)
            yield break;

        foreach (var item in atlas.GetAll(modId))
            yield return item;
    }

    // =========================================================
    // 🔴 API HOT PATH (modId directo)
    // =========================================================

    public static T GetFast<T>(byte modId, int id) where T : class
        => Instance.InternalGetFast<T, int>(modId, id);

    public static bool TryGetFast<T>(byte modId, int id, out T value) where T : class
        => Instance.InternalTryGetFast<T, int>(modId, id, out value);

    private T InternalGetFast<T, TKey>(byte modId, TKey id)
        where T : class where TKey : notnull
    {
        var atlas = GetAtlas<T>() as AtlasMods<TKey, T>;
        return atlas?.Get(modId, id);
    }

    private bool InternalTryGetFast<T, TKey>(byte modId, TKey id, out T value)
        where T : class where TKey : notnull
    {
        value = null;

        var atlas = GetAtlas<T>() as AtlasMods<TKey, T>;
        return atlas != null && atlas.TryGet(modId, id, out value);
    }

    // =========================================================
    // 🟡 TILE TEXTURE (STRING KEY ATLAS)
    // =========================================================
    public static bool TryGetTileTextureIndex(string modName, int indexTexture, out TileTextureData value)
    {
        string keyMod = modName + ":" + indexTexture;
        return Instance.InternalTryGetTileTexture(modName, keyMod, out value);
    }
    
    public static bool TryGetTileTexture(string modName, string name, out TileTextureData value)
        => Instance.InternalTryGetTileTexture(modName, name, out value);
    private bool InternalTryGetTileTexture(string modName, string name, out TileTextureData value)
    {
        value = null;

        if (!TryGetModId(modName, out var modId))
            return false;

        return tilesTextureData.TryGet(modId, name, out value);
    }
    public static TileTextureData GetTileTexture(string modName, string name)
        => Instance.InternalGetTileTexture(modName, name);

    private TileTextureData InternalGetTileTexture(string modName, string name)
    {
        if (!TryGetModId(modName, out var modId))
            return null;

        return tilesTextureData.Get(modId, name);
    }

    // =========================================================
    // 🔵 QUERY SYSTEM (RELACIÓN MATERIAL -> TILES)
    // =========================================================

    public static IReadOnlyList<TileTextureData> GetTilesByMaterial(string modName, int materialId)
        => Instance.InternalGetTilesByMaterial(modName, materialId);

    private IReadOnlyList<TileTextureData> InternalGetTilesByMaterial(string modName, int materialId)
    {
        if (!TryGetModId(modName, out var modId))
            return Array.Empty<TileTextureData>();

        if (_tilesByMaterialIndex.TryGetValue(modId, out var index))
            return index.Get(materialId);

        return Array.Empty<TileTextureData>();
    }

    // =========================================================
    // LOADERS
    // =========================================================

    private void CargarMateriales(byte idMod, string name)
    {
        var data = DataBaseManager.Instance.FindAll<MaterialData>();

        foreach (var item in data)
        {
            item.idNameMod = name +":"+ item.id;
            materiales.Register(idMod, item.id, item);
        }
    
    }

    private void CargarPorMod(byte idMod)
    {
        CargarDatos(fuenteRecursos, idMod);
        CargarDatos(spriteData, idMod);
        CargarDatos(terrainData, idMod);
        CargarDatos(terrainDataTransicion, idMod);
        CargarDatos(autoSpriteData, idMod);
    }

    private void CargarPorModEspecial(byte idMod)
    {
        CargarDatosEspecial(buildingData, idMod);
        CargarDatosEspecial(bulletData, idMod);
        CargarDatosEspecial(characterData, idMod);
    }

    private void CargarTilesTextureData(byte idMod)
    {
        var data = DataBaseManager.Instance.FindAll<TileTextureData>();

        if (!_tilesByMaterialIndex.ContainsKey(idMod))
            _tilesByMaterialIndex[idMod] = new MultiIndex<int, TileTextureData>();

        var index = _tilesByMaterialIndex[idMod];

        foreach (var item in data)
        {
            tilesTextureData.Register(idMod, item.name, item);
            index.Add(item.idMaterial, item);
        }
    }

    private void CargarDatos<T>(AtlasMods<int, T> atlas, byte idMod) where T : IdDataLong
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
            atlas.Register(idMod, item.idSave, item);
    }

    private void CargarDatosEspecial<T>(AtlasMods<int, T> atlas, byte idMod) where T : IdData
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
            atlas.Register(idMod, item.id, item);
    }
    public static Dictionary<byte, Dictionary<TKey, T>> GetDictionaryAll<T, TKey>() where T : class
    => Instance.InternalGetDictionaryAll<T, TKey>();
    private Dictionary<byte, Dictionary<TKey, T>> InternalGetDictionaryAll<T, TKey>()
    where T : class where TKey : notnull
    {
        var atlas = GetAtlas<T>() as AtlasMods<TKey, T>;

        if (atlas == null)
            return new Dictionary<byte, Dictionary<TKey, T>>();

        return atlas.GetRawData();
    }
    // =========================================================
    // HELPERS VARIOS
    // =========================================================

    public static AtlasTexture[] GetAtlasTexture(string idMod_idMaterial,int internalPosition,out bool isAnimated,out TileTextureData tileTextureData)
    {
        var data = AtlasTexturesModsManager.Instance.GetMaterialTexture(idMod_idMaterial);
        

        bool exist = TryGetTileTextureIndex(idMod_idMaterial, internalPosition, out tileTextureData);
        isAnimated = exist && tileTextureData.isAnimated;

        AtlasTexture baseUV = CalculateUVFromId(data.idSubTexture, (ushort)internalPosition);
        AtlasTexture[] animatedUVs = null;

        if (isAnimated)
        {
            animatedUVs = new AtlasTexture[tileTextureData.indexAnimation.Length];
            for (int i = 0; i < tileTextureData.indexAnimation.Length; i++)
            {
                animatedUVs[i] = CalculateUVFromId(data.idSubTexture, (ushort)tileTextureData.indexAnimation[i]);
            }
        }
        else
        {
            animatedUVs = new AtlasTexture[1];
            animatedUVs[0] = baseUV;
        }
        return animatedUVs;
    }

    private static AtlasTexture CalculateUVFromId(int subTextureId, ushort localIndex)
    {
        var data = AtlasTexturesModsManager.Instance.GetMaterialTextureBySubId(subTextureId);
        AtlasTexture atlasTexture = new AtlasTexture();
        if (data == null)
        {
            atlasTexture.Region = new Rect2(0,0,0,0);
            return atlasTexture;
        }

        int localColumns = data.widthAtlas / data.divisionPixelAtlasX;
        int row = localIndex / localColumns;
        int column = localIndex % localColumns;
        

        atlasTexture.Region = new Rect2(data.xInAtlas + (column * data.divisionPixelAtlasX), data.yInAtlas + (row * data.divisionPixelAtlasY),
            data.divisionPixelAtlasX, data.divisionPixelAtlasY);

        return atlasTexture;
    }

    public void ClearAll()
    {
        // ================================
        // CLEAR ALL ATLASES
        // ================================
        materiales.Clear();
        fuenteRecursos.Clear();
        spriteData.Clear();
        autoSpriteData.Clear();
        terrainData.Clear();
        terrainDataTransicion.Clear();
        buildingData.Clear();
        bulletData.Clear();
        characterData.Clear();
        tilesTextureData.Clear();

        // ================================
        // CLEAR INDEXES
        // ================================
        foreach (var kv in _tilesByMaterialIndex)
            kv.Value.Clear();

        _tilesByMaterialIndex.Clear();

        // ================================
        // CLEAR TYPE REGISTRY
        // ================================
        _atlases.Clear();
    }
    internal void FirstLoad()
    {
        RegisterAtlas(materiales);
        RegisterAtlas(fuenteRecursos);
        RegisterAtlas(spriteData);
        RegisterAtlas(autoSpriteData);
        RegisterAtlas(terrainData);
        RegisterAtlas(terrainDataTransicion);
        RegisterAtlas(buildingData);
        RegisterAtlas(bulletData);
        RegisterAtlas(characterData);
        RegisterAtlas(tilesTextureData);

        foreach (var mod in TableMods.Instance.ObtenerTodos())
        {
            byte idMod = mod.Key;
            var info = mod.Value;
            FileHelper.SetModsPath(info.FolderPath);
            DataBaseManager.Instance.LoadCustomDataBase(info.DbPath);

            CargarPorMod(idMod);
            CargarPorModEspecial(idMod);
            CargarTilesTextureData(idMod);
            CargarMateriales(idMod, info.Name);
        }
    }
}