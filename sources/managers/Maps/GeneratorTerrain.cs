using Arch.Core;
using Flecs.NET.Core;
using Godot;
using GodotEcsArch.sources.managers.Profiler;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;
using static Flecs.NET.Core.Ecs.Metrics;
using static Flecs.NET.Core.Ecs.Units;
using static Flecs.NET.Core.Ecs.Units.Forces;
using static Godot.HttpRequest;
using static Godot.OpenXRInterface;

namespace GodotEcsArch.sources.managers.Maps;
public partial class GeneratorTerrain : SingletonBase<GeneratorTerrain>
{
    private FastNoiseLite noise;
    private MapTerrain mapTerrain;
    private Dictionary<TerrainTileType, float> layersProbabilitys; 
    private TerrainData terrainData;
    private Dictionary<TerrainTileType, TerrainTileEntry> terrainTiles;
    private Dictionary<TerrainTileType, List<Vector2I>> resultsTiles;

    private int seed = 0;
    private Vector2I sizeMap;
    // Guarda qué tiles están activos para cada TerrainTileType
    private Dictionary<TerrainTileType, bool[,]> layerTileMask;
    public void ConfigGenerator(Vector2I size, MapTerrain mapTerrain, Dictionary<TerrainTileType, float> probabilitys, long idTerrainData)
    {
        this.sizeMap = size;
        this.mapTerrain = mapTerrain;
        this.layersProbabilitys = probabilitys;
        terrainData = MasterDataManager.GetData<TerrainData>(idTerrainData);
        //terrainTiles = terrainData.idsAutoTileSprite.ToDictionary(x => x.Type, x => x);
        layerTileMask = new Dictionary<TerrainTileType, bool[,]>();
        resultsTiles = new Dictionary<TerrainTileType, List<Vector2I>>();
        foreach (var type in terrainTiles.Keys)
        {
            layerTileMask[type] = new bool[sizeMap.X, sizeMap.Y];
            resultsTiles[type] = new List<Vector2I>();
        }
    }
    public void Generate(int seedEntry = 0)
    {
        mapTerrain.ClearMap();
        foreach (var item in resultsTiles)
        {
            item.Value.Clear();
        }
        seed = seedEntry;
        Random random = new Random(seed);
        noise = new FastNoiseLite(seed);
        GD.Print("Generacion");
        string stamp = DateTime.Now.ToString("HH:mm:ss.fff");
        GD.Print(stamp);
        foreach (var item in layersProbabilitys)
        {
            String tit= "Tiempo Generacion:" + item.Key.ToString();
            using (new ProfileScope(tit))
            {
                mapTerrain.EnableLayer(false, item.Key); // desactivamos la capa para evitar render costosos
                GenerateLayer(item.Key, item.Value);
                
            }
            PerformanceTimer.Instance.Print(tit);
        }
        GD.Print("Plot");
        stamp = DateTime.Now.ToString("HH:mm:ss.fff");
        GD.Print(stamp);
        foreach (var item in layersProbabilitys)
        {
            //var result = item.Value;
            var borders = GetLayerBordersAndInside(item.Key);
            if (item.Key == TerrainTileType.AdornosCesped)
            {
                borders.borders.Clear();
            }
            var entry = terrainTiles[item.Key];
            if (borders.inside.Count>0)
            {
                String tit = "Tiempo Plot:" + item.Key.ToString();
                using (new ProfileScope(tit))
                {
                    //mapTerrain.AddUpdateTileBulk(result, terrainData.id, entry, false);
                    mapTerrain.AddUpdateMatrix(borders.borders, borders.inside, terrainData.id, entry);
                    //mapTerrain.AplyRulesBulk(result, id, entry);
                    mapTerrain.EnableLayer(true, item.Key); // activamos la capa para renderizar 
                }
                PerformanceTimer.Instance.Print(tit);
            }
            
        }
        stamp = DateTime.Now.ToString("HH:mm:ss.fff");
        GD.Print(stamp);
    }
    public void GenerateLayer(TerrainTileType terrainTileType, float probabilidad)
    {
        // la probabilidad determina que tan frecuente aparece
        bool llenoTodo = false;
        if (probabilidad == 100)
        {
            llenoTodo = true;
        }


        long id = 1; //terrainTiles[terrainTileType].TileId;
        TerrainTileEntry entry = terrainTiles[terrainTileType];
        var data = MasterDataManager.GetData<TerrainData>(id);

        List<Vector2I> tiles = new List<Vector2I>();
        int initX = sizeMap.X / 2;
        int initY = sizeMap.Y / 2;
        if (llenoTodo) // no hago algoritmo y populo todo el mapa
        {
            for (int x = -initX; x < initX; x++)
            {
                for (int y = -initY; y < initY; y++)
                {
                    Vector2I posicion = new Vector2I(x, y);                    
                    tiles.Add(new Vector2I(x, y));
                    MarkTile(terrainTileType, posicion);
                }
            }
            resultsTiles[terrainTileType] = tiles;
            //mapTerrain.AddUpdateTileBulk(tiles, id, entry,false);
           // mapTerrain.AplyRulesBulk(tiles,id, entry);
        }
        else
        {
            switch (terrainTileType)
            {
                case TerrainTileType.Agua:
                    break;
                case TerrainTileType.TierraNivel1:
                    CreateTierraNivel1(TerrainTileType.TierraNivel1,probabilidad);
                    break;
                case TerrainTileType.CespedNivel1:
                    CreateCespedNivel1(TerrainTileType.CespedNivel1, probabilidad);
                    break;
                case TerrainTileType.TierraNivel2:
                    CreateTierraNivel2(TerrainTileType.TierraNivel2, probabilidad);                    
                    break;
                case TerrainTileType.CespedNivel2:
                    CreateCespedNivel2(TerrainTileType.CespedNivel2, probabilidad);
                    break;
                case TerrainTileType.AdornosAgua:
                    break;
                case TerrainTileType.AdornosTierra:

                    break;
                case TerrainTileType.AdornosCesped:
                    CreateAdornosCespedNivel1(TerrainTileType.AdornosCesped,probabilidad);
                    CreateAdornosCespedNivel2(TerrainTileType.AdornosCesped, probabilidad);
                    resultsTiles[TerrainTileType.AdornosCesped] = GetMarkedTiles(TerrainTileType.AdornosCesped);
                    break;
                case TerrainTileType.AguaCamino:
                    break;
                case TerrainTileType.TierraCamino:

                    
                    break;
                case TerrainTileType.TierraNivel0:
                    break;
                default:
                    break;
            }
        }      
    }

