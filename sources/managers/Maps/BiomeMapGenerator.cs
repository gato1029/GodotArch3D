using System;

public enum BiomeType
{
    Ocean,
    Beach,
    Plains,
    Forest,
    Desert,
    Mountain,
    Snow
}

public class BiomeTile
{
    public BiomeType Type;
    public float Elevation;
    public float Temperature;
    public float Moisture;
}

public class BiomeMapGenerator
{
    private int seed;
    private float scale;

    private FastNoiseLite elevationNoise;
    private FastNoiseLite temperatureNoise;
    private FastNoiseLite moistureNoise;

    public BiomeMapGenerator(int seed = 1337, float scale = 0.05f)
    {
        this.seed = seed;
        this.scale = scale;

        elevationNoise = new FastNoiseLite(seed);
        elevationNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        elevationNoise.SetFrequency(scale);

        temperatureNoise = new FastNoiseLite(seed + 100);
        temperatureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        temperatureNoise.SetFrequency(scale);

        moistureNoise = new FastNoiseLite(seed + 200);
        moistureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        moistureNoise.SetFrequency(scale);
    }

    public BiomeTile[,] Generate(int width, int height)
    {
        BiomeTile[,] map = new BiomeTile[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float elevation = Normalize(elevationNoise.GetNoise(x, y));
                float temperature = Normalize(temperatureNoise.GetNoise(x, y));
                float moisture = Normalize(moistureNoise.GetNoise(x, y));

                BiomeType biome = ClassifyBiome(elevation, temperature, moisture);

                map[x, y] = new BiomeTile
                {
                    Elevation = elevation,
                    Temperature = temperature,
                    Moisture = moisture,
                    Type = biome
                };
            }
        }

        return map;
    }

    private float Normalize(float value)
    {
        // FastNoiseLite devuelve valores entre -1 y 1, lo convertimos a 0–1
        return (value + 1f) / 2f;
    }

    private BiomeType ClassifyBiome(float elevation, float temperature, float moisture)
    {
        if (elevation < 0.3f)
            return BiomeType.Ocean;
        if (elevation < 0.35f)
            return BiomeType.Beach;

        if (elevation > 0.85f)
            return temperature < 0.4f ? BiomeType.Snow : BiomeType.Mountain;

        if (temperature > 0.8f)
            return moisture < 0.3f ? BiomeType.Desert : BiomeType.Plains;

        if (moisture > 0.6f)
            return BiomeType.Forest;

        return BiomeType.Plains;
    }
}
