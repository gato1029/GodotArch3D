using Arch.Core;
using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

public enum VecindadTipo
{
    VecindadSimple,
    VecindadCompleta
}

public struct TileAutoChunk
{
    public int xGlobal;
    public int yGlobal;
    public TerrainType terrainType;
}
public class TileTerrainAutomaticChunk
{
    public TileAutoChunk[,] dataInternal;      // [x, y] dentro del chunk
    public Vector2I chunkPosition;

    public TileTerrainAutomaticChunk(Vector2I chunkPos, Vector2I chunkSize)
    {
        chunkPosition = chunkPos;
        dataInternal = new TileAutoChunk[chunkSize.X, chunkSize.Y];
    }
}

public enum GenerateMode
{
    CenteredByTileDimensions,
    RadiusAroundChunk
}
public class TerrainGenerator
{
    private FastNoiseLite elevationNoise;
    private float elevationScale = 0.01f;
    private Random random;
    private bool useElevation;
    private SpriteMapChunk<TerrainDataGame> mapTerrainBasic; // representaciones Basicas no renderiza
    private VecindadTipo vecindadTipo;
    private readonly bool generarCaminos;
    private float waterThreshold = 0.3f;
    private float elevationThreshold = 0.6f;
    public TerrainGenerator(SpriteMapChunk<TerrainDataGame> mapBasic, int seed = 1337, bool useElevation = true, bool generarCaminos = true, VecindadTipo vecindadTipo = VecindadTipo.VecindadSimple)
    {
        this.mapTerrainBasic = mapBasic;
        this.generarCaminos = generarCaminos;
        elevationNoise = new FastNoiseLite(seed);
        elevationNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        elevationNoise.SetFrequency(elevationScale);

        random = new Random(seed);
        this.useElevation = useElevation;
        this.vecindadTipo = vecindadTipo;
    }
    public void SetTerrainDistribution(float waterPercent, float floorPercent, float elevationPercent)
    {
        float total = waterPercent + floorPercent + elevationPercent;
        if (total <= 0f)
            throw new ArgumentException("La suma de los porcentajes debe ser mayor a 0.");

        // Normalizar a 1
        waterPercent /= total;
        floorPercent /= total;
        elevationPercent /= total;

        // Calcular thresholds acumulativos
        waterThreshold = waterPercent;
        elevationThreshold = waterThreshold + floorPercent;
    }

    private TerrainType GetTerrainWithContext(Vector2I positionGlobal, List<TerrainType> neighbors)
    {
        float raw = elevationNoise.GetNoise(positionGlobal.X, positionGlobal.Y);
        float norm = Normalize(raw);

        if (norm < waterThreshold)
        {
            foreach (var n in neighbors)
            {
                if (n != TerrainType.Agua && n != TerrainType.AguaBorde)
                    return TerrainType.AguaBorde;
            }

            return TerrainType.Agua;
        }
        else if (norm < elevationThreshold)
        {
            return TerrainType.PisoBase;
        }
        else
        {
            return useElevation ? TerrainType.Elevacion : TerrainType.PisoBase;
        }
    }


    private List<TerrainType> GetNeighborTerrains(Vector2I pos, Dictionary<Vector2I, TerrainType> terrainCache)
    {
        List<TerrainType> neighbors = new();

        Vector2I[] directions = vecindadTipo == VecindadTipo.VecindadCompleta
            ? new Vector2I[]
            {
            new Vector2I(-1, -1), new Vector2I(0, -1), new Vector2I(1, -1),
            new Vector2I(-1,  0),                     new Vector2I(1,  0),
            new Vector2I(-1,  1), new Vector2I(0,  1), new Vector2I(1,  1),
            }
            : new Vector2I[]
            {
            new Vector2I(0, -1),
            new Vector2I(0, 1),
            new Vector2I(-1, 0),
            new Vector2I(1, 0)
            };

        foreach (var dir in directions)
        {
            Vector2I neighborPos = pos + dir;

            if (terrainCache.TryGetValue(neighborPos, out var neighborType))
            {
                neighbors.Add(neighborType);
            }
     
            var data = mapTerrainBasic.GetTileGlobalPosition(neighborPos);
            if (data != null)
            {
                neighbors.Add((TerrainType)data.GetTypeData()); // ajusta si el mapeo es distinto
            }
        }

        return neighbors;
    }
    private List<(Vector2I pos, TerrainType type)> GetNeighborTerrainsComplete(Vector2I pos, Dictionary<Vector2I, TerrainType> map)
    {
        Vector2I[] allDirs = new[]
        {
        new Vector2I(0, -1),   // N
        new Vector2I(1, 0),    // E
        new Vector2I(0, 1),    // S
        new Vector2I(-1, 0),   // W
        new Vector2I(-1, -1),  // NW
        new Vector2I(1, -1),   // NE
        new Vector2I(1, 1),    // SE
        new Vector2I(-1, 1),   // SW
    };

        List<(Vector2I, TerrainType)> result = new();

        foreach (var dir in allDirs)
        {
            Vector2I neighbor = pos + dir;
            if (map.TryGetValue(neighbor, out var terrain))
            {
                result.Add((neighbor, terrain));
            }
            else
            {
                var data = mapTerrainBasic.GetTileGlobalPosition(neighbor);
                if (data != null)
                {
                    var dataType = (TerrainType)data.GetTypeData();
                    result.Add((neighbor, dataType));
                }
                else
                {
                    // fuera del mapa = suelo
                    result.Add((neighbor, TerrainType.PisoBase));
                }
            }
        }

        return result;
    }

