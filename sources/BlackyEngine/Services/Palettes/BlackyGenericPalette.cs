using Godot;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using MessagePack;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

using System.IO;

namespace GodotEcsArch.sources.BlackyEngine.Services.Palettes;

[MessagePackObject]
public struct GenericPersistenceData
{
    [Key(0)]
    public string ModName { get; set; }

    [Key(1)]
    public long id { get; set; }
}

public readonly record struct PairCacheData(
    string ModName,
    long Id
);

public class BlackyPalletesPersistence
{
    public static BlackyGenericPalette<TerrainBaseData> terrainPalette { get; } = new("Terreno");
    public static BlackyGenericPalette<RampsData> rampsPalette { get; } = new("Rampas");
    public static BlackyGenericPalette<DecorationData> decorationsPalette { get; } = new("Adornos");
    public static BlackyGenericPalette<CaminosData> pathsPalette { get; } = new("Caminos");
    public static BlackyGenericPalette<SuperficieData> surfacesPalette { get; } = new("Superficies");
    public static BlackyGenericPalette<BiomaData> biomePalette { get; } = new("Biomas",true);
}

public class BlackyGenericPalette<T> where T : class
{
    private ushort _nextId = 1;
    private bool isDirty = false;
    private bool isfull = false;
    private readonly string _paletteName; // "terrain", "ramps", "decorations", "paths", "surfaces", etc


    private readonly Dictionary<PairCacheData, ushort> _cache = new();
    private Dictionary<ushort, GenericPersistenceData> _persistent = new();
    public BlackyGenericPalette(string paletteName, bool LoadAll=false)
    {
        _paletteName = paletteName;
        if (LoadAll)
        {
            LoadAllData();
        }
    }
    private void LoadAllData()
    {            
        Dictionary<ushort, Dictionary<long, T>> allInfo = AtlasModsManager.GetDictionaryAll<T, long>();
        foreach (var item in allInfo)
        {
            var modName = TableMods.Instance.ObtenerNombre(item.Key); 

            foreach (var bucket in item.Value)
            {
                var data = bucket.Value;
                if (_nextId == ushort.MaxValue)
                    throw new InvalidOperationException("Palette limit reached.");
                
               ushort paletteId = _nextId++;
               var key = new PairCacheData(modName, bucket.Key);
               _cache[key] = paletteId;

                _persistent[paletteId] = new GenericPersistenceData
                {
                    ModName = modName,
                    id = bucket.Key
                };
            }
        }
    }
    /// <summary>
    /// Devuelve un diccionario con todos los IDs de la paleta y sus respectivos datos deserializados/cargados en memoria.
    /// </summary>
    public Dictionary<ushort, T> GetAllPallete()
    {
        //if (!isfull)
        //{
        //    LoadAllData();
        //    isfull = true;
        //}
        var result = new Dictionary<ushort, T>();

        foreach (var kvp in _persistent)
        {
            ushort paletteId = kvp.Key;
            T data = GetData(paletteId);

            // Opcional: Validar si el dato no es nulo antes de agregarlo
            if (data != null)
            {
                result[paletteId] = data;
            }
        }

        return result;
    }
    public T GetData(ushort id)
    {
        if (_persistent.TryGetValue(id, out var persistenceData))
        {
            return AtlasModsManager.Get<T>(persistenceData.ModName, persistenceData.id);
        }
        return null;
    }
    public ushort GetIdPersistence(string modName, long originalId, out T data)
    {
        data = AtlasModsManager.Get<T>(modName, originalId);

        var key = new PairCacheData(modName, originalId);

        if (_cache.TryGetValue(key, out ushort paletteId))
        {
            return paletteId;
        }

        if (_nextId == ushort.MaxValue)
            throw new InvalidOperationException("Palette limit reached.");

        isDirty = true;
        paletteId = _nextId++;

        _cache[key] = paletteId;

        _persistent[paletteId] = new GenericPersistenceData
        {
            ModName = modName,
            id = originalId
        };

        return paletteId;
    }

    public IReadOnlyDictionary<ushort, GenericPersistenceData> GetPersistentPalette()
        => _persistent;



    public void LoadPersistentPalette(
        Dictionary<ushort, GenericPersistenceData> persistentData)
    {
        _persistent = new Dictionary<ushort, GenericPersistenceData>(persistentData);
        _cache.Clear();
        _nextId = 1;

        foreach (var kv in persistentData)
        {
            var pair = new PairCacheData(
                kv.Value.ModName,
                kv.Value.id);

            _cache[pair] = kv.Key;

            if (kv.Key >= _nextId)
                _nextId = (ushort)(kv.Key + 1);
        }
    }

    // =====================================================
    // SAVE / LOAD
    // =====================================================

    private string GetFilePath(string rootPath, SaveFormat format)
    {
        string folder = Path.Combine(rootPath, "palettes");
        Directory.CreateDirectory(folder);

        string ext = format == SaveFormat.Json ? "json" : "bin";
        return Path.Combine(folder, $"palette_{_paletteName}.{ext}");
    }

    /// <summary>
    /// Guarda esta paleta a disco solo si tiene cambios pendientes (isDirty).
    /// </summary>
    public void Save(string rootPath, SaveFormat format)
    {
        if (!isDirty)
            return;

        string fullPath = GetFilePath(rootPath, format);
        byte[] bytes = MessagePackSerializer.Serialize(_persistent);

        if (format == SaveFormat.Json)
        {
            string json = MessagePackSerializer.ConvertToJson(bytes);
            File.WriteAllText(fullPath, json);
        }
        else
        {
            File.WriteAllBytes(fullPath, bytes);
        }

        isDirty = false;
    }

    /// <summary>
    /// Carga esta paleta desde disco. Si no existe el archivo (mundo nuevo), queda vacía.
    /// </summary>
    public void Load(string rootPath, SaveFormat format)
    {
        string fullPath = GetFilePath(rootPath, format);

        if (!File.Exists(fullPath))
        {
            LoadPersistentPalette(new Dictionary<ushort, GenericPersistenceData>());
            isDirty = false;
            return;
        }

        Dictionary<ushort, GenericPersistenceData> data;

        if (format == SaveFormat.Json)
        {
            string json = File.ReadAllText(fullPath);
            byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
            data = MessagePackSerializer.Deserialize<Dictionary<ushort, GenericPersistenceData>>(bytes);
        }
        else
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            data = MessagePackSerializer.Deserialize<Dictionary<ushort, GenericPersistenceData>>(bytes);
        }

        LoadPersistentPalette(data);
        isDirty = false;
    }
}