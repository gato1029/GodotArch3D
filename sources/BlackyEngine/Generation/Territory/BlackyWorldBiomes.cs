using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.WindowsDataBase.Biomas;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MessagePack.GeneratedMessagePackResolver.GodotEcsArch.sources.BlackyEngine;

// (los using de BlackyWorldConfig / BlackyWorldSeed / BlackyWorldHeight / BiomaData / TemplateWorldProfileData ya existen en tu proyecto)

[Flags]
public enum BiomeCellBorderType : byte
{
    None = 0,
    Height = 1 << 0,
    Map = 1 << 1,
    Biome = 1 << 2
}

/// <summary>Resultado de la capa de biomas para una celda (coordenadas de mundo).</summary>
public struct BiomeCell
{
    /// <summary>Bioma de la celda. null si la celda está fuera del mundo.</summary>
    public BiomaData Bioma;

    /// <summary>
    /// Terreno de la celda. En altura 0 es SIEMPRE el terreno del bioma base (agua);
    /// en alturas 1..3 es el idTerreno de su propio bioma.
    /// </summary>
    public long IdTerreno;

    /// <summary>
    /// Superficie de la celda. En altura 0 lleva la superficie del bioma cuando no es el agua base.
    /// En una transición es el material ÚNICO del tramo: la superficie del bioma de MENOR id del par.
    /// Los DOS lados del borde devuelven exactamente el mismo valor; no se combinan ids.
    /// </summary>
    public long IdSuperficie;

    /// <summary>
    /// Bioma DUEÑO de esa superficie (el de menor id del par). Puede ser el propio bioma de la celda cuando esta
    /// ya usa ese material. null si la celda no está en transición.
    /// </summary>
    public BiomaData SurfaceBioma;

    /// <summary>
    /// TRANSICIÓN: el OTRO bioma del par (el que está al otro lado del borde), dentro de TransitionWidth tiles y al
    /// mismo nivel de altura. Se marca en LOS DOS lados del borde. null = la celda no está en una zona de transición.
    /// </summary>
    public BiomaData TransitionBioma;

    /// <summary>Nivel de altura de la geografía en esta celda (0..3). 0 = agua.</summary>
    public byte HeightLevel;

    /// <summary>0 = sin transición; 1..TransitionWidth = tiles hasta el bioma vecino (1 = pegada al borde).</summary>
    public byte TransitionDistance;

    /// <summary>
    /// TRANSICIÓN DE ALTURA: 0 = ninguna; 1..HeightTransitionWidth = tiles hasta la celda más cercana que tiene OTRO nivel
    /// de altura (1 = pegada al cambio). Se marca a ambos lados del cambio. Es independiente de la transición de biomas.
    /// </summary>
    public byte HeightTransitionDistance;

    /// <summary>Nivel de altura (0..3) de esa celda vecina más cercana. Solo es válido si IsHeightTransition.</summary>
    public byte HeightNeighborLevel;

    /// <summary>
    /// IdTerreno del lado de menor altura en la transición. Si esta celda es el lado bajo, es su propio IdTerreno;
    /// si la celda vecina es el lado bajo, es el terreno de esa vecina. 0 si no hay transición de altura.
    /// </summary>
    public long HeightLowerTerrainId;

    /// <summary>Tipos de borde o transición que afectan a esta celda. Puede contener varios valores a la vez.</summary>
    public BiomeCellBorderType BorderType;

    /// <summary>
    /// true si esta celda llevará una decoración (pasto, flor, mancha, etc.).
    /// Se decide con ruido y solo en interiores: nunca en borde de mapa,
    /// transición de bioma ni transición de altura.
    /// </summary>
    public bool HasDecoration;

    public bool IsMapBorder => (BorderType & BiomeCellBorderType.Map) != 0;
    public bool IsBiomeBorder => (BorderType & BiomeCellBorderType.Biome) != 0;
    public bool IsHeightBorder => (BorderType & BiomeCellBorderType.Height) != 0;
    public bool IsTransition => TransitionBioma != null;
    public bool IsHeightTransition => HeightTransitionDistance != 0;

    /// <summary>true si la celda vecina que marca el cambio es más alta que esta (esta celda queda "abajo" del cambio).</summary>
    public bool HeightNeighborIsHigher => HeightTransitionDistance != 0 && HeightNeighborLevel > HeightLevel;
}

/// <summary>Bloque rectangular de celdas de bioma, indexable por coordenadas de MUNDO.</summary>
public sealed class BiomeRegion
{
    public int MinX { get; }
    public int MinY { get; }
    public int Width { get; }
    public int Height { get; }

    public BiomeCell[] Cells { get; }    // fila por fila: índice = (y - MinY) * Width + (x - MinX)

    public BiomeRegion(int minX, int minY, int width, int height)
    {
        MinX = minX; MinY = minY; Width = width; Height = height;
        Cells = new BiomeCell[(long)width * height];
    }

    public bool Contains(int worldX, int worldY)
        => worldX >= MinX && worldX < MinX + Width && worldY >= MinY && worldY < MinY + Height;

    public BiomeCell this[int worldX, int worldY] => Cells[(long)(worldY - MinY) * Width + (worldX - MinX)];
}

/// <summary>
/// Capa de biomas: elige el bioma de cada celda a partir de las reglas del template (reglasBiomas) y de la
/// altura que dejó BlackyWorldGeography en heightMapWorld (la geografía debe estar generada antes de pedir biomas). No guarda un mapa del mundo: el bioma de una celda
/// depende solo de (semilla, coordenadas, nivel de altura), así que se calcula por regiones/chunks.
///
/// Reglas:
///  - Una regla aplica a un nivel L (0..3) si alturaMinima &lt;= L &lt;= alturaMaxima (ambos inclusive).
///  - Entre las reglas que aplican a un nivel, se reparte por 'peso' con un ruido lento, de modo que cada
///    bioma forma territorios continuos y las proporciones respetan los pesos (en el mundo completo).
///  - Cada celda devuelve su IdTerreno (siempre el de su bioma). Si está en transición de biomas, la superficie del
///    tramo es una sola: la del bioma de menor id del par (no se combinan ni alternan ids entre biomas). [FIX]
///  - Siempre debe existir al menos una regla para el nivel 0 (agua); si no, el constructor lanza error.
///  - Si un nivel alcanzable no tiene reglas, usa las del nivel inmediatamente inferior (y avisa en Warnings).
///
/// Rendimiento:
///  - Camino rápido: las celdas cuya ventana de transición es uniforme (la mayoría) se resuelven en O(1) con una tabla
///    de suma acumulada, sin recorrer el anillo de offsets.
///  - ForEachChunkParallel: genera chunks en paralelo (cada chunk en serie por dentro).
///  - Lectura de alturas: ver HeightReadsAreThreadSafe.
///  - Ruido de territorios: se evalúa en una rejilla y se interpola (ver NoiseGridStep).
/// </summary>
public class BlackyWorldBiomes
{
    private readonly BlackyWorldConfig worldConfig;
    private readonly BlackyWorldSeed worldSeed;
    private readonly BlackyWorldHeight heightMapWorld;
    private readonly TemplateWorldProfileData template;

    private const int LevelCount = 4;                  // niveles 0..3
    private const ushort NoBiome = ushort.MaxValue;    // celda fuera del mundo
    private const byte NoLevel = 255;
    private const long CdfSamples = 1_000_000;         // muestras para calcular los cortes por peso
    private const int MinLevelSamples = 200;           // por debajo, un nivel usa la distribución global
    private const int ParallelRowsThreshold = 20_000;  // celdas mínimas para paralelizar el ruido/biomas dentro de una región
    private const int ParallelCellsThreshold = 4_096;  // celdas mínimas para paralelizar el paso de transiciones

