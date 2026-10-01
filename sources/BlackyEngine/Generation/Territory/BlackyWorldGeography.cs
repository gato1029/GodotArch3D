using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using static MessagePack.GeneratedMessagePackResolver.GodotEcsArch.sources.BlackyEngine;
// (los using de BlackyWorldConfig / BlackyWorldSeed / BlackyWorldHeight ya existen en tu proyecto)

public class BlackyWorldGeography
{
    private readonly BlackyWorldConfig worldConfig;
    private readonly ConfiguracionGeograficaData configuracionGeografica;
    private readonly BlackyWorldSeed worldSeed;
    private readonly BlackyWorldHeight heightMapWorld;

    /// <summary>Muestras usadas para calcular los cortes de altura en mapas grandes.</summary>
    private const long MuestrasObjetivo = 4_000_000;

    /// <summary>Lado máximo (en píxeles) de la imagen de debug.</summary>
    private const int MaxImageSide = 8192;

    /// <summary>
    /// Ajuste MANUAL del tamaño del detalle del relieve (el ruido), debe ser > 0. Se multiplica por la
    /// escala automática (ver AutoTerrainScale):
    ///   1   = sin cambios      2 = detalle el doble de ancho      0.5 = la mitad
    /// La frecuencia efectiva del ruido es Frecuencia / EffectiveTerrainScale.
    /// La FORMA (valle, cuenca, costa, meseta) siempre ocupa el mundo entero.
    /// </summary>
    public float TerrainScale { get; set; } = 1f;

    /// <summary>
    /// Si es true (por defecto), los perfiles CON forma (Valle, Cuenca, Meseta, Costera) escalan el detalle del
    /// ruido con el tamaño del mapa, para que la forma y el ruido mantengan siempre la misma proporción:
    /// un mapa de 4096 se ve como el de 512 ampliado, sin llenarse de islas y lagos pequeños.
    /// Los perfiles SIN forma (Montanosa, Llanura, Escarpada) no se escalan: un mapa mayor tiene más relieve.
    /// </summary>
    public bool AutoTerrainScale { get; set; } = true;

    /// <summary>Tamaño de mapa (lado mayor) para el que están pensadas las Frecuencia del struct.</summary>
    public int ReferenceMapSize { get; set; } = 512;

    /// <summary>Escala automática según el tamaño del mapa (1 si no aplica).</summary>
    public float AutoScaleFactor
    {
        get
        {
            if (!AutoTerrainScale || configuracionGeografica.Forma == TipoForma.Ninguna) return 1f;
            if (ReferenceMapSize <= 0)
                throw new InvalidOperationException("ReferenceMapSize debe ser mayor que 0");
            var size = worldConfig.MapSize;
            return Math.Max(size.X, size.Y) / (float)ReferenceMapSize;
        }
    }

    /// <summary>Escala total que se usa realmente: TerrainScale (manual) x AutoScaleFactor.</summary>
    public float EffectiveTerrainScale
    {
        get
        {
            if (TerrainScale <= 0f)
                throw new InvalidOperationException("TerrainScale debe ser mayor que 0");
            return TerrainScale * AutoScaleFactor;
        }
    }

    /// <summary>Semilla numérica derivada de worldSeed.GetRng("geography").</summary>
    public int GeographySeed => worldSeed.GetRng("geography").Next();

    // ------------------------------------------------------------
    //  Coordenadas del mundo: el centro es (0,0) y crece hacia ambos lados.
    //  Mapa de 512: X e Y van de -256 a 255 (256 tiles a cada lado del 0, el +256 no existe).
    //  Si tu mundo SÍ incluye el +256 (513 tiles), cambia aquí el cálculo de MinX/MinY.
    //  Y crece hacia abajo, igual que las filas del .txt y de la imagen (fila 0 = Y mínima).
    // ------------------------------------------------------------
    public int MinX => -(worldConfig.MapSize.X / 2);
    public int MinY => -(worldConfig.MapSize.Y / 2);
    public int MaxX => MinX + worldConfig.MapSize.X - 1;
    public int MaxY => MinY + worldConfig.MapSize.Y - 1;

