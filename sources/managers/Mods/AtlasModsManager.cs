using Godot;
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
using System.Collections.Generic;
using System.Collections;

namespace GodotEcsArch.sources.managers.Mods;

public class AtlasModsManager : SingletonBase<AtlasModsManager>
{
    // =========================================================
    // ATLASS
    // =========================================================

    private readonly AtlasModsGeneric<int, ResourceSourceData> fuenteRecursos = new();
    private readonly AtlasModsGeneric<int, TileSpriteData> spriteData = new();
    private readonly AtlasModsGeneric<int, AutoTileSpriteData> autoSpriteData = new();
    private readonly AtlasModsGeneric<int, TerrainData> terrainData = new();
    private readonly AtlasModsGeneric<int, TerrainDataTransition> terrainDataTransicion = new();

    private readonly AtlasModsGeneric<int, BuildingData> buildingData = new();
    private readonly AtlasModsGeneric<int, BulletData> bulletData = new();
    private readonly AtlasModsGeneric<int, CharacterModelBaseData> characterData = new();

    private readonly AtlasModsGeneric<string, TileTextureData> tilesTextureData = new();
    private readonly Dictionary<byte, Dictionary<int, List<TileTextureData>>> _tilesByMaterialIndex = new();

    private readonly Dictionary<Type, IAtlasModsGeneric> _atlasByType = new();

    // =========================================================
    // INIT
    // =========================================================

    protected override void Initialize()
    {
        RegistrarAtlas();

        foreach (var mod in TableMods.Instance.ObtenerTodos())
        {
            byte idMod = mod.Key;
            var info = mod.Value;

            DataBaseManager.Instance.LoadCustomDataBase(info.DbPath);

            CargarPorMod(idMod);
            CargarPorModEspecial(idMod);
            CargarTilesTextureData(tilesTextureData, idMod);
        }
    }

    private void RegistrarAtlas()
    {
        _atlasByType[typeof(ResourceSourceData)] = fuenteRecursos;
        _atlasByType[typeof(TileSpriteData)] = spriteData;
        _atlasByType[typeof(AutoTileSpriteData)] = autoSpriteData;
        _atlasByType[typeof(TerrainData)] = terrainData;
        _atlasByType[typeof(TerrainDataTransition)] = terrainDataTransicion;
        _atlasByType[typeof(BuildingData)] = buildingData;
        _atlasByType[typeof(BulletData)] = bulletData;
        _atlasByType[typeof(CharacterModelBaseData)] = characterData;
        _atlasByType[typeof(TileTextureData)] = tilesTextureData;
    }

    // =========================================================
    // CARGA
    // =========================================================

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

    private void CargarTilesTextureData(AtlasModsGeneric<string, TileTextureData> atlas, byte idMod)
    {
        var data = DataBaseManager.Instance.FindAll<TileTextureData>();

        // Inicializamos el contenedor del mod para el índice si no existe
        if (!_tilesByMaterialIndex.ContainsKey(idMod))
            _tilesByMaterialIndex[idMod] = new Dictionary<int, List<TileTextureData>>();

        var modIndex = _tilesByMaterialIndex[idMod];

        foreach (var item in data)
        {
            // 1. Registro original (para que TryGet<string, TileTextureData> siga funcionando)
            atlas.Registrar(idMod, item.name, item);

            // 2. Registro en el índice de materiales
            if (!modIndex.ContainsKey(item.idMaterial))
                modIndex[item.idMaterial] = new List<TileTextureData>();

            modIndex[item.idMaterial].Add(item);
        }
    }

    public List<TileTextureData> GetTilesByMaterial(string nameMod, int idMaterial)
    {
        var modIdNullable = TableMods.Instance.ObtenerId(nameMod);
        if (modIdNullable == null) return new List<TileTextureData>();

        byte modId = modIdNullable.Value;

        // Buscamos en el índice secundario directamente
        if (_tilesByMaterialIndex.TryGetValue(modId, out var materialDict))
        {
            if (materialDict.TryGetValue(idMaterial, out var list))
            {
                return list; // O(1) - Acceso instantáneo
            }
        }

        return new List<TileTextureData>();
    }