    private void CreateCespedNivel2(TerrainTileType type, float probabilidad)
    {
        TerrainTileEntry entry = terrainTiles[type];
        long id = 1;// entry.TileId;

        var border = GetLayerBorders(TerrainTileType.TierraNivel2);

        CopyLayer(TerrainTileType.TierraNivel2, type);
        // Parámetros automáticos inteligentes
        var opts = GetAutoSpots(type, probabilidad);
        CreateSpots(type, opts);

        CompleteNeighborhoodWithUnderType(type, TerrainTileType.TierraNivel2, border, true);
        NormalizeLayer(type);
        ClearUnderType(type, TerrainTileType.TierraNivel2);
        resultsTiles[type] = GetMarkedTiles(type);
    }

    private void CreateAdornosCespedNivel2(TerrainTileType adornosCesped, float probabilidad)
    {
        // Recorremos cada tile del mapa en coordenadas centradas
        ForEachTilePosition(pos =>
        {
            // Si no es césped → saltamos
            if (!IsTileMarked(TerrainTileType.CespedNivel2, pos))
                return true;
            if (!IsTileMarked(TerrainTileType.TierraNivel2, pos))
            {
                return true;
            }
            float p = probabilidad / 100f;
            if (GD.Randf() > p)
                return true;

            // Finalmente marcamos el tile como adorno
            MarkTile(adornosCesped, pos);            
            return true; // seguir iterando
        });

      var border = GetLayerBordersAndInside(TerrainTileType.TierraNivel2);

        foreach (var item in border.borders)
        {
            UnmarkTile(adornosCesped, item);
        }
    }

    private void CreateAdornosCespedNivel1(TerrainTileType adornosCesped, float probabilidad)
    {
        // Recorremos cada tile del mapa en coordenadas centradas
        ForEachTilePosition(pos =>
        {
            // Si no es césped → saltamos
            if (!IsTileMarked(TerrainTileType.CespedNivel1, pos))
                return true;
            if (IsTileMarked(TerrainTileType.TierraNivel2,pos))
            {
                return true;
            }
            float p = probabilidad / 100f;
            if (GD.Randf() > p)
                return true;
           
            // Finalmente marcamos el tile como adorno
            MarkTile(adornosCesped, pos);

            return true; // seguir iterando
        });

        var result = GetBottomPositionsOfMarkedBlobs(TerrainTileType.TierraNivel2);

        foreach (var item in result)
        {
            for (int i = 0; i < 4; i++)
            {
                var newPos = new Vector2I(item.X, (item.Y) - i);
                // Finalmente marcamos el tile como adorno
                UnmarkTile(adornosCesped, newPos);
            }            
        }

        var border = GetLayerBordersAndInside(TerrainTileType.TierraNivel1);

        foreach (var item in border.borders)
        {
            UnmarkTile(adornosCesped, item);
        }
    }
   
    public bool[,] ConvertPositionsToMatrix(List<Vector2I> positions, out Vector2I offset)
    {
        if (positions.Count == 0)
        {
            offset = Vector2I.Zero;
            return new bool[0, 0];
        }

        int minX = positions.Min(p => p.X);
        int maxX = positions.Max(p => p.X);
        int minY = positions.Min(p => p.Y);
        int maxY = positions.Max(p => p.Y);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        bool[,] matrix = new bool[width, height];

        // Offset para convertir posiciones centradas a índices del array
        offset = new Vector2I(minX, minY);

        foreach (var pos in positions)
        {
            int ix = pos.X - minX;
            int iy = pos.Y - minY;
            matrix[ix, iy] = true;
        }

        return matrix;
    }
    private void CreateTierraNivel2(TerrainTileType type, float probabilidad)
    { 
        var tiles = CreateSmallMediumBlobsLayer(
           type: type,
           underType: TerrainTileType.TierraNivel1,
           minBlobSize: 100,
           maxBlobSize: 500,
           probabilidad,     // Muy pocas porciones
           minBorderDistance: 20 // No tocar el borde del underType
           ,40
       );

        NormalizeLayer(type);
        //resultsTiles[type] = GetMarkedTiles(type);

   
    }

