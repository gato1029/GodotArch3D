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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.managers.Mods;

public class AtlasModsManager : SingletonBase<AtlasModsManager>
{

    
    // 🔹 Atlas por tipo de dato. Cada mod se registra con su ID de mod y el ID del dato, apuntando al dato en sí.    
    private AtlasModsGeneric<ResourceSourceData> fuenteRecursos = new();
    private AtlasModsGeneric<TileSpriteData> spriteData = new();
    private AtlasModsGeneric<AutoTileSpriteData> autoSpriteData = new();
    private AtlasModsGeneric<TerrainData> terrainData = new();
    private AtlasModsGeneric<TerrainDataTransition> terrainDataTransicion = new();

    // Estos datos son especiales porque no heredan de IdDataLong, sino de IdData. Por eso necesitan un método de carga especial
    private AtlasModsGeneric<BuildingData> buildingData = new();
    private AtlasModsGeneric<BulletData> bulletData = new();
    private AtlasModsGeneric<CharacterModelBaseData> characterData = new();

    private Dictionary<Type, object> _atlasByType = new();


    private Dictionary<string, TileTextureData> tilesTextureData = new();
    protected override void Initialize()
    {
        // Registrar atlas
        
        _atlasByType[typeof(ResourceSourceData)] = fuenteRecursos;
        _atlasByType[typeof(TileSpriteData)] = spriteData;
        _atlasByType[typeof(TerrainData)] = terrainData;
        _atlasByType[typeof(TerrainDataTransition)] = terrainDataTransicion;
        _atlasByType[typeof(AutoTileSpriteData)] = autoSpriteData;
        _atlasByType[typeof(BuildingData)] = buildingData;
        _atlasByType[typeof(BulletData)] = bulletData;
        _atlasByType[typeof(CharacterModelBaseData)] = characterData;


        foreach (var mod in TableMods.Instance.ObtenerTodos())
        {
            var idMod = mod.Key;
            var info = mod.Value;

            DataBaseManager.Instance.LoadCustomDataBase(info.DbPath);

            CargarPorMod(idMod);
            CargarPorModEspecial(idMod);
            CargarTilesTextureData();
        }
        
        
    }

    private void CargarTilesTextureData()
    {
        var data = DataBaseManager.Instance.FindAll<TileTextureData>();

        foreach (var item in data)
        {
            tilesTextureData.Add(item.name, item);
        }
    }

    private void CargarPorModEspecial(byte idMod)
    {
        CargarDatosEspecial(buildingData, idMod);
        CargarDatosEspecial(bulletData, idMod);
        CargarDatosEspecial(characterData, idMod);

    }

    // 🔹 Carga todos los tipos de datos de un mod ya con DB cargada
    private void CargarPorMod(byte idMod)
    {        
        CargarDatos(fuenteRecursos, idMod);
        CargarDatos(spriteData, idMod);
        CargarDatos(terrainData, idMod);
        CargarDatos(terrainDataTransicion, idMod);
        CargarDatos(autoSpriteData, idMod);
    }

    public bool TryGet(string nameMod, ushort index, out TileTextureData value) 
    {
        string key = nameMod + ":" + index;
        value = default;

        if (!tilesTextureData.TryGetValue(key, out value))
            return false;
        
        return value != null;
    }
    public bool TryGet<T>(string nameMod, int dataId, out T value) where T : class
    {
        var modId = TableMods.Instance.ObtenerId(nameMod).Value;
        value = default;

        if (!_atlasByType.TryGetValue(typeof(T), out var atlasObj))
            return false;

        var atlas = (AtlasModsGeneric<T>)atlasObj;

        value = atlas.Obtener(modId, dataId);
        return value != null;
    }

    public T Get<T>(string nameMod, int dataId) where T : class
    {
        var modId = TableMods.Instance.ObtenerId(nameMod).Value;
        if (!_atlasByType.TryGetValue(typeof(T), out var atlasObj))
            throw new Exception($"No hay atlas registrado para el tipo {typeof(T).Name}");

        var atlas = (AtlasModsGeneric<T>)atlasObj;

        return atlas.Obtener(modId, dataId);
    }

    // 🔹 Método genérico reutilizable
    private void CargarDatosEspecial<T>(AtlasModsGeneric<T> atlas, byte idMod) where T : IdData
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
        {
            atlas.Registrar(idMod, item.id, item);
        }
    }
    // 🔹 Método genérico reutilizable
    private void CargarDatos<T>(AtlasModsGeneric<T> atlas, byte idMod) where T : IdDataLong
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
        {
            atlas.Registrar(idMod, item.idSave, item);
        }
    }


    internal void FirstLoad()
    {
        GD.Print("AtlasModsManager: FirstLoad - Cargando mods...");
    }
}