    public List<TileTerrainAutomaticChunk> GenerateTerrain(
    Vector2I originChunk,
    GenerateMode mode,
    Vector2I sizeOrRangeInTilesOrChunks)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;
        List<TileTerrainAutomaticChunk> chunks = new();
        Dictionary<Vector2I, TerrainType> terrainCache = new();

        int minChunkX, minChunkY, maxChunkX, maxChunkY;

        if (mode == GenerateMode.CenteredByTileDimensions)
        {
            int totalChunksX = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.X / chunkSize.X);
            int totalChunksY = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.Y / chunkSize.Y);

            minChunkX = -totalChunksX / 2;
            minChunkY = -totalChunksY / 2;
            maxChunkX = minChunkX + totalChunksX;
            maxChunkY = minChunkY + totalChunksY;
        }
        else // RadiusAroundChunk
        {
            int range = sizeOrRangeInTilesOrChunks.X;
            minChunkX = originChunk.X - range + 1;
            minChunkY = originChunk.Y - range + 1;
            maxChunkX = originChunk.X + range;
            maxChunkY = originChunk.Y + range;
        }

        for (int chunkY = minChunkY; chunkY < maxChunkY; chunkY++)
        {
            for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++)
            {
                Vector2I chunkCoord = new Vector2I(chunkX, chunkY);
                var chunkData = new TileTerrainAutomaticChunk(chunkCoord, chunkSize);

                for (int y = 0; y < chunkSize.Y; y++)
                {
                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        Vector2I globalPos = chunkCoord * chunkSize + new Vector2I(x, y);

                        if (mode == GenerateMode.CenteredByTileDimensions)
                        {
                            if (Math.Abs(globalPos.X) > sizeOrRangeInTilesOrChunks.X / 2 ||
                                Math.Abs(globalPos.Y) > sizeOrRangeInTilesOrChunks.Y / 2)
                                continue;
                        }

                        List<TerrainType> neighbors = GetNeighborTerrains(globalPos, terrainCache);
                        TerrainType terrain = GetTerrainWithContext(globalPos, neighbors);

                        bool isCenterChunk =
                            (mode == GenerateMode.CenteredByTileDimensions && chunkCoord == Vector2I.Zero) ||
                            (mode == GenerateMode.RadiusAroundChunk && chunkCoord == originChunk);

                        if (generarCaminos && isCenterChunk)
                        {
                            if (x == chunkSize.X / 2 || y == chunkSize.Y / 2)
                            {
                                if (terrain == TerrainType.PisoBase)
                                    terrain = TerrainType.CaminoPiso;
                                else if (terrain == TerrainType.Agua)
                                    terrain = TerrainType.CaminoAgua;
                            }
                        }

                        chunkData.dataInternal[x, y] = new TileAutoChunk
                        {
                            xGlobal = globalPos.X,
                            yGlobal = globalPos.Y,
                            terrainType = terrain
                        };

                        terrainCache[globalPos] = terrain;
                    }
                }

                chunks.Add(chunkData);
            }
        }

        PostProcessWaterBorders(chunks, chunkSize);
        LimpiarBordesAislados(chunks, chunkSize);
        RefinarElevacionesSueltas(chunks, chunkSize);
        MarcarBordesElevacionConBase(chunks, chunkSize);

        return chunks;
    }


    private void RefinarElevacionesSueltas(List<TileTerrainAutomaticChunk> chunks, Vector2I chunkSize)
    {
        // Construimos un mapa global para consulta rápida
        Dictionary<Vector2I, TerrainType> terrainMap = new();

        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    var tile = chunk.dataInternal[x, y];
                    Vector2I global = new(tile.xGlobal, tile.yGlobal);
                    terrainMap[global] = tile.terrainType;
                }
            }
        }

        // Direcciones cardinales (N, E, S, O)
        Vector2I[] dirs = new[]
        {
        new Vector2I(0, -1),
        new Vector2I(1, 0),
        new Vector2I(0, 1),
        new Vector2I(-1, 0),
    };

        // Evaluar cada tile de elevación
        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    ref var tile = ref chunk.dataInternal[x, y];

                    if (tile.terrainType != TerrainType.Elevacion)
                        continue;

                    Vector2I pos = new(tile.xGlobal, tile.yGlobal);
                    int count = 0;

                    foreach (var dir in dirs)
                    {
                        Vector2I neighborPos = pos + dir;

                        if (terrainMap.TryGetValue(neighborPos, out var t))
                        {
                            if (t == TerrainType.Elevacion)
                                count++;
                        }
                        else
                        {
                            var data = mapTerrainBasic.GetTileGlobalPosition(neighborPos);
                            if (data != null)
                            {
                                var type = (TerrainType)data.GetTypeData();
                                if (type == TerrainType.Elevacion)
                                    count++;
                            }
                        }
                    }

                    if (count < 2)
                    {
                        tile.terrainType = TerrainType.PisoBase; // Tile de elevación inválido
                    }
                }
            }
        }
    }

    private void MarcarBordesElevacionConBase(List<TileTerrainAutomaticChunk> chunks, Vector2I chunkSize)
    {
        // Mapa global para ubicar tiles rápidamente
        Dictionary<Vector2I, TerrainType> terrainMap = new();

        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    var tile = chunk.dataInternal[x, y];
                    Vector2I global = new(tile.xGlobal, tile.yGlobal);
                    terrainMap[global] = tile.terrainType;
                }
            }
        }

        Vector2I[] dirs = new[]
        {
        new Vector2I(0, -1),
        new Vector2I(1, 0),
        new Vector2I(0, 1),
        new Vector2I(-1, 0),
    };

        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    ref var tile = ref chunk.dataInternal[x, y];

                    // Solo nos interesan tiles de tipo Elevacion
                    if (tile.terrainType != TerrainType.Elevacion)
                        continue;

                    Vector2I pos = new(tile.xGlobal, tile.yGlobal);

                    foreach (var dir in dirs)
                    {
                        Vector2I neighborPos = pos + dir;

                        if (terrainMap.TryGetValue(neighborPos, out var neighborType))
                        {
                            if (neighborType != TerrainType.Elevacion && neighborType != TerrainType.ElevacionBase)
                            {
                                tile.terrainType = TerrainType.ElevacionBase;
                                break;
                            }
                        }
                        else
                        {
                            // Si no hay vecino, asumimos que es borde
                            tile.terrainType = TerrainType.ElevacionBase;
                            break;
                        }
                    }
                }
            }
        }
    }



    private void LimpiarBordesAislados(List<TileTerrainAutomaticChunk> chunks, Vector2I chunkSize)
    {
        // Creamos un mapa global para consultar tipos de terreno
        Dictionary<Vector2I, TerrainType> terrainMap = new();

        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    var tile = chunk.dataInternal[x, y];
                    Vector2I pos = new(tile.xGlobal, tile.yGlobal);
                    terrainMap[pos] = tile.terrainType;
                }
            }
        }

        // Direcciones cardinales
        Vector2I[] dirs = new[]
        {
        new Vector2I(0, -1),  // Norte
        new Vector2I(1, 0),   // Este
        new Vector2I(0, 1),   // Sur
        new Vector2I(-1, 0),  // Oeste
    };

        // Verificamos cada tile de tipo AguaBorde
        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    ref var tile = ref chunk.dataInternal[x, y];

                    if (tile.terrainType != TerrainType.AguaBorde)
                        continue;

                    Vector2I pos = new(tile.xGlobal, tile.yGlobal);
                    int count = 0;

                    foreach (var dir in dirs)
                    {
                        Vector2I neighborPos = pos + dir;

                        if (terrainMap.TryGetValue(neighborPos, out var t))
                        {
                            if (t == TerrainType.Agua || t == TerrainType.AguaBorde)
                                count++;
                        }
                        else
                        {
                            var data = mapTerrainBasic.GetTileGlobalPosition(neighborPos);
                            if (data != null)
                            {
                                var dataType = (TerrainType)data.GetTypeData();
                                if (dataType == TerrainType.Agua || dataType == TerrainType.AguaBorde)
                                    count++;
                            }
                        }
                    }

                    if (count < 2)
                    {
                        tile.terrainType = TerrainType.PisoBase; // AguaBorde inválida
                    }
                }
            }
        }
    }

    private void PostProcessWaterBorders(List<TileTerrainAutomaticChunk> chunks, Vector2I chunkSize)
    {
        Dictionary<Vector2I, TerrainType> terrainMap = new();

        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    var tile = chunk.dataInternal[x, y];
                    Vector2I global = new Vector2I(tile.xGlobal, tile.yGlobal);
                    terrainMap[global] = tile.terrainType;
                }
            }
        }

        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    ref TileAutoChunk tile = ref chunk.dataInternal[x, y];

                    if (tile.terrainType != TerrainType.Agua)
                        continue;

                    Vector2I pos = new(tile.xGlobal, tile.yGlobal);
                    var neighbors = GetNeighborTerrainsComplete(pos, terrainMap);

                    bool isBorder = false;
                    int nonWaterCardinals = 0;

                    foreach (var (neighborPos, terrain) in neighbors)
                    {
                        if (terrain != TerrainType.Agua && terrain != TerrainType.AguaBorde)
                        {
                            var dir = neighborPos - pos;

                            // cardinal
                            if (Math.Abs(dir.X) + Math.Abs(dir.Y) == 1)
                            {
                                isBorder = true;
                                nonWaterCardinals++;
                            }
                        }
                    }

                    // si solo hay contacto diagonal con no-agua, aun así consideramos borde
                    if (!isBorder)
                    {
                        foreach (var (neighborPos, terrain) in neighbors)
                        {
                            if (terrain != TerrainType.Agua && terrain != TerrainType.AguaBorde)
                            {
                                var dir = neighborPos - pos;

                                if (Math.Abs(dir.X) == 1 && Math.Abs(dir.Y) == 1) // diagonal
                                {
                                    isBorder = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (isBorder)
                        tile.terrainType = TerrainType.AguaBorde;
                }
            }
        }
    }
   
   
    public void AplicarChunksAlMapaBase(List<TileTerrainAutomaticChunk> chunks)
    {
        foreach (var chunk in chunks)
        {
            int width = chunk.dataInternal.GetLength(0);
            int height = chunk.dataInternal.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileAutoChunk tile = chunk.dataInternal[x, y];
                    Vector2I globalPos = new Vector2I(tile.xGlobal, tile.yGlobal);                    
                    AddTileBasicConfig(globalPos,tile.terrainType);
                }
            }
        }
    }
    public void AplicarChunkAlMapaReal(List<TileTerrainAutomaticChunk> chunks)
    {
        foreach (var item in chunks)
        {
            foreach (var itemMap in MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerDesign)
            {
                TerrainType terrainType = (TerrainType)Enum.Parse(typeof(TerrainType), itemMap.Key);
                var chunkDesign = itemMap.Value.GetTilesByChunk(item.chunkPosition);
                if (chunkDesign != null)
                {
                    int width = chunkDesign.tiles.GetLength(0);
                    int height = chunkDesign.tiles.GetLength(1);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var dataItem = chunkDesign.tiles[x, y];
                            if (dataItem != null)
                            {


                                var globalPos = dataItem.positionTileWorld;
                                TerrainType terrainTypeInternal = (TerrainType)dataItem.GetTypeData();
                                switch (terrainType)
                                {
                                    case TerrainType.PisoBase:
                                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Suelo.ToString()).AddUpdatedTile(globalPos, dataItem.idData);
                                        break;
                                    case TerrainType.Elevacion:
                                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Elevacion.ToString()).AddUpdatedTile(globalPos, dataItem.idData);
                                        break;
                                    case TerrainType.Agua:
                                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Agua.ToString()).AddUpdatedTile(globalPos, dataItem.idData);
                                        break;
                                    case TerrainType.CaminoPiso:
                                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Suelo.ToString()).AddUpdatedTile(globalPos, dataItem.idData);
                                        break;
                                    case TerrainType.CaminoAgua:
                                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Suelo.ToString()).AddUpdatedTile(globalPos, dataItem.idData);
                                        break;
                                    case TerrainType.AguaBorde:
                                        if (terrainTypeInternal == TerrainType.AguaBorde)
                                        {
                                            MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Suelo.ToString()).AddUpdatedTile(globalPos, dataItem.idData);
                                        }
                                        else
                                        {
                                            MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Suelo.ToString()).Remove(globalPos);
                                        }
                                            break;
                                    case TerrainType.Ornamentos:
                                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.MapLayerReal.GetLayer(TerrainMapReal.Ornamentos.ToString()).AddUpdatedTile(globalPos, dataItem.idData);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
             }
        }
    }
    public void AplicarChunksAlMapaDisenio(List<TileTerrainAutomaticChunk> chunks, TerrainCategoryType terrainCategoryType)
    {
        BsonExpression bsonExpression = BsonExpression.Create("category = @0 and isRule = @1", terrainCategoryType.ToString(), true);
        var result = DataBaseManager.Instance.FindAllFilter<TerrainData>(bsonExpression);

        Dictionary<TerrainType, int> tableData = result.ToDictionary(r => r.terrainType, r => r.id);

        // Fase 1: recopilar todos los tiles
        List<(Vector2I pos, TerrainType terrainType, int idData)> allTiles = new();

        foreach (var chunk in chunks)
        {
            int width = chunk.dataInternal.GetLength(0);
            int height = chunk.dataInternal.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileAutoChunk tile = chunk.dataInternal[x, y];
                    Vector2I globalPos = new Vector2I(tile.xGlobal, tile.yGlobal);
                    TerrainType terrainType = tile.terrainType;

                    if (!tableData.ContainsKey(terrainType))
                        continue; // O lanzar advertencia

                    int idData = tableData[terrainType];

                    allTiles.Add((globalPos, terrainType, idData));
                }
            }
        }

        // Fase 2: ordenar los tiles por tipo (orden de pintado)
        TerrainType[] ordenPintado = new TerrainType[]
        {
        TerrainType.Agua,
        TerrainType.PisoBase,
        TerrainType.AguaBorde,
        TerrainType.Elevacion,
        TerrainType.ElevacionBase
        };

        // Fase 3: aplicar los tiles por orden
        foreach (var tipo in ordenPintado)
        {
            foreach (var (pos, terrainType, idData) in allTiles.Where(t => t.terrainType == tipo))
            {
                switch (terrainType)
                {
                    case TerrainType.PisoBase:                        
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idData,0);
                        break;

                    case TerrainType.Elevacion:                        
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idData);
                        break;
                    case TerrainType.ElevacionBase:
                        int idPiso = tableData[TerrainType.PisoBase];
                        int idElevacion = tableData[TerrainType.Elevacion];
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idElevacion);                        
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idPiso,0);
                        break;
                    case TerrainType.Agua:
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idData);
                        //int idBordeRemove = tableData[TerrainType.AguaBorde];
                       // MapManagerEditor.Instance.currentMapLevelData.terrainMap.RemoveTile(pos, idBordeRemove);
                        break;

                    case TerrainType.CaminoPiso:
                    case TerrainType.CaminoAgua:
                    case TerrainType.AguaBorde:
                        int idAguaBorde = tableData[TerrainType.AguaBorde];
                        int idAgua = tableData[TerrainType.Agua];
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idAguaBorde);
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idAgua);
                        break;

                    default:
                        MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(pos, idData);
                        break;
                }
            }
        }
    }

    public void AddTileBasicConfig(Vector2I tilePositionGlobal, TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.PisoBase:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 68);
                break;
            case TerrainType.Elevacion:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 70);
                break;
            case TerrainType.Agua:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 69);
                break;
            case TerrainType.CaminoPiso:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 72);
                break;
            case TerrainType.CaminoAgua:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 71);
                break;
            case TerrainType.AguaBorde:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 73);
                break;
            case TerrainType.ElevacionBase:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 74);
                break;
            default:
                break;
        }
        mapTerrainBasic.Refresh(tilePositionGlobal);

    }
    private float Normalize(float value)
    {
        return (value + 1f) / 2f;
    }
}