    private void CreateCespedNivel1(TerrainTileType type, float probabilidad)
    {
        TerrainTileEntry entry = terrainTiles[type];
        long id = 1;// entry.TileId;

        var border = GetLayerBorders(TerrainTileType.TierraNivel1);

        CopyLayer(TerrainTileType.TierraNivel1, type);
        // Parámetros automáticos inteligentes
        var opts = GetAutoSpots(type, probabilidad);
        CreateSpots(type, opts);

        
        
        CompleteNeighborhoodWithUnderType(type, TerrainTileType.TierraNivel1, border, true);

        NormalizeLayer(type);
        ClearUnderType(type, TerrainTileType.TierraNivel1);
        //RemoveLowDensityTiles(type);
        //resultsTiles[type] = GetMarkedTiles(type);

 
    }

    private void ClearUnderType(TerrainTileType type, TerrainTileType underType)
    {        
        var srcUnder = layerTileMask[underType];
        for (int x = 0; x < sizeMap.X; x++)
        {
            for (int y = 0; y < sizeMap.Y; y++)
            {
                if (srcUnder[x,y]==false)
                {
                    layerTileMask[type][x, y] = false;
                }
            }
        }
    }
    private void CreateTierraNivel1(TerrainTileType type, float probabilidad)
    {
        TerrainTileEntry entry = terrainTiles[type];
        long id = 1;// entry.TileId;

        

        CreateLayerFromNoise(type, probabilidad);
        NormalizeLayer(type);

        
        //resultsTiles[type] = GetMarkedTiles(type);


    }

    public List<Vector2I> GetBottomPositionsOfMarkedBlobs(TerrainTileType type)
    {
        var result = new List<Vector2I>();
        var visited = new HashSet<Vector2I>();

        // Movimientos 4-direcciones
        Vector2I[] directions = new[]
        {
        new Vector2I(1, 0),
        new Vector2I(-1, 0),
        new Vector2I(0, 1),
        new Vector2I(0, -1),
    };

        ForEachTilePosition(pos =>
        {
            if (!IsTileMarked(type, pos) || visited.Contains(pos))
                return true;

            // BFS para el blob actual
            var queue = new Queue<Vector2I>();
            var blobTiles = new List<Vector2I>();

            queue.Enqueue(pos);
            visited.Add(pos);

            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                blobTiles.Add(cur);

                foreach (var d in directions)
                {
                    var nb = cur + d;
                    if (visited.Contains(nb))
                        continue;

                    if (!IsTileMarked(type, nb))
                        continue;

                    visited.Add(nb);
                    queue.Enqueue(nb);
                }
            }

            // Elegir los tiles del blob que NO tengan un tile marcado justamente debajo (y - 1)
            foreach (var t in blobTiles)
            {
                var below = new Vector2I(t.X, t.Y - 1);
                // Si debajo NO está marcado, entonces t es parte del borde inferior.
                if (!IsTileMarked(type, below))
                    result.Add(t);
            }

            return true;
        });