    public BlackyWorldGeography(BlackyWorldConfig worldConfig, BlackyWorldSeed worldSeed, BlackyWorldHeight heightMapWorld, ConfiguracionGeograficaData configuracionGeografica)
    {
        this.worldConfig = worldConfig;
        this.configuracionGeografica = configuracionGeografica;
        this.worldSeed = worldSeed;
        this.heightMapWorld = heightMapWorld;
    }

    // ============================================================
    //  GENERACIÓN
    // ============================================================

    /// <summary>
    /// Genera la geografía de todo el mundo (niveles 0..AlturaMaxima) y la escribe en heightMapWorld
    /// con SetTopHeight, usando coordenadas de mundo (centro 0,0). Las proporciones de Altura0..3
    /// se cumplen sobre el mundo completo.
    ///
    /// Pasos: (1) cada celda recibe un valor = ruido mezclado con la forma del perfil;
    /// (2) los cortes por percentil convierten ese valor en nivel 0..3.
    /// </summary>
    public void Generate()
    {
        var size = worldConfig.MapSize;
        int w = size.X, h = size.Y;
        if (w <= 0 || h <= 0)
            throw new InvalidOperationException($"MapSize inválido: {w}x{h}");

        var cfg = configuracionGeografica;
        int minX = MinX, minY = MinY;
        float effectiveFreq = cfg.Frecuencia / EffectiveTerrainScale;   // valida TerrainScale y ReferenceMapSize
        var field = new TerrainField(cfg, GeographySeed, w, h, minX, minY, effectiveFreq);
        float[] weights = GetWeights(cfg);
        float[] cuts = ComputeCuts(field, w, h, weights);
        int maxLevel = cfg.AlturaMaxima;

        // 1) Valor -> nivel, en paralelo, sobre un buffer temporal (1 byte por celda).
        //    Aquí x e y son ÍNDICES (0..w-1, 0..h-1); la conversión a coordenadas de mundo es minX + x.
        byte[] levels = new byte[(long)w * h];
        Parallel.For(0, h, y =>
        {
            long row = (long)y * w;
            for (int x = 0; x < w; x++)
            {
                float v = field.Evaluate(x, y);
                int level = v < cuts[0] ? 0 : v < cuts[1] ? 1 : v < cuts[2] ? 2 : 3;
                levels[row + x] = (byte)Math.Min(level, maxLevel);
            }
        });

        // 2) Se vuelca al mapa de alturas con coordenadas de mundo (en serie, por si no es thread-safe)
        for (int y = 0; y < h; y++)
        {
            long row = (long)y * w;
            int worldY = minY + y;
            for (int x = 0; x < w; x++)
                heightMapWorld.SetTopHeight(minX + x, worldY, levels[row + x]);
        }
    }

    /// <summary>
    /// Calcula los 3 cortes de valor (nivel 0|1, 1|2, 2|3) a partir de una muestra regular del mapa,
    /// de forma que las proporciones Altura0..3 se cumplan sobre el mundo completo.
    /// En mapas de hasta ~4M de celdas la muestra es el mapa entero (equivale a ordenar todo).
    /// </summary>
    private static float[] ComputeCuts(TerrainField field, int w, int h, float[] weights)
    {
        long total = (long)w * h;
        int s = Math.Max(1, (int)Math.Ceiling(Math.Sqrt((double)total / MuestrasObjetivo)));
        int sw = (w + s - 1) / s, sh = (h + s - 1) / s;

        float[] sample = new float[(long)sw * sh];
        Parallel.For(0, sh, j =>
        {
            for (int i = 0; i < sw; i++)
                sample[j * sw + i] = field.Evaluate(i * s, j * s);
        });
        Array.Sort(sample);

        float[] cum = { weights[0], weights[0] + weights[1], weights[0] + weights[1] + weights[2] };
        float[] cuts = new float[3];
        for (int k = 0; k < 3; k++)
            cuts[k] = cum[k] >= 0.9999f
                ? float.PositiveInfinity
                : sample[Math.Min(sample.Length - 1, (int)(cum[k] * sample.Length))];
        return cuts;
    }