    // ---- Estructura resuelta desde las reglas ----
    private readonly List<BiomaData> biomes = new();                     // índice de bioma -> BiomaData
    private readonly int[][] levelBiomeIdx = new int[LevelCount][];      // por nivel: bioma de cada tramo
    private readonly float[][] levelWeights = new float[LevelCount][];   // por nivel: peso normalizado de cada tramo
    private readonly bool[] isWaterBiome;                                // bioma base de agua (isLowerBase)
    private int baseBiomeIndex = -1;                                     // índice del bioma isLowerBase, -1 si no hay
    private readonly List<string> warnings = new();
    // [FIX] id de bioma más pequeño de todos: permite cortar antes la búsqueda del par de menor id.
    private long globalMinBiomeId = long.MaxValue;

    // ---- Estado calculado (lazy) ----
    private readonly object sync = new();
    private readonly object heightLock = new();   // protege las lecturas de heightMapWorld cuando no es thread-safe
    private volatile bool prepared;
    private PerlinNoise noise;
    private PerlinNoise decorationNoise;
    private float decorationFrequency = 0.15f;
    private int decorationGridStep = 3;
    private float decorationThreshold = 0.45f;
    private float[][] levelCuts;
    private float biomeFrequency = 0.004f;
    private int noiseGridStep = 4;
    private int transitionWidth = 4;
    private int heightTransitionWidth = 2;
    private readonly Dictionary<int, (int dx, int dy, int d2)[]> ringOffsetsCache = new();

    /// <summary>
    /// Ciclos del ruido de territorios por tile. 0.004 => territorios de ~250 tiles. Es un tamaño ABSOLUTO
    /// (no depende del tamaño del mundo). Más bajo = territorios más grandes.
    /// </summary>
    public float BiomeFrequency
    {
        get => biomeFrequency;
        set { biomeFrequency = value; prepared = false; }
    }

    /// <summary>
    /// Paso (en tiles) de la rejilla donde se evalúa el ruido de territorios; entre nodos se interpola de forma bilineal.
    /// 1 = ruido exacto en cada celda (el más lento, igual que la versión original). 4 (por defecto) = ~16 veces menos
    /// evaluaciones de ruido; con BiomeFrequency 0.004 la diferencia visual es imperceptible. Rango 1..16.
    /// La rejilla está alineada a coordenadas de MUNDO, así que el resultado de una celda no depende de cómo partas las regiones.
    /// Cambiarlo cambia (muy poco) los bordes de los biomas respecto a otro valor.
    /// </summary>
    public int NoiseGridStep
    {
        get => noiseGridStep;
        set { noiseGridStep = Math.Clamp(value, 1, 16); prepared = false; }
    }

    /// <summary>
    /// Ciclos del ruido de decoraciones por tile. 0.05 => parches de ~20 tiles.
    /// Más bajo = manchas más grandes de decoración.
    /// </summary>
    public float DecorationFrequency
    {
        get => decorationFrequency;
        set => decorationFrequency = Math.Max(0.0001f, value);
    }

    /// <summary>
    /// Paso (en tiles) de la rejilla del ruido de decoraciones. 2 = buena calidad y barato.
    /// La rejilla está alineada a coordenadas de mundo.
    /// </summary>
    public int DecorationGridStep
    {
        get => decorationGridStep;
        set => decorationGridStep = Math.Clamp(value, 1, 16);
    }

    /// <summary>
    /// Umbral del ruido de decoración: la celda se marca si el valor supera este umbral.
    /// Más bajo = más decoración; más alto = menos. Rango -1..1.
    /// </summary>
    public float DecorationThreshold
    {
        get => decorationThreshold;
        set => decorationThreshold = Math.Clamp(value, -1f, 1f);
    }

    /// <summary>
    /// Pon true SOLO si BlackyWorldHeight.GetTopHeight es seguro para llamarlo desde varios hilos a la vez (por ejemplo,
    /// si solo lee de arreglos ya generados). Entonces las alturas se leen en paralelo y ForEachChunkParallel no se
    /// serializa en esa lectura. Con false (por defecto) cada lectura de alturas se hace en serie bajo un lock:
    /// es seguro, y el resto del trabajo (ruido, transiciones) sigue siendo paralelo.
    /// </summary>
    public bool HeightReadsAreThreadSafe { get; set; } = false;

    /// <summary>
    /// Ancho (en tiles) de la zona de transición entre dos biomas distintos, a cada lado del borde.
    /// 0 = no marcar transiciones. Máximo 16.
    /// </summary>
    public int TransitionWidth
    {
        get => transitionWidth;
        set => transitionWidth = Math.Clamp(value, 0, 16);
    }

    /// <summary>
    /// Ancho (en tiles) de la zona de transición de ALTURA: una celda está en transición de altura si hay otra celda con un
    /// nivel de altura distinto a esa distancia o menos. Se marca a ambos lados del cambio de altura.
    /// 0 = no marcar. Máximo 16. Es independiente de TransitionWidth (la de biomas).
    /// </summary>
    public int HeightTransitionWidth
    {
        get => heightTransitionWidth;
        set => heightTransitionWidth = Math.Clamp(value, 0, 16);
    }

    /// <summary>
    /// Fuerza a recalcular los cortes de biomas en la próxima consulta. Llámalo si vuelves a generar la geografía
    /// (los cortes se calculan una vez, la primera vez que se pide un bioma, leyendo las alturas ya generadas).
    /// </summary>
    public void RefreshFromGeography() => prepared = false;

    /// <summary>Avisos de validación del template (reglas inválidas, niveles sin reglas, etc.).</summary>
    public IReadOnlyList<string> Warnings => warnings;

    /// <summary>
    /// Solo afecta al DEBUG (ExportDebugPng), no a los datos: true = Y+ hacia arriba (la fila de arriba es MaxY);
    /// false = Y+ hacia abajo (la fila de arriba es MinY). Debe coincidir con BlackyWorldGeography.DebugYAxisUp.
    /// </summary>
    public bool DebugYAxisUp { get; set; } = true;

    // Misma convención de coordenadas que BlackyWorldGeography: centro (0,0), -N/2 .. N/2-1
    public int MinX => -(worldConfig.MapSize.X / 2);
    public int MinY => -(worldConfig.MapSize.Y / 2);
    public int MaxX => MinX + worldConfig.MapSize.X - 1;
    public int MaxY => MinY + worldConfig.MapSize.Y - 1;

    public BlackyWorldBiomes(BlackyWorldConfig worldConfig, BlackyWorldSeed worldSeed, BlackyWorldHeight heightMapWorld, TemplateWorldProfileData template)
    {
        this.worldConfig = worldConfig;
        this.worldSeed = worldSeed;
        this.heightMapWorld = heightMapWorld;
        this.template = template ?? throw new ArgumentNullException(nameof(template));

        BuildLevelRules();
        ResolveBaseBiome();

        isWaterBiome = new bool[biomes.Count];
        if (baseBiomeIndex >= 0)
        {
            isWaterBiome[baseBiomeIndex] = true;
        }
        else
        {
            // Compatibilidad: sin isLowerBase, todos los biomas del nivel 0 se tratan como agua.
            foreach (int b in levelBiomeIdx[0]) isWaterBiome[b] = true;
        }

        // [FIX] cache del id de bioma más pequeño (corte temprano al buscar el par de menor id)
        globalMinBiomeId = long.MaxValue;
        foreach (var b in biomes)
            if (b.id < globalMinBiomeId) globalMinBiomeId = b.id;
    }

    /// <summary>Un solo lugar para decidir cuándo dos BiomaData son "el mismo bioma": mismo id (o misma instancia).</summary>
    private static bool SameBiome(BiomaData a, BiomaData b) => ReferenceEquals(a, b) || a.id == b.id;

