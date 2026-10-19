using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.managers.Mods;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TerrainBase;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.BlackyEngine.Generation.Territory;

public class BlackyWorldProceduralTerritory
{
    private readonly BlackyWorld _world;
    private BlackyWorldSeed _worldSeed;    
    private BlackyWorldConfig _config;
    private BlackyWorldGeography _geography;
    private TemplateWorldProfileData _worldProfile;
    private int _heightMax;
    private BlackyWorldBiomes _biomes;
    public BlackyWorldProceduralTerritory(BlackyWorld world, BlackyWorldConfig worldConfig)
    {
        _worldProfile = AtlasModsManager.GetDirect<TemplateWorldProfileData>(worldConfig.IdPerfiladorMundo);        
        ConfiguracionGeograficaData configuracionGeografica = new ConfiguracionGeograficaData(_worldProfile.perfilGeografico);
        _heightMax = configuracionGeografica.AlturaMaxima;
        _world = world;
        _config = worldConfig;
        _worldSeed = new BlackyWorldSeed(_config.WorldSeed);        
        _geography = new BlackyWorldGeography(worldConfig,_worldSeed,_world.Services.HeightMapWorld,  configuracionGeografica);

        _biomes= new BlackyWorldBiomes(worldConfig, _worldSeed, _world.Services.HeightMapWorld, _worldProfile);
        
        

        GenerateGeography();
        GenerateTerritory();
        world.Streaming.chunkManagerLocal.Teleport(new Vector2(0, 0)); // esto hace que se renderice el chunk central y se carguen los chunks alrededor
    }
    private void GenerateGeography()
    {
        _geography.Generate();

        // debug: exporta el mapa de terreno generado a un archivo txt y una imagen png en el escritorio
        //string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".txt");
        string imgPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".png");
        //string filePath2 = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoOri" + _config.MapSize.ToString() + _worldProfile.perfilGeografico.ToString() + ".txt");
        //_world.Services.HeightMapWorld.ExportHeightsToTxt(filePath2);
        //_geography.ExportDebugTxt(filePath); // debug: genera un txt con el mapa de terreno generado
        _geography.ExportDebugPng(imgPath); // debug: genera una imagen con el mapa de terreno generado
    }
    public void GenerateTerritory()
    {
        const int chunkSize = 32;
        var terrainCache = new Dictionary<long, TerrainBaseData>();

        foreach (var (chunkX, chunkY, region) in _biomes.EnumerateChunks(chunkSize))
        {

            for (int y = 0; y < region.Height; y++)
            {
                int worldY = region.MinY + y;

                for (int x = 0; x < region.Width; x++)
                {
                    BiomeCell cell = region.Cells[y * region.Width + x];
                    if (cell.Bioma == null) continue; // Fuera del mundo

                    int worldX = region.MinX + x;

                    bool esBordeMapa = worldX == _biomes.MinX || worldX == _biomes.MaxX || worldY == _biomes.MinY || worldY == _biomes.MaxY;

                    bool esBordeChunk = x == 0 || x == region.Width - 1 || y == 0 || y == region.Height - 1;

                    if (!terrainCache.TryGetValue(cell.IdTerreno, out var terreno))
                    {
                        terreno = AtlasModsManager.GetDirect<TerrainBaseData>(cell.IdTerreno);
                        terrainCache[cell.IdTerreno] = terreno;
                    }

                    TerrainBaseData terrenoInferior = null;
                    if (cell.IsHeightTransition && cell.HeightLevel > 0)
                    {
                        long idTerrenoInferior = cell.HeightLowerTerrainId;

                        if (!terrainCache.TryGetValue(idTerrenoInferior, out terrenoInferior))
                        {
                            terrenoInferior =
                                AtlasModsManager.GetDirect<TerrainBaseData>(idTerrenoInferior);
                            terrainCache[idTerrenoInferior] = terrenoInferior;
                        }

                        _world.Services.TerrainDataLienzo.SetTerrainDirectNoRender(worldX,worldY, cell.HeightLevel - 1,false, terrenoInferior);
                    }                 
                    if (esBordeMapa || esBordeChunk)
                    {
                        _world.Services.TerrainDataLienzo.SetTerrainDirectNoRender(worldX, worldY, cell.HeightLevel, true, terreno);
                    }
                    else
                    {
                        _world.Services.TerrainDataLienzo.SetTerrainDirectNoRender(worldX, worldY, cell.HeightLevel, true, terreno);
                    }
                
                }
            }
        }
    }


}
