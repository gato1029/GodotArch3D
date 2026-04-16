using Godot;
using GodotEcsArch.sources.BlackyTiles.Procedural.Terrain;
using GodotEcsArch.sources.BlackyTiles.Systems;
using GodotEcsArch.sources.utils;
using GodotEcsArch.sources.WindowsDataBase.Terrain.DataBase;
using GodotEcsArch.sources.WindowsDataBase.TileSprite;
using GodotFlecs.sources.KuroTiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Flecs.NET.Core.Ecs;

namespace GodotEcsArch.sources.BlackyTiles.Procedural;

public class BlackyWorldTerrainGenerator
{
    private readonly int worldSeed;
    private readonly BlackyWorldBiomeMap biomeMap;

    private readonly Dictionary<Vector2I, ushort[,]> chunkHeightCache = new();
    private readonly Dictionary<Vector2I, ushort[,]> chunkHeightBorderCache = new();

    private readonly FastNoiseLite heightNoise;

    // =========================================
    // ⚙️ CONFIGURACIÓN DE ELEVACIÓN
    // =========================================

    // 🔥 FRECUENCIA DEL RUIDO (ESCALA / TAMAÑO DE MANCHAS)
    // Controla qué tan “zoom in/out” está el noise.
    //
    // ⬆️ AUMENTAR (ej: 0.05 → 0.08 → 0.12)
    //     → más detalle
    //     → manchas MÁS PEQUEÑAS
    //     → terreno más ruidoso
    //
    // ⬇️ DISMINUIR (ej: 0.05 → 0.03 → 0.02)
    //     → menos detalle
    //     → manchas MÁS GRANDES
    //     → terreno más suave / continuo
    //
    // 💡 Este es el parámetro MÁS importante para el tamaño visual
    private const float HEIGHT_NOISE_FREQUENCY = 0.02f;


    // 🔥 THRESHOLD (CANTIDAD DE SEMILLAS / DENSIDAD)
    // Decide cuántos puntos iniciales se convierten en altura.
    //
    // ⬆️ AUMENTAR (ej: 0.65 → 0.70 → 0.75)
    //     → menos puntos pasan el filtro
    //     → MENOS manchas
    //     → manchas más aisladas y grandes
    //
    // ⬇️ DISMINUIR (ej: 0.65 → 0.60 → 0.55)
    //     → más puntos pasan el filtro
    //     → MÁS manchas
    //     → terreno más fragmentado
    //
    // 💡 Controla “cuántas manchas hay”, no tanto su tamaño
    private const float HEIGHT_NOISE_THRESHOLD = 0.48f;


    // 🔥 EXPANSIÓN (CRECIMIENTO DE MANCHAS)
    // Cuántas veces se expande la altura desde las semillas.
    //
    // ⬆️ AUMENTAR (ej: 2 → 3 → 4 → 5)
    //     → manchas crecen MÁS
    //     → zonas más grandes y conectadas
    //     → puede suavizar huecos pequeños
    //
    // ⬇️ DISMINUIR (ej: 3 → 2 → 1)
    //     → manchas crecen MENOS
    //     → quedan más pequeñas y separadas
    //
    // 💡 Define el “grosor” o alcance de cada mancha
    private const int HEIGHT_EXPANSION_ITERATIONS = 2;


    // 🔥 DISTANCIA AL BORDE DEL BIOMA
    // Evita que las elevaciones aparezcan cerca de los bordes del bioma.
    //
    // ⬆️ AUMENTAR (ej: 2 → 3 → 4)
    //     → más espacio limpio en los bordes
    //     → transiciones entre biomas más suaves
    //     → menos ruido en límites
    //
    // ⬇️ DISMINUIR (ej: 3 → 2 → 1)
    //     → elevaciones pueden aparecer pegadas al borde
    //     → transiciones más bruscas / naturales según estilo
    //
    // 💡 Útil para evitar artefactos en cambios de bioma
    private const int MIN_DISTANCE_FROM_BORDER = 10;
    private readonly int chunkSize;
    public BlackyWorldTerrainGenerator(int worldSeed, int chunkSize, BlackyWorldBiomeMap biomeMap)
    {
        this.worldSeed = worldSeed;
        this.biomeMap = biomeMap;
        this.chunkSize = chunkSize;
        heightNoise = new FastNoiseLite(worldSeed + 5000);
        heightNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

        // 🔥 Controla tamaño de las formas
        heightNoise.SetFrequency(HEIGHT_NOISE_FREQUENCY);
    }

