using Godot;
using GodotEcsArch.sources.BlackyEngine.Data;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.Building.DataBase;
using GodotEcsArch.sources.WindowsDataBase.CharacterCreator.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Resources.DataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
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

[MessagePackObject]
public class PaletteSaveContainer
{
    [Key(0)]
    public List<GenericPersistenceData> Items { get; set; } = new();
}

public class BlackyPalletesPersistence
{
    public static BlackyGenericPalette<TerrainBaseData> terrainPalette { get; } = new("Terreno");
    public static BlackyGenericPalette<RampsData> rampsPalette { get; } = new("Rampas");
    public static BlackyGenericPalette<DecorationData> decorationsPalette { get; } = new("Adornos");
    public static BlackyGenericPalette<CaminosData> pathsPalette { get; } = new("Caminos");
    public static BlackyGenericPalette<SuperficieData> surfacesPalette { get; } = new("Superficies");
    public static BlackyGenericPalette<BiomaData> biomePalette { get; } = new("Biomas", true);
    public static BlackyGenericPalette<CharacterModelBaseData> characterPalette { get; } = new("Personajes", true);
    public static BlackyGenericPalette<ResourceSourceData> resourcesPalette { get; } = new("Recursos", true);
    public static BlackyGenericPalette<BuildingData> buildingPalette { get; } = new("Edificios", true);
}

public class BlackyGenericPalette<T> where T : class
{
    private bool isDirty = false;
    private readonly string _paletteName;

    private readonly Dictionary<PairCacheData, ushort> _cache = new();

    // Usamos una lista en lugar de un diccionario para evitar problemas de serialización JSON en MessagePack.
    // El índice de la lista actúa directamente como el ushort ID.
    private List<GenericPersistenceData> _persistent = new()
    {
        // Elemento en la posición 0 (para que los IDs válidos comiencen en 1)
        new GenericPersistenceData { ModName = "", id = 0 }
    };

    public BlackyGenericPalette(string paletteName, bool LoadAll = false)
    {
        _paletteName = paletteName;
        if (LoadAll)
        {
            LoadAllData();
        }
    }

    private void LoadAllData()
    {
        isDirty = true;
        Dictionary<ushort, Dictionary<long, T>> allInfo = AtlasModsManager.GetDictionaryAll<T, long>();
        foreach (var item in allInfo)
        {
            var modName = TableMods.Instance.ObtenerNombre(item.Key);

            foreach (var bucket in item.Value)
            {
                if (_persistent.Count >= ushort.MaxValue)
                    throw new InvalidOperationException("Palette limit reached.");

                ushort paletteId = (ushort)_persistent.Count;
                var key = new PairCacheData(modName, bucket.Key);
                _cache[key] = paletteId;

                _persistent.Add(new GenericPersistenceData
                {
                    ModName = modName,
                    id = bucket.Key
                });
            }
        }
    }

    /// <summary>
    /// Devuelve un diccionario con todos los IDs de la paleta y sus respectivos datos deserializados/cargados en memoria.
    /// </summary>
    public Dictionary<ushort, T> GetAllPallete()
    {
        var result = new Dictionary<ushort, T>();

        for (ushort i = 0; i < _persistent.Count; i++)
        {
            T data = GetData(i);
            if (data != null)
            {
                result[i] = data;
            }
        }

        return result;
    }

    public T GetData(ushort id)
    {
        if (id >= 0 && id < _persistent.Count)
        {
            var persistenceData = _persistent[id];
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

        if (_persistent.Count >= ushort.MaxValue)
            throw new InvalidOperationException("Palette limit reached.");

        isDirty = true;
        paletteId = (ushort)_persistent.Count;

        _cache[key] = paletteId;

        _persistent.Add(new GenericPersistenceData
        {
            ModName = modName,
            id = originalId
        });

        return paletteId;
    }

    public IReadOnlyList<GenericPersistenceData> GetPersistentPalette()
        => _persistent;

    public void LoadPersistentPalette(List<GenericPersistenceData> persistentData)
    {
        _persistent = persistentData ?? new List<GenericPersistenceData>();

        // Si la lista está vacía o no tiene el elemento 0 de seguridad, lo insertamos
        if (_persistent.Count == 0)
        {
            _persistent.Add(new GenericPersistenceData { ModName = "", id = 0 });
        }

        _cache.Clear();

        // Empezamos desde 1 para ignorar el índice 0 en la caché
        for (int i = 1; i < _persistent.Count; i++)
        {
            var kv = _persistent[i];
            var pair = new PairCacheData(kv.ModName, kv.id);
            _cache[pair] = (ushort)i;
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

        var container = new PaletteSaveContainer
        {
            Items = _persistent
        };

        byte[] bytes = MessagePackSerializer.Serialize(container);

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
            LoadPersistentPalette(new List<GenericPersistenceData>());
            isDirty = false;
            return;
        }

        try
        {
            PaletteSaveContainer container;

            if (format == SaveFormat.Json)
            {
                string json = File.ReadAllText(fullPath);
                byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
                container = MessagePackSerializer.Deserialize<PaletteSaveContainer>(bytes);
            }
            else
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                container = MessagePackSerializer.Deserialize<PaletteSaveContainer>(bytes);
            }

            LoadPersistentPalette(container?.Items ?? new List<GenericPersistenceData>());
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[Palette] Error al cargar {_paletteName}: {ex.Message}");
            LoadPersistentPalette(new List<GenericPersistenceData>());
        }

        isDirty = false;
    }
}