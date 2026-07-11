using GodotEcsArch.sources.BlackyEngine.Services.Palettes;
using GodotEcsArch.sources.BlackyEngine.Services.Render.TilesTexture;
using GodotEcsArch.sources.BlackyTiles.Data;
using MessagePack;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;

namespace GodotEcsArch.sources.BlackyEngine.Data;

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

    string path = "D:\\GitKraken\\MapsGame";
    string rootPath;
    string nameMap;
    private readonly BlackyWorldRegions _regions;

    private readonly BlackyTerrainWorldData _terrainWorld;

    private readonly BlackyRampVisualWorld _rampWorld;

    // aun me faltan agregar el de los caminos, decoraciones, superficies

    private readonly SaveFormat _format;

    public BlackyWorldPersistence(string nameMap,
        BlackyWorldRegions regions,
        BlackyTerrainWorldData terrainWorld,
        BlackyRampVisualWorld rampWorld,
        SaveFormat format = SaveFormat.Binary)
    {
        this.nameMap = nameMap;
        _regions = regions;
        _terrainWorld = terrainWorld;
        _rampWorld = rampWorld;

        _format = format;

        rootPath = path + "\\ " + nameMap;
    }

    // =====================================================
    // LOAD
    // =====================================================

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
    // SAVE ALL DIRTY
    // =====================================================

    public void SavePalletes(string rootPath)
    {
        _terrainWorld.terrainPalette.Save(rootPath, _format);
        _rampWorld.rampsPalette.Save(rootPath, _format);
    }
    public void SaveAllDirtyRegions()
    {
        

        SavePalletes(rootPath);

        foreach (var region in _regions.GetDirtyRegions())
        {
            // ============================================
            // BUILD SAVE
            // ============================================

            RegionSaveData save = SaveRegion( region.X, region.Y);

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

        _regions.ClearDirtyRegion(regionX, regionY);

        _terrainWorld.ClearDirtyRegion(region);

        _rampWorld.ClearDirtyRegion(region);

        return save;
    }

    // =====================================================
    // SAVE VISUAL
    // =====================================================

    private List<GenericChunkSave> SaveVisualRegion(
        BlackyRegion region,
        BlackyWorldDataMap<VisualTileCell> world)
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
                        ushort tile =
                            heightData
                                .GetCell(x, y)
                                .TileId;

                        tiles[
                            y * world.ChunkSize + x]
                                = tile;

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

    private List<TerrainChunkSave> SaveTerrainRegion(
        BlackyRegion region)
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