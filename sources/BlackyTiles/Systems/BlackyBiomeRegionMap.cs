using Godot;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Systems;


public struct BiomePoint
{
    public Vector2I Position;
    public ushort BiomeId;

    public bool IsBorder;

    public BiomePoint(Vector2I position, ushort biomeId, bool isBorder)
    {
        Position = position;
        BiomeId = biomeId;
        IsBorder = isBorder;
    }
}
public struct BiomeRegion
{
    // Coordenada de la región en el grid
    public Vector2I RegionCoord;

    // Centro aproximado
    public Vector2 Center;

    // tamaño de región en chunks
    public int RegionSize;

    // puntos que definen los bordes del bioma
    public BiomePoint[] BorderPoints;

    // puntos internos del bioma
    public BiomePoint[] FillPoints;
}

public enum WorldType
{
    Islands,
    Balanced,
    Continents
}
public class BlackyBiomeRegionMap
{
    private readonly int seed;
    private readonly int regionSize;

    private readonly Dictionary<Vector2I, BiomeRegion> regions = new();
    private ushort[] biomeIds;
    private float[] cumulativeWeights;
    private float totalWeight;

    private FastNoiseLite continentalNoise;
    private WorldType worldType;

    private readonly Dictionary<Vector2I, BiomePoint[,]> chunkCache = new();
    public BlackyBiomeRegionMap(int seed, int regionSize, WorldType worldType)
    {
        this.seed = seed;
        this.regionSize = regionSize;
        this.worldType = worldType;

        continentalNoise = new FastNoiseLite(seed);

        continentalNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        switch (worldType)
        {
            case WorldType.Islands:
                continentalNoise.SetFrequency(0.02f);
                break;
            case WorldType.Balanced:
                continentalNoise.SetFrequency(0.03f);
                break;
            case WorldType.Continents:
                continentalNoise.SetFrequency(0.005f);
                break;
            default:
                break;
        }
        

        BuildBiomeWeights();
    }
    private bool IsLand(int worldX, int worldY)
    {
        float value = continentalNoise.GetNoise(worldX, worldY);

        float threshold = worldType switch
        {
            WorldType.Islands => 0.4f,
            WorldType.Balanced => 0.0f,
            WorldType.Continents => -0.4f,
            _ => 0f
        };

        return value > threshold;
    }
    public BiomePoint[,] GetBiomesForChunk(Vector2I chunkCoord, int chunkSize)
    {
        if (chunkCache.TryGetValue(chunkCoord, out var cached))
            return cached;

        BiomePoint[,] result = new BiomePoint[chunkSize, chunkSize];

        int startX = chunkCoord.X * chunkSize;
        int startY = chunkCoord.Y * chunkSize;

        int regionX = Mathf.FloorToInt((float)startX / regionSize);
        int regionY = Mathf.FloorToInt((float)startY / regionSize);

        List<BiomeRegion> nearbyRegions = new();

        for (int rx = -1; rx <= 1; rx++)
        {
            for (int ry = -1; ry <= 1; ry++)
            {
                var coord = new Vector2I(regionX + rx, regionY + ry);
                nearbyRegions.Add(GetRegion(coord));
            }
        }

        const float BORDER_THRESHOLD = 64f; // ancho del borde

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = startX + x;
                int worldY = startY + y;

                bool isLand = IsLand(worldX, worldY);
                //if (!isLand)
                //{
                //    result[x, y] = new BiomePoint(
                //        new Vector2I(worldX, worldY),
                //        0, // océano
                //        false
                //    );
                //    continue;
                //}
                int neighbors = CountLandNeighbors(worldX, worldY);

                if (isLand)
                {
                    if (neighbors <= 2)
                        isLand = false;
                }
                else
                {
                    if (neighbors >= 5)
                        isLand = true;
                }

                float bestDist = float.MaxValue;
                float secondDist = float.MaxValue;

                ushort bestBiome = 0;
                ushort secondBiome = 0;

                bool bestIsBorderPoint = false;

                foreach (var region in nearbyRegions)
                {
                    CheckPointsImproved(
                        region.FillPoints,
                        worldX,
                        worldY,
                        ref bestDist,
                        ref secondDist,
                        ref bestBiome,
                        ref secondBiome,
                        ref bestIsBorderPoint,
                        false
                    );

                    CheckPointsImproved(
                        region.BorderPoints,
                        worldX,
                        worldY,
                        ref bestDist,
                        ref secondDist,
                        ref bestBiome,
                        ref secondBiome,
                        ref bestIsBorderPoint,
                        true
                    );
                }

                bool isBorder =
                    bestIsBorderPoint ||
                    (bestBiome != secondBiome &&
                    Math.Abs(secondDist - bestDist) < BORDER_THRESHOLD);

                result[x, y] = new BiomePoint(
                    new Vector2I(worldX, worldY),
                    bestBiome,
                    isBorder
                );
            }
        }

        chunkCache[chunkCoord] = result;

        return result;
    }
    private bool IsIsolatedLand(int worldX, int worldY)
    {
        if (!IsLand(worldX, worldY))
            return false;

        int neighbors = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                if (IsLand(worldX + x, worldY + y))
                    neighbors++;
            }
        }

        return neighbors <= 1;
    }
    private int CountLandNeighbors(int worldX, int worldY)
    {
        int neighbors = 0;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                if (IsLand(worldX + x, worldY + y))
                    neighbors++;
            }
        }

        return neighbors;
    }
    private void CheckPointsImproved(
    BiomePoint[] points,
    int worldX,
    int worldY,
    ref float bestDist,
    ref float secondDist,
    ref ushort bestBiome,
    ref ushort secondBiome,
    ref bool bestIsBorderPoint,
    bool isBorderPoint)
    {
        for (int i = 0; i < points.Length; i++)
        {
            float dx = worldX - points[i].Position.X;
            float dy = worldY - points[i].Position.Y;

            float dist = dx * dx + dy * dy;

            if (dist < bestDist)
            {
                secondDist = bestDist;
                secondBiome = bestBiome;

                bestDist = dist;
                bestBiome = points[i].BiomeId;
                bestIsBorderPoint = isBorderPoint;
            }
            else if (dist < secondDist)
            {
                secondDist = dist;
                secondBiome = points[i].BiomeId;
            }
        }
    }
    private void CheckPoints(
    BiomePoint[] points,
    int worldX,
    int worldY,
    ref float bestDist,
    ref ushort biomeId,
    ref bool isBorder)
    {
        for (int i = 0; i < points.Length; i++)
        {
            float dx = worldX - points[i].Position.X;
            float dy = worldY - points[i].Position.Y;

            float dist = dx * dx + dy * dy;

            if (dist < bestDist)
            {
                bestDist = dist;
                biomeId = points[i].BiomeId;
                isBorder = points[i].IsBorder;
            }
        }
    }
    public ushort GetBiomeId(int worldX, int worldY)
    {
        if (!IsLand(worldX, worldY))
            return 0; // biome 0 = océano

        int regionX = Mathf.FloorToInt((float)worldX / regionSize);
        int regionY = Mathf.FloorToInt((float)worldY / regionSize);

        float bestDist = float.MaxValue;
        ushort biomeId = 0;

        for (int rx = -1; rx <= 1; rx++)
        {
            for (int ry = -1; ry <= 1; ry++)
            {
                var coord = new Vector2I(regionX + rx, regionY + ry);

                var region = GetRegion(coord);

                CheckPoints(region.FillPoints, worldX, worldY, ref bestDist, ref biomeId);
                CheckPoints(region.BorderPoints, worldX, worldY, ref bestDist, ref biomeId);
            }
        }

        return biomeId;
    }

    private void CheckPoints(
        BiomePoint[] points,
        int worldX,
        int worldY,
        ref float bestDist,
        ref ushort biomeId)
    {
        for (int i = 0; i < points.Length; i++)
        {
            float dx = worldX - points[i].Position.X;
            float dy = worldY - points[i].Position.Y;

            float dist = dx * dx + dy * dy;

            if (dist < bestDist)
            {
                bestDist = dist;
                biomeId = points[i].BiomeId;
            }
        }
    }

    private BiomeRegion GetRegion(Vector2I coord)
    {
        if (!regions.TryGetValue(coord, out var region))
        {
            region = GenerateRegion(coord);
            regions.Add(coord, region);
        }

        return region;
    }

    private BiomeRegion GenerateRegion(Vector2I coord)
    {
        Random rng = CreateRegionRandom(coord);

        float minX = coord.X * regionSize;
        float minY = coord.Y * regionSize;

        float maxX = minX + regionSize;
        float maxY = minY + regionSize;

        Vector2 center = new Vector2(
            (minX + maxX) * 0.5f,
            (minY + maxY) * 0.5f
        );

        // Distancias mínimas entre puntos
        float fillMinDistance = regionSize * 0.35f;
        float borderMinDistance = regionSize * 0.45f;

        // Cantidad máxima de puntos
        int fillMaxPoints = 5;
        int borderMaxPoints = 4;

        // Generar puntos Poisson
        var fillPositions = GeneratePoissonPoints(
            minX,
            minY,
            maxX,
            maxY,
            fillMinDistance,
            fillMaxPoints,
            rng
        );

        var borderPositions = GeneratePoissonPoints(
            minX,
            minY,
            maxX,
            maxY,
            borderMinDistance,
            borderMaxPoints,
            rng
        );

        // Convertir a BiomePoints
        BiomePoint[] fillPoints = new BiomePoint[fillPositions.Count];

        for (int i = 0; i < fillPositions.Count; i++)
        {
            ushort biome = PickBiome(rng);

            fillPoints[i] = new BiomePoint(
                fillPositions[i],
                biome,false
            );
        }

        BiomePoint[] borderPoints = new BiomePoint[borderPositions.Count];

        for (int i = 0; i < borderPositions.Count; i++)
        {
            ushort biome = PickBiome(rng);

            borderPoints[i] = new BiomePoint(
                borderPositions[i],
                biome,true
            );
        }

        return new BiomeRegion
        {
            RegionCoord = coord,
            Center = center,
            RegionSize = regionSize,
            FillPoints = fillPoints,
            BorderPoints = borderPoints
        };
    }

    private Random CreateRegionRandom(Vector2I coord)
    {
        int hash = seed;
        hash ^= coord.X * 73856093;
        hash ^= coord.Y * 19349663;
        return new Random(hash);
    }
    private void BuildBiomeWeights()
    {
        MasterDataManager.RegisterAllData<long, TerrainData>(); 
        var data = MasterDataManager.GetAllData<long, TerrainData>();

        if (data == null || data.Count() == 0)
            return;

        int count = data.Count();

        biomeIds = new ushort[count];
        cumulativeWeights = new float[count];

        float cumulative = 0f;

        int i = 0;

        foreach (var biome in data)
        {
            //cumulative += biome.humendity;

            biomeIds[i] = (ushort)biome.idSave;
            cumulativeWeights[i] = cumulative;

            i++;
        }

        totalWeight = cumulative;
    }
    private ushort PickBiome(Random rng)
    {
        if (biomeIds == null || biomeIds.Length == 0)
            return 0;

        float value = (float)(rng.NextDouble() * totalWeight);

        for (int i = 0; i < cumulativeWeights.Length; i++)
        {
            if (value <= cumulativeWeights[i])
                return biomeIds[i];
        }

        return biomeIds[0];
    }

    private List<Vector2I> GeneratePoissonPoints(
    float minX,
    float minY,
    float maxX,
    float maxY,
    float minDistance,
    int maxPoints,
    Random rng)
    {
        List<Vector2I> points = new();

        int attempts = maxPoints * 10;

        while (points.Count < maxPoints && attempts > 0)
        {
            attempts--;

            int x = (int)(minX + rng.NextDouble() * (maxX - minX));
            int y = (int)(minY + rng.NextDouble() * (maxY - minY));

            Vector2I candidate = new Vector2I(x, y);

            bool valid = true;

            for (int i = 0; i < points.Count; i++)
            {
                float dist = candidate.DistanceSquaredTo(points[i]);

                if (dist < minDistance * minDistance)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
                points.Add(candidate);
        }

        return points;
    }
}