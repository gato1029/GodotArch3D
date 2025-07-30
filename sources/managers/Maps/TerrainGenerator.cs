using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Numerics;

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
    public List<TileTerrainAutomaticChunk> Generate(Vector2I chunkPosition, int range)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;
        List<TileTerrainAutomaticChunk> chunks = new();

        // Cache temporal de los terrenos generados en esta ejecución
        Dictionary<Vector2I, TerrainType> terrainCache = new();

        for (int offsetX = -range + 1; offsetX < range; offsetX++)
        {
            for (int offsetY = -range + 1; offsetY < range; offsetY++)
            {
                Vector2I currentChunk = chunkPosition + new Vector2I(offsetX, offsetY);
                var chunkData = new TileTerrainAutomaticChunk(currentChunk, chunkSize);

                for (int y = 0; y < chunkSize.Y; y++)
                {
                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        Vector2I globalPos = currentChunk * chunkSize + new Vector2I(x, y);

                        // Buscar vecino desde el cache o mapa persistente
                        //TerrainType? neighbor = GetNeighborTerrain(globalPos, terrainCache);
                        //TerrainType terrain = GetTerrainWithContext(globalPos, neighbor);

                        List<TerrainType> neighbors = GetNeighborTerrains(globalPos, terrainCache);
                        TerrainType terrain = GetTerrainWithContext(globalPos, neighbors);

                        // Solo el chunk central genera caminos
                        if (generarCaminos && offsetX == 0 && offsetY == 0)
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
        RefineWaterBorders(chunks, chunkSize);
        AddElevacionBase(chunks, chunkSize);
        return chunks;
    }


    private void PostProcessWaterBorders(List<TileTerrainAutomaticChunk> chunks, Vector2I chunkSize)
    {
        // Construimos un diccionario global para acceso rápido
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

        // Segunda pasada: actualizar bordes
        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    ref TileAutoChunk tile = ref chunk.dataInternal[x, y];

                    if (tile.terrainType == TerrainType.Agua)
                    {
                        Vector2I pos = new(tile.xGlobal, tile.yGlobal);
                        var neighbors = GetNeighborTerrains(pos, terrainMap);

                        foreach (var n in neighbors)
                        {
                            if (n != TerrainType.Agua && n != TerrainType.AguaBorde)
                            {
                                tile.terrainType = TerrainType.AguaBorde;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
    private void RefineWaterBorders(List<TileTerrainAutomaticChunk> chunks, Vector2I chunkSize)
    {
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

        Vector2I[] allDirs = new[]
        {
        new Vector2I(-1, -1), // esquina sup-izq
        new Vector2I(0, -1),  // arriba
        new Vector2I(1, -1),  // esquina sup-der
        new Vector2I(-1, 0),  // izquierda
        new Vector2I(1, 0),   // derecha
        new Vector2I(-1, 1),  // esquina inf-izq
        new Vector2I(0, 1),   // abajo
        new Vector2I(1, 1),   // esquina inf-der
    };

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

                    foreach (var dir in allDirs)
                    {
                        Vector2I neighborPos = pos + dir;

                        if (IsNonWater(neighborPos))
                        {
                            tile.terrainType = TerrainType.AguaBorde;
                            break; // Basta con uno
                        }
                    }

                    bool IsNonWater(Vector2I p)
                    {
                        // Primero, verifica si ya lo tenemos en la generación actual
                        if (terrainMap.TryGetValue(p, out var t))
                            return t != TerrainType.Agua && t != TerrainType.AguaBorde;

                        // Luego consulta el mapa base consolidado
                        var data = mapTerrainBasic.GetTileGlobalPosition(p);                        
                        if (data != null)
                        {
                            var dataType = (TerrainType)data.GetTypeData();
                            return dataType != TerrainType.Agua && dataType != TerrainType.AguaBorde;

                        }
                      
                 

                        // Si aún no está definido en ningún lado, podrías decidir una política:
                        // O asumir que es Agua, o asumir que es No Agua.
                        // Aquí podrías retornar true para forzar el cierre del borde:
                        return true;
                    }
                }
            }
        }
    }


    void AddElevacionBase(List<TileTerrainAutomaticChunk> chunks, Vector2I chunkSize)
    {
        Dictionary<Vector2I, TileAutoChunk> globalMap = new();

        // 1. Consolidar todos los tiles en un diccionario global
        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    var tile = chunk.dataInternal[x, y];
                    globalMap[new Vector2I(tile.xGlobal, tile.yGlobal)] = tile;
                }
            }
        }

        // 2. Agregar ElevacionBase vertical y lateral
        foreach (var chunk in chunks)
        {
            for (int x = 0; x < chunkSize.X; x++)
            {
                for (int y = 0; y < chunkSize.Y; y++)
                {
                    var tile = chunk.dataInternal[x, y];

                    if (tile.terrainType == TerrainType.Elevacion)
                    {
                        Vector2I tilePos = new Vector2I(tile.xGlobal, tile.yGlobal);

                        // ↓↓↓ DEBAJO ↓↓↓
                        Vector2I below = tilePos + new Vector2I(0, -1);
                        if (!globalMap.TryGetValue(below, out var tBelow) ||
                            tBelow.terrainType != TerrainType.Elevacion)
                        {
                            for (int i = 1; i <= 3; i++)
                            {
                                Vector2I ebPos = tilePos + new Vector2I(0, -i);
                                PlaceEB(globalMap, ebPos);
                            }
                        }

                        // ← IZQUIERDA
                        Vector2I left = tilePos + new Vector2I(-1, 0);
                        if (!globalMap.TryGetValue(left, out var tLeft) ||
                            tLeft.terrainType != TerrainType.Elevacion)
                        {
                            Vector2I ebLeft = tilePos + new Vector2I(-1, 0);
                            PlaceEB(globalMap, ebLeft);
                        }

                        // → DERECHA
                        Vector2I right = tilePos + new Vector2I(1, 0);
                        if (!globalMap.TryGetValue(right, out var tRight) ||
                            tRight.terrainType != TerrainType.Elevacion)
                        {
                            Vector2I ebRight = tilePos + new Vector2I(1, 0);
                            PlaceEB(globalMap, ebRight);
                        }
                    }
                }
            }
        }

        // 3. Volcar datos nuevamente a los chunks
        foreach (var chunk in chunks)
        {
            for (int y = 0; y < chunkSize.Y; y++)
            {
                for (int x = 0; x < chunkSize.X; x++)
                {
                    Vector2I pos = new Vector2I(
                        chunk.dataInternal[x, y].xGlobal,
                        chunk.dataInternal[x, y].yGlobal
                    );

                    if (globalMap.TryGetValue(pos, out var updated))
                    {
                        chunk.dataInternal[x, y] = updated;
                    }
                }
            }
        }
    }

    // Función auxiliar para colocar EB si es válido
    void PlaceEB(Dictionary<Vector2I, TileAutoChunk> map, Vector2I pos)
    {
        if (map.TryGetValue(pos, out var existing))
        {
            if (existing.terrainType == TerrainType.PisoBase || existing.terrainType == TerrainType.Agua)
            {
                existing.terrainType = TerrainType.ElevacionBase;
                map[pos] = existing;
            }
        }
        else
        {
            map[pos] = new TileAutoChunk
            {
                xGlobal = pos.X,
                yGlobal = pos.Y,
                terrainType = TerrainType.ElevacionBase
            };
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
    public void AplicarChunksAlMapaReal(List<TileTerrainAutomaticChunk> chunks, TerrainCategoryType terrainCategoryType)
    {
        BsonExpression bsonExpression = null;
        List<TerrainData> result = new List<TerrainData>();

        string expressionText = "category = @0 and isRule =@1";
        bsonExpression = BsonExpression.Create(expressionText, terrainCategoryType.ToString(),true);
        result = DataBaseManager.Instance.FindAllFilter<TerrainData>(bsonExpression);

        Dictionary<TerrainMapLevelDesign, int> tableData = new Dictionary<TerrainMapLevelDesign, int>();
        foreach (var item in result)
        {
            switch (item.terrainType)
            {                
                case TerrainType.PisoBase:
                    tableData.Add(TerrainMapLevelDesign.Piso, item.id);
                    break;
                case TerrainType.Elevacion:
                    //tableData.Add(TerrainMapLevelDesign., item.id);
                    break;
                case TerrainType.Agua:
                    break;
                case TerrainType.CaminoPiso:
                    break;
                case TerrainType.CaminoAgua:
                    break;
                default:
                    break;
            }
        }


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
                    int idData = 0;
                    MapManagerEditor.Instance.currentMapLevelData.terrainMap.AddUpdateTile(globalPos,idData , (TerrainMapLevelDesign)tile.terrainType);
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