    /// <summary>Proporciones normalizadas (suman 1) y respetando AlturaMaxima.</summary>
    private static float[] GetWeights(ConfiguracionGeograficaData c)
    {
        float[] p = { c.Altura0, c.Altura1, c.Altura2, c.Altura3 };
        for (int i = c.AlturaMaxima + 1; i < 4; i++) p[i] = 0f;
        float sum = p[0] + p[1] + p[2] + p[3];
        if (sum <= 0f) { p[0] = 1f; return p; }
        for (int i = 0; i < 4; i++) p[i] /= sum;
        return p;
    }

    // ============================================================
    //  CAMPO DE TERRENO: ruido + forma  (thread-safe una vez construido)
    // ============================================================

    /// <summary>
    /// Valor del terreno en cada celda (más alto = más elevado), antes de convertirlo en niveles:
    ///     valor = (1 - FormaPeso) * ruido + FormaPeso * forma(u, v)
    /// - El ruido se calcula con coordenadas de MUNDO (centro 0,0).
    /// - (u, v) son coordenadas normalizadas 0..1 de la celda dentro del mapa (la forma ocupa todo el mundo).
    /// Los valores solo importan por su ORDEN (los cortes son por percentil).
    /// </summary>
    private sealed class TerrainField
    {
        private readonly PerlinNoise noise;
        private readonly TipoForma shape;
        private readonly float freq;
        private readonly float shapeWeight;
        private readonly float irregularity;       // = Sinuosidad
        private readonly int minX, minY;
        private readonly float invW, invH;

        // Costera: dirección hacia el agua
        private readonly float coastDx, coastDy, coastInv;

        // Valle: eje rotado + tabla del serpenteo a lo largo del eje
        private readonly float valleyCos, valleySin, valleyHalf;
        private readonly float[] valleyOffsets;
        private const int ValleyTableSize = 2048;
        private const float ValleyTableRange = 0.75f;   // t en [-0.75, 0.75] cubre cualquier orientación

        // Cuenca / Meseta: elipse con centro desplazado y borde irregular
        private readonly PerlinNoise rimNoise;
        private readonly float centerU, centerV;
        private readonly float ellipseCos, ellipseSin, ellipseInvRatio;
        private readonly float ellipseInvMax;

