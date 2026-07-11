using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Accesories.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Character.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Materials;
using GodotEcsArch.sources.WindowsDataBase.Projectile.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotEcsArch.sources.WindowsDataBase.TilesTexture;
using SadRogue.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using static Flecs.NET.Core.Ecs.Units;
using static Flecs.NET.Core.Ecs.Units.Masses;


namespace GodotEcsArch.sources.managers.Mods;


public class AtlasModsManager : SingletonBase<AtlasModsManager>
{
    private class AtlasMetadata
    {
        public Type KeyType { get; init; }
        public object Atlas { get; init; }
    }
    // =========================================================
    // ATLASES (ID -> DATA)
    // =========================================================

    int palleteCount = 0;
    private readonly Dictionary<long, int> spriteLookup = new();
    private readonly Dictionary<int, TileSpriteData> spritePallete = new();
    
    private readonly Dictionary<ushort, MultiIndex<int, int>> materialIdSprites = new();

    // Datos solo cacheados nunca en disco, resueltos de otra forma
    private readonly AtlasMods<int, MaterialData> materiales = new(); // estos nunca se guardan en disco solo en memoria, resuelto de otra forma
    private readonly AtlasMods<long, TileSpriteData> tileSpriteData = new(); // estos nunca se guardan en disco solo en memoria
    private readonly AtlasMods<long, AutoTileSpriteData> autoSpriteData = new(); // estos nunca se guardan en disco solo en memoria
    private readonly AtlasMods<long, DualTileTemplate> dualTileTemplate = new();

    // si requieren persistencia
    private readonly AtlasMods<ushort, ResourceSourceData> fuenteRecursos = new();    
    private readonly AtlasMods<ushort, TerrainData> terrainData = new();    
    private readonly AtlasMods<ushort, TerrainDataTransition> terrainDataTransicion = new();
    private readonly AtlasMods<ushort, TerrainBaseData> terrenos = new ();
    private readonly AtlasMods<ushort, RampsData> rampsData = new();
    private readonly AtlasMods<ushort, CaminosData> caminosData = new();
    private readonly AtlasMods<ushort, DecorationData> decorationData = new();
    private readonly AtlasMods<ushort, SuperficieData> superficieData = new();

    private readonly AtlasMods<int, BuildingData> buildingData = new(); // deben cambiar a ushort
    private readonly AtlasMods<int, BulletData> bulletData = new(); // deben cambiar a ushort
    private readonly AtlasMods<int, CharacterModelBaseData> characterData = new(); // deben cambiar a ushort

    // string-key atlas
    private readonly AtlasMods<string, TileTextureData> tilesTextureData = new();
    private readonly AtlasMods<string, TileSpriteData> tilesSpriteDataDual = new();

    // =========================================================
    // INDEXES (RELACIONES / QUERIES)
    // =========================================================

    private readonly Dictionary<ushort, MultiIndex<int, TileTextureData>> _tilesByMaterialIndex = new();
    private readonly Dictionary<ushort, MultiIndex<int, TileSpriteData>> _tilesSpriteByMaterialIndex = new();

    // =========================================================
    // TYPE REGISTRY (interno seguro)
    // =========================================================

    private readonly Dictionary<Type, AtlasMetadata> _atlases = new();
    private void RegisterAtlas<TKey, T>(AtlasMods<TKey, T> atlas)
    where T : class
    where TKey : notnull
    {
        _atlases[typeof(T)] = new AtlasMetadata
        {
            KeyType = typeof(TKey),
            Atlas = atlas
        };
    }

    private AtlasMods<TKey, T> GetAtlas<TKey, T>()
        where T : class
        where TKey : notnull
    {
        if (!_atlases.TryGetValue(typeof(T), out var meta))
            return null;



        return meta.Atlas as AtlasMods<TKey, T>;
    }

    // =========================================================
    // MOD RESOLUTION (SIN CACHE, SIMPLE Y CORRECTO)
    // =========================================================