    /// <summary>Texto corto de un bioma para la leyenda de debug (BiomaData no tiene nombre).</summary>
    private static string BiomeLabel(BiomaData b) => $"bioma #{b.id} (terreno {b.idTerreno}, superficie {b.idSuperficie})";

    // ============================================================
    //  REGLAS -> ESTRUCTURA POR NIVEL
    // ============================================================
    private void BuildLevelRules()
    {
        var rules = template.reglasBiomas ?? new List<BiomePlacementRuleData>();

        // Niveles que el perfil geográfico puede producir (para avisar solo de lo que importa)
        int reachable = new ConfiguracionGeograficaData(template.perfilGeografico).AlturaMaxima;

        // Validación de cada regla (una sola vez)
        var valid = new bool[rules.Count];
        for (int i = 0; i < rules.Count; i++)
        {
            var r = rules[i];
            if (r == null || r.bioma == null) { warnings.Add($"Regla #{i}: sin bioma, se ignora."); continue; }
            if (r.peso <= 0f) { warnings.Add($"Regla #{i}: peso {r.peso} <= 0, se ignora."); continue; }
            if (r.alturaMinima > r.alturaMaxima) { warnings.Add($"Regla #{i}: alturaMinima {r.alturaMinima} > alturaMaxima {r.alturaMaxima}, se ignora."); continue; }
            valid[i] = true;
        }

        for (int level = 0; level < LevelCount; level++)
        {
            var entries = new List<(int biome, float weight)>();
            for (int i = 0; i < rules.Count; i++)
            {
                if (!valid[i]) continue;
                var r = rules[i];
                if (level < r.alturaMinima || level > r.alturaMaxima) continue;

                int b = GetBiomeIndex(r.bioma);
                int existing = entries.FindIndex(e => e.biome == b);
                if (existing >= 0) entries[existing] = (b, entries[existing].weight + r.peso);   // mismo bioma en 2 reglas
                else entries.Add((b, r.peso));
            }

            if (entries.Count == 0)
            {
                if (level == 0)
                    throw new InvalidOperationException(
                        "El template no tiene ninguna regla válida para la altura 0 (agua). Siempre debe existir una.");

                // Sin reglas en este nivel: se usan las del nivel inferior
                levelBiomeIdx[level] = levelBiomeIdx[level - 1];
                levelWeights[level] = levelWeights[level - 1];
                if (level <= reachable)
                    warnings.Add($"Nivel {level}: ninguna regla lo cubre; se usan las reglas del nivel {level - 1}.");
                continue;
            }

            float sum = 0f;
            foreach (var e in entries) sum += e.weight;

            levelBiomeIdx[level] = new int[entries.Count];
            levelWeights[level] = new float[entries.Count];
            for (int k = 0; k < entries.Count; k++)
            {
                levelBiomeIdx[level][k] = entries[k].biome;
                levelWeights[level][k] = entries[k].weight / sum;
            }
        }
    }

    /// <summary>
    /// Identifica el bioma base (isLowerBase), que para la altura 0 representa el agua.
    /// Si no hay ninguna regla marcada, se mantiene el comportamiento anterior.
    /// </summary>
    private void ResolveBaseBiome()
    {
        baseBiomeIndex = -1;

        var rules = template.reglasBiomas;
        if (rules == null) return;

        for (int i = 0; i < rules.Count; i++)
        {
            var r = rules[i];
            if (r == null || r.bioma == null || !r.isLowerBase) continue;
            if (r.peso <= 0f || r.alturaMinima > r.alturaMaxima) continue;

            // La base debe existir en la altura 0 (agua).
            if (r.alturaMinima > 0f || r.alturaMaxima < 0f)
            {
                warnings.Add($"Regla #{i}: isLowerBase=true pero no cubre la altura 0; se ignora como base de agua.");
                continue;
            }

            int b = GetBiomeIndex(r.bioma);
            if (baseBiomeIndex >= 0 && baseBiomeIndex != b)
            {
                warnings.Add($"Regla #{i}: hay más de un bioma isLowerBase; se usa '{biomes[baseBiomeIndex].name}'.");
                continue;
            }

            baseBiomeIndex = b;
        }

        if (baseBiomeIndex < 0)
            warnings.Add("Ninguna regla tiene isLowerBase=true; no se autocompleta la superficie de los biomas de altura 0.");
    }

    /// <summary>
    /// Devuelve el terreno que corresponde a un bioma en una altura.
    /// En altura 0 siempre es el terreno del bioma base (agua); el bioma real se representa en la superficie.
    /// </summary>
    private long TerrainIdAt(int biomeIndex, int height)
    {
        if (height == 0 && baseBiomeIndex >= 0)
            return biomes[baseBiomeIndex].idTerreno;

        return biomes[biomeIndex].idTerreno;
    }

