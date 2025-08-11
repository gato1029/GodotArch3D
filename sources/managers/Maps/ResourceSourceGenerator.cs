using Godot;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;

// Clase para guardar configuración de cada tipo de recurso
public class ResourceGenerationConfig
{
    public float frecuenciaRuido;
    public float umbralColocacion;
    public HashSet<TerrainType> terrenosPermitidos; // opcional, si quieres filtrar por terreno
}

public class ResourceSourceGenerator
{
    private SpriteMapChunk<ResourceSourceDataGame> mapResourceSource; // mapa de fuentes de recursos
    private SpriteMapChunk<TerrainDataGame> mapTerrainBase; // mapa de terreno 
    private Vector2I sizeMap;
    private Dictionary<ResourceSourceType, HashSet<int>> resourcesList = new Dictionary<ResourceSourceType, HashSet<int>>();
    private Dictionary<ResourceSourceType, ResourceGenerationConfig> configuraciones;    
    public ResourceSourceGenerator(MapLevelData mapLevelData)
    {
        this.mapResourceSource = mapLevelData.resourceSourceMap.MapData;
        this.mapTerrainBase = mapLevelData.terrainMap.MapTerrainBasic;
        sizeMap = mapLevelData.size;
        LoadResourcesSource();
        ConfigurarGeneracion();

    }
    private void ConfigurarGeneracion()
    {
        configuraciones = new Dictionary<ResourceSourceType, ResourceGenerationConfig>();

        // Árboles
        configuraciones[ResourceSourceType.Arboles] = new ResourceGenerationConfig
        {
            frecuenciaRuido = 0.08f,
            umbralColocacion = 0.6f,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase, TerrainType.Elevacion }
        };

        // Minas de oro
        //configuraciones[ResourceSourceType.MinaOro] = new ResourceGenerationConfig
        //{
        //    frecuenciaRuido = 0.08f,
        //    umbralColocacion = 0.6f,
        //    terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase }
        //};

        //// Piedras
        //configuraciones[ResourceSourceType.Piedras] = new ResourceGenerationConfig
        //{
        //    frecuenciaRuido = 0.07f,
        //    umbralColocacion = 0.4f,
        //    terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase, TerrainType.PisoBase }
        //};
    }
    private void LoadResourcesSource()
    {
        var list =DataBaseManager.Instance.FindAll<ResourceSourceData>();
        foreach (var resourceSource in list) {
            if (resourcesList.ContainsKey(resourceSource.resourceSourceType))
            {
                resourcesList[resourceSource.resourceSourceType].Add(resourceSource.id);
            }
            else
            {
                HashSet<int> ints = new HashSet<int>();
                ints.Add(resourceSource.id);
                resourcesList[resourceSource.resourceSourceType] = ints;
            }
            
        }
    }
    private int GetRandomResourceId(ResourceSourceType tipo)
    {
        var list = resourcesList[tipo].ToList();
        return list[CommonOperations.GetRandomInt(0, list.Count)];
    }
    public void Create()
    {
        mapResourceSource.SetRenderEnabled(false);
        mapResourceSource.ClearAllChunks();
        mapResourceSource.ClearAllFiles();

        int startX = -(sizeMap.X / 2);
        int endX = sizeMap.X / 2;
        int startY = -(sizeMap.Y / 2);
        int endY = sizeMap.Y / 2;

        foreach (var tipoRecurso in configuraciones.Keys)
        {
            var config = configuraciones[tipoRecurso];

            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(config.frecuenciaRuido);
            noise.SetSeed(CommonOperations.GetRandomInt());

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    Vector2I positionGlobal = new Vector2I(x, y);
                    TerrainDataGame terreno = mapTerrainBase.GetTileGlobalPosition(positionGlobal);

                    if (terreno == null || !config.terrenosPermitidos.Contains((TerrainType)terreno.GetTypeData()))
                        continue;

                    float valorRuido = noise.GetNoise(x, y);

                    if (valorRuido >= config.umbralColocacion && !TieneVecino(positionGlobal,3))
                    {
                        int id = 1;// GetRandomResourceId(tipoRecurso);
                        mapResourceSource.AddUpdatedTile(positionGlobal, id);
                    }
                }
            }
        }

        mapResourceSource.SetRenderEnabled(true);
    }

    private bool TieneVecino(Vector2I pos, int distancia)
    {
        for (int dy = -distancia; dy <= distancia; dy++)
        {
            for (int dx = -distancia; dx <= distancia; dx++)
            {
                if (dx == 0 && dy == 0) continue; // No revisar la celda actual
                Vector2I vecino = new Vector2I(pos.X + dx, pos.Y + dy);
                if (mapResourceSource.GetTileGlobalPosition(vecino) != null) // Ya hay algo colocado
                    return true;
                if (mapTerrainBase.GetTileGlobalPosition(vecino)!=null)
                {
                    var terrainTypeInternal = (TerrainType)mapTerrainBase.GetTileGlobalPosition(vecino).GetTypeData();
                    if (terrainTypeInternal != TerrainType.PisoBase && terrainTypeInternal != TerrainType.Elevacion) // Ya hay algo colocado
                        return true;
                }
                
            }   
        }
        return false;
    }

}
