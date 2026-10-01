using Godot;
using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
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
    
    public BlackyWorldProceduralTerritory(BlackyWorld world, BlackyWorldConfig worldConfig)
    {
        // solo para pruebas, luego se debe obtener de la base de datos del perfil del mundo
        ConfiguracionGeograficaData configuracionGeografica = new ConfiguracionGeograficaData(PerfilGeografico.Cuenca);
        
        
        _world = world;
        _config = worldConfig;
        _worldSeed = new BlackyWorldSeed(_config.WorldSeed);        
        _geography = new BlackyWorldGeography(worldConfig,_worldSeed,_world.Services.HeightMapWorld,  configuracionGeografica);

        string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + configuracionGeografica.Perfil.ToString() + ".txt");
        string imgPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "worldTerrenoGeografico" + _config.MapSize.ToString() + configuracionGeografica.Perfil.ToString() + ".png");
        _geography.Generate();
        _geography.ExportDebugTxt(filePath); // debug: genera un txt con el mapa de terreno generado
        _geography.ExportDebugPng(imgPath); // debug: genera una imagen con el mapa de terreno generado
        world.Streaming.chunkManagerLocal.Teleport(new Vector2(0, 0)); // esto hace que se renderice el chunk central y se carguen los chunks alrededor
    }
    public void GenerateTerritory()
    {
     
    }
}