    private void CargarDatos<T>(AtlasModsGeneric<int, T> atlas, byte idMod) where T : IdDataLong
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
            atlas.Registrar(idMod, item.idSave, item);
    }

    private void CargarDatosEspecial<T>(AtlasModsGeneric<int, T> atlas, byte idMod) where T : IdData
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
            atlas.Registrar(idMod, item.id, item);
    }

    // =========================================================
    // GET PUNTUAL
    // =========================================================

    public bool TryGet<TKey, T>(string nameMod, TKey dataId, out T value) where T : class
    {
        value = default;

        var modIdNullable = TableMods.Instance.ObtenerId(nameMod);
        if (modIdNullable == null)
            return false;

        byte modId = modIdNullable.Value;

        if (!_atlasByType.TryGetValue(typeof(T), out var atlas))
            return false;

        value = atlas.ObtenerRaw(modId, dataId) as T;
        return value != null;
    }

    public T Get<TKey, T>(string nameMod, TKey dataId) where T : class
    {
        var modIdNullable = TableMods.Instance.ObtenerId(nameMod);

        if (modIdNullable == null)
            throw new Exception($"No existe mod {nameMod}");

        byte modId = modIdNullable.Value;

        if (!_atlasByType.TryGetValue(typeof(T), out var atlas))
            throw new Exception($"No existe atlas para {typeof(T).Name}");

        T result = atlas.ObtenerRaw(modId, dataId) as T;

        if (result == null)
            throw new Exception($"No existe dataId {dataId} en {typeof(T).Name}");

        return result;
    }

    // =========================================================
    // DICCIONARIO POR MOD
    // =========================================================

    public Dictionary<TKey, T> GetDictionaryByMod<TKey, T>(string nameMod) where T : class
    {
        var modIdNullable = TableMods.Instance.ObtenerId(nameMod);

        if (modIdNullable == null)
            return new Dictionary<TKey, T>();

        byte modId = modIdNullable.Value;

        if (!_atlasByType.TryGetValue(typeof(T), out var atlas))
            return new Dictionary<TKey, T>();

        var result = atlas.ObtenerDictionaryRaw(modId) as Dictionary<TKey, T>;
        return result ?? new Dictionary<TKey, T>();
    }

    public IEnumerable<T> GetAllByMod<T>(string nameMod) where T : class
    {
        var modIdNullable = TableMods.Instance.ObtenerId(nameMod);
        if (modIdNullable == null)
            yield break;

        byte modId = modIdNullable.Value;

        if (!_atlasByType.TryGetValue(typeof(T), out var atlas))
            yield break;

        foreach (var item in atlas.ObtenerValoresRaw(modId))
        {
            if (item is T typed)
                yield return typed;
        }
    }

    // =========================================================
    // TODO GLOBAL (TODOS LOS MODS)
    // =========================================================

    public IEnumerable<T> GetAll<T>() where T : class
    {
        if (!_atlasByType.TryGetValue(typeof(T), out var atlas))
            yield break;

        foreach (var item in atlas.ObtenerValoresTodosRaw())
        {
            if (item is T typed)
                yield return typed;
        }
    }

    public IEnumerable<(byte modId, TKey key, T value)> GetDictionaryAll<TKey, T>() where T : class
    {
        if (!_atlasByType.TryGetValue(typeof(T), out var atlas))
            yield break;

        foreach (var raw in atlas.ObtenerTodoRaw())
        {
            if (raw is ValueTuple<byte, TKey, T> tuple)
                yield return tuple;
        }
    }

    // =========================================================
    // DEBUG
    // =========================================================

    internal void FirstLoad()
    {
        GD.Print("AtlasModsManager: FirstLoad - Cargando mods...");
    }
}