        return result;
    }

    

    private bool IsNearOtherBlob(HashSet<Vector2I> blobPositions, Vector2I pos, int minDistance)
    {
        for (int dx = -minDistance; dx <= minDistance; dx++)
        {
            for (int dy = -minDistance; dy <= minDistance; dy++)
            {
                if (blobPositions.Contains(new Vector2I(pos.X + dx, pos.Y + dy)))
                    return true;
            }
        }

        return false;
    }
   

    /// <summary>
    /// Rellena huecos verticales entre bloques sólidos (X).
    /// No elimina nada, sólo rellena agujeros.
    /// Funciona para todos los bordes.
    /// </summary>
    public void FillVerticalGaps(TerrainTileType[,] grid, TerrainTileType solidType)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            int y = 0;

            while (y < height)
            {
                // Buscar el primer bloque sólido
                if (grid[x, y] != solidType)
                {
                    y++;
                    continue;
                }

                // Encontramos el inicio del bloque de X
                int topSolidStart = y;

                // Saltar todos los X consecutivos
                while (y < height && grid[x, y] == solidType)
                    y++;

                // Ahora empieza un posible bloque de hueco (cualquier no sólido)
                int gapStart = y;

                while (y < height && grid[x, y] != solidType)
                    y++;

                int gapEnd = y - 1;

                // Si al final del hueco encontramos otro bloque X → es un hueco
                if (gapStart <= gapEnd && y < height && grid[x, y] == solidType)
                {
                    // RELLENAR EL HUECO VERTICAL
                    for (int yy = gapStart; yy <= gapEnd; yy++)
                        grid[x, yy] = solidType;
                }
            }
        }
    }


    private void PrintBlobMatrix(HashSet<Vector2I> blobTiles)
    {
        if (blobTiles.Count == 0)
        {
            GD.Print("Blob vacío.");
            return;
        }

        int minX = blobTiles.Min(p => p.X);
        int maxX = blobTiles.Max(p => p.X);
        int minY = blobTiles.Min(p => p.Y);
        int maxY = blobTiles.Max(p => p.Y);

        GD.Print($"Blob matrix (X=tile, V=vacío), bounding box X({minX}-{maxX}), Y({minY}-{maxY}):");

        for (int y = maxY; y >= minY; y--) // De arriba hacia abajo (y decrece)
        {
            string line = "";
            for (int x = minX; x <= maxX; x++)
            {
                Vector2I pos = new Vector2I(x, y);
                if (blobTiles.Contains(pos))
                    line += "X";
                else
                    line += "V";
            }
            GD.Print(line);
        }
    }


    private void FloodBlobRandomFrontier(
    Vector2I start,
    int size,
    TerrainTileType type,
    TerrainTileType underType,
    List<Vector2I> output,
    HashSet<Vector2I> blobPositions)
    {
        // Si el punto inicial no es válido, salir
        if (!IsTileMarked(underType, start))
            return;

        Random rng = new Random();

        // blob = celdas ya pintadas
        HashSet<Vector2I> blob = new HashSet<Vector2I>();
        // frontier = celdas candidatas (adyacentes al blob, aún no pintadas)
        List<Vector2I> frontier = new List<Vector2I>();
        HashSet<Vector2I> inFrontierOrBlob = new HashSet<Vector2I>();

        // inicializar con el start (lo añadimos como primer tile)
        blob.Add(start);
        inFrontierOrBlob.Add(start);
        MarkTile(type, start);
        output.Add(start);
        blobPositions.Add(start);

        // construir frontier inicial (vecinos válidos de start)
        foreach (var n in GetNeighbors(start))
        {
            if (inFrontierOrBlob.Contains(n))
                continue;
            if (!IsTileMarked(underType, n))
                continue;
            frontier.Add(n);
            inFrontierOrBlob.Add(n);
        }

        // Expandir hasta alcanzar 'size' o hasta no haber más candidatos
        while (blob.Count < size && frontier.Count > 0)
        {
            // Elegir aleatoriamente una posición del frontier (random pop)
            int idx = rng.Next(frontier.Count);
            Vector2I pick = frontier[idx];

            // removerla del frontier (swap-remove para eficiencia)
            frontier[idx] = frontier[frontier.Count - 1];
            frontier.RemoveAt(frontier.Count - 1);

            // Pintar la celda elegida
            blob.Add(pick);
            MarkTile(type, pick);
            output.Add(pick);
            blobPositions.Add(pick);

            // Añadir vecinos válidos del pick al frontier (si no están ya en blob/o frontier)
            foreach (var n in GetNeighbors(pick))
            {
                if (inFrontierOrBlob.Contains(n))
                    continue;
                if (!IsTileMarked(underType, n))
                    continue;

                frontier.Add(n);
                inFrontierOrBlob.Add(n);
            }
        }
    }

    private void FloodBlob(
     Vector2I start,
     int size,
     TerrainTileType type,
     TerrainTileType underType,
     List<Vector2I> output,
     HashSet<Vector2I> blobPositions)
    {
        Queue<Vector2I> q = new();
        HashSet<Vector2I> visited = new();

        q.Enqueue(start);
        visited.Add(start);

        int count = 0;
        Random rng = new Random();

        while (q.Count > 0 && count < size)
        {
            var pos = q.Dequeue();

            if (!IsTileMarked(underType, pos))
                continue;

            // ---- Marcar ----
            MarkTile(type, pos);
            output.Add(pos);
            blobPositions.Add(pos);

            count++;
            
            var neigh = GetNeighbors(pos).ToList();
            Shuffle(neigh, rng); // <<< ÚNICO CAMBIO
            foreach (var n in neigh)
            {
                if (visited.Contains(n))
                    continue;

                visited.Add(n);


                q.Enqueue(n);
            }
        }
    }
    private void Shuffle<T>(IList<T> list, Random rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }


    private IEnumerable<Vector2I> GetNeighbors(Vector2I pos)
    {
        foreach (var n in NeighborDirs)
            yield return pos + n;
    }
    private List<Vector2I> CreateControlledBlobs(
        TerrainTileType type,
        TerrainTileType? underType,
        int minSize,
        int maxSize,
        int intentos,          // cuántas porciones intentar crear
        float probabilidad)     // 0–100
    {
        List<Vector2I> generated = new List<Vector2I>();

        for (int i = 0; i < intentos; i++)
        {
            // Probabilidad de generar esta porción
            if (GD.Randf() > probabilidad / 100f)
                continue;

            // Obtener punto inicial válido
            Vector2I start = GetRandomValidPosition(underType);
            if (start == Vector2I.Zero)
                continue;

            // Generar blob compacto
            var blob = GenerateBlob(start, minSize, maxSize, type, underType);

            // Marcar
            foreach (var p in blob)
            {
                MarkTile(type, p);
                generated.Add(p);
            }
        }

        return generated;
    }
    private List<Vector2I> GenerateBlob(
    Vector2I start,
    int minSize,
    int maxSize,
    TerrainTileType type,
    TerrainTileType? underType)
    {
        Queue<Vector2I> open = new Queue<Vector2I>();
        HashSet<Vector2I> blob = new HashSet<Vector2I>();

        open.Enqueue(start);
        blob.Add(start);

        while (blob.Count < maxSize && open.Count > 0)
        {
            var pos = open.Dequeue();

            foreach (var dir in NeighborDirs) // 4 direcciones = forma más compacta
            {
                var next = pos + dir;

                if (blob.Contains(next))
                    continue;

                // Si requiere base (nivel 1)
                if (underType.HasValue && !IsTileMarked(underType.Value, next))
                    continue;

                blob.Add(next);
                open.Enqueue(next);

                if (blob.Count >= maxSize)
                    break;
            }
        }

        // Si quedó muy pequeño (< minSize), descártalo
        if (blob.Count < minSize)
            return new List<Vector2I>();

        return blob.ToList();
    }

    private Vector2I GetRandomValidPosition(TerrainTileType? underType)
    {
        for (int i = 0; i < 50; i++)
        {
            var p = new Vector2I(
                GD.RandRange(-sizeMap.X/2, sizeMap.X / 2),
                GD.RandRange(-sizeMap.Y / 2, sizeMap.Y / 2)
            );

            if (!underType.HasValue || IsTileMarked(underType.Value, p))
                return p;
        }

        return Vector2I.Zero;
    }


    /// <summary>
    /// Completa la vecindad total (8-way) de una lista de tiles,
    /// pero solo incluye aquellos vecinos cuya posición esté marcada
    /// en layerTileMask[underType].
    /// 
    /// Si mark == true → marca esos tiles con true en layerTileMask[type].
    /// Si mark == false → desmarca
    /// 
    /// Devuelve una lista final sin duplicados.
    /// </summary>
    public List<Vector2I> CompleteNeighborhoodWithUnderType(
        TerrainTileType type,
        TerrainTileType underType,
        List<Vector2I> sourceTiles,
        bool mark)
    {
        List<Vector2I> result = new List<Vector2I>();
        if (!layerTileMask.ContainsKey(type) || !layerTileMask.ContainsKey(underType))
            return result;

        bool[,] layer = layerTileMask[type];
        bool[,] underLayer = layerTileMask[underType];

        int w = layer.GetLength(0);
        int h = layer.GetLength(1);

        HashSet<Vector2I> visited = new HashSet<Vector2I>();

        // 8 direcciones + centro
        Vector2I[] neighbors =
        {
        new Vector2I(0,0),
        new Vector2I(1,0), new Vector2I(-1,0),
        new Vector2I(0,1), new Vector2I(0,-1),
        new Vector2I(1,1), new Vector2I(1,-1),
        new Vector2I(-1,1), new Vector2I(-1,-1),
    };

        foreach (var pos in sourceTiles)
        {
            foreach (var d in neighbors)
            {
                int nx = pos.X + d.X;
                int ny = pos.Y + d.Y;

                // límites
                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    continue;

                var npos = new Vector2I(nx, ny);

                // ya procesado
                if (!visited.Add(npos))
                    continue;

                // Debe existir debajo
                if (!underLayer[nx, ny])
                    continue;

          
                layer[nx, ny] = mark;

                result.Add(npos);
            }
        }

        return result;
    }

    public List<Vector2I> GetLayerBorders(TerrainTileType type)
    {
        List<Vector2I> borders = new List<Vector2I>();

        if (!layerTileMask.ContainsKey(type))
            return borders;

        var src = layerTileMask[type];
        int w = src.GetLength(0);
        int h = src.GetLength(1);

        // 8 direcciones
        Vector2I[] dirs =
        {
        new Vector2I(1,0), new Vector2I(-1,0),
        new Vector2I(0,1), new Vector2I(0,-1),
        new Vector2I(1,1), new Vector2I(1,-1),
        new Vector2I(-1,1), new Vector2I(-1,-1)
    };

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (!src[x, y])
                    continue;

                bool isBorder = false;

                // Revisar vecinos
                for (int i = 0; i < dirs.Length; i++)
                {
                    int nx = x + dirs[i].X;
                    int ny = y + dirs[i].Y;

                    // Fuera de límites → también borde
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    {
                        isBorder = true;
                        break;
                    }

                    // Vecino vacío → borde
                    if (!src[nx, ny])
                    {
                        isBorder = true;
                        break;
                    }
                }

                if (isBorder)
                    borders.Add(new Vector2I(x, y));
            }
        }

        return borders;
    }
    public void RemoveBordersAndGetInside(TerrainTileType type)
    {
        

        if (!layerTileMask.ContainsKey(type))
            return ;

        var src = layerTileMask[type];
        int w = src.GetLength(0);
        int h = src.GetLength(1);

        Vector2I[] dirs =
        {
        new Vector2I(1,0), new Vector2I(-1,0),
        new Vector2I(0,1), new Vector2I(0,-1),
        new Vector2I(1,1), new Vector2I(1,-1),
        new Vector2I(-1,1), new Vector2I(-1,-1)
    };

        int initX = sizeMap.X / 2;
        int initY = sizeMap.Y / 2;

        // Guardamos los bordes para borrarlos al final.
        List<(int ix, int iy)> toRemove = new();

        for (int x = -initX; x < initX; x++)
        {
            for (int y = -initY; y < initY; y++)
            {
                var pos = new Vector2I(x, y);
                var posNorm = Normalize(pos);
                if (!src[posNorm.ix, posNorm.iy])
                    continue;

                bool isBorder = false;

                for (int i = 0; i < dirs.Length; i++)
                {
                    int nx = x + dirs[i].X;
                    int ny = y + dirs[i].Y;

                    var posI = new Vector2I(nx, ny);
                    var posNormI = Normalize(posI);

                    if (posNormI.ix < 0 || posNormI.iy < 0 ||
                        posNormI.ix >= w || posNormI.iy >= h)
                    {
                        isBorder = true;
                        break;
                    }

                    if (!src[posNormI.ix, posNormI.iy])
                    {
                        isBorder = true;
                        break;
                    }
                }

                if (isBorder)
                {
                    // Se eliminará después
                    toRemove.Add((posNorm.ix, posNorm.iy));
                }
        
            }
        }

        // Borrar bordes del mask
        foreach (var r in toRemove)
            src[r.ix, r.iy] = false;

    }

    public (List<Vector2I> borders, List<Vector2I> inside)
    GetLayerBordersAndInside(TerrainTileType type)
    {
        var borders = new List<Vector2I>();
        var inside = new List<Vector2I>();

        if (!layerTileMask.ContainsKey(type))
            return (borders, inside);

        var src = layerTileMask[type];
        int w = src.GetLength(0);
        int h = src.GetLength(1);

        Vector2I[] dirs =
        {
        new Vector2I(1,0), new Vector2I(-1,0),
        new Vector2I(0,1), new Vector2I(0,-1),
        new Vector2I(1,1), new Vector2I(1,-1),
        new Vector2I(-1,1), new Vector2I(-1,-1)
    };
        int initX = sizeMap.X / 2;
        int initY = sizeMap.Y / 2;
        for (int x = -initX; x < initX; x++)
        {
            for (int y = -initY; y < initY; y++)
            {
                var pos = new Vector2I(x, y);
                var posNorm = Normalize(pos);
                if (src[posNorm.ix, posNorm.iy] == false)
                    continue;

                bool isBorder = false;

                for (int i = 0; i < dirs.Length; i++)
                {
                    int nx = x + dirs[i].X;
                    int ny = y + dirs[i].Y;
                    var posI = new Vector2I(nx, ny);
                    var posNormI = Normalize(posI);
                    // Fuera de la matriz = borde
                    if (posNormI.ix < 0 || posNormI.iy < 0 || posNormI.ix >= w || posNormI.iy >= h)
                    {
                        isBorder = true;
                        break;
                    }
                
                    // Vecino vacío = borde
                    if (src[posNormI.ix, posNormI.iy] == false)
                    {
                        isBorder = true;
                        break;
                    }
                }


                if (isBorder)
                {
                    borders.Add(pos);
                    inside.Add(pos);
                }
                else
                {
                    inside.Add(pos);
                }
            }
        }

        return (borders, inside);
    }

    public void CopyLayerBorders(TerrainTileType origen, TerrainTileType destino)
    {
        if (!layerTileMask.ContainsKey(origen))
            return;

        var src = layerTileMask[origen];
        int w = src.GetLength(0);
        int h = src.GetLength(1);

        // Si la capa destino no existe, la creamos limpia
        if (!layerTileMask.ContainsKey(destino))
            layerTileMask[destino] = new bool[w, h];

        var dst = layerTileMask[destino];

        // Direcciones 8-conexión
        Vector2I[] dirs =
        {
        new Vector2I(1,0), new Vector2I(-1,0),
        new Vector2I(0,1), new Vector2I(0,-1),
        new Vector2I(1,1), new Vector2I(1,-1),
        new Vector2I(-1,1), new Vector2I(-1,-1)
    };

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                if (!src[x, y])
                    continue;

                bool isBorder = false;

                // Revisar vecinos
                for (int i = 0; i < dirs.Length; i++)
                {
                    int nx = x + dirs[i].X;
                    int ny = y + dirs[i].Y;

                    if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    {
                        // Borde del mapa también cuenta como borde
                        isBorder = true;
                        break;
                    }

                    if (!src[nx, ny])
                    {
                        isBorder = true;
                        break;
                    }
                }

                if (isBorder)
                    dst[x, y] = true;
            }
        }
    }
    public void CopyLayer(TerrainTileType origen, TerrainTileType destino)
    {
        if (!layerTileMask.ContainsKey(origen))
            return;

        var src = layerTileMask[origen];

        int w = src.GetLength(0);
        int h = src.GetLength(1);

        var dst = new bool[w, h];
        Array.Copy(src, dst, src.Length);

        layerTileMask[destino] = dst;
    }
    private SpotOptions GetSpotParameters(float probabilidad)
    {
        int totalTiles = sizeMap.X * sizeMap.Y;
        float mapScale = Mathf.Sqrt(totalTiles) / 50f;
        mapScale = Mathf.Clamp(mapScale, 0.5f, 4f);

        float holeFactor = 1f - (probabilidad / 100f);
        holeFactor = Mathf.Clamp(holeFactor, 0f, 1f);

        int countSpots = Mathf.RoundToInt(
            Mathf.Lerp(3, 40, holeFactor) * mapScale
        );

        int minRadius = Mathf.RoundToInt(
            Mathf.Lerp(1, 3, holeFactor) * mapScale
        );

        int maxRadius = Mathf.RoundToInt(
            Mathf.Lerp(2, 8, holeFactor) * mapScale
        );

        minRadius = Mathf.Clamp(minRadius, 1, 10);
        maxRadius = Mathf.Clamp(maxRadius, minRadius + 1, 20);

        return new SpotOptions
        {
            Shape = SpotShape.Circle,
            CountSpots = countSpots,
            MinRadius = minRadius,
            MaxRadius = maxRadius,
            Mark = false
        };
    }

    private SpotOptions GetAutoSpots(TerrainTileType type, float probabilidad)
    {
        int totalTiles = sizeMap.X * sizeMap.Y;
        float mapScale = Mathf.Sqrt(totalTiles) / 50f;
        mapScale = Mathf.Clamp(mapScale, 0.5f, 4f);

        float holeFactor = 1f - (probabilidad / 100f);
        holeFactor = Mathf.Clamp(holeFactor, 0f, 1f);

        // Evaluar ruido global para variación
        float noiseSample = noise.GetNoise(1000 + (int)type * 17, seed * 13);

        SpotShape shape;

        // --- Determinar forma base según probabilidad ---
        if (probabilidad >= 75f)
            shape = SpotShape.Circle;
        else if (probabilidad <= 35f)
            shape = SpotShape.Cluster;
        else
            shape = SpotShape.Circle; // intermedio → círculo suave por defecto

        // --- Ruido altera la decisión para más naturalidad ---
        if (noiseSample < -0.3f)
            shape = SpotShape.Cluster;
        else if (noiseSample > 0.3f)
            shape = SpotShape.Circle;

        // --- Cálculo de parámetros ---
        int countSpots = Mathf.RoundToInt(
            Mathf.Lerp(2, 60, holeFactor) * mapScale
        );

        int minR = Mathf.RoundToInt(
            Mathf.Lerp(1, 3, holeFactor) * mapScale
        );

        int maxR = Mathf.RoundToInt(
            Mathf.Lerp(2, 10, holeFactor) * mapScale
        );

        minR = Mathf.Clamp(minR, 1, 12);
        maxR = Mathf.Clamp(maxR, minR + 1, 25);

        return new SpotOptions
        {
            Shape = shape,
            CountSpots = countSpots,
            MinRadius = minR,
            MaxRadius = maxR,
            Mark = false,
            UnderType = type // puedes cambiar esto si lo deseas
        };
    }
    /// <summary>
    /// Genera manchas (blobs) orgánicas encima de un tipo de terreno base (opcional).
    /// Permite marcar o desmarcar los tiles resultantes.
    /// </summary>
    /// <param name="type">Tipo de capa a modificar</param>
    /// <param name="count">Cantidad de manchas</param>
    /// <param name="minSize">Tamaño mínimo de una mancha</param>
    /// <param name="maxSize">Tamaño máximo de una mancha</param>
    /// <param name="setValue">true = marcar tiles, false = desmarcar tiles</param>
    /// <param name="underType">Opcional. Solo se modifica si el UnderType está marcado debajo</param>
    public List<Vector2I> CreateBlobsLayer(
        TerrainTileType type,
        int count,
        int minSize,
        int maxSize,
        bool setValue,
        TerrainTileType? underType = null)
    {
        List<Vector2I> affectedTiles = new List<Vector2I>();

        int width = sizeMap.X;
        int height = sizeMap.Y;
        int initX = width / 2;
        int initY = height / 2;

        Random rng = new Random(seed + 777);

        for (int i = 0; i < count; i++)
        {
            // Punto inicial aleatorio
            int sx = rng.Next(0, width);
            int sy = rng.Next(0, height);

            // Si hay UnderType, respetarlo
            if (underType.HasValue)
            {
                if (!layerTileMask[underType.Value][sx, sy])
                    continue;
            }

            int blobSize = rng.Next(minSize, maxSize + 1);

            Queue<(int x, int y)> q = new Queue<(int, int)>();
            HashSet<(int x, int y)> visited = new HashSet<(int, int)>();

            q.Enqueue((sx, sy));
            visited.Add((sx, sy));

            int created = 0;

            while (q.Count > 0 && created < blobSize)
            {
                var (cx, cy) = q.Dequeue();

                // Aplicar setValue (marcar o desmarcar)
                layerTileMask[type][cx, cy] = setValue;

                affectedTiles.Add(new Vector2I(cx - initX, cy - initY));

                created++;

                // Vecinos 8 direcciones
                int[,] dirs =
                {
                { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 },
                { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 }
            };

                for (int d = 0; d < dirs.GetLength(0); d++)
                {
                    int nx = cx + dirs[d, 0];
                    int ny = cy + dirs[d, 1];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    if (visited.Contains((nx, ny)))
                        continue;

                    // Si hay UnderType, verificarlo
                    if (underType.HasValue && !layerTileMask[underType.Value][nx, ny])
                        continue;

                    // Probabilidad de expansión variable (hace formas orgánicas)
                    if (rng.NextDouble() < 0.75)
                    {
                        q.Enqueue((nx, ny));
                    }

                    visited.Add((nx, ny));
                }
            }
        }

        return affectedTiles;
    }

    /// <summary>
    /// Crea una capa usando ruido. Permite opcionalmente exigir que
    /// el tile base (underType) exista para poder generar encima.
    /// </summary>
    private List<Vector2I> CreateLayerFromNoise(
        TerrainTileType type,
        float probabilidad,
        TerrainTileType? underType = null)
    {
        TerrainTileEntry entry = terrainTiles[type];
        long id = 1;// entry.TileId;

        List<Vector2I> tilesGenerated = new List<Vector2I>();

        // --- Probabilidad 100: llenar todo ---
        if (probabilidad >= 100f)
        {
            var tiles = ForEachTilePosition(pos =>
            {
                // Si se exige base, verificarla
                if (underType.HasValue && !IsTileMarked(underType.Value, pos))
                    return true;

                MarkTile(type, pos);
                return true;
            });

            return tiles;
        }

        // --- Probabilidad 0: no generar nada ---
        if (probabilidad <= 0f)
            return tilesGenerated;

        // --- Configurar ruido ---
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        // Frecuencia determina tamaño de continentes
        float frequency = Mathf.Lerp(0.04f, 0.006f, probabilidad / 100f);
        noise.SetFrequency(frequency);

        // Threshold determina qué partes se activan
        float threshold = Mathf.Lerp(1f, -1f, probabilidad / 100f);

        ForEachTilePosition(pos =>
        {
            // Si hay base requerida, verificarla
            if (underType.HasValue && !IsTileMarked(underType.Value, pos))
                return true;

            float v = noise.GetNoise(pos.X, pos.Y);

            if (v > threshold)
            {
                MarkTile(type, pos);
                tilesGenerated.Add(pos);
            }

            return true;
        });

        return tilesGenerated;
    }



    /// <summary>
    /// Obtiene la lista de todos los tiles marcados para un TerrainTileType.
    /// Devuelve las coordenadas reales del mapa (negativos y positivos).
    /// </summary>
    public List<Vector2I> GetMarkedTiles(TerrainTileType type)
    {
        bool[,] mask = layerTileMask[type];
        int width = sizeMap.X;
        int height = sizeMap.Y;

        int initX = width / 2;
        int initY = height / 2;

        List<Vector2I> tiles = new List<Vector2I>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mask[x, y])
                {
                    // Convertimos índices de matriz → coordenada real del mapa
                    int realX = x - initX;
                    int realY = y - initY;

                    tiles.Add(new Vector2I(realX, realY));
                }
            }
        }

        return tiles;
    }

    /// <summary>
    /// Recorre todas las posiciones del mapa y ejecuta una acción por cada una.
    /// Devuelve además la lista de posiciones recorridas.
    /// </summary>
    private List<Vector2I> ForEachTilePosition(Func<Vector2I, bool>? onTile = null)
    {
        int initX = sizeMap.X / 2;
        int initY = sizeMap.Y / 2;

        List<Vector2I> tiles = new List<Vector2I>();

        for (int x = -initX; x < initX; x++)
        {
            for (int y = -initY; y < initY; y++)
            {
                Vector2I pos = new Vector2I(x, y);

                // Se agrega siempre la posición
                tiles.Add(pos);

                // Si hay callback, ejecutarlo
                // Si retorna false, se puede cortar la iteración (por si lo necesitas)
                if (onTile != null)
                {
                    if (!onTile(pos))
                        return tiles;
                }
            }
        }

        return tiles;
    }
    /// <summary>
    /// Normaliza la capa eliminando tiles aislados o con poca vecindad.
    /// Regla:
    ///  - Debe tener al menos 1 vecino vertical (arriba o abajo)
    ///  - Debe tener al menos 1 vecino horizontal (izquierda o derecha)
    ///  - Debe tener al menos 1 vecino diagonal
    ///  - Si tiene 4 vecinos se completa rededor
    /// </summary>
    public List<Vector2I> NormalizeLayer(TerrainTileType type)
    {
        bool[,] mask = layerTileMask[type];
        int width = sizeMap.X;
        int height = sizeMap.Y;

        List<Vector2I> removed = new List<Vector2I>();

        bool[,] newMask = (bool[,])mask.Clone();

        int initX = width / 2;
        int initY = height / 2;

        // Offsets de las 8 direcciones
        Vector2I[] dirs = {
        new Vector2I(0, -1),  // arriba
        new Vector2I(0, 1),   // abajo
        new Vector2I(-1, 0),  // izquierda
        new Vector2I(1, 0),   // derecha
        new Vector2I(-1, -1), // arriba izq
        new Vector2I(1, -1),  // arriba der
        new Vector2I(-1, 1),  // abajo izq
        new Vector2I(1, 1)    // abajo der
    };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!mask[x, y])
                    continue;

                // --- Contadores de vecinos ---
                int totalNeighbors = 0;

       

                // Lista para detectar casillas faltantes
                List<(int nx, int ny)> missingNeighbors = new List<(int, int)>();
                List<(int nx, int ny)> existingNeighbors = new List<(int, int)>();

                for (int i = 0; i < dirs.Length; i++)
                {
                    int nx = x + dirs[i].X;
                    int ny = y + dirs[i].Y;

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    if (mask[nx, ny])
                    {
                        totalNeighbors++;
                        existingNeighbors.Add((nx, ny));
                    }
                    else
                    {
                        missingNeighbors.Add((nx, ny));
                    }
                }


                if (totalNeighbors <= 8)
                {
                    foreach (var (mx, my) in missingNeighbors)
                    {
                        newMask[mx, my] = true;
                    }

                    // no se elimina el tile
                    continue;
                }
            }
        }

        layerTileMask[type] = newMask;

        return removed;
    }


    public List<Vector2I> RemoveLowDensityTiles(TerrainTileType type)
    {
        bool[,] mask = layerTileMask[type];
        int width = sizeMap.X;
        int height = sizeMap.Y;

        List<Vector2I> removed = new List<Vector2I>();
        bool[,] newMask = (bool[,])mask.Clone();

        // offsets 8-direcciones
        Vector2I[] dirs = {
        new Vector2I(0, -1),
        new Vector2I(0, 1),
        new Vector2I(-1, 0),
        new Vector2I(1, 0),
        new Vector2I(-1, -1),
        new Vector2I(1, -1),
        new Vector2I(-1, 1),
        new Vector2I(1, 1)
    };

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!mask[x, y])
                    continue;

                int neighborCount = 0;
                List<Vector2I> neighbors = new List<Vector2I>();

                // contamos vecinos existentes y guardamos posiciones
                for (int i = 0; i < dirs.Length; i++)
                {
                    int nx = x + dirs[i].X;
                    int ny = y + dirs[i].Y;

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    neighbors.Add(new Vector2I(nx, ny));

                    if (mask[nx, ny])
                        neighborCount++;
                }

                // --- REGLA NUEVA ---
                if (neighborCount <= 3)
                {
                    // eliminar el tile central
                    newMask[x, y] = false;
                    removed.Add(new Vector2I(x, y));

                    // eliminar cada vecino circundante
                    foreach (var n in neighbors)
                    {
                        if (newMask[n.X, n.Y])
                        {
                            newMask[n.X, n.Y] = false;
                            removed.Add(n);
                        }
                    }
                }
            }
        }

        layerTileMask[type] = newMask;
        return removed;
    }

}