    /// <summary>true si el bioma tiene al menos una decoración con probabilidad > 0.</summary>
    private static bool HasDecorationCandidates(BiomaData bioma)
    {
        var entries = bioma.idDecoraciones;
        if (entries == null) return false;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry != null && entry.idDecoracion > 0 && entry.Probability > 0f)
                return true;
        }

        return false;
    }

    private int GetBiomeIndex(BiomaData bioma)
    {
        for (int i = 0; i < biomes.Count; i++)
            if (SameBiome(biomes[i], bioma)) return i;

        biomes.Add(bioma);
        return biomes.Count - 1;
    }

    // ============================================================
    //  UTILIDADES DE PARALELISMO / LECTURA DE ALTURAS
    // ============================================================
    /// <summary>Ejecuta body(fila) para cada fila; en paralelo solo si se pide y hay suficiente trabajo.</summary>
    private static void RunRows(int rows, int cols, Action<int> body, bool parallel, int threshold)
    {
        if (parallel && (long)rows * cols >= threshold) Parallel.For(0, rows, body);
        else for (int y = 0; y < rows; y++) body(y);
    }

    /// <summary>
    /// Rellena 'levels' (ew x eh) con el nivel de altura de cada celda de la región ampliada que empieza en (ex0, ey0).
    /// Fuera del mundo = NoLevel. Si heightMapWorld no es thread-safe, toda la lectura va en serie bajo un lock
    /// (un solo lock por región, no por celda).
    /// </summary>
    private void FillLevels(byte[] levels, int ex0, int ey0, int ew, int eh, bool parallel)
    {
        Action<int> row = ey =>
        {
            int wy = ey0 + ey;
            long o = (long)ey * ew;
            for (int ex = 0; ex < ew; ex++)
            {
                int wx = ex0 + ex;
                levels[o + ex] = InsideWorld(wx, wy) ? ReadLevel(wx, wy) : NoLevel;
            }
        };

        if (HeightReadsAreThreadSafe)
        {
            RunRows(eh, ew, row, parallel, ParallelRowsThreshold);
        }
        else
        {
            lock (heightLock)
                for (int ey = 0; ey < eh; ey++) row(ey);
        }
    }

    // ============================================================
    //  RUIDO DE TERRITORIOS (rejilla + interpolación bilineal)
    // ============================================================
    /// <summary>Ruido en un punto de la rejilla (coordenadas de mundo). Único lugar donde se evalúa el Fbm.</summary>
    private float NodeNoise(int worldX, int worldY)
        => noise.Fbm(worldX * biomeFrequency, worldY * biomeFrequency, 3);

    /// <summary>Ruido de parches de decoración. 2 octavas para manchas pequeñas y naturales.</summary>
    private float DecorationNodeNoise(int worldX, int worldY)
        => decorationNoise.Fbm(worldX * decorationFrequency, worldY * decorationFrequency, 2);

    /// <summary>Ruido de una sola celda, con la misma interpolación que ComputeNoiseBlock (mismos resultados).</summary>
    private float NoiseAt(int worldX, int worldY, int g)
    {
        if (g <= 1) return NodeNoise(worldX, worldY);

        int gx = FloorDiv(worldX, g), gy = FloorDiv(worldY, g);
        float fx = (worldX - gx * g) / (float)g;
        float fy = (worldY - gy * g) / (float)g;

        float a = NodeNoise(gx * g, gy * g);
        float b = NodeNoise((gx + 1) * g, gy * g);
        float c = NodeNoise(gx * g, (gy + 1) * g);
        float d = NodeNoise((gx + 1) * g, (gy + 1) * g);

        float top = a + (b - a) * fx;
        float bot = c + (d - c) * fx;
        return top + (bot - top) * fy;
    }

    /// <summary>Ruido de todo un bloque rectangular (coordenadas de mundo). Evalúa Fbm solo en los nodos y interpola.</summary>
    private float[] ComputeNoiseBlock(int ex0, int ey0, int ew, int eh, int g, bool parallel, Func<int, int, float> nodeNoise = null)
    {
        nodeNoise ??= NodeNoise;
        float[] block = new float[(long)ew * eh];

        if (g <= 1)
        {
            RunRows(eh, ew, ey =>
            {
                int wy = ey0 + ey;
                long o = (long)ey * ew;
                for (int ex = 0; ex < ew; ex++) block[o + ex] = nodeNoise(ex0 + ex, wy);
            }, parallel, ParallelRowsThreshold);
            return block;
        }

        int gx0 = FloorDiv(ex0, g), gx1 = FloorDiv(ex0 + ew - 1, g) + 1;
        int gy0 = FloorDiv(ey0, g), gy1 = FloorDiv(ey0 + eh - 1, g) + 1;
        int gw = gx1 - gx0 + 1, gh = gy1 - gy0 + 1;

        float[] nodes = new float[(long)gw * gh];
        RunRows(gh, gw, j =>
        {
            int ny = (gy0 + j) * g;
            long o = (long)j * gw;
            for (int i = 0; i < gw; i++) nodes[o + i] = nodeNoise((gx0 + i) * g, ny);
        }, parallel, ParallelRowsThreshold / 4);

        // Índice de nodo y fracción por columna (se calculan una vez)
        int[] colNode = new int[ew];
        float[] colFrac = new float[ew];
        for (int ex = 0; ex < ew; ex++)
        {
            int wx = ex0 + ex;
            int gx = FloorDiv(wx, g);
            colNode[ex] = gx - gx0;
            colFrac[ex] = (wx - gx * g) / (float)g;
        }

        RunRows(eh, ew, ey =>
        {
            int wy = ey0 + ey;
            int gy = FloorDiv(wy, g);
            float fy = (wy - gy * g) / (float)g;

            long n0 = (long)(gy - gy0) * gw, n1 = n0 + gw;
            long o = (long)ey * ew;

            for (int ex = 0; ex < ew; ex++)
            {
                int i = colNode[ex];
                float fx = colFrac[ex];

                float a = nodes[n0 + i], b = nodes[n0 + i + 1], c = nodes[n1 + i], d = nodes[n1 + i + 1];
                float top = a + (b - a) * fx;
                float bot = c + (d - c) * fx;
                block[o + ex] = top + (bot - top) * fy;
            }
        }, parallel, ParallelRowsThreshold);

        return block;
    }

    // ============================================================
    //  PREPARACIÓN: ruido de territorios + cortes por peso (una vez)
    // ============================================================
    private void EnsurePrepared()
    {
        if (prepared) return;

        lock (sync)
        {
            if (prepared) return;

            if (biomeFrequency <= 0f) throw new InvalidOperationException("BiomeFrequency debe ser mayor que 0");

            noise = new PerlinNoise(worldSeed.GetRng("biomes").Next());
            decorationNoise = new PerlinNoise(worldSeed.GetRng("decorations").Next());
            int g = noiseGridStep;

            // Se muestrea el mundo en una cuadrícula y se guarda (valor de ruido, nivel de altura) de cada muestra.
            // Los cortes de cada nivel salen SOLO de las muestras de ese nivel: así cada bioma ocupa la proporción
            // de su peso dentro de su nivel, aunque la forma de la geografía y el ruido estén correlacionados.
            var size = worldConfig.MapSize;
            int w = size.X, h = size.Y;
            int minX = MinX, minY = MinY;
            long total = (long)w * h;
            int s = Math.Max(1, (int)Math.Ceiling(Math.Sqrt((double)total / CdfSamples)));
            int sw = (w + s - 1) / s, sh = (h + s - 1) / s;

            byte[] sampleLevel = new byte[(long)sw * sh];
            Action<int> readRow = j =>
            {
                for (int i = 0; i < sw; i++)
                    sampleLevel[j * sw + i] = ReadLevel(minX + i * s, minY + j * s);
            };
            if (HeightReadsAreThreadSafe) Parallel.For(0, sh, readRow);
            else lock (heightLock) for (int j = 0; j < sh; j++) readRow(j);

            float[] sample = new float[(long)sw * sh];
            Parallel.For(0, sh, j =>
            {
                for (int i = 0; i < sw; i++)
                    sample[j * sw + i] = NoiseAt(minX + i * s, minY + j * s, g);
            });

            var perLevel = new float[LevelCount][];
            for (int level = 0; level < LevelCount; level++)
            {
                int count = 0;
                foreach (byte lv in sampleLevel) if (lv == level) count++;

                var arr = new float[count];
                int k = 0;
                for (int i = 0; i < sample.Length; i++) if (sampleLevel[i] == level) arr[k++] = sample[i];
                Array.Sort(arr);
                perLevel[level] = arr;
            }
            Array.Sort(sample);   // distribución global: respaldo para niveles con muy pocas celdas

            var cuts = new float[LevelCount][];
            for (int level = 0; level < LevelCount; level++)
            {
                float[] src = perLevel[level].Length >= MinLevelSamples ? perLevel[level] : sample;
                float[] wts = levelWeights[level];

                cuts[level] = new float[wts.Length - 1];
                float cum = 0f;
                for (int k = 0; k < wts.Length - 1; k++)
                {
                    cum += wts[k];
                    cuts[level][k] = src[Math.Min(src.Length - 1, (int)(cum * src.Length))];
                }
            }

            levelCuts = cuts;
            prepared = true;
        }
    }

    /// <summary>Índice de bioma para un valor de ruido y un nivel de altura.</summary>
    private ushort BiomeFromNoise(float v, int level)
    {
        float[] cuts = levelCuts[level];
        int r = 0;
        while (r < cuts.Length && v >= cuts[r]) r++;
        return (ushort)levelBiomeIdx[level][r];
    }

    private bool InsideWorld(int x, int y) => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY;

    private byte ReadLevel(int x, int y)
    {
        int v = (int)heightMapWorld.GetTopHeight(x, y);
        return (byte)Math.Clamp(v, 0, LevelCount - 1);
    }

    // ============================================================
    //  TABLAS DE SUMA ACUMULADA (camino rápido de transiciones)
    // ============================================================
    /// <summary>
    /// Tabla de suma acumulada de "hay un cambio aquí": la celda difiere de la de su derecha o de la de abajo, o es
    /// noValue (fuera del mundo). Si la suma en una ventana es 0, todas las celdas de la ventana son iguales y están dentro del mundo.
    /// </summary>
    private static int[] BuildChangeSat<T>(T[] a, int ew, int eh, T noValue) where T : struct, IEquatable<T>
    {
        int S = ew + 1;
        int[] sat = new int[(long)S * (eh + 1)];

        for (int y = 0; y < eh; y++)
        {
            int rowSum = 0;
            long row = (long)y * ew;
            for (int x = 0; x < ew; x++)
            {
                T v = a[row + x];
                bool change = v.Equals(noValue)
                    || (x + 1 < ew && !v.Equals(a[row + x + 1]))
                    || (y + 1 < eh && !v.Equals(a[row + ew + x]));

                if (change) rowSum++;
                sat[(long)(y + 1) * S + (x + 1)] = sat[(long)y * S + (x + 1)] + rowSum;
            }
        }

        return sat;
    }

    /// <summary>Suma de cambios en la ventana cuadrada de radio r centrada en (cx, cy) (coordenadas de la región ampliada).</summary>
    private static int WindowChanges(int[] sat, int ew, int cx, int cy, int r)
    {
        int S = ew + 1;
        int x0 = cx - r, x1 = cx + r + 1, y0 = cy - r, y1 = cy + r + 1;
        return sat[(long)y1 * S + x1] - sat[(long)y0 * S + x1] - sat[(long)y1 * S + x0] + sat[(long)y0 * S + x0];
    }

    // ============================================================
    //  API
    // ============================================================
    /// <summary>Bioma de una celda (coordenadas de mundo). null si está fuera del mundo. Sin transiciones: es la consulta barata.</summary>
    public BiomaData GetBiome(int worldX, int worldY)
    {
        if (!InsideWorld(worldX, worldY)) return null;
        EnsurePrepared();

        byte level;
        if (HeightReadsAreThreadSafe) level = ReadLevel(worldX, worldY);
        else lock (heightLock) level = ReadLevel(worldX, worldY);

        return biomes[BiomeFromNoise(NoiseAt(worldX, worldY, noiseGridStep), level)];
    }

    /// <summary>Celda completa (bioma + transición). Para muchas celdas usa GenerateRegion, que es mucho más rápido.</summary>
    public BiomeCell GetCell(int worldX, int worldY) => GenerateRegion(worldX, worldY, 1, 1)[worldX, worldY];

    /// <summary>
    /// Calcula biomas y transiciones de un rectángulo (por ejemplo un chunk). Se calcula también un borde extra de
    /// tiles alrededor (el mayor entre TransitionWidth y HeightTransitionWidth), así el resultado de una celda es el mismo sin importar cómo partas el mundo
    /// en regiones. Las celdas que caigan fuera del mundo quedan con Bioma = null.
    /// Usa paralelismo interno (ideal para una región grande o una llamada suelta). Para recorrer muchos chunks usa ForEachChunkParallel.
    /// </summary>
    public BiomeRegion GenerateRegion(int minX, int minY, int width, int height)
        => GenerateRegionCore(minX, minY, width, height, parallel: true);

    private BiomeRegion GenerateRegionCore(int minX, int minY, int width, int height, bool parallel)
    {
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width), "El tamaño de la región debe ser positivo.");
        EnsurePrepared();

        int tw = transitionWidth, hw = heightTransitionWidth;   // se leen una vez por si cambian durante la ejecución
        int g = noiseGridStep;
        int m = Math.Max(tw, hw);                                // borde extra: cubre las dos ventanas
        int ew = width + 2 * m, eh = height + 2 * m;
        int ex0 = minX - m, ey0 = minY - m;

        // 1) Niveles de altura de la región ampliada (en serie bajo lock si heightMapWorld no es thread-safe)
        byte[] levels = new byte[(long)ew * eh];
        FillLevels(levels, ex0, ey0, ew, eh, parallel);

        // 2) Ruido de la región ampliada (rejilla + interpolación) y bioma de cada celda
        float[] noiseBlock = ComputeNoiseBlock(ex0, ey0, ew, eh, g, parallel);
        float[] decorationBlock = ComputeNoiseBlock(ex0, ey0, ew, eh, decorationGridStep, parallel, DecorationNodeNoise);
        ushort[] ids = new ushort[(long)ew * eh];
        RunRows(eh, ew, ey =>
        {
            long row = (long)ey * ew;
            for (int ex = 0; ex < ew; ex++)
            {
                byte lv = levels[row + ex];
                ids[row + ex] = lv == NoLevel ? NoBiome : BiomeFromNoise(noiseBlock[row + ex], lv);
            }
        }, parallel, ParallelRowsThreshold);

        // 3) Tablas de suma acumulada para el camino rápido (ventana uniforme => sin transición)
        int[] idSat = tw > 0 ? BuildChangeSat(ids, ew, eh, NoBiome) : null;
        int[] lvSat = hw > 0 ? BuildChangeSat(levels, ew, eh, NoLevel) : null;

        // 4) Celdas finales + transición (bioma distinto más cercano dentro de TransitionWidth)
        var region = new BiomeRegion(minX, minY, width, height);
        var offsets = GetRingOffsets(tw);
        var heightOffsets = GetRingOffsets(hw);

        Action<int> doCells = y =>
        {
            for (int x = 0; x < width; x++)
            {
                long center = (long)(y + m) * ew + (x + m);
                ushort id = ids[center];
                if (id == NoBiome) continue;

                var bioma = biomes[id];
                var cell = new BiomeCell
                {
                    Bioma = bioma,
                    IdTerreno = TerrainIdAt(id, levels[center]),
                    HeightLevel = levels[center]
                };

                // Camino rápido: si toda la ventana es uniforme, no hay nada que buscar en los anillos
                bool maybeBiome = tw > 0 && WindowChanges(idSat, ew, x + m, y + m, tw) != 0;
                bool maybeHeight = hw > 0 && WindowChanges(lvSat, ew, x + m, y + m, hw) != 0;

                // TRANSICIÓN DE BIOMA (simétrica: se marca en LOS DOS LADOS del borde):
                // El material del tramo es la superficie del bioma de MENOR id presente en la vecindad
                // (dentro de TransitionWidth y al mismo nivel de altura). Por eso los dos lados quedan
                // marcados con el MISMO IdSuperficie: no se combinan ni alternan ids entre biomas.
                //  - SurfaceBioma = bioma dueño del material (puede ser el PROPIO bioma de la celda).
                //  - TransitionBioma = el "otro" bioma del par, en ambos lados.
                //  - Entre varios vecinos gana el de menor id (no el más cercano): así el id no cambia
                //    celda a celda dentro del mismo parche por un vaivén de distancias.
                if (maybeBiome)
                {
                    byte ownLevel = cell.HeightLevel;
                    long ownSurface = bioma.idSuperficie;
                    long ownId = bioma.id;

                    int bestBiome = -1;
                    long bestD2 = long.MaxValue;
                    long bestPriority = long.MaxValue;   // min(ownId, id del vecino): manda el bioma de menor id
                    long bestOtherId = long.MaxValue;

                    for (int k = 0; k < offsets.Length; k++)
                    {
                        long neighbor = center + (long)offsets[k].dy * ew + offsets[k].dx;
                        ushort other = ids[neighbor];
                        if (other == NoBiome || other == id) continue;     // fuera del mundo o el mismo bioma
                        if (levels[neighbor] != ownLevel) continue;        // un cambio de altura no es transición de bioma

                        long otherSurface = biomes[other].idSuperficie;
                        if (otherSurface == ownSurface) continue;          // mismos materiales: no hay nada que pintar

                        long otherId = biomes[other].id;
                        long priority = ownId < otherId ? ownId : otherId;
                        long d2 = offsets[k].d2;

                        // Mejor candidato: menor id implicado; a igual id, el más cercano; y luego el vecino de menor id.
                        if (priority < bestPriority
                            || (priority == bestPriority && (d2 < bestD2 || (d2 == bestD2 && otherId < bestOtherId))))
                        {
                            bestPriority = priority;
                            bestBiome = other;
                            bestD2 = d2;
                            bestOtherId = otherId;
                        }

                        // Ya apareció el bioma de menor id del mapa y está pegado: nada puede mejorar.
                        if (bestPriority == globalMinBiomeId && bestD2 == 1) break;
                    }

                    if (bestBiome >= 0)
                    {
                        var partner = biomes[bestBiome];

                        // Dueño del material = el bioma de menor id de los dos (puede ser el propio).
                        var owner = ownId <= partner.id ? bioma : partner;

                        cell.SurfaceBioma = owner;                       // quién aporta la superficie
                        cell.IdSuperficie = owner.idSuperficie;          // el MISMO id en ambos lados del borde
                        cell.TransitionBioma = partner;                  // el "otro" bioma, también en ambos lados
                        cell.TransitionDistance = (byte)Math.Ceiling(Math.Sqrt(bestD2));
                    }
                }

                // Transición de altura: celda más cercana con otro nivel (offsets ordenados por distancia)
                if (maybeHeight)
                {
                    for (int k = 0; k < heightOffsets.Length; k++)
                    {
                        byte other = levels[center + (long)heightOffsets[k].dy * ew + heightOffsets[k].dx];
                        if (other != NoLevel && other != cell.HeightLevel)
                        {
                            cell.HeightNeighborLevel = other;
                            cell.HeightTransitionDistance = (byte)Math.Ceiling(Math.Sqrt(heightOffsets[k].d2));

                            long neighbor = center + (long)heightOffsets[k].dy * ew + heightOffsets[k].dx;
                            ushort neighborBiomeId = ids[neighbor];

                            // El terreno inferior también respeta la regla de altura 0 (agua).
                            cell.HeightLowerTerrainId = other < cell.HeightLevel
                                ? TerrainIdAt(neighborBiomeId, other)
                                : cell.IdTerreno;
                            break;
                        }
                    }
                }

                // Altura 0: el terreno es SIEMPRE agua y solo los biomas NO base aportan
                // su superficie. El agua base no lleva capa de superficie.
                if (cell.HeightLevel == 0 && baseBiomeIndex >= 0)
                {
                    if (id == baseBiomeIndex)
                        cell.IdSuperficie = 0;
                    else if (bioma.idSuperficie != 0)
                        cell.IdSuperficie = bioma.idSuperficie;
                }

                int worldX = minX + x;
                int worldY = minY + y;
                if (worldX == MinX || worldX == MaxX || worldY == MinY || worldY == MaxY)
                    cell.BorderType |= BiomeCellBorderType.Map;
                if (cell.IsTransition)
                    cell.BorderType |= BiomeCellBorderType.Biome;
                if (cell.IsHeightTransition)
                    if (cell.HeightTransitionDistance == 1)
                        cell.BorderType |= BiomeCellBorderType.Height;

                // Decoración: puede estar DENTRO de una transición, pero no en sus bordes.
                // Los bordes de la banda son la distancia 1 (el borde real) y la distancia
                // máxima (el límite exterior de la transición). Igual para altura.
                bool biomeTransitionEdge = cell.IsTransition
                    && (cell.TransitionDistance <= 1 || cell.TransitionDistance >= tw);
                bool heightTransitionEdge = cell.IsHeightTransition
                    && (cell.HeightTransitionDistance <= 1 || cell.HeightTransitionDistance >= hw);

                if (!cell.IsMapBorder
                    && !biomeTransitionEdge
                    && !heightTransitionEdge
                    && HasDecorationCandidates(bioma)
                    && decorationBlock[center] > decorationThreshold)
                {
                    cell.HasDecoration = true;
                }

                region.Cells[(long)y * width + x] = cell;
            }
        };

        if (parallel && (long)width * height >= ParallelCellsThreshold) Parallel.For(0, height, doCells);
        else for (int y = 0; y < height; y++) doCells(y);

        return region;
    }

    // ============================================================
    //  RECORRIDO DEL MUNDO POR CHUNKS
    //  (un mapa de 8192x8192 son 67M de celdas: no cabe en un solo arreglo, por eso se recorre por chunks)
    // ============================================================

    /// <summary>
    /// Chunk (chunkX, chunkY) de lado chunkSize. La cuadrícula de chunks parte del (0,0) del mundo, así que hay
    /// índices negativos: el chunk (0,0) cubre X e Y de 0 a chunkSize-1, y el (-1,-1) de -chunkSize a -1.
    /// Si el chunk se sale del mundo, las celdas de fuera vienen con Bioma = null.
    /// </summary>
    public BiomeRegion GenerateChunk(int chunkX, int chunkY, int chunkSize)
    {
        if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
        return GenerateRegionCore(chunkX * chunkSize, chunkY * chunkSize, chunkSize, chunkSize, parallel: true);
    }

    /// <summary>
    /// Igual que GenerateChunk pero SIN paralelismo interno: pensado para llamarlo desde tu propio Parallel.For
    /// (varios chunks a la vez, cada uno en un solo hilo). Es seguro entre hilos; con HeightReadsAreThreadSafe = true
    /// no hay ninguna lectura serializada.
    /// </summary>
    public BiomeRegion GenerateChunkSerial(int chunkX, int chunkY, int chunkSize)
    {
        if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
        EnsurePrepared();
        return GenerateRegionCore(chunkX * chunkSize, chunkY * chunkSize, chunkSize, chunkSize, parallel: false);
    }

    /// <summary>Rango (inclusivo) de índices de chunk que tocan el mundo.</summary>
    public void GetChunkRange(int chunkSize, out int minChunkX, out int maxChunkX, out int minChunkY, out int maxChunkY)
    {
        if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
        minChunkX = FloorDiv(MinX, chunkSize); maxChunkX = FloorDiv(MaxX, chunkSize);
        minChunkY = FloorDiv(MinY, chunkSize); maxChunkY = FloorDiv(MaxY, chunkSize);
    }

    /// <summary>
    /// Recorre TODO el mundo chunk por chunk (filas de arriba a abajo, de izquierda a derecha), en serie. Cada chunk se calcula
    /// cuando se pide y se puede soltar al terminar, así la memoria se mantiene baja.
    /// Los chunks de los bordes pueden salirse del mundo: salta las celdas con Bioma == null.
    /// Usa el tamaño de TU chunk para que cada región coincida con un chunk del juego.
    /// Si el orden no importa, ForEachChunkParallel es bastante más rápido.
    /// </summary>
    public IEnumerable<(int chunkX, int chunkY, BiomeRegion region)> EnumerateChunks(int chunkSize)
    {
        GetChunkRange(chunkSize, out int minCx, out int maxCx, out int minCy, out int maxCy);
        for (int cy = minCy; cy <= maxCy; cy++)
            for (int cx = minCx; cx <= maxCx; cx++)
                yield return (cx, cy, GenerateChunk(cx, cy, chunkSize));
    }

    /// <summary>
    /// Recorre TODO el mundo generando los chunks EN PARALELO (cada chunk se calcula en serie por dentro, así no hay
    /// sobrecarga de Parallel.For anidados). El orden NO está garantizado y 'onChunk' se llama desde varios hilos a la vez:
    /// debe ser thread-safe (por ejemplo, escribir cada chunk en su propio sitio o encolarlo en una ConcurrentQueue).
    /// El BiomeRegion recibido es tuyo; no se reutiliza.
    /// Consejo: con HeightReadsAreThreadSafe = true no hay ninguna lectura serializada; con false, solo se serializa la
    /// lectura de alturas de cada chunk (lo demás sigue en paralelo).
    /// Tamaños de chunk de 64..256 funcionan bien: más grande = menos relectura del borde extra, más memoria por chunk.
    /// </summary>
    public void ForEachChunkParallel(int chunkSize, Action<int, int, BiomeRegion> onChunk,
        int maxDegreeOfParallelism = -1, CancellationToken cancellationToken = default)
    {
        if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
        if (onChunk == null) throw new ArgumentNullException(nameof(onChunk));

        GetChunkRange(chunkSize, out int minCx, out int maxCx, out int minCy, out int maxCy);
        EnsurePrepared();   // una sola vez, antes de lanzar los hilos

        int cw = maxCx - minCx + 1, ch = maxCy - minCy + 1;
        long totalChunks = (long)cw * ch;

        var options = new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism, CancellationToken = cancellationToken };
        Parallel.For(0L, totalChunks, options, i =>
        {
            int cx = minCx + (int)(i % cw);
            int cy = minCy + (int)(i / cw);
            var region = GenerateRegionCore(cx * chunkSize, cy * chunkSize, chunkSize, chunkSize, parallel: false);
            onChunk(cx, cy, region);
        });
    }

    private static int FloorDiv(int a, int b) => a >= 0 ? a / b : -((-a + b - 1) / b);

    /// <summary>Desplazamientos (dx,dy) con distancia &lt;= m, ordenados de más cerca a más lejos (orden estable).</summary>
    private (int dx, int dy, int d2)[] GetRingOffsets(int m)
    {
        lock (ringOffsetsCache)
        {
            if (ringOffsetsCache.TryGetValue(m, out var cached)) return cached;

            var list = new List<(int dx, int dy, int d2)>();
            for (int dy = -m; dy <= m; dy++)
                for (int dx = -m; dx <= m; dx++)
                {
                    int d2 = dx * dx + dy * dy;
                    if (d2 > 0 && d2 <= m * m) list.Add((dx, dy, d2));
                }

            list.Sort((a, b) => a.d2 != b.d2 ? a.d2.CompareTo(b.d2) : a.dy != b.dy ? a.dy.CompareTo(b.dy) : a.dx.CompareTo(b.dx));

            var arr = list.ToArray();
            ringOffsetsCache[m] = arr;
            return arr;
        }
    }

    // ============================================================
    //  DEBUG: imagen PNG de biomas + leyenda .txt
    // ============================================================
    /// <summary>
    /// Guarda un PNG con los biomas (cada bioma un color; los azules son biomas del nivel 0/agua) y, si
    /// <paramref name="markTransitions"/> es true, las celdas de transición de bioma en una versión más clara del color;
    /// con <paramref name="markHeightTransitions"/>, las de transición de altura en una versión más oscura (tiene prioridad).
    /// Con más de 64 biomas los colores se repiten.
    /// Escribe también "&lt;path&gt;.leyenda.txt" con el color y la proporción de cada bioma por nivel.
    /// Arriba-izquierda = (MinX, MaxY) si DebugYAxisUp (por defecto), o (MinX, MinY) si no. pixelsPerTile: 0 = automático (como BlackyWorldGeography.ExportDebugPng).
    /// En mundos grandes tarda (calcula todas las celdas, por franjas); es una ruta del sistema de archivos.
    /// </summary>
    public void ExportDebugPng(string path, int pixelsPerTile = 0, bool markTransitions = true, bool markHeightTransitions = true)
    {
        EnsurePrepared();

        var size = worldConfig.MapSize;
        int w = size.X, h = size.Y;
        int minX = MinX, minY = MinY;

        int maxSide = Math.Max(w, h);
        const int MaxImageSide = 8192;
        int scale = pixelsPerTile > 0 ? pixelsPerTile : Math.Max(1, 2048 / maxSide);
        scale = Math.Clamp(scale, 1, Math.Max(1, MaxImageSide / maxSide));
        int iw = w * scale, ih = h * scale;

        // Paleta: 0..63 colores de biomas, 64..127 versión clara (transición de bioma), 128..191 versión oscura (transición de altura)
        byte[] palette = new byte[256 * 3];
        int waterCount = 0, landCount = 0;
        var colors = new (byte r, byte g, byte b)[biomes.Count];
        for (int b = 0; b < biomes.Count; b++)
        {
            double hue, sat, val;
            if (isWaterBiome[b]) { hue = 0.56 + 0.05 * (waterCount % 3); sat = 0.65; val = 0.55 + 0.12 * (waterCount % 2); waterCount++; }
            else { hue = (landCount * 0.61803398875 + 0.08) % 1.0; sat = 0.50 + 0.25 * (landCount % 3) / 2.0; val = 0.78 - 0.14 * (landCount % 2); landCount++; }
            colors[b] = HsvToRgb(hue, sat, val);
        }

        for (int i = 0; i < 64; i++)
        {
            (byte r, byte g, byte b) c = colors.Length > 0 ? colors[i % colors.Length] : ((byte)0, (byte)0, (byte)0);
            palette[i * 3] = c.r; palette[i * 3 + 1] = c.g; palette[i * 3 + 2] = c.b;
            palette[(64 + i) * 3] = (byte)(c.r + (255 - c.r) * 55 / 100);
            palette[(64 + i) * 3 + 1] = (byte)(c.g + (255 - c.g) * 55 / 100);
            palette[(64 + i) * 3 + 2] = (byte)(c.b + (255 - c.b) * 55 / 100);
            palette[(128 + i) * 3] = (byte)(c.r * 40 / 100);
            palette[(128 + i) * 3 + 1] = (byte)(c.g * 40 / 100);
            palette[(128 + i) * 3 + 2] = (byte)(c.b * 40 / 100);
        }

        string dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        // Estadísticas: celdas por nivel y bioma, y cuántas están en transición
        long[,] perLevel = new long[LevelCount, biomes.Count];
        long[] transitions = new long[biomes.Count];
        long[,] heightPairs = new long[LevelCount, LevelCount];   // celdas en transición de altura: nivel propio -> nivel vecino

        const int StripRows = 64;
        BiomeRegion strip = null;
        int stripStart = -1, stripRows = 0;
        byte[] cache = new byte[iw];
        int cachedTy = -1;

        // Las celdas devuelven siempre las instancias de 'biomes', así que se indexan por referencia
        var indexOf = new Dictionary<BiomaData, int>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < biomes.Count; i++) indexOf[biomes[i]] = i;

        BlackyDebugPng.WriteIndexed(path, iw, ih, palette, (py, row) =>
        {
            int ty = py / scale;
            if (ty != cachedTy)
            {
                int iy = DebugYAxisUp ? h - 1 - ty : ty;    // fila del mundo (0 = MinY) que va en esta fila de la imagen

                if (strip == null || iy < stripStart || iy >= stripStart + stripRows)
                {
                    stripStart = DebugYAxisUp ? Math.Max(0, iy - StripRows + 1) : iy;
                    stripRows = Math.Min(StripRows, h - stripStart);
                    strip = GenerateRegion(minX, minY + stripStart, w, stripRows);
                }

                int sy = iy - stripStart;
                for (int tx = 0; tx < w; tx++)
                {
                    var cell = strip.Cells[(long)sy * w + tx];
                    int b = indexOf[cell.Bioma];
                    bool trans = markTransitions && cell.IsTransition;
                    bool hTrans = markHeightTransitions && cell.IsHeightTransition;
                    byte idx = (byte)((b % 64) + (hTrans ? 128 : trans ? 64 : 0));
                    for (int k = 0; k < scale; k++) cache[tx * scale + k] = idx;

                    perLevel[cell.HeightLevel, b]++;
                    if (cell.IsHeightTransition) heightPairs[cell.HeightLevel, cell.HeightNeighborLevel]++;
                    if (cell.IsTransition) transitions[b]++;
                }
                cachedTy = ty;
            }

            Buffer.BlockCopy(cache, 0, row, 1, iw);
        });

        // Leyenda
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        using var sw = new StreamWriter(Path.ChangeExtension(path, ".leyenda.txt"), false, new UTF8Encoding(false));

        sw.WriteLine("# LEYENDA DE BIOMAS (debug)");
        sw.WriteLine($"# Perfil geográfico: {template.perfilGeografico}   Mundo: {w}x{h}   BiomeFrequency: {biomeFrequency.ToString(inv)}   NoiseGridStep: {noiseGridStep}   TransitionWidth: {transitionWidth}   HeightTransitionWidth: {heightTransitionWidth}");
        sw.WriteLine("# Color claro en el PNG = celda en transición hacia otro bioma. Color oscuro = celda en transición de altura (tiene prioridad).");
        sw.WriteLine("# Superficie de transición: UN solo id por par de biomas (el del bioma de menor id).");
        sw.WriteLine($"# Orientación del PNG: {(DebugYAxisUp ? "Y+ hacia arriba (fila de arriba = MaxY)" : "Y+ hacia abajo (fila de arriba = MinY)")}");
        foreach (string wmsg in warnings) sw.WriteLine("# AVISO: " + wmsg);
        sw.WriteLine("#");
        sw.WriteLine("# Por nivel de altura: bioma | color | esperado | obtenido (sobre las celdas de ese nivel)");
        for (int level = 0; level < LevelCount; level++)
        {
            long levelTotal = 0;
            for (int b = 0; b < biomes.Count; b++) levelTotal += perLevel[level, b];
            if (levelTotal == 0) continue;

            sw.WriteLine($"Nivel {level}  ({levelTotal} celdas)");
            for (int k = 0; k < levelBiomeIdx[level].Length; k++)
            {
                int b = levelBiomeIdx[level][k];
                var c = colors[b];
                sw.WriteLine($"   {BiomeLabel(biomes[b])}  | #{c.r:X2}{c.g:X2}{c.b:X2}{(isWaterBiome[b] ? " (agua)" : "")} | esperado {levelWeights[level][k] * 100,5:0.0}% | obtenido {perLevel[level, b] * 100.0 / levelTotal,5:0.0}%");
            }
        }

        sw.WriteLine("#");
        sw.WriteLine("# Celdas en transición por bioma:");
        for (int b = 0; b < biomes.Count; b++)
        {
            long total = 0;
            for (int level = 0; level < LevelCount; level++) total += perLevel[level, b];
            sw.WriteLine($"   {BiomeLabel(biomes[b])}: {transitions[b]} de {total} celdas ({(total > 0 ? transitions[b] * 100.0 / total : 0):0.0}%)");
        }

        sw.WriteLine("#");
        sw.WriteLine("# Celdas en transición de altura (nivel propio -> nivel de la celda vecina más cercana):");
        for (int a = 0; a < LevelCount; a++)
            for (int c2 = 0; c2 < LevelCount; c2++)
                if (heightPairs[a, c2] > 0) sw.WriteLine($"   nivel {a} -> vecino {c2}: {heightPairs[a, c2]} celdas");
    }

    private static (byte r, byte g, byte b) HsvToRgb(double h, double s, double v)
    {
        double c = v * s, hp = h * 6.0, x = c * (1 - Math.Abs(hp % 2 - 1)), m = v - c;
        double r = 0, g = 0, b = 0;
        switch ((int)hp % 6)
        {
            case 0: r = c; g = x; break;
            case 1: r = x; g = c; break;
            case 2: g = c; b = x; break;
            case 3: g = x; b = c; break;
            case 4: r = x; b = c; break;
            default: r = c; b = x; break;
        }
        return ((byte)((r + m) * 255), (byte)((g + m) * 255), (byte)((b + m) * 255));
    }

    // ============================================================
    //  RUIDO PERLIN 2D con semilla + fBm (thread-safe una vez construido)
    // ============================================================
    private sealed class PerlinNoise
    {
        private readonly int[] _p = new int[512];

        public PerlinNoise(int seed)
        {
            var rng = new Random(seed);
            int[] perm = new int[256];
            for (int i = 0; i < 256; i++) perm[i] = i;
            for (int i = 255; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (perm[i], perm[j]) = (perm[j], perm[i]);
            }
            for (int i = 0; i < 512; i++) _p[i] = perm[i & 255];
        }

        public float Fbm(float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
        {
            float sum = 0f, amp = 1f, freq = 1f, norm = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Perlin(x * freq, y * freq) * amp;
                norm += amp;
                amp *= gain;
                freq *= lacunarity;
            }
            return Math.Clamp((sum / norm / 0.7071f + 1f) * 0.5f, 0f, 1f);
        }

        private float Perlin(float x, float y)
        {
            int xi = (int)MathF.Floor(x) & 255;
            int yi = (int)MathF.Floor(y) & 255;
            float xf = x - MathF.Floor(x);
            float yf = y - MathF.Floor(y);
            float u = Fade(xf), v = Fade(yf);
            int aa = _p[_p[xi] + yi];
            int ab = _p[_p[xi] + yi + 1];
            int ba = _p[_p[xi + 1] + yi];
            int bb = _p[_p[xi + 1] + yi + 1];
            float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
            float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);
            return Lerp(x1, x2, v);
        }

        private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static float Grad(int hash, float x, float y)
        {
            switch (hash & 7)
            {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                case 4: return x;
                case 5: return -x;
                case 6: return y;
                default: return -y;
            }
        }
    }
}

