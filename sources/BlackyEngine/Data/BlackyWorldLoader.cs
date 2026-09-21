using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Data;

public class BlackyWorldLoader
{
    private readonly string _rootPath;
    private readonly SaveFormat _format;

    public BlackyWorldLoader(string nameMap, SaveFormat format = SaveFormat.Binary)
    {
        string basePath = CommonAtributes.RutaGuardadoMapas;
        _rootPath = Path.Combine(basePath, nameMap);
        _format = format;
    }

    // =====================================================
    // LOAD UNITS
    // =====================================================
    public GlobalUnitsSaveData LoadUnits()
    {
        string saveFolder = Path.Combine(_rootPath, "world");
        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"world_units.{extension}";
        string fullPath = Path.Combine(saveFolder, fileName);

        if (!File.Exists(fullPath))
        {
            return new GlobalUnitsSaveData();
        }

        return DeserializeFile<GlobalUnitsSaveData>(fullPath);
    }

    // =====================================================
    // LOAD ALL ENTITIES REGIONS
    // =====================================================
    public List<RegionEntitiesSaveData> LoadAllEntitiesRegions()
    {
        List<RegionEntitiesSaveData> allRegionsData = new();
        string regionFolder = Path.Combine(_rootPath, "regionsEntities");

        if (!Directory.Exists(regionFolder))
        {
            return allRegionsData;
        }

        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string searchPattern = $"*_entities.{extension}";
        string[] files = Directory.GetFiles(regionFolder, searchPattern);

        foreach (string filePath in files)
        {
            try
            {
                var regionData = DeserializeFile<RegionEntitiesSaveData>(filePath);
                if (regionData != null)
                {
                    allRegionsData.Add(regionData);
                }
            }
            catch (Exception)
            {
                // Manejo de errores opcional si algún archivo está corrupto
            }
        }

        return allRegionsData;
    }

    // =====================================================
    // LOAD ENTITIES REGION SPECIFIC
    // =====================================================
    public RegionEntitiesSaveData LoadEntitiesRegion(int regionX, int regionY)
    {
        string regionFolder = Path.Combine(_rootPath, "regionsEntities");
        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"region_{regionX}_{regionY}_entities.{extension}";
        string fullPath = Path.Combine(regionFolder, fileName);

        if (!File.Exists(fullPath))
        {
            return new RegionEntitiesSaveData
            {
                RegionX = regionX,
                RegionY = regionY
            };
        }

        return DeserializeFile<RegionEntitiesSaveData>(fullPath);
    }

    // =====================================================
    // LOAD ALL REGIONS (TERRAIN Y CAPAS VISUALES)
    // =====================================================
    public List<RegionSaveData> LoadAllRegions()
    {
        List<RegionSaveData> allRegionsData = new();
        string regionFolder = Path.Combine(_rootPath, "regions");

        if (!Directory.Exists(regionFolder))
        {
            return allRegionsData;
        }

        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string[] files = Directory.GetFiles(regionFolder, $"region_*.{extension}");

        foreach (string filePath in files)
        {
            if (filePath.Contains("_entities"))
            {
                continue;
            }

            try
            {
                RegionSaveData regionData = LoadRegion(filePath);
                if (regionData != null)
                {
                    allRegionsData.Add(regionData);
                }
            }
            catch (Exception)
            {
                // Manejo de errores opcional
            }
        }

        return allRegionsData;
    }

    public RegionSaveData LoadRegion(string path)
    {
        return DeserializeFile<RegionSaveData>(path);
    }

    // =====================================================
    // LOAD INFO MAP
    // =====================================================
    public SavedMapData LoadInfoMap()
    {
        string saveFolder = Path.Combine(_rootPath, "world");
        // El InfoMap siempre se guarda en JSON según tu lógica original
        string fileName = "world_info.json";
        string fullPath = Path.Combine(saveFolder, fileName);

        if (!File.Exists(fullPath))
        {
            return new SavedMapData();
        }

        string json = File.ReadAllText(fullPath);
        byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
        return MessagePackSerializer.Deserialize<SavedMapData>(bytes);
    }

    // =====================================================
    // LOAD PALETTES
    // =====================================================
    public void LoadPalettes()
    {
        BlackyPalletesPersistence.terrainPalette.Load(_rootPath, _format);
        BlackyPalletesPersistence.rampsPalette.Load(_rootPath, _format);
        BlackyPalletesPersistence.surfacesPalette.Load(_rootPath, _format);
        BlackyPalletesPersistence.decorationsPalette.Load(_rootPath, _format);
        BlackyPalletesPersistence.pathsPalette.Load(_rootPath, _format);

        // Entidades
        BlackyPalletesPersistence.buildingPalette.Load(_rootPath, _format);
        BlackyPalletesPersistence.resourcesPalette.Load(_rootPath, _format);
        BlackyPalletesPersistence.characterPalette.Load(_rootPath, _format);
    }

    // =====================================================
    // MÉTODO AUXILIAR DE DESERIALIZACIÓN (JSON / BINARY)
    // =====================================================
    private T DeserializeFile<T>(string fullPath)
    {
        string extension = Path.GetExtension(fullPath);

        if (extension == ".json")
        {
            string json = File.ReadAllText(fullPath);
            byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
            return MessagePackSerializer.Deserialize<T>(bytes);
        }
        else
        {
            byte[] data = File.ReadAllBytes(fullPath);
            return MessagePackSerializer.Deserialize<T>(data);
        }
    }
}