        public TerrainField(ConfiguracionGeograficaData cfg, int seed, int w, int h, int minX, int minY, float frequency)
        {
            noise = new PerlinNoise(seed);
            shape = cfg.Forma;
            shapeWeight = Math.Clamp(cfg.FormaPeso, 0f, 1f);
            irregularity = cfg.Sinuosidad;
            freq = frequency;
            this.minX = minX;
            this.minY = minY;
            invW = 1f / w;
            invH = 1f / h;

            if (shape == TipoForma.Costera)
            {
                float angle = CoastWaterAngle(seed);
                coastDx = MathF.Cos(angle);
                coastDy = MathF.Sin(angle);                       // y crece hacia abajo
                coastInv = 1f / (MathF.Abs(coastDx) + MathF.Abs(coastDy));
            }
            else if (shape == TipoForma.Valle)
            {
                float angle = ValleyAngle(seed);
                valleyCos = MathF.Cos(angle);
                valleySin = MathF.Sin(angle);
                valleyHalf = (MathF.Abs(valleyCos) + MathF.Abs(valleySin)) * 0.5f;

                // El eje serpentea: desplazamiento lateral según un ruido lento a lo largo del eje.
                var axisNoise = new PerlinNoise(seed ^ 0x5F3759DF);
                valleyOffsets = new float[ValleyTableSize];
                for (int i = 0; i < ValleyTableSize; i++)
                {
                    float t = -ValleyTableRange + 2f * ValleyTableRange * i / (ValleyTableSize - 1);
                    float dev = Math.Clamp((axisNoise.Fbm(0.37f, (t + 0.5f) * 2f) - 0.5f) * 4f, -1f, 1f); // ~[-1, 1]
                    valleyOffsets[i] = irregularity * dev;
                }
            }
            else if (shape == TipoForma.Cuenca || shape == TipoForma.Meseta)
            {
                rimNoise = new PerlinNoise(seed ^ 0x6A09E667);
                ShapeEllipse(seed, irregularity, out centerU, out centerV, out float angle, out float ratio);
                ellipseCos = MathF.Cos(angle);
                ellipseSin = MathF.Sin(angle);
                ellipseInvRatio = 1f / ratio;

                // Distancia (elíptica) al rincón más lejano: normaliza r a 0..1
                float max = 0f;
                for (int cu = 0; cu <= 1; cu++)
                    for (int cv = 0; cv <= 1; cv++)
                        max = MathF.Max(max, EllipseDistance(cu - centerU, cv - centerV));
                ellipseInvMax = 1f / max;
            }
        }

        /// <summary>x, y son ÍNDICES de celda (0..w-1, 0..h-1).</summary>
        public float Evaluate(int x, int y)
        {
            float n = noise.Fbm((x + minX) * freq, (y + minY) * freq);
            if (shape == TipoForma.Ninguna || shapeWeight <= 0f) return n;

            float u = (x + 0.5f) * invW;
            float v = (y + 0.5f) * invH;
            float s;

            switch (shape)
            {
                case TipoForma.Valle:
                    {
                        // t = posición a lo largo del eje, p = distancia perpendicular al eje (con signo)
                        float a = u - 0.5f, b = v - 0.5f;
                        float t = a * valleyCos + b * valleySin;
                        float p = -a * valleySin + b * valleyCos;
                        s = Math.Clamp(MathF.Abs(p - SampleValleyOffset(t)) / valleyHalf, 0f, 1f); // 0 en el eje
                        break;
                    }

                case TipoForma.Cuenca:
                    s = RimRadius(u, v);                 // 0 en el centro, 1 en el borde
                    break;

                case TipoForma.Meseta:
                    s = 1f - RimRadius(u, v);            // alto en el centro, baja hacia el borde
                    break;

                case TipoForma.Costera:
                    // 0 en el lado del agua (borde o esquina según la semilla), sube hacia el interior
                    s = Math.Clamp(0.5f - ((u - 0.5f) * coastDx + (v - 0.5f) * coastDy) * coastInv, 0f, 1f);
                    break;

                default:
                    return n;
            }

            return (1f - shapeWeight) * n + shapeWeight * s;
        }

        private float SampleValleyOffset(float t)
        {
            float f = (t + ValleyTableRange) / (2f * ValleyTableRange) * (ValleyTableSize - 1);
            f = Math.Clamp(f, 0f, ValleyTableSize - 1.001f);
            int i = (int)f;
            float k = f - i;
            return valleyOffsets[i] + (valleyOffsets[i + 1] - valleyOffsets[i]) * k;
        }

        /// <summary>Distancia elíptica normalizada (0..1) al centro, con el borde deformado por ruido.</summary>
        private float RimRadius(float u, float v)
        {
            float r = EllipseDistance(u - centerU, v - centerV) * ellipseInvMax;
            float warp = Math.Clamp((rimNoise.Fbm(u * 3f + 11.3f, v * 3f + 7.7f) - 0.5f) * 4f, -1f, 1f);
            return Math.Clamp(r + irregularity * warp, 0f, 1f);
        }

