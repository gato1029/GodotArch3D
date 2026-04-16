using Godot;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotEcsArch.sources.BlackyTiles.Procedural.Terrain;

class BiomeRuntimeData
{
    public ushort Id;
    public float CenterTemp;
    public float CenterHumidity;
}

public class BlackyWorldBiomeMap
{
    private readonly int seed;
    private readonly int chunkSize;
    private readonly WorldType worldType;

    private readonly FastNoiseLite continentalNoise;
    private readonly FastNoiseLite temperatureNoise;
    private readonly FastNoiseLite humidityNoise;

    private readonly Dictionary<Vector2I, ushort[,]> chunkCache = new();
    private readonly Dictionary<Vector2I, ushort[,]> chunkBorderCache = new();

    private BiomeRuntimeData[] biomeRuntime;

    // 🔥 TRANSITIONS COMPLETAS
    private Dictionary<(ushort, ushort), TerrainDataTransition> transitions;

    public BlackyWorldBiomeMap(int seed, int chunkSize, WorldType worldType)
    {
        this.seed = seed;
        this.chunkSize = chunkSize;
        this.worldType = worldType;

        continentalNoise = new FastNoiseLite(seed);
        continentalNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

        continentalNoise.SetFrequency(worldType switch
        {
            WorldType.Islands => 0.015f,
            WorldType.Balanced => 0.01f,
            WorldType.Continents => 0.005f,
            _ => 0.01f
        });

        temperatureNoise = new FastNoiseLite(seed + 1000);
        temperatureNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        temperatureNoise.SetFrequency(0.01f);

        humidityNoise = new FastNoiseLite(seed + 2000);
        humidityNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        humidityNoise.SetFrequency(0.01f);

        LoadBiomes();
        LoadTransitions();
    }

    // =========================================
    // 🌱 BIOMAS BASE
    // =========================================

    private void LoadBiomes()
    {
        MasterDataManager.RegisterAllData<long, TerrainData>();

        var data = MasterDataManager.GetAllData<long, TerrainData>();

        if (data == null)
            return;

        biomeRuntime = data
            .Where(b => !b.isTransition && !b.isWater)
            .Select(b => new BiomeRuntimeData
            {
                Id = (ushort)b.idSave,
                CenterTemp = (b.minTemperature + b.maxTemperature) * 0.5f,
                CenterHumidity = (b.minHumidity + b.maxHumidity) * 0.5f
            })
            .ToArray();
    }

    // =========================================
    // 🔥 TRANSICIONES
    // =========================================

    private void LoadTransitions()
    {
        MasterDataManager.RegisterAllData<long, TerrainDataTransition>();

        var data = MasterDataManager.GetAllData<long, TerrainDataTransition>();

        transitions = data?
            .ToDictionary(
                t => (t.idTerrainBeginId, t.idTerrainEndId),
                t => t
            )
            ?? new Dictionary<(ushort, ushort), TerrainDataTransition>();
    }

    private bool TryGetTransition(ushort a, ushort b, out TerrainDataTransition t)
    {
        if (transitions.TryGetValue((a, b), out t))
            return true;

        if (transitions.TryGetValue((b, a), out t))
            return true;

        t = null;
        return false;
    }

    // =========================================
    // 🌍 CHUNK API (OPTIMIZADO)
    // =========================================

    public ushort[,] GetChunkBorders(Vector2I chunkCoord)
    {
        if (chunkBorderCache.TryGetValue(chunkCoord, out var borders))
            return borders;

        GetChunkBiomes(chunkCoord);

        return chunkBorderCache[chunkCoord];
    }