    // =========================================
    // 🌍 PUBLIC API
    // =========================================
    public ushort[,] GetChunkHeightBorders(Vector2I chunkCoord)
    {
        if (chunkHeightBorderCache.TryGetValue(chunkCoord, out var cached))
            return cached;

        int padding = 1; // 🔥 suficiente para vecinos
        int paddedSize = chunkSize + padding * 2;

        ushort[,] heightExtended = new ushort[paddedSize, paddedSize];
        ushort[,] borderExtended = new ushort[paddedSize, paddedSize];

        int startX = chunkCoord.X * chunkSize - padding;
        int startY = chunkCoord.Y * chunkSize - padding;

        // 🔥 construir alturas extendidas (IMPORTANTE)
        for (int x = 0; x < paddedSize; x++)
        {
            for (int y = 0; y < paddedSize; y++)
            {
                int worldX = startX + x;
                int worldY = startY + y;

                heightExtended[x, y] = GetHeightAt(worldX, worldY);
            }
        }

        // 🔥 calcular bordes con contexto real
        GenerateHeightBorders(heightExtended, borderExtended);

        // ✂️ recortar
        ushort[,] final = new ushort[chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                final[x, y] = borderExtended[x + padding, y + padding];
            }
        }

        chunkHeightBorderCache[chunkCoord] = final;
        return final;
    }
    public ushort[,] GetChunkHeights(Vector2I chunkCoord)
    {
        if (chunkHeightCache.TryGetValue(chunkCoord, out var cached))
            return cached;

        int padding = MIN_DISTANCE_FROM_BORDER + 2;
        int paddedSize = chunkSize + padding * 2;

        ushort[,] biomeExtended = new ushort[paddedSize, paddedSize];
        ushort[,] borderExtended = new ushort[paddedSize, paddedSize];
        ushort[,] heightExtended = new ushort[paddedSize, paddedSize];

        ushort[,] tempMap = new ushort[paddedSize, paddedSize];

        int startX = chunkCoord.X * chunkSize - padding;
        int startY = chunkCoord.Y * chunkSize - padding;

        // =====================================
        // 🔥 1. CONSTRUIR CONTEXTO GLOBAL
        // =====================================
        for (int x = 0; x < paddedSize; x++)
        {
            for (int y = 0; y < paddedSize; y++)
            {
                int worldX = startX + x;
                int worldY = startY + y;

                biomeExtended[x, y] = biomeMap.GetBiomeAt(worldX, worldY);
                borderExtended[x, y] = biomeMap.GetBorderAt(worldX, worldY);
            }
        }

        // =====================================
        // 🔥 2. GENERAR ALTURAS (EXTENDIDO)
        // =====================================
        var terrainCache = new Dictionary<ushort, TerrainData>();
        var baseHeightCache = new Dictionary<ushort, ushort>();

        GenerateHeights(
            startX,
            startY,
            biomeExtended,
            borderExtended,
            heightExtended,
            terrainCache,
            baseHeightCache
        );

        // =====================================
        // 🔥 3. LIMPIAR HUÉRFANOS (AQUÍ VA)
       // // =====================================
       //tempMap = RemoveOrphanHeights(
       //     biomeExtended,
       //     heightExtended,
       //     terrainCache,
       //     baseHeightCache
       // );

        // (opcional 🔥 mejora mucho)
        // RemoveOrphanHeights(...);

        // =====================================
        // ✂️ 4. RECORTAR
        // =====================================
        ushort[,] finalMap = new ushort[chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                finalMap[x, y] = heightExtended[x + padding, y + padding];
            }
        }

        // =====================================
        // 💾 5. CACHE
        // =====================================
        chunkHeightCache[chunkCoord] = finalMap;

        return finalMap;
    }

    public void ClearCache()
    {
        chunkHeightBorderCache.Clear(); // 🔥 no olvidar
        chunkHeightCache.Clear();
    }

    // =========================================
    // 🧠 CORE
    // =========================================
    private void GenerateHeightBorders(
    ushort[,] heightMap,
    ushort[,] borderMap)
    {
        int size = heightMap.GetLength(0);

        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                ushort current = heightMap[x, y];

                // 🔥 ignorar base
                if (current == 0)
                {
                    borderMap[x, y] = 0;
                    continue;
                }

                bool isBorder = false;

                // 🔥 vecinos 8-direcciones
                for (int dx = -1; dx <= 1 && !isBorder; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        if (heightMap[x + dx, y + dy] != current)
                        {
                            isBorder = true;
                            break;
                        }
                    }
                }

                borderMap[x, y] = isBorder ? heightMap[x,y] : (ushort)0;
            }
        }
    }

    private ushort[,] RemoveOrphanHeights(
    ushort[,] biomeMap,
    ushort[,] heightMap,
    Dictionary<ushort, TerrainData> terrainCache,
    Dictionary<ushort, ushort> baseHeightCache)
    {
        int size = heightMap.GetLength(0);
        ushort[,] buffer = new ushort[size, size];

        Array.Copy(heightMap, buffer, heightMap.Length);

        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                ushort biomeId = biomeMap[x, y];

                if (!terrainCache.TryGetValue(biomeId, out var terrain))
                    continue;

                ushort baseHeight = baseHeightCache[biomeId];
                ushort current = heightMap[x, y];

                // 🔹 solo limpiar alturas superiores
                if (current <= baseHeight)
                    continue;

                int neighbors = CountSameNeighbors(heightMap, x, y, current);

                // 🔥 REGLA SIMPLE (la que tú quieres)
                if (neighbors <= 4)
                {
                    buffer[x, y] = baseHeight; //revisar aqui
                }
            }
        }


        return buffer;
        //Array.Copy(buffer, heightMap, heightMap.Length);
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

                //if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                //    continue;

                if (map[nx, ny] == biome)
                    count++;
            }
        }

        return count;
    }
    private void GenerateHeights(
    int startX,
    int startY,
    ushort[,] biomeMap,
    ushort[,] borderMap,
    ushort[,] heightMap,
    Dictionary<ushort, TerrainData> terrainCache,
    Dictionary<ushort, ushort> baseHeightCache)
    {
        int size = biomeMap.GetLength(0);

        // 🔹 buffer de semillas
        ushort[,] seedMap = new ushort[size, size];

        // =====================================
        // 🔥 1. GENERAR SEMILLAS (SIN SOPORTE)
        // =====================================
        for (int x = 1; x < size - 1; x++)
        {
            for (int y = 1; y < size - 1; y++)
            {
                ushort biomeId = biomeMap[x, y];

                // 🔹 cache terrain
                if (!terrainCache.TryGetValue(biomeId, out var terrain))
                {
                    terrain = MasterDataManager.GetBySaveIds<TerrainData>(biomeId);
                    terrainCache[biomeId] = terrain;
                }

                if (terrain == null || terrain.terrains.Count == 0)
                {
                    seedMap[x, y] = 0;
                    continue;
                }

                // 🔹 cache base height
                if (!baseHeightCache.TryGetValue(biomeId, out ushort baseHeight))
                {
                    baseHeight = GetBaseHeight(terrain);
                    baseHeightCache[biomeId] = baseHeight;
                }

                ushort upperHeight = GetNextHeight(terrain, baseHeight);

                // 🔹 distancia a borde
                if (!IsFarFromBorder(borderMap, x, y, MIN_DISTANCE_FROM_BORDER))
                {
                    seedMap[x, y] = baseHeight;
                    continue;
                }

                // 🔹 ruido
                int worldX = startX + x;
                int worldY = startY + y;

                float elevation = heightNoise.GetNoise(worldX, worldY);

                // 🔥 seed inicial
                seedMap[x, y] = (elevation > HEIGHT_NOISE_THRESHOLD && upperHeight > baseHeight)
                    ? upperHeight
                    : baseHeight;
            }
        }

        // =====================================
        // 🔥 2. COPIAR A HEIGHT MAP
        // =====================================
        Array.Copy(seedMap, heightMap, seedMap.Length);

        // =====================================
        // 🔥 3. EXPANSIÓN (SOPORTE REAL)
        // =====================================
        for (int i = 0; i < HEIGHT_EXPANSION_ITERATIONS; i++)
        {
            for (int x = 1; x < size - 1; x++)
            {
                for (int y = 1; y < size - 1; y++)
                {
                    ushort biomeId = biomeMap[x, y];

                    if (!terrainCache.TryGetValue(biomeId, out var terrain))
                        continue;

                    ushort baseHeight = baseHeightCache[biomeId];
                    ushort upperHeight = GetNextHeight(terrain, baseHeight);

                    // 🔹 solo expandir base
                    if (heightMap[x, y] > baseHeight)
                        continue;

                    int support = 0;

                    if (heightMap[x - 1, y] > baseHeight) support++;
                    if (heightMap[x + 1, y] > baseHeight) support++;
                    if (heightMap[x, y - 1] > baseHeight) support++;
                    if (heightMap[x, y + 1] > baseHeight) support++;

                    // 🔥 regla de crecimiento
                    if (support >= 2)
                    {
                        heightMap[x, y] = upperHeight;
                    }
                }
            }
        }
    }
    private ushort GetNextHeight(TerrainData terrain, ushort baseHeight)
    {
        ushort next = ushort.MaxValue;

        foreach (var kv in terrain.terrains)
        {
            ushort h = (ushort)kv.Value.heightReal;

            if (h > baseHeight && h < next)
                next = h;
        }

        return next == ushort.MaxValue ? baseHeight : next;
    }


    // =========================================
    // 🔍 UTILIDADES
    // =========================================

    private ushort GetBaseHeight(TerrainData terrain)
    {
        int min = int.MaxValue;

        foreach (var kv in terrain.terrains)
        {
            int h = kv.Value.heightReal;

            if (h < min)
                min = h;
        }

        return (ushort)min;
    }

    private bool IsFarFromBorder(ushort[,] borderMap, int x, int y, int minDistance)
    {
        int size = borderMap.GetLength(0);

        for (int r = 1; r <= minDistance; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx < 0 || ny < 0 || nx >= size || ny >= size)
                        continue;             
                    if (borderMap[nx, ny] != 0)
                        return false;
                }
            }
        }

        return true;
    }

    public ushort GetBiomeAt(int worldX, int worldY)
    {
        return biomeMap.GetBiomeAt(worldX, worldY);
    }
    public ushort GetBorderAt(int worldX, int worldY)
    {
        return biomeMap.GetBorderAt(worldX, worldY);
    }
    public ushort GetHeightAt(int worldX, int worldY)
    {
        Vector2I chunkCoord = new Vector2I(
            Mathf.FloorToInt((float)worldX / chunkSize),
            Mathf.FloorToInt((float)worldY / chunkSize)
        );

        ushort[,] chunk = GetChunkHeights(chunkCoord);

        int localX = Mod(worldX, chunkSize);
        int localY = Mod(worldY, chunkSize);

        return chunk[localX, localY];
    }

    public ushort GetHeightBorderAt(int worldX, int worldY)
    {
        Vector2I chunkCoord = new Vector2I(
            Mathf.FloorToInt((float)worldX / chunkSize),
            Mathf.FloorToInt((float)worldY / chunkSize)
        );

        ushort[,] chunk = GetChunkHeightBorders(chunkCoord);

        int localX = Mod(worldX, chunkSize);
        int localY = Mod(worldY, chunkSize);

        return chunk[localX, localY];
    }
    private int Mod(int a, int m)
    {
        int r = a % m;
        return r < 0 ? r + m : r;
    }

    public void ExportWorldHeightsStitched(
    Dictionary<Vector2I, ushort[,]> chunks,
    string path = "user://terrain_debug/world_heights.txt")
    {
        DirAccess.MakeDirRecursiveAbsolute("user://terrain_debug/");

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

        // 🔥 escribir archivo
        for (int y = worldHeight - 1; y >= 0; y--)
        {
            string line = "";

            for (int x = 0; x < worldWidth; x++)
            {
                line += worldMap[x, y].ToString();
            }

            file.StoreLine(line);
        }

        GD.Print($"🌍 World alturas exportado: {path}");
    }
    public void ExportWorldHeightBordersStitched(
    Dictionary<Vector2I, ushort[,]> chunks,
    string path = "user://terrain_debug/world_height_borders.txt")
    {
        DirAccess.MakeDirRecursiveAbsolute("user://terrain_debug/");

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

        // =========================================
        // 🔥 copiar chunks (bordes)
        // =========================================
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

        // =========================================
        // 📝 escribir archivo
        // =========================================
        for (int y = worldHeight - 1; y >= 0; y--)
        {
            string line = "";

            for (int x = 0; x < worldWidth; x++)
            {
                // 🔥 para debug visual (más claro)
                line += worldMap[x, y] ;
            }

            file.StoreLine(line);
        }

        GD.Print($"🌍 World bordes de altura exportado: {path}");
    }
}