        private float EllipseDistance(float du, float dv)
        {
            float major = du * ellipseCos + dv * ellipseSin;
            float minor = (-du * ellipseSin + dv * ellipseCos) * ellipseInvRatio;
            return MathF.Sqrt(major * major + minor * minor);
        }

        // ---- Parámetros derivados de la semilla (también los usa el debug) ----

        /// <summary>Ángulo (radianes) de la dirección hacia el agua.</summary>
        public static float CoastWaterAngle(int seed)
            => (float)(new Random(seed ^ 0x1B873593).NextDouble() * 2.0 * Math.PI);

        /// <summary>Ángulo (radianes, 0..π) del eje del valle.</summary>
        public static float ValleyAngle(int seed)
            => (float)(new Random(seed ^ 0x2C1B3C6D).NextDouble() * Math.PI);

        /// <summary>Centro (0..1), ángulo (rad) y proporción menor/mayor (0.6..1) de la elipse de Cuenca/Meseta.</summary>
        public static void ShapeEllipse(int seed, float irregularity, out float centerU, out float centerV, out float angle, out float ratio)
        {
            var r = new Random(seed ^ 0x3C6EF372);
            centerU = 0.5f + irregularity * 0.5f * (float)(r.NextDouble() * 2.0 - 1.0);
            centerV = 0.5f + irregularity * 0.5f * (float)(r.NextDouble() * 2.0 - 1.0);
            angle = (float)(r.NextDouble() * Math.PI);
            ratio = 0.6f + 0.4f * (float)r.NextDouble();
        }
    }

    // ============================================================
    //  DEBUG: volcado a .txt (lee lo que realmente hay en heightMapWorld)
    // ============================================================