    private bool TryGetModId(string modName, out ushort modId)
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
    public static int GetSpriteUniqueId(long tileSpriteId)
    {
        return Instance.IdSpriteUniqueId(tileSpriteId, out _);
    }
    public static int GetSpriteUniqueId(long tileSpriteId, out TileSpriteData tileSpriteData)
    {
        return Instance.IdSpriteUniqueId(tileSpriteId, out tileSpriteData);        
    }
    public static bool TryGetTileSprite(int idSpriteInternal, out TileSpriteData tileSpriteData)
    {
        return Instance.GetTileSprite(idSpriteInternal, out tileSpriteData);
    }
    public static List<int> GetAllSpriteByMaterial(string nameMod,int idMaterial)
    {
        return Instance.GetSpriteByMaterial(nameMod, idMaterial);
    }
    private List<int> GetSpriteByMaterial(string nameMod, int idMaterial)
    {
        if (!TryGetModId(nameMod, out var modId))
            return null;
        if (materialIdSprites.TryGetValue(modId, out var multiIndex))
        {
            multiIndex.TryGet(idMaterial, out var list);
            return list;
        }
        return null;        
    }
    private bool GetTileSprite(int idSpriteInternal, out TileSpriteData tileSpriteData)
    {
        if (spritePallete.TryGetValue(idSpriteInternal, out tileSpriteData))            
            return true;
        tileSpriteData = null;
        return false;
    }
    private int IdSpriteUniqueId(long tileSpriteId, out TileSpriteData tileSpriteData)
    {
        var key = spriteLookup[tileSpriteId];
        tileSpriteData = spritePallete[key];
        return key;        
    }

    public static T Get<T>(string modName, int id) where T : class
        => Instance.InternalGet<T, int>(modName, id);

    public static T Get<T>(string modName, long id) where T : class
    => Instance.InternalGet<T, long>(modName, id);

    public static T Get<T>(string modName, ushort id) where T : class
=> Instance.InternalGet<T, ushort>(modName, id);

    public static bool TryGet<T>(string modName, int id, out T value) where T : class
        => Instance.InternalTryGet<T, int>(modName, id, out value);

    private T InternalGet<T, TKey>(string modName, TKey id)
        where T : class where TKey : notnull
    {
        if (!TryGetModId(modName, out var modId))
            return null;

        var atlas = GetAtlas<TKey, T>();
        return atlas?.Get(modId, id);
    }

    private bool InternalTryGet<T, TKey>(string modName, TKey id, out T value)
        where T : class where TKey : notnull
    {
        value = null;

        if (!TryGetModId(modName, out var modId))
            return false;

        var atlas = GetAtlas<TKey, T>();

        return atlas != null && atlas.TryGet(modId, id, out value);
    }

    public static IEnumerable<T> GetAll<T>(string modName) where T : class
    => Instance.InternalGetAll<T>(modName);

    private IEnumerable<T> InternalGetAll<T>(string modName)
      where T : class
    {
        if (!TryGetModId(modName, out var modId))
            yield break;

        if (!_atlases.TryGetValue(typeof(T), out var meta))
            yield break;

        switch (meta.KeyType)
        {
            case var t when t == typeof(int):
                foreach (var item in ((AtlasMods<int, T>)meta.Atlas).GetAll(modId))
                    yield return item;
                break;

            case var t when t == typeof(ushort):
                foreach (var item in ((AtlasMods<ushort, T>)meta.Atlas).GetAll(modId))
                    yield return item;
                break;

            case var t when t == typeof(long):
                foreach (var item in ((AtlasMods<long, T>)meta.Atlas).GetAll(modId))
                    yield return item;
                break;

            case var t when t == typeof(string):
                foreach (var item in ((AtlasMods<string, T>)meta.Atlas).GetAll(modId))
                    yield return item;
                break;
        }
    }
    // =========================================================
    // 🔴 API HOT PATH (modId directo)
    // =========================================================

    public static T GetFast<T>(byte modId, int id) where T : class
        => Instance.InternalGetFast<T, int>(modId, id);

    public static bool TryGetFast<T>(ushort modId, int id, out T value) where T : class
        => Instance.InternalTryGetFast<T, int>(modId, id, out value);

    private T InternalGetFast<T, TKey>(ushort modId, TKey id)
        where T : class where TKey : notnull
    {
        var atlas = GetAtlas<TKey, T>();
        return atlas?.Get(modId, id);
    }

