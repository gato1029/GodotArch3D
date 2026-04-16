using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyTiles.Systems;
public class BlackyVegetationSystem
{
    private readonly FastNoiseLite vegetationNoise;

    public BlackyVegetationSystem(int seed)
    {
        vegetationNoise = new FastNoiseLite(seed + 1000);
        vegetationNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        vegetationNoise.SetFrequency(0.15f);
    }

    public void Generate(BlackyWorld world, Vector2I chunk, BiomeType biome)
    {
        //var tiles = world.Terrain.GetChunkTiles(chunk);

        //foreach (var pos in tiles)
        //{
        //    float value = vegetationNoise.GetNoise2D(pos.X, pos.Y);

        //    if (value < 0.7f)
        //        continue;

        //    switch (biome)
        //    {
        //        case BiomeType.Forest:
        //            SpawnTree(world, pos);
        //            break;

        //        case BiomeType.Desert:
        //            SpawnCactus(world, pos);
        //            break;

        //        case BiomeType.Tundra:
        //            SpawnRock(world, pos);
        //            break;
        //    }
        //}
    }

    //private void SpawnTree(BlackyWorld world, Vector2I pos)
    //{
    //    // ejemplo
    //    world.Terrain.PlaceDecoration(pos, DecorationType.Tree);
    //}

    //private void SpawnCactus(BlackyWorld world, Vector2I pos)
    //{
    //    world.Terrain.PlaceDecoration(pos, DecorationType.Cactus);
    //}

    //private void SpawnRock(BlackyWorld world, Vector2I pos)
    //{
    //    world.Terrain.PlaceDecoration(pos, DecorationType.Rock);
    //}
}