/// <summary>Escritor mínimo de PNG indexado (8 bits con paleta) para los exports de debug. Sin dependencias.</summary>
internal static class BlackyDebugPng
{
    /// <summary>fillRow rellena row[1..width] con los índices de paleta de la fila y (row[0] es el filtro PNG).</summary>
    public static void WriteIndexed(string path, int width, int height, byte[] paletteRgb, Action<int, byte[]> fillRow)
    {
        using var compressed = new MemoryStream();
        using (var z = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
        {
            byte[] row = new byte[width + 1];
            for (int y = 0; y < height; y++)
            {
                row[0] = 0;
                fillRow(y, row);
                z.Write(row, 0, row.Length);
            }
        }

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        fs.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, 0, 8);

        byte[] ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, width);
        WriteBigEndian(ihdr, 4, height);
        ihdr[8] = 8;    // bits por píxel
        ihdr[9] = 3;    // color indexado
        WriteChunk(fs, "IHDR", ihdr, ihdr.Length);
        WriteChunk(fs, "PLTE", paletteRgb, paletteRgb.Length);
        WriteChunk(fs, "IDAT", compressed.GetBuffer(), (int)compressed.Length);
        WriteChunk(fs, "IEND", new byte[0], 0);
    }

    private static void WriteBigEndian(byte[] buf, int offset, int value)
    {
        buf[offset] = (byte)(value >> 24);
        buf[offset + 1] = (byte)(value >> 16);
        buf[offset + 2] = (byte)(value >> 8);
        buf[offset + 3] = (byte)value;
    }

    private static void WriteChunk(Stream s, string type, byte[] data, int length)
    {
        byte[] head = new byte[8];
        WriteBigEndian(head, 0, length);
        for (int i = 0; i < 4; i++) head[4 + i] = (byte)type[i];
        s.Write(head, 0, 8);
        s.Write(data, 0, length);

        uint crc = 0xFFFFFFFFu;
        crc = Crc32Update(crc, head, 4, 4);
        crc = Crc32Update(crc, data, 0, length);
        crc ^= 0xFFFFFFFFu;

        byte[] tail = new byte[4];
        WriteBigEndian(tail, 0, unchecked((int)crc));
        s.Write(tail, 0, 4);
    }

    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var t = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            t[n] = c;
        }
        return t;
    }

    private static uint Crc32Update(uint crc, byte[] buf, int offset, int len)
    {
        for (int i = 0; i < len; i++)
            crc = CrcTable[(crc ^ buf[offset + i]) & 0xFF] ^ (crc >> 8);
        return crc;
    }
}