    private bool InternalTryGetFast<T, TKey>(ushort modId, TKey id, out T value)
        where T : class where TKey : notnull
    {
        value = null;

        var atlas = GetAtlas<TKey, T>();
        return atlas != null && atlas.TryGet(modId, id, out value);
    }

    // 

    

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

    public static bool TryGetTileSprite(string modName, string key, int index, out TileSpriteData value)
        => Instance.InternalTryGetTileSprite(modName, key, out value);

    private bool InternalTryGetTileSprite(string modName,string key, out TileSpriteData value)
    {
        value = null;
        if (!TryGetModId(modName, out var modId))
            return false;

        bool resp = tilesSpriteDataDual.TryGet(modId,key, out value);
        return resp;
    }

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

    private void CargarMateriales(ushort idMod, string name)
    {
        var data = DataBaseManager.Instance.FindAll<MaterialData>();

        foreach (var item in data)
        {
            item.idNameMod = name +":"+ item.id;
            materiales.Register(idMod, item.id, item);
        }
    
    }

    private void CargarPorModGenerico(ushort idMod, string name)
    {
        CargarDatosBaseTileSprite(tileSpriteData, idMod, name);
        CargarDatosBase(autoSpriteData, idMod, name);
        CargarDatosBase(dualTileTemplate, idMod, name);
    }

 

    private void CargarPorMod(ushort idMod, string name)
    {
        CargarDatos(fuenteRecursos, idMod,name);        
        CargarDatos(terrainData, idMod, name);
        CargarDatos(terrainDataTransicion, idMod, name);                
        CargarDatos(terrenos, idMod, name);
        CargarDatos(rampsData, idMod, name);
        CargarDatos(decorationData, idMod, name);
        CargarDatos(caminosData, idMod, name);
        CargarDatos(superficieData,idMod, name);

    }

    private void CargarPorModEspecial(ushort idMod)
    {
        CargarDatosEspecial(buildingData, idMod);
        CargarDatosEspecial(bulletData, idMod);
        CargarDatosEspecial(characterData, idMod);
    }

    private void CargarTilesTextureData(ushort idMod)
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
    private void CargarDatosBaseTileSprite(AtlasMods<long, TileSpriteData> atlas, ushort idMod, string nameMod) 
    {
        var data = DataBaseManager.Instance.FindAll<TileSpriteData>();

        if (!materialIdSprites.ContainsKey(idMod))
            materialIdSprites[idMod] = new MultiIndex<int, int>();

        var indexMod = materialIdSprites[idMod];

        foreach (var item in data)
        {
            var tileSprite = item as TileSpriteData;
            tileSprite.idMod = idMod;
            tileSprite.nameMod = nameMod;
            int id = GetOrCreateSpriteId(tileSprite.id, tileSprite);               
            indexMod.Add(tileSprite.idMaterial, id);
        }
    }
    
    int GetOrCreateSpriteId(long tileSpriteId, TileSpriteData tileSpriteData)
    {        
        if (spriteLookup.TryGetValue(tileSpriteId, out int id))
            return id;
        palleteCount++;
        int newId = palleteCount;        
        spriteLookup[tileSpriteId] = newId;
        spritePallete[newId] = tileSpriteData;  
        return newId;
    }
 