    public ushort[,] GetChunkBiomes(Vector2I chunkCoord)
    {
        if (chunkCache.TryGetValue(chunkCoord, out var cached))
            return cached;

        int padding = 1;
        int paddedSize = chunkSize + padding * 2;

        ushort[,] baseMap = new ushort[paddedSize, paddedSize];
        ushort[,] finalMap = new ushort[chunkSize, chunkSize];

        int startX = chunkCoord.X * chunkSize - padding;
        int startY = chunkCoord.Y * chunkSize - padding;

        for (int x = 0; x < paddedSize; x++)
        {
            for (int y = 0; y < paddedSize; y++)
            {
                int worldX = startX + x;
                int worldY = startY + y;

                baseMap[x, y] = GetBaseBiome(worldX, worldY);
            }
        }

        ushort[,] tempMap = new ushort[paddedSize, paddedSize];

        for (int x = 0; x < paddedSize; x++)
        {
            for (int y = 0; y < paddedSize; y++)
            {
                tempMap[x, y] = ApplyTransitions(baseMap, x, y);
            }
        }

        tempMap = SmoothMapFast(tempMap, 3);        

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                finalMap[x, y] = tempMap[x + padding, y + padding];
            }
        }

        ushort[,] paddedBorderMap = BuildBorderMap(tempMap);
        ushort[,] borderMap = new ushort[chunkSize, chunkSize];
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                borderMap[x, y] = paddedBorderMap[x + padding, y + padding];
            }
        }
     

        // cachear ambos
        chunkCache[chunkCoord] = finalMap;
        chunkBorderCache[chunkCoord] = borderMap;

        return finalMap;
    }
    private ushort[,] BuildBorderMap(ushort[,] map)
    {
        int size = map.GetLength(0);
        ushort[,] borderMap = new ushort[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                borderMap[x, y] = GetBorderSelf(map, x, y);
            }
        }

        return borderMap;
    }
    private ushort GetBorderSelf(ushort[,] map, int x, int y)
    {
        int size = map.GetLength(0);
        ushort current = map[x, y];

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                    continue;

                if (map[nx, ny] != current)
                    return current; // 🔥 TU enfoque
            }
        }

        return 0;
    }
    private ushort[,] SmoothMapFast(ushort[,] map, int minNeighbors = 3)
    {
        int size = map.GetLength(0);
        ushort[,] result = new ushort[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                ushort current = map[x, y];

                int sameCount = 0;

                // 👇 conteo directo sin estructuras
                Span<ushort> neighbors = stackalloc ushort[9];
                int n = 0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                            continue;

                        ushort b = map[nx, ny];
                        neighbors[n++] = b;

                        if (b == current)
                            sameCount++;
                    }
                }

                if (sameCount >= minNeighbors)
                {
                    result[x, y] = current;
                    continue;
                }

                // 🔥 mayoría sin diccionario
                ushort bestBiome = current;
                int bestCount = 0;

                for (int i = 0; i < n; i++)
                {
                    ushort candidate = neighbors[i];
                    int count = 0;

                    for (int j = 0; j < n; j++)
                    {
                        if (neighbors[j] == candidate)
                            count++;
                    }

                    if (count > bestCount)
                    {
                        bestCount = count;
                        bestBiome = candidate;
                    }
                }

                result[x, y] = bestBiome;
            }
        }

        return result;
    }
    private ushort[,] SmoothMap(ushort[,] map, int minNeighbors = 3)
    {
        int size = map.GetLength(0);
        ushort[,] result = new ushort[size, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                ushort current = map[x, y];

                int same = CountSameNeighbors(map, x, y, current);

                // 🔥 si no cumple coherencia → reemplazar
                if (same < minNeighbors)
                {
                    result[x, y] = GetMajorityBiome(map, x, y);
                }
                else
                {
                    result[x, y] = current;
                }
            }
        }

        return result;
    }
    private int CountSameNeighbors(ushort[,] map, int x, int y, ushort biome)
    {
        int count = 0;
        int size = map.GetLength(0);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                    continue;

                if (map[nx, ny] == biome)
                    count++;
            }
        }

        return count;
    }
    private ushort GetMajorityBiome(ushort[,] map, int x, int y)
    {
        Dictionary<ushort, int> counts = new();
        int size = map.GetLength(0);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                    continue;

                ushort b = map[nx, ny];

                if (!counts.ContainsKey(b))
                    counts[b] = 0;

                counts[b]++;
            }
        }

        return counts.OrderByDescending(kv => kv.Value).First().Key;
    }

    public ushort GetBiomeAt(int worldX, int worldY)
    {
        Vector2I chunkCoord = new Vector2I(
            Mathf.FloorToInt((float)worldX / chunkSize),
            Mathf.FloorToInt((float)worldY / chunkSize)
        );

        ushort[,] chunk = GetChunkBiomes(chunkCoord);

        int localX = Mod(worldX, chunkSize);
        int localY = Mod(worldY, chunkSize);

        return chunk[localX, localY];
    }
    public ushort GetBorderAt(int worldX, int worldY)
    {
        Vector2I chunkCoord = new Vector2I(
            Mathf.FloorToInt((float)worldX / chunkSize),
            Mathf.FloorToInt((float)worldY / chunkSize)
        );

        ushort[,] chunk = GetChunkBorders(chunkCoord);

        int localX = Mod(worldX, chunkSize);
        int localY = Mod(worldY, chunkSize);

        return chunk[localX, localY];
    }

    private int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }
    // =========================================
    // 🌍 BIOMA BASE (SIN TRANSICIONES)
    // =========================================

    private ushort GetBaseBiome(int x, int y)
    {
        if (!IsLand(x, y))
            return 1;

        float temp = GetTemperature(x, y);
        float humidity = GetHumidity(x, y);

        return ResolveBiome(temp, humidity);
    }

    private ushort ResolveBiome(float temp, float humidity)
    {
        float bestDist = float.MaxValue;
        ushort best = 0;

        for (int i = 0; i < biomeRuntime.Length; i++)
        {
            var b = biomeRuntime[i];

            float dT = temp - b.CenterTemp;
            float dH = humidity - b.CenterHumidity;

            float dist = dT * dT + dH * dH;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = b.Id;
            }
        }

        return best;
    }

    // =========================================
    // 🔥 TRANSICIONES CON THICKNESS (OPTIMIZADAS)
    // =========================================

    private ushort ApplyTransitions(ushort[,] map, int x, int y)
    {
        ushort biome = map[x, y];

        int maxSearch = 6;

        ushort other = FindClosestDifferentBiome(map, x, y, biome, maxSearch);

        if (other == biome)
            return biome;

        if (!TryGetTransition(biome, other, out var transition))
            return biome;

        int distance = DistanceToBiome(map, x, y, other, transition.thickness);

        if (distance <= transition.thickness)
            return transition.idTerrainResoluteId;

        return biome;
    }

    private ushort FindClosestDifferentBiome(ushort[,] map, int x, int y, ushort biome, int maxRadius)
    {
        int size = map.GetLength(0);

        for (int r = 1; r <= maxRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                        continue;

                    ushort other = map[nx, ny];

                    if (other != biome)
                        return other;
                }
            }
        }

        return biome;
    }

    private int DistanceToBiome(ushort[,] map, int x, int y, ushort target, int maxRadius)
    {
        int size = map.GetLength(0);

        for (int r = 1; r <= maxRadius; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                        continue;

                    if (map[nx, ny] == target)
                        return r;
                }
            }
        }

        return int.MaxValue;
    }

    // =========================================
    // 🌊 NOISE
    // =========================================

    private bool IsLand(int x, int y)
    {
        float value = continentalNoise.GetNoise(x, y);

        float threshold = worldType switch
        {
            WorldType.Islands => 0.4f,
            WorldType.Balanced => 0.0f,
            WorldType.Continents => -0.4f,
            _ => 0f
        };

        return value > threshold;
    }

    private float GetTemperature(int x, int y)
        => (temperatureNoise.GetNoise(x, y) + 1f) * 0.5f;

    private float GetHumidity(int x, int y)
        => (humidityNoise.GetNoise(x, y) + 1f) * 0.5f;

    // =========================================
    // 🧹 CACHE
    // =========================================

    public void ClearCache()
    {
        chunkCache.Clear();
    }
    public void ExportWorldStitched(
Dictionary<Vector2I, ushort[,]> chunks,
string path = "user://biome_debug/world_full.txt")
    {
        DirAccess.MakeDirRecursiveAbsolute("user://biome_debug/");

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        if (chunks.Count == 0)
            return;

        int chunkSize = chunks.First().Value.GetLength(0);

        // 🔥 calcular bounds del mundo
        int minX = chunks.Keys.Min(c => c.X);
        int maxX = chunks.Keys.Max(c => c.X);
        int minY = chunks.Keys.Min(c => c.Y);
        int maxY = chunks.Keys.Max(c => c.Y);

        int worldWidth = (maxX - minX + 1) * chunkSize;
        int worldHeight = (maxY - minY + 1) * chunkSize;

        ushort[,] worldMap = new ushort[worldWidth, worldHeight];

        // 🔥 copiar chunks al mapa global
        foreach (var kv in chunks
              .OrderBy(c => c.Key.Y)
              .ThenBy(c => c.Key.X))
        {
            Vector2I coord = kv.Key;
            ushort[,] chunk = kv.Value;

            int offsetX = (coord.X - minX) * chunkSize;
            int offsetY = (coord.Y - minY) * chunkSize;

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    worldMap[offsetX + x, offsetY + y] = chunk[x, y];
                }
            }
        }

        // 🔥 escribir archivo (invertimos Y para verlo bien)
        for (int y = worldHeight - 1; y >= 0; y--)
        {
            string line = "";

            for (int x = 0; x < worldWidth; x++)
            {
                line += worldMap[x, y].ToString();
            }

            file.StoreLine(line);
        }

        GD.Print($"🌍 World exportado en: {path}");
    }

    public void ExportWorldBordersStitched(
    Dictionary<Vector2I, ushort[,]> chunks,
    string path = "user://biome_debug/world_borders.txt")
    {
        DirAccess.MakeDirRecursiveAbsolute("user://biome_debug/");

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        if (chunks.Count == 0)
            return;

        int chunkSize = chunks.First().Value.GetLength(0);

        int minX = chunks.Keys.Min(c => c.X);
        int maxX = chunks.Keys.Max(c => c.X);
        int minY = chunks.Keys.Min(c => c.Y);
        int maxY = chunks.Keys.Max(c => c.Y);

        int worldWidth = (maxX - minX + 1) * chunkSize;
        int worldHeight = (maxY - minY + 1) * chunkSize;

        ushort[,] worldMap = new ushort[worldWidth, worldHeight];

        // 🔥 copiar chunks
        foreach (var kv in chunks
                .OrderBy(c => c.Key.Y)
                .ThenBy(c => c.Key.X))
        {
            Vector2I coord = kv.Key;
            ushort[,] chunk = kv.Value;

            int offsetX = (coord.X - minX) * chunkSize;
            int offsetY = (coord.Y - minY) * chunkSize;

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    worldMap[offsetX + x, offsetY + y] = chunk[x, y];
                }
            }
        }

        // 🔥 escribir con espacios para claridad
        for (int y = worldHeight - 1; y >= 0; y--)
        {
            string line = "";

            for (int x = 0; x < worldWidth; x++)
            {
                line += worldMap[x, y].ToString();
            }

            file.StoreLine(line);
        }

        GD.Print($"🟥 World borders exportado: {path}");
    }
    public void ExportChunkToTxt(Vector2I chunkCoord, ushort[,] map, string folder = "user://biome_debug/")
    {
        DirAccess.MakeDirRecursiveAbsolute(folder);

        string path = $"{folder}chunk_{chunkCoord.X}_{chunkCoord.Y}.txt";

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);

        int size = map.GetLength(0);

        for (int y = 0; y < size; y++)
        {
            string line = "";

            for (int x = 0; x < size; x++)
            {
                line += map[x, y].ToString().PadLeft(3, ' ') + " ";
            }

            file.StoreLine(line);
        }

        GD.Print($"Chunk exportado: {path}");
    }
}