
using Flecs.NET.Core;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyEngine.State.RuntimeCaches;
using GodotEcsArch.sources.BlackyTiles.Data;
using GodotFlecs.sources.Flecs;
using GodotFlecs.sources.Flecs.Components;
using MessagePack;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using static Flecs.NET.Core.Ecs.Units;

namespace GodotEcsArch.sources.BlackyEngine.Data;

// =====================================================
// InfoMap
// =====================================================

[MessagePackObject]
public struct SavedMapData
{
    [Key(0)] public string Name { get; set; }
    [Key(1)] public BlackyWorldTypeDetail MapType { get; set; }
    [Key(2)] public int ChunkSize { get; set; }
    [Key(3)] public int HeightCount { get; set; }
    [Key(4)] public int seed { get; set; }
    [Key(5)] public int TileSizeX { get; set; }
    [Key(6)] public int TileSizeY { get; set; }
}

// =====================================================
// Unidades
// =====================================================

[MessagePackObject]
public struct SavedUnitData
{
    [Key(0)] public ushort TemplateId { get; set; }
    [Key(1)] public int Health { get; set; }
    [Key(2)] public float PosX { get; set; }
    [Key(3)] public float PosY { get; set; }
    [Key(4)] public int Height { get; set; } 
    [Key(5)] public ushort Team { get; set; } 
}

[MessagePackObject]
public class GlobalUnitsSaveData
{
    [Key(0)] public List<SavedUnitData> Units { get; set; } = new();
}

// =====================================================
// ENTIDADES POR REGIÓN (Independiente del Terreno)
// =====================================================

[MessagePackObject]
public struct SavedBuildingData
{
    [Key(0)] public ushort TemplateId { get; set; }
    [Key(1)] public int Health { get; set; }
    [Key(2)] public int WorldTileX { get; set; }
    [Key(3)] public int WorldTileY { get; set; }
    [Key(4)] public int Height { get; set; }
}

[MessagePackObject]
public struct SavedResourceData
{
    [Key(0)] public ushort TemplateId { get; set; }
    [Key(1)] public int Health { get; set; }
    [Key(2)] public int Amount { get; set; }
    [Key(3)] public int WorldTileX { get; set; }
    [Key(4)] public int WorldTileY { get; set; }
    [Key(5)] public int Height { get; set; }
}

[MessagePackObject]
public class EntityChunkSave
{
    [Key(0)] public int ChunkX { get; set; }
    [Key(1)] public int ChunkY { get; set; }
    [Key(2)] public List<SavedBuildingData> Buildings { get; set; } = new();
    [Key(3)] public List<SavedResourceData> Resources { get; set; } = new();
}

[MessagePackObject]
public class RegionEntitiesSaveData
{
    [Key(0)] public int RegionX { get; set; }
    [Key(1)] public int RegionY { get; set; }
    [Key(2)] public List<EntityChunkSave> EntityChunks { get; set; } = new();
}

// =====================================================
// TERRAIN
// =====================================================

[MessagePackObject]
public class TerrainHeightSave
{
    [Key(0)]
    public int Height { get; set; }

    [Key(1)]
    public ushort[] TerrainIds { get; set; }
}

[MessagePackObject]
public class TerrainChunkSave
{
    [Key(0)]
    public int ChunkX { get; set; }

    [Key(1)]
    public int ChunkY { get; set; }

    [Key(2)]
    public List<TerrainHeightSave> Heights { get; set; } = new();
}

// =====================================================
// Generico
// Aqui solo guardamos, el identificador del tipo de dato que guardamos
// por que estos datos o existen o no existe, no hay intermedios, y no se guarda mas informacion
// =====================================================

[MessagePackObject]
public class GenericHeightSave
{
    [Key(0)]
    public int Height { get; set; }

    [Key(1)]
    public ushort[] idData { get; set; } // representa su identificador de dato de lo que estamos guardando
}

[MessagePackObject]
public class GenericChunkSave
{
    [Key(0)]
    public int ChunkX { get; set; }

    [Key(1)]
    public int ChunkY { get; set; }