    /// <summary>
    /// Escribe el mapa de alturas en un .txt (un dígito por celda) con cabecera y estadísticas.
    /// <paramref name="step"/>: se escribe 1 celda de cada 'step' (0 = automático, máx. ~1024 columnas).
    /// Usa 1 para volcar el mapa completo (a 8192x8192 el archivo pesa ~67 MB).
    /// Primera fila = Y mínima (arriba); primera columna = X mínima (izquierda).
    /// Ojo: es una ruta del sistema de archivos, no "res://" ni "user://".
    /// </summary>
    public void ExportDebugTxt(string path, int step = 0)
    {
        var size = worldConfig.MapSize;
        int w = size.X, h = size.Y;
        int minX = MinX, minY = MinY;
        if (step <= 0) step = Math.Max(1, (int)Math.Ceiling(Math.Max(w, h) / 1024.0));

        var cfg = configuracionGeografica;
        float[] expected = GetWeights(cfg);
        var counts = new Dictionary<int, long>();
        long sampled = 0;
        int cols = (w + step - 1) / step, rows = (h + step - 1) / step;

        string dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        // Primero el mapa a un buffer de texto (para poder poner las estadísticas en la cabecera)
        var map = new StringBuilder((cols + 1) * rows);
        for (int y = 0; y < h; y += step)
        {
            for (int x = 0; x < w; x += step)
            {
                int v = (int)heightMapWorld.GetTopHeight(minX + x, minY + y);
                counts[v] = counts.TryGetValue(v, out long c) ? c + 1 : 1;
                sampled++;
                map.Append(v >= 0 && v <= 9 ? (char)('0' + v) : '?');
            }
            map.Append('\n');
        }

        var inv = System.Globalization.CultureInfo.InvariantCulture;
        int seed = GeographySeed;
        using var sw = new StreamWriter(path, false, new UTF8Encoding(false));
        sw.WriteLine("# MAPA DE ALTURAS - DEBUG GEOGRAFIA");
        sw.WriteLine($"# Fecha:        {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sw.WriteLine($"# Perfil:       {cfg.Perfil}");
        sw.WriteLine($"# Semilla:      {seed}");
        sw.WriteLine($"# Mundo:        {w} x {h} celdas");
        sw.WriteLine($"# Coordenadas:  X de {MinX} a {MaxX}, Y de {MinY} a {MaxY} (centro 0,0; Y crece hacia abajo)");
        sw.WriteLine($"# Volcado:      1 de cada {step} celdas -> {cols} x {rows}");
        float autoFactor = AutoScaleFactor, effScale = EffectiveTerrainScale;
        sw.WriteLine($"# TerrainScale: {TerrainScale.ToString(inv)} (manual) x {autoFactor.ToString("0.###", inv)} (auto{(autoFactor == 1f ? ", no aplica o desactivada" : $", mapa {Math.Max(w, h)} / referencia {ReferenceMapSize}")}) = {effScale.ToString("0.###", inv)}");
        sw.WriteLine($"# Frecuencia:   {cfg.Frecuencia.ToString(inv)}  (efectiva {(cfg.Frecuencia / effScale).ToString("0.#####", inv)})");
        sw.WriteLine($"# Forma:        {cfg.Forma}  peso {cfg.FormaPeso.ToString(inv)}  sinuosidad {cfg.Sinuosidad.ToString(inv)}");
        switch (cfg.Forma)
        {
            case TipoForma.Costera:
                sw.WriteLine($"# Agua hacia:   {DescribeWaterSide(TerrainField.CoastWaterAngle(seed))}");
                break;
            case TipoForma.Valle:
                sw.WriteLine($"# Eje valle:    {DescribeValley(TerrainField.ValleyAngle(seed))}");
                break;
            case TipoForma.Cuenca:
            case TipoForma.Meseta:
                TerrainField.ShapeEllipse(seed, cfg.Sinuosidad, out float cu, out float cv, out float ang, out float ratio);
                int cx = MinX + (int)(cu * w), cy = MinY + (int)(cv * h);
                sw.WriteLine($"# Centro:       ({cx}, {cy}) en coordenadas de mundo");
                sw.WriteLine($"# Elipse:       proporción {ratio.ToString("0.00", inv)}, eje mayor a {(ang * 180.0 / Math.PI).ToString("0", inv)}°");
                break;
        }
        sw.WriteLine($"# AlturaMaxima: {cfg.AlturaMaxima}");
        sw.WriteLine($"# Proporciones: A0={cfg.Altura0.ToString(inv)} A1={cfg.Altura1.ToString(inv)} A2={cfg.Altura2.ToString(inv)} A3={cfg.Altura3.ToString(inv)}");
        sw.WriteLine("#");
        sw.WriteLine("# --- ESTADÍSTICAS (sobre las celdas volcadas) ---");
        sw.WriteLine("# Nivel | Esperado | Obtenido | Celdas");
        for (int i = 0; i < 4; i++)
        {
            counts.TryGetValue(i, out long n);
            sw.WriteLine($"#   {i}   | {expected[i] * 100,6:0.0}%  | {(sampled > 0 ? n * 100.0 / sampled : 0),6:0.0}%  | {n}");
        }
        foreach (var kv in counts)
            if (kv.Key < 0 || kv.Key > 3)
                sw.WriteLine($"# (fuera de 0..3) altura {kv.Key}: {kv.Value} celdas");
        sw.WriteLine("#");
        sw.WriteLine("# --- MAPA (una fila por línea, Y mínima arriba) ---");
        sw.Write(map.ToString());
    }

    private static string DescribeWaterSide(float angleRad)
    {
        string[] names = { "este (derecha)", "sureste (abajo-derecha)", "sur (abajo)", "suroeste (abajo-izquierda)",
                           "oeste (izquierda)", "noroeste (arriba-izquierda)", "norte (arriba)", "noreste (arriba-derecha)" };
        double deg = angleRad * 180.0 / Math.PI;
        int idx = ((int)Math.Round(deg / 45.0)) % 8;
        return $"{names[idx]}  (ángulo {deg:0}°)";
    }

    private static string DescribeValley(float angleRad)
    {
        double deg = angleRad * 180.0 / Math.PI;     // 0..180, medido desde el eje X hacia abajo
        string kind = deg < 15 || deg > 165 ? "horizontal"
                    : Math.Abs(deg - 90) < 15 ? "vertical"
                    : deg < 90 ? "diagonal (de arriba-izquierda a abajo-derecha)"
                               : "diagonal (de abajo-izquierda a arriba-derecha)";
        return $"{kind}  (ángulo {deg:0}°)";
    }

    // ============================================================
    //  DEBUG: imagen PNG de la geografía (1 píxel por tile, o ampliada si el mapa es pequeño)
    // ============================================================

    /// <summary>
    /// Guarda la geografía como imagen PNG de colores (hasta 8192x8192 píxeles). Lee heightMapWorld.
    /// Colores: 0 azul, 1 verde, 2 ocre, 3 marrón; otra altura = magenta.
    /// Arriba-izquierda = (MinX, MinY). El centro del mundo (0,0) está en el centro de la imagen.
    /// <paramref name="pixelsPerTile"/>: 0 = automático (los mapas pequeños se amplían hasta ~2048 px).
    ///   Si el mapa es mayor que 8192 se reduce tomando 1 tile de cada N.
    /// Es una ruta del sistema de archivos, no "res://" ni "user://".
    /// </summary>
    public void ExportDebugPng(string path, int pixelsPerTile = 0)
    {
        var size = worldConfig.MapSize;
        int w = size.X, h = size.Y;
        int minX = MinX, minY = MinY;
        int maxSide = Math.Max(w, h);

        int step = 1, scale = 1;
        if (maxSide > MaxImageSide)
            step = (maxSide + MaxImageSide - 1) / MaxImageSide;
        else
        {
            scale = pixelsPerTile > 0 ? pixelsPerTile : Math.Max(1, 2048 / maxSide);
            scale = Math.Clamp(scale, 1, Math.Max(1, MaxImageSide / maxSide));
        }

        int tilesX = (w + step - 1) / step, tilesY = (h + step - 1) / step;
        int iw = tilesX * scale, ih = tilesY * scale;

        byte[] palette =
        {
             46,  94, 140,   // 0 azul             
            194, 176,  90,   // 1 ocre
            94, 158,  78,   // 2 verde
            140, 106,  75,   // 3 marrón
            255,   0, 255    // otra altura
        };

        string dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        byte[] cache = new byte[iw];
        int cachedTileRow = -1;

        WriteIndexedPng(path, iw, ih, palette, (py, row) =>
        {
            int ty = py / scale;
            if (ty != cachedTileRow)   // las filas repetidas por la ampliación no vuelven a leer el mapa
            {
                int worldY = minY + ty * step;
                for (int tx = 0; tx < tilesX; tx++)
                {
                    int v = (int)heightMapWorld.GetTopHeight(minX + tx * step, worldY);
                    byte idx = (byte)(v >= 0 && v <= 3 ? v : 4);
                    int px = tx * scale;
                    for (int k = 0; k < scale; k++) cache[px + k] = idx;
                }
                cachedTileRow = ty;
            }
            Buffer.BlockCopy(cache, 0, row, 1, iw);   // row[0] es el byte de filtro PNG
        });
    }

    /// <summary>PNG de 8 bits con paleta. fillRow rellena row[1..width] con los índices de la fila y.</summary>
    private static void WriteIndexedPng(string path, int width, int height, byte[] paletteRgb, Action<int, byte[]> fillRow)
    {
        using var compressed = new MemoryStream();
        using (var z = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
        {
            byte[] row = new byte[width + 1];
            for (int y = 0; y < height; y++)
            {
                row[0] = 0;   // filtro "None"
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
        ihdr[9] = 3;    // color indexado (paleta)
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

        /// <summary>fBm normalizado a ~[0,1].</summary>
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