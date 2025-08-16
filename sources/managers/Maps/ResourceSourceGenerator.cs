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
    public float distancePoison;
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
            frecuenciaRuido = 0.06f,
            umbralColocacion = 0.4f,
            distancePoison = 2,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase }
        };

        // Minas de oro
        //configuraciones[ResourceSourceType.MinaOro] = new ResourceGenerationConfig
        //{
        //    frecuenciaRuido = 0.08f,
        //    umbralColocacion = 0.6f,
        //    terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase }
        //};

        // Piedras
        configuraciones[ResourceSourceType.Piedras] = new ResourceGenerationConfig
        {
            frecuenciaRuido = 0.08f,
            umbralColocacion = 0.7f,
            distancePoison = 10,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase }
        };
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
        int id = list[CommonOperations.GetRandomInt(0, list.Count - 1)];
        return id;
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
     
            var puntosPoisson = GenerarPuntosPoissonOptimizado(sizeMap.X, sizeMap.Y, config.distancePoison);

            foreach (var pos in puntosPoisson)
            {
                Vector2I positionGlobal = new Vector2I(pos.X - sizeMap.X / 2, pos.Y - sizeMap.Y / 2); // si necesitas offset

                TerrainDataGame terreno = mapTerrainBase.GetTileGlobalPosition(positionGlobal);
                if (terreno == null || !config.terrenosPermitidos.Contains((TerrainType)terreno.GetTypeData()))
                    continue;

                float valorRuido = noise.GetNoise(positionGlobal.X, positionGlobal.Y);

                if (valorRuido >= config.umbralColocacion && !TieneVecino(positionGlobal,2))
                {
                    int id =GetRandomResourceId(tipoRecurso);                
                    mapResourceSource.AddUpdatedTile(positionGlobal, id);
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
                //if (mapResourceSource.GetTileGlobalPosition(vecino) != null) // Ya hay algo colocado
                //    return true;
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
    private List<Vector2I> GenerarPuntosPoissonOptimizado(int width, int height, float radio, int k = 20)
    {
        float cellSize = radio / Mathf.Sqrt(2);
        int gridWidth = (int)Mathf.Ceil(width / cellSize);
        int gridHeight = (int)Mathf.Ceil(height / cellSize);

        Vector2I?[,] grid = new Vector2I?[gridWidth, gridHeight];
        List<Vector2I> puntos = new List<Vector2I>();
        List<Vector2I> procesar = new List<Vector2I>();

        System.Random rand = new System.Random();

        Vector2I primerPunto = new Vector2I(rand.Next(0, width), rand.Next(0, height));
        puntos.Add(primerPunto);
        procesar.Add(primerPunto);

        int gx = (int)(primerPunto.X / cellSize);
        int gy = (int)(primerPunto.Y / cellSize);
        grid[gx, gy] = primerPunto;

        while (procesar.Count > 0)
        {
            int index = rand.Next(procesar.Count);
            Vector2I punto = procesar[index];
            bool puntoValidoEncontrado = false;

            for (int i = 0; i < k; i++)
            {
                double angulo = rand.NextDouble() * Math.PI * 2;
                double distancia = radio + rand.NextDouble() * radio;
                int nx = (int)(punto.X + Math.Cos(angulo) * distancia);
                int ny = (int)(punto.Y + Math.Sin(angulo) * distancia);
                Vector2I nuevoPunto = new Vector2I(nx, ny);

                if (nuevoPunto.X >= 0 && nuevoPunto.X < width && nuevoPunto.Y >= 0 && nuevoPunto.Y < height)
                {
                    int cgx = (int)(nuevoPunto.X / cellSize);
                    int cgy = (int)(nuevoPunto.Y / cellSize);
                    bool demasiadoCerca = false;

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nxg = cgx + dx;
                            int nyg = cgy + dy;
                            if (nxg >= 0 && nxg < gridWidth && nyg >= 0 && nyg < gridHeight)
                            {
                                var puntoEnCelda = grid[nxg, nyg];
                                if (puntoEnCelda.HasValue && Distance(puntoEnCelda.Value, nuevoPunto) < radio)
                                {
                                    demasiadoCerca = true;
                                    break;
                                }
                            }
                        }
                        if (demasiadoCerca) break;
                    }

                    if (!demasiadoCerca)
                    {
                        puntos.Add(nuevoPunto);
                        procesar.Add(nuevoPunto);
                        grid[cgx, cgy] = nuevoPunto;
                        puntoValidoEncontrado = true;
                        break;
                    }
                }
            }

            if (!puntoValidoEncontrado)
            {
                procesar.RemoveAt(index);
            }
        }

        return puntos;
    }

    private float Distance(Vector2I a, Vector2I b)
    {
        int dx = a.X - b.X;
        int dy = a.Y - b.Y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