    [Key(2)]
    public List<GenericHeightSave> Heights { get; set; } = new();
}




// =====================================================
// REGION
// =====================================================

[MessagePackObject]
public class RegionSaveData
{
    [Key(0)]
    public int RegionX { get; set; }

    [Key(1)]
    public int RegionY { get; set; }

    [Key(2)]
    public List<TerrainChunkSave> TerrainChunks { get; set; } = new();

    [Key(3)]
    public List<GenericChunkSave> RampChunks { get; set; } = new();
    
    [Key(4)]
    public List<GenericChunkSave> SurfaceChunks { get; set; } = new();

    [Key(5)]
    public List<GenericChunkSave> DecorationChunks { get; set; } = new();

    [Key(6)]
    public List<GenericChunkSave> PathChunks { get; set; } = new();
}

// =====================================================
// PERSISTENCE
// =====================================================
public enum SaveFormat
{
    Binary,
    Json
}
public class BlackyWorldPersistence
{
    // luego cambiar el path donde deberia estar
    string path = "D:\\GitKraken\\MapsGame";
    string rootPath;
    string nameMap;

    private readonly BlackyWorldRegions _regions;
    private readonly BlackyTerrainWorldData _terrainWorld;
    private readonly BlackyRampVisualWorld _rampWorld;
    private readonly BlackySurfaceWorldData _superficiesData;
    private readonly BlackyDecorationWorldData _adornosData;
    private readonly BlackyPathWorldData _caminosData;
    private readonly BlackySpatialEntityMap _entidadesMap;
    private readonly FlecsManager _flecsManager;
    private readonly BlackyWorldConfig _blackyWorldConfig;
    private readonly SaveFormat _format;

    public BlackyWorldPersistence(string nameMap,
        BlackyWorldRegions regions,
        BlackyTerrainWorldData terrainWorld,
        BlackyRampVisualWorld rampWorld,
        BlackySurfaceWorldData superficiesData,
        BlackyDecorationWorldData adornosData,
        BlackyPathWorldData caminosData,
        BlackySpatialEntityMap entidades,
        FlecsManager flecs,
        BlackyWorldConfig inf,
        SaveFormat format = SaveFormat.Binary)
    {
        this.nameMap = nameMap;
        _regions = regions;
        _terrainWorld = terrainWorld;
        _rampWorld = rampWorld;
        _superficiesData = superficiesData;
        _adornosData = adornosData;
        _caminosData = caminosData;
        _format = format;
        _entidadesMap = entidades;
        _flecsManager = flecs;
        _blackyWorldConfig = inf;
        rootPath = path + "\\ " + nameMap;
    }

    // =====================================================
    // LOAD
    // =====================================================
    // =====================================================
    // LOAD UNITS
    // =====================================================

    public GlobalUnitsSaveData LoadUnits()
    {
        string saveFolder = Path.Combine(rootPath, "world");
        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"world_units.{extension}";
        string fullPath = Path.Combine(saveFolder, fileName);

        if (!File.Exists(fullPath))
        {
            // Retorna un objeto vacío si aún no existe el archivo de guardado
            return new GlobalUnitsSaveData();
        }

        if (_format == SaveFormat.Json)
        {
            string json = File.ReadAllText(fullPath);
            byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
            return MessagePackSerializer.Deserialize<GlobalUnitsSaveData>(bytes);
        }
        else
        {
            byte[] data = File.ReadAllBytes(fullPath);
            return MessagePackSerializer.Deserialize<GlobalUnitsSaveData>(data);
        }
    }
    // =====================================================
    // LOAD ALL ENTITIES REGIONS
    // =====================================================

