using Godot;
using GodotEcsArch.sources.managers.SpriteMapChunk;
using GodotEcsArch.sources.WindowsDataBase;
using GodotEcsArch.sources.WindowsDataBase.ResourceSource.DataBase;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GodotEcsArch.sources.managers.Maps;

// Clase para guardar configuración de cada tipo de recurso
public class ResourceGenerationConfig
{
    public float frecuenciaRuido;
    public float umbralColocacion;
    public float distancePoison;
    public int vecindadTerrenoBase;
    public bool overrite;
    public HashSet<TerrainType> terrenosPermitidos; // opcional, si quieres filtrar por terreno
    public HashSet<ResourceSourceType> recursosPermitidos; // opcional, si quieres filtrar por terreno
    public int prioridad; // <- nuevo
    public int radioDisco = 0;
    public List<int> idsDisponibles = new List<int>();

    

    public void SetIds(Dictionary<ResourceSourceType, HashSet<int>> resourcesList)
    {
        foreach (var tipo in recursosPermitidos)
        {
            if (resourcesList.TryGetValue(tipo, out var lista))
            {
                idsDisponibles.AddRange(lista);
            }
        }
    }

    public int GetRandomId()
    {
        int index = CommonOperations.GetRandomInt(0, idsDisponibles.Count - 1);
        return idsDisponibles[index];
    }
}

public class ResourceSourceGenerator
{
    private SpriteMapChunk<ResourceSourceDataGame> mapResourceSource; // mapa de fuentes de recursos
    private SpriteMapChunk<TerrainDataGame> mapTerrainBase; // mapa de terreno 
    private Vector2I sizeMap;
    private Dictionary<ResourceSourceType, HashSet<int>> resourcesList = new Dictionary<ResourceSourceType, HashSet<int>>();
    private List<ResourceGenerationConfig> configuraciones = new List<ResourceGenerationConfig>();
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

        configuraciones = new ();

        // Árboles
        configuraciones.Add(  new ResourceGenerationConfig
        {
            frecuenciaRuido = 0.01f,                 // Zonas amplias, claros más grandes y naturales
            umbralColocacion = 0.001f,                // Muy permisivo (casi siempre árbol, a veces claro)
            distancePoison = 1.5f,                   // Árboles apretados, bosque cerrado
            vecindadTerrenoBase = 1,
            prioridad = 1,
            overrite = false,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase },
            recursosPermitidos = new HashSet<ResourceSourceType> { ResourceSourceType.Arboles },
        });

        // configuracion claro 
        configuraciones.Add(new ResourceGenerationConfig
        {
            frecuenciaRuido = 0.08f,                 // ruido más suave, menos parches
            umbralColocacion = 0.7f,                 // muy exigente → pocos claros
            distancePoison = 30f,                   // Árboles apretados, bosque cerrado
            vecindadTerrenoBase = 1,
            prioridad = 2,
            overrite = true,
            radioDisco = 10,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase },
            recursosPermitidos = new HashSet<ResourceSourceType> { ResourceSourceType.FloresVisual, ResourceSourceType.CespedVisual },
        });
        configuraciones.Add( new ResourceGenerationConfig
        {
            frecuenciaRuido = 0.08f,
            umbralColocacion = 0.7f,
            distancePoison = 10,
            vecindadTerrenoBase = 1,
            prioridad = 3,
            overrite = false,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase },
            recursosPermitidos = new HashSet<ResourceSourceType> { ResourceSourceType.Piedras },
        });
        configuraciones.Add(new ResourceGenerationConfig
        {
            frecuenciaRuido = 0.12f,                   // Parchecitos más pequeños y variados
            umbralColocacion = 0.6f,                   // Menor densidad (más raro que aparezcan)
            distancePoison = 5,                        // Bien espaciadas para que no se agrupen demasiado
            vecindadTerrenoBase = 0,
            prioridad = 4,
            overrite = false,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase },
            recursosPermitidos = new HashSet<ResourceSourceType> { ResourceSourceType.FloresVisual },
        });
        configuraciones.Add( new ResourceGenerationConfig
        {
            frecuenciaRuido = 0.0f,                   // Parche grande, sin cortes bruscos
            umbralColocacion = 0.0f,                  // Mucha cobertura (más fácil que aparezca)
            distancePoison = 1,                        // Muy juntos para formar "alfombra"
            vecindadTerrenoBase = 0,
            prioridad = 10,
            overrite = false,
            terrenosPermitidos = new HashSet<TerrainType> { TerrainType.PisoBase },
            recursosPermitidos = new HashSet<ResourceSourceType> { ResourceSourceType.CespedVisual },
        });
        foreach (var item in configuraciones)
        {
            item.SetIds(resourcesList);
        }
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
    private int GetRandomResourceId(HashSet<ResourceSourceType> tipos)
    {
        var idsDisponibles = new List<int>();

        foreach (var tipo in tipos)
        {
            if (resourcesList.TryGetValue(tipo, out var lista))
            {
                idsDisponibles.AddRange(lista);
            }
        }

        if (idsDisponibles.Count == 0)
            return 0; // o lanzar excepción, depende de tu lógica

        int index = CommonOperations.GetRandomInt(0, idsDisponibles.Count - 1);
        return idsDisponibles[index];
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

        foreach (var config in configuraciones.OrderBy(c => c.prioridad))
        {
            var tipoRecurso = config.recursosPermitidos;
            

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

                if (valorRuido >= config.umbralColocacion && !TieneVecino(positionGlobal,config.vecindadTerrenoBase))
                {

                    if (config.overrite)
                    {
                        
                        if (config.radioDisco > 0 )
                        {
                            var puntosDisco = GenerarDisco(positionGlobal, config.radioDisco, config.terrenosPermitidos);
                            foreach (var posDis in puntosDisco)
                            {
                                int id = config.GetRandomId();
                                mapResourceSource.AddUpdatedTile(posDis, id);
                            }
                        }
                        else
                        {
                            int id = config.GetRandomId();
                            mapResourceSource.AddUpdatedTile(positionGlobal, id);
                        }
                        
                    }
                    else
                    {
                        var data = mapResourceSource.GetTileGlobalPosition(positionGlobal);
                        if (data == null || data.idData == 0)
                        {
                            int id = config.GetRandomId();
                            mapResourceSource.AddUpdatedTile(positionGlobal, id);
                        }
                    }
                    
                    
                    
                }
            }

        }

        mapResourceSource.SetRenderEnabled(true);
    }
    List<Vector2I> GenerarDisco(Vector2I centro, int radio, HashSet<TerrainType> terrenosPermitidos)
    {
        var puntos = new List<Vector2I>();

        for (int dx = -radio; dx <= radio; dx++)
        {
            for (int dy = -radio; dy <= radio; dy++)
            {
                if (dx * dx + dy * dy <= radio * radio) // dentro del círculo
                {
                    var pos = new Vector2I(centro.X + dx, centro.Y + dy);

                    // Verificar si el terreno permite colocar
                    TerrainDataGame terreno = mapTerrainBase.GetTileGlobalPosition(pos);
                    if (terreno != null && terrenosPermitidos.Contains((TerrainType)terreno.GetTypeData()))
                    {
                        puntos.Add(pos);
                    }
                }
            }
        }

        return puntos;
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
