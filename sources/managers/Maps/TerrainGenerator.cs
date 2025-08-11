using Arch.Core;
using Godot;
using GodotEcsArch.sources.managers.Maps;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.managers.Tilemap;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
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

public enum GenerateMode
{
    CenteredByTileDimensions,
    RadiusAroundChunk
}
public class TerrainGenerator
{
    private FastNoiseLite elevationNoise;
    private float elevationScale = 0.01f;
    
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


    private List<TerrainType> GetNeighborTerrains(Vector2I pos)
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
            var data = mapTerrainBasic.GetTileGlobalPosition(neighborPos);
            if (data != null)
            {
                neighbors.Add((TerrainType)data.GetTypeData()); // ajusta si el mapeo es distinto
            }
        }

        return neighbors;
    }

    private List<(Vector2I pos, TerrainType type)> GetNeighborTerrainsComplete(Vector2I pos)
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

        return result;
    }
    public void GenerateTerrainOptimized(
    Vector2I originChunk,
    GenerateMode mode,
    Vector2I sizeOrRangeInTilesOrChunks)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;

        int minChunkX, minChunkY, maxChunkX, maxChunkY;
        int halfWidth = 0, halfHeight = 0;

        if (mode == GenerateMode.CenteredByTileDimensions)
        {
            int totalChunksX = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.X / chunkSize.X);
            int totalChunksY = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.Y / chunkSize.Y);

            minChunkX = -totalChunksX / 2;
            minChunkY = -totalChunksY / 2;
            maxChunkX = minChunkX + totalChunksX;
            maxChunkY = minChunkY + totalChunksY;

            // Precalcular límites de filtrado
            halfWidth = sizeOrRangeInTilesOrChunks.X / 2;
            halfHeight = sizeOrRangeInTilesOrChunks.Y / 2;
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
                Vector2I chunkBasePos = chunkCoord * chunkSize;

                bool isCenterChunk =
                    (mode == GenerateMode.CenteredByTileDimensions && chunkCoord == Vector2I.Zero) ||
                    (mode == GenerateMode.RadiusAroundChunk && chunkCoord == originChunk);

                for (int y = 0; y < chunkSize.Y; y++)
                {
                    int globalY = chunkBasePos.Y + y;

                    // Filtrado Y si es por dimensiones
                    if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalY) > halfHeight)
                        continue;

                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        int globalX = chunkBasePos.X + x;

                        // Filtrado X si es por dimensiones
                        if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalX) > halfWidth)
                            continue;

                        Vector2I globalPos = new Vector2I(globalX, globalY);

                        List<TerrainType> neighbors = GetNeighborTerrains(globalPos);
                        TerrainType terrain = GetTerrainWithContext(globalPos, neighbors);

                        if (generarCaminos && isCenterChunk &&
                            (x == chunkSize.X / 2 || y == chunkSize.Y / 2))
                        {
                            if (terrain == TerrainType.PisoBase)
                                terrain = TerrainType.CaminoPiso;
                            else if (terrain == TerrainType.Agua)
                                terrain = TerrainType.CaminoAgua;
                        }

                        AddTileBasicConfig(globalPos, terrain);
                    }
                }
            }
        }

        //Post - procesado directamente sobre mapTerrainBasic
        PostProcessWaterBordersOptimized(originChunk, mode, sizeOrRangeInTilesOrChunks);
        LimpiarBordesAisladosOptimized(originChunk, mode, sizeOrRangeInTilesOrChunks);
        RefinarElevacionesSueltasOptimized(originChunk, mode, sizeOrRangeInTilesOrChunks);
        MarcarBordesElevacionConBaseOptimized(originChunk, mode, sizeOrRangeInTilesOrChunks);
        AgregarBordesLimiteOptimized(originChunk, sizeOrRangeInTilesOrChunks, 2);
    }


  

    private void AgregarBordesLimiteOptimized(
    Vector2I mapaCentroGlobal,
    Vector2I tamanoMapaGlobal, // en tiles
    int grosorBorde)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;

        // Cálculo de límites del mapa
        Vector2I mitadMapa = tamanoMapaGlobal / 2;
        int minX = mapaCentroGlobal.X - mitadMapa.X;
        int maxX = mapaCentroGlobal.X + mitadMapa.X - 1;
        int minY = mapaCentroGlobal.Y - mitadMapa.Y;
        int maxY = mapaCentroGlobal.Y + mitadMapa.Y - 1;

        HashSet<Vector2I> bordePositions = new();

        // Generar posiciones del borde en grosor
        for (int d = 1; d <= grosorBorde; d++)
        {
            for (int x = minX - d; x <= maxX + d; x++)
            {
                bordePositions.Add(new Vector2I(x, minY - d)); // borde inferior
                bordePositions.Add(new Vector2I(x, maxY + d)); // borde superior
            }

            for (int y = minY - (d - 1); y <= maxY + (d - 1); y++)
            {
                bordePositions.Add(new Vector2I(minX - d, y)); // borde izquierdo
                bordePositions.Add(new Vector2I(maxX + d, y)); // borde derecho
            }
        }

        // Asignar directamente en mapTerrainBasic
        foreach (var pos in bordePositions)
        {
            var tile = mapTerrainBasic.GetTileGlobalPosition(pos);

            // Solo asignar si no hay tile o está vacío
            if (tile == null || (TerrainType)tile.GetTypeData() == default)
            {
                AddTileBasicConfig(pos, TerrainType.Limite);
            }
        }
    }






    private void RefinarElevacionesSueltasOptimized(
    Vector2I originChunk,
    GenerateMode mode,
    Vector2I sizeOrRangeInTilesOrChunks)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;

        int minChunkX, minChunkY, maxChunkX, maxChunkY;
        int halfWidth = 0, halfHeight = 0;

        if (mode == GenerateMode.CenteredByTileDimensions)
        {
            int totalChunksX = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.X / chunkSize.X);
            int totalChunksY = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.Y / chunkSize.Y);

            minChunkX = -totalChunksX / 2;
            minChunkY = -totalChunksY / 2;
            maxChunkX = minChunkX + totalChunksX;
            maxChunkY = minChunkY + totalChunksY;

            halfWidth = sizeOrRangeInTilesOrChunks.X / 2;
            halfHeight = sizeOrRangeInTilesOrChunks.Y / 2;
        }
        else // RadiusAroundChunk
        {
            int range = sizeOrRangeInTilesOrChunks.X;
            minChunkX = originChunk.X - range + 1;
            minChunkY = originChunk.Y - range + 1;
            maxChunkX = originChunk.X + range;
            maxChunkY = originChunk.Y + range;
        }

        // Direcciones cardinales
        Vector2I[] dirs = new[]
        {
        new Vector2I(0, -1),  // Norte
        new Vector2I(1, 0),   // Este
        new Vector2I(0, 1),   // Sur
        new Vector2I(-1, 0),  // Oeste
    };

        for (int chunkY = minChunkY; chunkY < maxChunkY; chunkY++)
        {
            for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++)
            {
                Vector2I chunkCoord = new Vector2I(chunkX, chunkY);
                Vector2I chunkBasePos = chunkCoord * chunkSize;

                for (int y = 0; y < chunkSize.Y; y++)
                {
                    int globalY = chunkBasePos.Y + y;
                    if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalY) > halfHeight)
                        continue;

                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        int globalX = chunkBasePos.X + x;
                        if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalX) > halfWidth)
                            continue;

                        Vector2I pos = new(globalX, globalY);
                        var tile = mapTerrainBasic.GetTileGlobalPosition(pos);

                        if ((TerrainType)tile.GetTypeData() != TerrainType.Elevacion)
                            continue;

                        int count = 0;

                        foreach (var dir in dirs)
                        {
                            Vector2I neighborPos = pos + dir;
                            var neighborTile = mapTerrainBasic.GetTileGlobalPosition(neighborPos);

                            if (neighborTile != null &&
                                (TerrainType)neighborTile.GetTypeData() == TerrainType.Elevacion)
                            {
                                count++;
                            }
                        }

                        // Si menos de 2 vecinos son elevación → convertir a piso base
                        if (count < 2)
                        {
                            AddTileBasicConfig(pos, TerrainType.PisoBase);
                        }
                    }
                }
            }
        }
    }


    private void MarcarBordesElevacionConBaseOptimized(
    Vector2I originChunk,
    GenerateMode mode,
    Vector2I sizeOrRangeInTilesOrChunks)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;

        int minChunkX, minChunkY, maxChunkX, maxChunkY;
        int halfWidth = 0, halfHeight = 0;

        if (mode == GenerateMode.CenteredByTileDimensions)
        {
            int totalChunksX = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.X / chunkSize.X);
            int totalChunksY = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.Y / chunkSize.Y);

            minChunkX = -totalChunksX / 2;
            minChunkY = -totalChunksY / 2;
            maxChunkX = minChunkX + totalChunksX;
            maxChunkY = minChunkY + totalChunksY;

            halfWidth = sizeOrRangeInTilesOrChunks.X / 2;
            halfHeight = sizeOrRangeInTilesOrChunks.Y / 2;
        }
        else // RadiusAroundChunk
        {
            int range = sizeOrRangeInTilesOrChunks.X;
            minChunkX = originChunk.X - range + 1;
            minChunkY = originChunk.Y - range + 1;
            maxChunkX = originChunk.X + range;
            maxChunkY = originChunk.Y + range;
        }

        // Direcciones cardinales
        Vector2I[] dirs = new[]
        {
        new Vector2I(0, -1),  // abajo
        new Vector2I(1, 0),   // derecha
        new Vector2I(0, 1),   // arriba
        new Vector2I(-1, 0),  // izquierda
    };

        for (int chunkY = minChunkY; chunkY < maxChunkY; chunkY++)
        {
            for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++)
            {
                Vector2I chunkCoord = new Vector2I(chunkX, chunkY);
                Vector2I chunkBasePos = chunkCoord * chunkSize;

                for (int y = 0; y < chunkSize.Y; y++)
                {
                    int globalY = chunkBasePos.Y + y;
                    if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalY) > halfHeight)
                        continue;

                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        int globalX = chunkBasePos.X + x;
                        if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalX) > halfWidth)
                            continue;

                        Vector2I pos = new(globalX, globalY);
                        var tile = mapTerrainBasic.GetTileGlobalPosition(pos);
                        if (tile == null || (TerrainType)tile.GetTypeData() != TerrainType.Elevacion)
                            continue;

                        foreach (var dir in dirs)
                        {
                            Vector2I neighborPos = pos + dir;
                            var neighborTile = mapTerrainBasic.GetTileGlobalPosition(neighborPos);
                            var neighborType = neighborTile != null
                                ? (TerrainType)neighborTile.GetTypeData()
                                : (TerrainType?)null;

                            // Detectar borde
                            if (neighborType == null ||
                                (neighborType != TerrainType.Elevacion && neighborType != TerrainType.ElevacionBorde))
                            {
                                AddTileBasicConfig(pos, TerrainType.ElevacionBorde);

                                // Si el borde está al sur, marcar hasta 2 tiles hacia abajo como ElevacionBase
                                if (dir.X == 0 && dir.Y == -1)
                                {
                                    for (int i = 1; i <= 2; i++)
                                    {
                                        Vector2I abajo = new(pos.X, pos.Y - i);
                                        var abajoTile = mapTerrainBasic.GetTileGlobalPosition(abajo);

                                        if (abajoTile != null &&
                                            (TerrainType)abajoTile.GetTypeData() != TerrainType.Elevacion)
                                        {
                                            AddTileBasicConfig(abajo, TerrainType.ElevacionBase);
                                        }
                                        else
                                        {
                                            break; // detenemos extensión si es Elevacion o no hay tile
                                        }
                                    }
                                }
                                break; // ya marcado, no revisar otras direcciones
                            }
                        }
                    }
                }
            }
        }
    }


    private void LimpiarBordesAisladosOptimized(
    Vector2I originChunk,
    GenerateMode mode,
    Vector2I sizeOrRangeInTilesOrChunks)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;

        int minChunkX, minChunkY, maxChunkX, maxChunkY;
        int halfWidth = 0, halfHeight = 0;

        if (mode == GenerateMode.CenteredByTileDimensions)
        {
            int totalChunksX = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.X / chunkSize.X);
            int totalChunksY = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.Y / chunkSize.Y);

            minChunkX = -totalChunksX / 2;
            minChunkY = -totalChunksY / 2;
            maxChunkX = minChunkX + totalChunksX;
            maxChunkY = minChunkY + totalChunksY;

            halfWidth = sizeOrRangeInTilesOrChunks.X / 2;
            halfHeight = sizeOrRangeInTilesOrChunks.Y / 2;
        }
        else // RadiusAroundChunk
        {
            int range = sizeOrRangeInTilesOrChunks.X;
            minChunkX = originChunk.X - range + 1;
            minChunkY = originChunk.Y - range + 1;
            maxChunkX = originChunk.X + range;
            maxChunkY = originChunk.Y + range;
        }

        // Direcciones cardinales
        Vector2I[] dirs = new[]
        {
        new Vector2I(0, -1),  // Norte
        new Vector2I(1, 0),   // Este
        new Vector2I(0, 1),   // Sur
        new Vector2I(-1, 0),  // Oeste
    };

        for (int chunkY = minChunkY; chunkY < maxChunkY; chunkY++)
        {
            for (int chunkX = minChunkX; chunkX < maxChunkX; chunkX++)
            {
                Vector2I chunkCoord = new Vector2I(chunkX, chunkY);
                Vector2I chunkBasePos = chunkCoord * chunkSize;

                for (int y = 0; y < chunkSize.Y; y++)
                {
                    int globalY = chunkBasePos.Y + y;
                    if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalY) > halfHeight)
                        continue;

                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        int globalX = chunkBasePos.X + x;
                        if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalX) > halfWidth)
                            continue;

                        Vector2I pos = new(globalX, globalY);
                        var tile = mapTerrainBasic.GetTileGlobalPosition(pos);

                        if ((TerrainType)tile.GetTypeData() != TerrainType.AguaBorde)
                            continue;

                        int count = 0;

                        foreach (var dir in dirs)
                        {
                            Vector2I neighborPos = pos + dir;
                            var neighborTile = mapTerrainBasic.GetTileGlobalPosition(neighborPos);

                            if (neighborTile != null)
                            {
                                var tType = (TerrainType)neighborTile.GetTypeData();
                                if (tType == TerrainType.Agua || tType == TerrainType.AguaBorde)
                                    count++;
                            }
                        }

                        // Si menos de 2 vecinos son agua/agua borde → lo convertimos en piso base
                        if (count < 2)
                        {
                            AddTileBasicConfig(pos, TerrainType.PisoBase);
                        }
                    }
                }
            }
        }
    }


    private void PostProcessWaterBordersOptimized(
    Vector2I originChunk,
    GenerateMode mode,
    Vector2I sizeOrRangeInTilesOrChunks)
    {
        Vector2I chunkSize = mapTerrainBasic.ChunkSize;

        int minChunkX, minChunkY, maxChunkX, maxChunkY;
        int halfWidth = 0, halfHeight = 0;

        if (mode == GenerateMode.CenteredByTileDimensions)
        {
            int totalChunksX = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.X / chunkSize.X);
            int totalChunksY = (int)Math.Ceiling((double)sizeOrRangeInTilesOrChunks.Y / chunkSize.Y);

            minChunkX = -totalChunksX / 2;
            minChunkY = -totalChunksY / 2;
            maxChunkX = minChunkX + totalChunksX;
            maxChunkY = minChunkY + totalChunksY;

            halfWidth = sizeOrRangeInTilesOrChunks.X / 2;
            halfHeight = sizeOrRangeInTilesOrChunks.Y / 2;
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
                Vector2I chunkBasePos = chunkCoord * chunkSize;

                for (int y = 0; y < chunkSize.Y; y++)
                {
                    int globalY = chunkBasePos.Y + y;
                    if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalY) > halfHeight)
                        continue;

                    for (int x = 0; x < chunkSize.X; x++)
                    {
                        int globalX = chunkBasePos.X + x;
                        if (mode == GenerateMode.CenteredByTileDimensions && Math.Abs(globalX) > halfWidth)
                            continue;

                        Vector2I pos = new(globalX, globalY);

                        var tile = mapTerrainBasic.GetTileGlobalPosition(pos);
                        if ((TerrainType)tile.GetTypeData() != TerrainType.Agua)
                            continue;

                        var neighbors = GetNeighborTerrainsComplete(pos);

                        bool isBorder = false;
                        int nonWaterCardinals = 0;

                        foreach (var (neighborPos, terrain) in neighbors)
                        {
                            if (terrain != TerrainType.Agua && terrain != TerrainType.AguaBorde)
                            {
                                var dir = neighborPos - pos;
                                if (Math.Abs(dir.X) + Math.Abs(dir.Y) == 1) // cardinal
                                {
                                    isBorder = true;
                                    nonWaterCardinals++;
                                }
                            }
                        }

                        // Verificar diagonales si no hay cardinales
                        if (!isBorder)
                        {
                            foreach (var (neighborPos, terrain) in neighbors)
                            {
                                if (terrain != TerrainType.Agua && terrain != TerrainType.AguaBorde)
                                {
                                    var dir = neighborPos - pos;
                                    if (Math.Abs(dir.X) == 1 && Math.Abs(dir.Y) == 1)
                                    {
                                        isBorder = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (isBorder)
                        {                            
                            AddTileBasicConfig(pos, TerrainType.AguaBorde);                            
                        }
                    }
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

    public static IEnumerable<Vector2I> GetChunksInRange(
    Vector2I originChunk,
    Vector2I sizeOrRangeInTilesOrChunks,
    GenerateMode mode)
    {
        List<Vector2I> chunks = new();

        switch (mode)
        {
            case GenerateMode.CenteredByTileDimensions:
                {
                    // sizeOrRangeInTilesOrChunks = tamaño en tiles
                    Vector2I halfChunks = new(
                        Mathf.CeilToInt(sizeOrRangeInTilesOrChunks.X / 2f),
                        Mathf.CeilToInt(sizeOrRangeInTilesOrChunks.Y / 2f)
                    );

                    Vector2I start = originChunk - halfChunks;
                    Vector2I end = originChunk + halfChunks;

                    for (int cy = start.Y; cy <= end.Y; cy++)
                    {
                        for (int cx = start.X; cx <= end.X; cx++)
                        {
                            chunks.Add(new Vector2I(cx, cy));
                        }
                    }
                }
                break;

            case GenerateMode.RadiusAroundChunk:
                {
                    // sizeOrRangeInTilesOrChunks.X = radio en chunks
                    int radius = sizeOrRangeInTilesOrChunks.X;

                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            chunks.Add(new Vector2I(originChunk.X + dx, originChunk.Y + dy));
                        }
                    }
                }
                break;
        }

        return chunks;
    }

    public void AplicarMapaDisenioOptimized(Vector2I originChunk,
    GenerateMode mode,
    Vector2I sizeOrRangeInTilesOrChunks, TerrainCategoryType terrainCategoryType)
    {
        // 1. Cargar tabla de equivalencias TerrainType -> idData
        BsonExpression bsonExpression = BsonExpression.Create(
            "category = @0 and isRule = @1",
            terrainCategoryType.ToString(),
            true
        );
        var result = DataBaseManager.Instance.FindAllFilter<TerrainData>(bsonExpression);

        Dictionary<TerrainType, int> tableData = result.ToDictionary(r => r.terrainType, r => r.id);
        tableData[TerrainType.Limite] = 0;
        tableData[TerrainType.ElevacionBorde] = 0;

        // 2. Recopilar tiles solo del rango de chunks
        List<(Vector2I pos, TerrainType terrainType, int idData)> allTiles = new();
        IEnumerable<Vector2I> chunksToProcess = GetChunksInRange(originChunk, sizeOrRangeInTilesOrChunks, mode);

        foreach (var chunkPos in chunksToProcess)
        {
            var tiles = mapTerrainBasic.GetTilesByChunk(chunkPos);
            if (tiles == null) continue;

            for (int x = 0; x < tiles.size.X; x++)
            {
                for (int y = 0; y < tiles.size.Y; y++)
                {
                    var tileData = tiles.GetTileAt(x, y);
                    if (tileData == null)
                        continue;

                    TerrainType terrainType = (TerrainType)tileData.GetTypeData();
                    if (!tableData.ContainsKey(terrainType))
                        continue;

                    int idData = tableData[terrainType];

                    // Convertir coordenadas locales del chunk a coordenadas globales
                    Vector2I globalPos = new Vector2I(
                        chunkPos.X * tiles.size.X + x,
                        chunkPos.Y * tiles.size.Y + y
                    );

                    allTiles.Add((globalPos, terrainType, idData));
                }
            }
        }

        // 3. Orden de pintado
        TerrainType[] ordenPintado = new TerrainType[]
        {
        TerrainType.Agua,
        TerrainType.PisoBase,
        TerrainType.Limite,
        TerrainType.AguaBorde,
        TerrainType.Elevacion,
        TerrainType.ElevacionBase,
        TerrainType.ElevacionBorde,
        };
        int idPiso = tableData[TerrainType.PisoBase];

        // 4. Aplicar al mapa en orden
        foreach (var tipo in ordenPintado)
        {
            foreach (var (pos, terrainType, idData) in allTiles.Where(t => t.terrainType == tipo))
            {
                switch (terrainType)
                {
                    case TerrainType.Limite:
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idPiso, 0);
                        break;

                    case TerrainType.PisoBase:
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idData, 0);
                        break;

                    case TerrainType.Elevacion:
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idData);
                        break;

                    case TerrainType.ElevacionBase:
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idPiso, 0);
                        break;

                    case TerrainType.ElevacionBorde:
                        int idElevacion = tableData[TerrainType.Elevacion];
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idElevacion);
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idPiso, 0);
                        break;

                    case TerrainType.Agua:
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idData);
                        break;

                    case TerrainType.CaminoPiso:
                    case TerrainType.CaminoAgua:
                    case TerrainType.AguaBorde:
                        int idAguaBorde = tableData[TerrainType.AguaBorde];
                        int idAgua = tableData[TerrainType.Agua];
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idAguaBorde);
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idAgua);
                        break;

                    default:
                        MapManagerEditor.Instance.CurrentMapLevelData.terrainMap.AddUpdateTile(pos, idData);
                        break;
                }
            }
        }
    }


    public void AddTileBasicConfig(Vector2I tilePositionGlobal, TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Limite:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 2);
                break;
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
            case TerrainType.ElevacionBorde:
                mapTerrainBasic.AddUpdatedTile(tilePositionGlobal, 10);
                break;
            default:
                break;
        }

    }
    private float Normalize(float value)
    {
        return (value + 1f) / 2f;
    }
}