    public List<RegionEntitiesSaveData> LoadAllEntitiesRegions()
    {
        List<RegionEntitiesSaveData> allRegionsData = new();
        string regionFolder = Path.Combine(rootPath, "regions");

        if (!Directory.Exists(regionFolder))
        {
            return allRegionsData;
        }

        string extension = _format == SaveFormat.Json ? "json" : "bin";
        // Buscamos todos los archivos de entidades que coincidan con el formato actual
        string searchPattern = $"*_entities.{extension}";
        string[] files = Directory.GetFiles(regionFolder, searchPattern);

        foreach (string filePath in files)
        {
            try
            {
                RegionEntitiesSaveData regionData;

                if (_format == SaveFormat.Json)
                {
                    string json = File.ReadAllText(filePath);
                    byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
                    regionData = MessagePackSerializer.Deserialize<RegionEntitiesSaveData>(bytes);
                }
                else
                {
                    byte[] data = File.ReadAllBytes(filePath);
                    regionData = MessagePackSerializer.Deserialize<RegionEntitiesSaveData>(data);
                }

                if (regionData != null)
                {
                    allRegionsData.Add(regionData);
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores opcional si algún archivo está corrupto
                // GD.PrintErr($"Error al cargar entidades de región en {filePath}: {ex.Message}");
            }
        }

        return allRegionsData;
    }
    // =====================================================
    // LOAD ENTITIES REGION
    // =====================================================

    public RegionEntitiesSaveData LoadEntitiesRegion(int regionX, int regionY)
    {
        string regionFolder = Path.Combine(rootPath, "regions");
        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"region_{regionX}_{regionY}_entities.{extension}";
        string fullPath = Path.Combine(regionFolder, fileName);

        if (!File.Exists(fullPath))
        {
            // Retorna un contenedor vacío si la región no tiene archivo de entidades guardado
            return new RegionEntitiesSaveData
            {
                RegionX = regionX,
                RegionY = regionY
            };
        }

        if (_format == SaveFormat.Json)
        {
            string json = File.ReadAllText(fullPath);
            byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
            return MessagePackSerializer.Deserialize<RegionEntitiesSaveData>(bytes);
        }
        else
        {
            byte[] data = File.ReadAllBytes(fullPath);
            return MessagePackSerializer.Deserialize<RegionEntitiesSaveData>(data);
        }
    }
    public RegionSaveData LoadRegion(string path)
    {
        string extension =
            Path.GetExtension(path);

        if (extension == ".json")
        {
            string json =
                File.ReadAllText(path);

            byte[] bytes =
                MessagePackSerializer
                    .ConvertFromJson(json);

            return MessagePackSerializer
                .Deserialize<RegionSaveData>(bytes);
        }

        byte[] data =
            File.ReadAllBytes(path);

        return MessagePackSerializer
            .Deserialize<RegionSaveData>(data);
    }
    // =====================================================
    // LOAD INFO MAP
    // =====================================================

    public SavedMapData LoadInfoMap()
    {
        string saveFolder = Path.Combine(rootPath, "world");
        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"world_info.{extension}"; // Nota: Si prefieres cambiarlo a "map_info.{extension}" en el save y load, sería ideal para no mezclarlo con las unidades.
        string fullPath = Path.Combine(saveFolder, fileName);

        //if (!File.Exists(fullPath))
        //{
        //    return null; // O puedes retornar una nueva instancia por defecto si lo prefieres
        //}

        if (_format == SaveFormat.Json)
        {
            string json = File.ReadAllText(fullPath);
            byte[] bytes = MessagePackSerializer.ConvertFromJson(json);
            return MessagePackSerializer.Deserialize<SavedMapData>(bytes);
        }
        else
        {
            byte[] data = File.ReadAllBytes(fullPath);
            return MessagePackSerializer.Deserialize<SavedMapData>(data);
        }
    }

    // =====================================================
    // SAVE ALL DIRTY
    // =====================================================

    public void SavePalletes(string rootPath)
    {
        // terreno
        BlackyPalletesPersistence.terrainPalette.Save(rootPath, _format);
        BlackyPalletesPersistence.rampsPalette.Save(rootPath, _format);
        BlackyPalletesPersistence.surfacesPalette.Save(rootPath, _format);
        BlackyPalletesPersistence.decorationsPalette.Save(rootPath, _format);
        BlackyPalletesPersistence.pathsPalette.Save(rootPath, _format);

        //entidades
        BlackyPalletesPersistence.buildingPalette.Save(rootPath, _format);
        BlackyPalletesPersistence.resourcesPalette.Save(rootPath, _format);
        BlackyPalletesPersistence.characterPalette.Save(rootPath, _format);
    }
    public void SaveAllDirtyRegions()
    {
        SaveInfoMap();
        SavePalletes(rootPath);
        SaveTerrain();
        SaveEntities();
        SaveUnits();

    }
    public void SaveInfoMap()
    {
        SavedMapData data = new SavedMapData();
        data.MapType = _blackyWorldConfig.WorldTypeDetail;
        data.Name = _blackyWorldConfig.Name;
        data.seed = _blackyWorldConfig.WorldSeed;
        data.ChunkSize = _blackyWorldConfig.ChunkSize;
        data.TileSizeX = _blackyWorldConfig.MapSize.X;
        data.TileSizeY = _blackyWorldConfig.MapSize.Y;

        string saveFolder = Path.Combine(rootPath, "world"); // O la ruta raíz donde guardes tus archivos globales
        Directory.CreateDirectory(saveFolder);

        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"world_info.{extension}";
        string fullPath = Path.Combine(saveFolder, fileName);

        byte[] bytes = MessagePackSerializer.Serialize(data);

        if (_format == SaveFormat.Json)
        {
            string json = MessagePackSerializer.ConvertToJson(bytes);
            File.WriteAllText(fullPath, json);
        }
        else
        {
            File.WriteAllBytes(fullPath, bytes);
        }
    }
    public void SaveUnits()
    {
        GlobalUnitsSaveData globalSave = new();

        // 1. Construimos la query para obtener todas las entidades marcadas como persistentes
        var query = _flecsManager.WorldFlecs.QueryBuilder<PositionComponent, HealthComponent, UnitDefinitionComponent, TeamComponent>()
            .With<PersistEntityTag>()
            .Build();

        // 2. Iteramos de forma ultra-performante por cada entidad encontrada
        query.Each((Entity ent, ref PositionComponent pos, ref HealthComponent health, ref UnitDefinitionComponent def, ref TeamComponent team) =>
        {


            globalSave.Units.Add(new SavedUnitData
            {
                TemplateId = def.idTemplate,
                Health = health.value,
                PosX = pos.position.X,
                PosY = pos.position.Y,
                Height = pos.height,
                Team = team.TeamId
            });
        });

        // 3. Escribimos el archivo global en el disco
        WriteGlobalUnitsToDisk(globalSave);
    }
    private void WriteGlobalUnitsToDisk(GlobalUnitsSaveData data)
    {
        string saveFolder = Path.Combine(rootPath, "world"); // O la ruta raíz donde guardes tus archivos globales
        Directory.CreateDirectory(saveFolder);

        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"world_units.{extension}";
        string fullPath = Path.Combine(saveFolder, fileName);

        byte[] bytes = MessagePackSerializer.Serialize(data);

        if (_format == SaveFormat.Json)
        {
            string json = MessagePackSerializer.ConvertToJson(bytes);
            File.WriteAllText(fullPath, json);
        }
        else
        {
            File.WriteAllBytes(fullPath, bytes);
        }
    }
    private void SaveEntities()
    {
        foreach (var regionCoord in _entidadesMap.GetDirtyRegions())
        {
            RegionEntitiesSaveData regionSave = new()
            {
                RegionX = regionCoord.X,
                RegionY = regionCoord.Y
            };

            foreach (var chunkCoord in _entidadesMap.GetChunksForRegion(regionCoord))
            {
                var bucket = _entidadesMap.GetBucket(chunkCoord);
                if (bucket == null || bucket.Count == 0) continue;

                EntityChunkSave chunkSave = new()
                {
                    ChunkX = chunkCoord.X,
                    ChunkY = chunkCoord.Y
                };

                for (int i = 0; i < bucket.Count; i++)
                {
                    bool isBuilding = bucket.IsBuilding[i];
                    Entity ent = bucket.Entities[i];

                    if (isBuilding)
                    {
                        var bt = ent.Get<BuildingDefinitionComponent>();
                        var health = ent.Get<HealthComponent>();
                        var position = ent.Get<PositionComponent>();
                        chunkSave.Buildings.Add(new SavedBuildingData
                        {
                            TemplateId = bt.idTemplate,
                            Health = health.value,
                            WorldTileX = position.tilePosition.X,
                            WorldTileY = position.tilePosition.Y,
                            Height = position.height
                        });
                    }
                    else
                    {
                        var res = ent.Get<ResourceDefinitionComponent>();
                        var health = ent.Get<HealthComponent>();
                        var amount = ent.Get<AmountComponent>();
                        var position = ent.Get<PositionComponent>();
                        chunkSave.Resources.Add(new SavedResourceData
                        {
                            TemplateId = res.idTemplate,
                            Health = health.value,
                            Amount = amount.value,
                            WorldTileX = position.tilePosition.X,
                            WorldTileY = position.tilePosition.Y,
                            Height = position.height
                        });
                    }
                }

                if (chunkSave.Buildings.Count > 0 || chunkSave.Resources.Count > 0)
                {
                    regionSave.EntityChunks.Add(chunkSave);
                }
            }

            // Guardar en su propio archivo independiente por región (ej: region_0_0_entities.bin)
            WriteEntitiesRegionToDisk(regionCoord.X, regionCoord.Y, regionSave);

            // Limpiar estado sucio de entidades
            _entidadesMap.ClearDirtyRegion(regionCoord);
        }
    }

    private void WriteEntitiesRegionToDisk(int regionX, int regionY, RegionEntitiesSaveData data)
    {
        string regionFolder = Path.Combine(rootPath, "regions");
        Directory.CreateDirectory(regionFolder);

        string extension = _format == SaveFormat.Json ? "json" : "bin";
        string fileName = $"region_{regionX}_{regionY}_entities.{extension}";
        string fullPath = Path.Combine(regionFolder, fileName);

        byte[] bytes = MessagePackSerializer.Serialize(data);

        if (_format == SaveFormat.Json)
        {
            string json = MessagePackSerializer.ConvertToJson(bytes);
            File.WriteAllText(fullPath, json);
        }
        else
        {
            File.WriteAllBytes(fullPath, bytes);
        }
    }
    private void SaveTerrain()
    {
        foreach (var region in _regions.GetDirtyRegions())
        {
            // ============================================
            // BUILD SAVE
            // ============================================

            RegionSaveData save = SaveRegion(region.X, region.Y);

            // ============================================
            // PATH
            // ============================================

            string regionFolder =
                Path.Combine(
                    rootPath,
                    "regions");

            Directory.CreateDirectory(
                regionFolder);

            string extension =
                _format == SaveFormat.Json
                    ? "json"
                    : "bin";

            string fileName =
                $"region_{region.X}_{region.Y}.{extension}";

            string fullPath =
                Path.Combine(
                    regionFolder,
                    fileName);

            // ============================================
            // SAVE JSON
            // ============================================

            if (_format == SaveFormat.Json)
            {
                byte[] bytes =
                    MessagePackSerializer.Serialize(save);

                string json =
                    MessagePackSerializer
                        .ConvertToJson(bytes);

                File.WriteAllText(
                    fullPath,
                    json);
            }

            // ============================================
            // SAVE BINARY
            // ============================================

            else
            {
                byte[] bytes =
                    MessagePackSerializer
                        .Serialize(save);

                File.WriteAllBytes(
                    fullPath,
                    bytes);
            }

            // ============================================
            // CLEAR DIRTY
            // ============================================

            _terrainWorld.ClearDirtyRegion(
                region);

            _rampWorld.ClearDirtyRegion(
                region);

            _regions.ClearDirtyRegion(
                region.X,
                region.Y);
        }
    }
    

    // =====================================================
    // SAVE REGION
    // =====================================================

    public RegionSaveData SaveRegion(
        int regionX,
        int regionY)
    {
        var region =
            _regions.GetOrCreateRegion(
                regionX,
                regionY);

        RegionSaveData save =new RegionSaveData  {
                RegionX = regionX,
                RegionY = regionY
            };

        // =================================================
        // maps layers
        // =================================================

        save.TerrainChunks =  SaveTerrainRegion(region);

        save.RampChunks = SaveVisualRegion(region, _rampWorld);
        save.SurfaceChunks = SaveVisualRegion(region, _superficiesData);
        save.DecorationChunks = SaveVisualRegion(region, _adornosData);
        save.PathChunks = SaveVisualRegion(region, _caminosData);

        _regions.ClearDirtyRegion(regionX, regionY);

        _terrainWorld.ClearDirtyRegion(region);

        _rampWorld.ClearDirtyRegion(region);

        _adornosData.ClearDirtyRegion(region);
        _caminosData.ClearDirtyRegion(region);
        _superficiesData.ClearDirtyRegion(region);

        return save;
    }

    // =====================================================
    // SAVE VISUAL
    // =====================================================

    private List<GenericChunkSave> SaveVisualRegion( BlackyRegion region, BlackyWorldDataMap<SerializerCellGeneric> world)
    {
        List<GenericChunkSave> result = new();

        foreach (var coord in world.GetDirtyChunksForRegion(region))
        {
            if (!world.TryGetChunk(
                coord.X,
                coord.Y,
                out var chunk))
            {
                continue;
            }

            GenericChunkSave chunkSave =
                new()
                {
                    ChunkX = coord.X,
                    ChunkY = coord.Y
                };

            foreach (var pair in chunk.Heights)
            {
                int height = pair.Key;

                var heightData = pair.Value;

                ushort[] tiles =
                    new ushort[
                        world.ChunkSize *
                        world.ChunkSize];

                bool hasData = false;

                for (int y = 0; y < world.ChunkSize; y++)
                {
                    for (int x = 0; x < world.ChunkSize; x++)
                    {
                        ushort tile = heightData.GetCell(x, y).id;

                        tiles[y * world.ChunkSize + x] = tile;

                        if (tile != 0)
                            hasData = true;
                    }
                }

                if (!hasData)
                    continue;

                chunkSave.Heights.Add(
                    new GenericHeightSave
                    {
                        Height = height,
                        idData = tiles
                    });
            }

            if (chunkSave.Heights.Count > 0)
            {
                result.Add(chunkSave);
            }
        }

        return result;
    }

    // =====================================================
    // SAVE TERRAIN
    // =====================================================

    private List<TerrainChunkSave> SaveTerrainRegion(BlackyRegion region)
    {
        List<TerrainChunkSave> result = new();

        foreach (var coord in _terrainWorld.GetDirtyChunksForRegion(region))
        {
            if (!_terrainWorld.TryGetChunk(
                coord.X,
                coord.Y,
                out var chunk))
            {
                continue;
            }

            TerrainChunkSave chunkSave =
                new()
                {
                    ChunkX = coord.X,
                    ChunkY = coord.Y
                };

            foreach (var pair in chunk.Heights)
            {
                int height = pair.Key;

                var heightData = pair.Value;

                ushort[] terrain =
                    new ushort[
                        _terrainWorld.ChunkSize *
                        _terrainWorld.ChunkSize];

                bool hasData = false;

                for (int y = 0;
                    y < _terrainWorld.ChunkSize;
                    y++)
                {
                    for (int x = 0;
                        x < _terrainWorld.ChunkSize;
                        x++)
                    {
                        ushort id =
                            heightData
                                .GetCell(x, y)
                                .id;

                        terrain[
                            y * _terrainWorld.ChunkSize + x]
                                = id;

                        if (id != 0)
                            hasData = true;
                    }
                }

                if (!hasData)
                    continue;

                chunkSave.Heights.Add(
                    new TerrainHeightSave
                    {
                        Height = height,
                        TerrainIds = terrain
                    });
            }

            if (chunkSave.Heights.Count > 0)
            {
                result.Add(chunkSave);
            }
        }

        return result;
    }
}