    public List<Godot.Color> CalculateUVFromId(MaterialModData data,bool mirrorX, bool mirrorY, FrameData[] framesArray)
    {        
        var offset = new Vector2(data.xInAtlas, data.yInAtlas);
        List<Godot.Color> uvList = new List<Godot.Color>();
        foreach (var spriteData in framesArray)
        {
            // Calcular nueva posición y tamaño
            float newX = offset.X + spriteData.x;
            float newY = offset.Y + spriteData.y;

            Godot.Color uvColor = new Godot.Color();
            uvColor.R = newX;
            uvColor.G = newY;
            uvColor.B = mirrorX ? -spriteData.widht : spriteData.widht;
            uvColor.A = mirrorY ? -spriteData.height : spriteData.height;

            uvList.Add(uvColor);
        }
        return uvList;
    }
    public Godot.Color CalculateUVFromId(MaterialModData data, float xFormat, float yFormat, float widhtFormat, float heightFormat, bool mirrorX, bool mirrorY)
    {

        
        var offset = new Vector2(data.xInAtlas, data.yInAtlas);

        // Calcular nueva posición y tamaño
        float newX = offset.X + xFormat;
        float newY = offset.Y + yFormat;

        Godot.Color uvColor = new Godot.Color();
        uvColor.R = newX;
        uvColor.G = newY;
        uvColor.B = mirrorX ? -widhtFormat : widhtFormat;
        uvColor.A = mirrorY ? -heightFormat : heightFormat;
        return uvColor;        
    }
    private void CargarDatosBase<T>(AtlasMods<long, T> atlas, ushort idMod, string nameMod) where T : IdDataLong
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
        {
            item.idMod = idMod;
            item.nameMod = nameMod;
            atlas.Register(idMod, item.id, item);
        }
    }
    private void CargarDatos<T>(AtlasMods<ushort, T> atlas, ushort idMod, string nameMod) where T : IdDataLong
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
        {
            item.idMod = idMod;
            item.nameMod = nameMod;
            atlas.Register(idMod, item.idSave, item);
        }
    }

    private void CargarDatosEspecial<T>(AtlasMods<int, T> atlas, ushort idMod) where T : IdData
    {
        var data = DataBaseManager.Instance.FindAll<T>();

        foreach (var item in data)
            atlas.Register(idMod, item.id, item);
    }
    public static Dictionary<ushort, Dictionary<TKey, T>> GetDictionaryAll<T, TKey>() where T : class
    => Instance.InternalGetDictionaryAll<T, TKey>();
    private Dictionary<ushort, Dictionary<TKey, T>> InternalGetDictionaryAll<T, TKey>()
    where T : class where TKey : notnull
    {
        var atlas = GetAtlas<TKey, T>();

        if (atlas == null)
            return new Dictionary<ushort, Dictionary<TKey, T>>();

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
        atlasTexture.Atlas = data.textureVisual;

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
        tileSpriteData.Clear();
        autoSpriteData.Clear();
        terrainData.Clear();
        terrainDataTransicion.Clear();
        buildingData.Clear();
        bulletData.Clear();
        characterData.Clear();
        tilesTextureData.Clear();
        dualTileTemplate.Clear();
        terrenos.Clear();
        rampsData.Clear();
        decorationData.Clear();
        caminosData.Clear();
        superficieData.Clear();
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
        
        RegisterAtlas(fuenteRecursos);
        RegisterAtlas(tileSpriteData);
        RegisterAtlas(autoSpriteData);
        RegisterAtlas(terrainData);
        RegisterAtlas(terrainDataTransicion);
        RegisterAtlas(buildingData);
        RegisterAtlas(bulletData);
        RegisterAtlas(characterData);
        RegisterAtlas(tilesTextureData);
        RegisterAtlas(dualTileTemplate);
        RegisterAtlas(terrenos);
        RegisterAtlas(rampsData);
        RegisterAtlas(decorationData);
        RegisterAtlas(caminosData);
        RegisterAtlas(superficieData);

        foreach (var mod in TableMods.Instance.ObtenerTodos())
        {
            ushort idMod = mod.Key;
            var info = mod.Value;
            FileHelper.SetModsPath(info.FolderPath);
            DataBaseManager.Instance.LoadCustomDataBase(info.DbPath);

            CargarPorMod(idMod, info.Name);
            CargarPorModGenerico(idMod, info.Name);

            CargarPorModEspecial(idMod);            
            CargarTilesTextureData(idMod);            
        }
    }

    internal void FirstLoadMaterial()
    {
        RegisterAtlas(materiales);
        foreach (var mod in TableMods.Instance.ObtenerTodos())
        {
            ushort idMod = mod.Key;
            var info = mod.Value;
            FileHelper.SetModsPath(info.FolderPath);
            DataBaseManager.Instance.LoadCustomDataBase(info.DbPath);            
            CargarMateriales(idMod, info.Name);
        }
        
    }
}

