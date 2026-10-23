using GodotEcsArch.sources.BlackyEngine.Core;
using GodotEcsArch.sources.BlackyEngine.Generation.Procedural;
using GodotEcsArch.sources.BlackyEngine.Services.Paint;
using GodotEcsArch.sources.WindowsDataBase.WorldGameProfile;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

// (los using de BlackyWorldConfig / BlackyWorldSeed / BlackyWorldHeight ya existen en tu proyecto)

/// <summary>
/// Geografía "de arriba hacia abajo" para juegos top down: el terreno es más alto en un extremo vertical
/// del mapa y más bajo en el opuesto. Niveles: 0 agua, 1 costa, 2..4 tierra.
/// Usa Altura0..4, AlturaMaxima, Frecuencia, FormaPeso y Sinuosidad de ConfiguracionGeograficaData
/// (la propiedad Forma se ignora).
///   FormaPeso  = cuánto manda el gradiente vertical frente al ruido (0 = solo ruido, 1 = franjas casi rectas)
///   Sinuosidad = cuánto se deforman/ondulan las franjas (0 = rectas)
/// Requiere que BlackyWorldGeography.PerlinNoise sea 'internal'.
/// </summary>
public class BlackyWorldGeographyVertical
{
    private readonly BlackyWorldConfig worldConfig;
    private readonly ConfiguracionGeograficaData cfg;
    private readonly BlackyWorldSeed worldSeed;
    private readonly BlackyWorldHeight heightMapWorld;

    private const long MuestrasObjetivo = 4_000_000;

    /// <summary>Lado máximo (en píxeles) de la imagen de debug.</summary>
    private const int MaxImageSide = 8192;

    /// <summary>Niveles de altura: 0 agua, 1 costa, 2..4 tierra.</summary>
    private const int Niveles = ConfiguracionGeograficaData.NivelesTotales;

    /// <summary>true = la altura máxima está en Y positiva (MaxY); false = en Y negativa (MinY).</summary>
    public bool HighAtPositiveY { get; set; } = true;

    /// <summary>
    /// Distancia en tiles desde el borde izquierdo (MinX) dentro de la cual se fuerza altura 0.
    /// Los cambios de altura (0→1, 1→2, etc.) solo pueden aparecer a partir de esta distancia.
    /// 0 = sin margen.
    /// </summary>
    public int BorderMarginLeft { get; set; } = 16;

    /// <summary>Distancia en tiles desde el borde derecho (MaxX) donde se fuerza altura 0.</summary>
    public int BorderMarginRight { get; set; } = 16;

    /// <summary>Distancia en tiles desde el borde superior (MaxY) donde se fuerza altura 0.</summary>
    public int BorderMarginTop { get; set; } = 16;

    /// <summary>Distancia en tiles desde el borde inferior (MinY) donde se fuerza altura 0.</summary>
    public int BorderMarginBottom { get; set; } = 16;

    /// <summary>
    /// Margen en tiles entre la altura 1 y la altura 2.
    /// Las celdas de altura 2 que estén a menos de esta distancia de una celda de altura 1
    /// pasan a ser altura 1, para que la altura 2 no caiga justo sobre el borde de la altura 1.
    /// 0 = sin margen.
    /// </summary>
    public int Height1To2Margin { get; set; } = 8;

    /// <summary>Ajuste MANUAL del tamaño del detalle del ruido (debe ser > 0).</summary>
    public float TerrainScale { get; set; } = 1f;

    /// <summary>Si es true, el detalle del ruido escala con el tamaño del mapa respecto a ReferenceMapSize.</summary>
    public bool AutoTerrainScale { get; set; } = true;

    /// <summary>Tamaño de mapa (lado mayor) para el que están pensadas las Frecuencia del struct.</summary>
    public int ReferenceMapSize { get; set; } = 512;

    /// <summary>
    /// Solo afecta al DEBUG (.png), no a los datos generados: true = Y+ hacia arriba (la fila de arriba es
    /// MaxY); false = Y+ hacia abajo (la fila de arriba es MinY).
    /// </summary>
    public bool DebugYAxisUp { get; set; } = true;

    public int MinX => -(worldConfig.MapSize.X / 2);
    public int MinY => -(worldConfig.MapSize.Y / 2);
    public int MaxX => MinX + worldConfig.MapSize.X - 1;
    public int MaxY => MinY + worldConfig.MapSize.Y - 1;

    public int GeographySeed => worldSeed.GetRng("geography").Next();

    public BlackyWorldGeographyVertical(BlackyWorldConfig worldConfig, BlackyWorldSeed worldSeed,
        BlackyWorldHeight heightMapWorld, ConfiguracionGeograficaData cfg)
    {
        this.worldConfig = worldConfig;
        this.worldSeed = worldSeed;
        this.heightMapWorld = heightMapWorld;
        this.cfg = cfg;
    }

    // ============================================================
    //  GENERACIÓN
    // ============================================================

    public void Generate()
    {
        var size = worldConfig.MapSize;
        int w = size.X, h = size.Y;
        if (w <= 0 || h <= 0) throw new InvalidOperationException($"MapSize inválido: {w}x{h}");
        if (TerrainScale <= 0f) throw new InvalidOperationException("TerrainScale debe ser mayor que 0");
        if (ReferenceMapSize <= 0) throw new InvalidOperationException("ReferenceMapSize debe ser mayor que 0");

        float auto = AutoTerrainScale ? Math.Max(w, h) / (float)ReferenceMapSize : 1f;
        float freq = cfg.Frecuencia / (TerrainScale * auto);

        var field = new VerticalField(cfg, GeographySeed, w, h, MinX, MinY, freq, HighAtPositiveY);
        float[] cuts = ComputeCuts(field, w, h, GetWeights(cfg));
        int maxLevel = cfg.AlturaMaxima;
        int marginLeft = Math.Clamp(BorderMarginLeft, 0, w);
        int marginRight = Math.Clamp(BorderMarginRight, 0, w);
        int marginBottom = Math.Clamp(BorderMarginBottom, 0, h);
        int marginTop = Math.Clamp(BorderMarginTop, 0, h);

        byte[] levels = new byte[(long)w * h];
        Parallel.For(0, h, y =>
        {
            long row = (long)y * w;
            for (int x = 0; x < w; x++)
            {
                float v = field.Evaluate(x, y);
                int level = 0;
                while (level < cuts.Length && v >= cuts[level]) level++;
                level = Math.Min(level, maxLevel);

                // En los bordes del mapa no puede haber cambios de altura:
                // se mantiene altura 0 dentro de cada margen independiente.
                if (x < marginLeft || x >= w - marginRight ||
                    y < marginBottom || y >= h - marginTop)
                {
                    level = 0;
                }

                levels[row + x] = (byte)level;
            }
        });

        if (maxLevel >= 2)
            ApplyHeight1To2Margin(levels, w, h, Height1To2Margin);

        int minX = MinX, minY = MinY;
        for (int y = 0; y < h; y++)
        {
            long row = (long)y * w;
            for (int x = 0; x < w; x++)
                heightMapWorld.SetTopHeight(minX + x, minY + y, levels[row + x]);
        }
    }

    /// <summary>
    /// Erosiona la altura 2 contra la altura 1: cada pasada convierte en altura 1 las celdas
    /// de altura 2 que tocan una celda de altura 1. Repitiendo 'margin' pasadas queda una
    /// banda de altura 1 de ese ancho antes de empezar la altura 2.
    /// </summary>
    private static void ApplyHeight1To2Margin(byte[] levels, int w, int h, int margin)
    {
        margin = Math.Clamp(margin, 0, Math.Min(w, h) / 2);
        if (margin <= 0 || w <= 0 || h <= 0) return;

        byte[] current = levels;
        byte[] next = new byte[levels.Length];
        bool[] changedRows = new bool[h];

        for (int pass = 0; pass < margin; pass++)
        {
            Array.Copy(current, next, current.Length);
            Array.Clear(changedRows, 0, changedRows.Length);

            Parallel.For(0, h, y =>
            {
                bool changed = false;
                int row = y * w;

                for (int x = 0; x < w; x++)
                {
                    int index = row + x;
                    if (current[index] != 2) continue;

                    if (HasNeighborLevel1(current, w, h, x, y))
                    {
                        next[index] = 1;
                        changed = true;
                    }
                }

                changedRows[y] = changed;
            });

            bool anyChanged = false;
            for (int i = 0; i < changedRows.Length; i++)
            {
                if (changedRows[i])
                {
                    anyChanged = true;
                    break;
                }
            }

            (current, next) = (next, current);

            if (!anyChanged) break;
        }

        if (!ReferenceEquals(current, levels))
            Array.Copy(current, levels, levels.Length);
    }

    private static bool HasNeighborLevel1(byte[] levels, int w, int h, int x, int y)
    {
        int x0 = Math.Max(0, x - 1);
        int x1 = Math.Min(w - 1, x + 1);
        int y0 = Math.Max(0, y - 1);
        int y1 = Math.Min(h - 1, y + 1);

        for (int ny = y0; ny <= y1; ny++)
        {
            int row = ny * w;
            for (int nx = x0; nx <= x1; nx++)
            {
                if (nx == x && ny == y) continue;
                if (levels[row + nx] == 1) return true;
            }
        }

        return false;
    }

    private static float[] ComputeCuts(VerticalField field, int w, int h, float[] weights)
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

        float[] cuts = new float[weights.Length - 1];
        float acc = 0f;
        for (int k = 0; k < cuts.Length; k++)
        {
            acc += weights[k];
            cuts[k] = acc >= 0.9999f
                ? float.PositiveInfinity
                : sample[Math.Min(sample.Length - 1, (int)(acc * sample.Length))];
        }
        return cuts;
    }

    private static float[] GetWeights(ConfiguracionGeograficaData c)
    {
        float[] p = { c.Altura0, c.Altura1, c.Altura2, c.Altura3, c.Altura4 };
        for (int i = c.AlturaMaxima + 1; i < p.Length; i++) p[i] = 0f;
        float sum = 0f;
        foreach (float v in p) sum += v;
        if (sum <= 0f) { p[0] = 1f; return p; }
        for (int i = 0; i < p.Length; i++) p[i] /= sum;
        return p;
    }

    // ============================================================
    //  CAMPO DE TERRENO: ruido + gradiente vertical (thread-safe una vez construido)
    // ============================================================

    /// <summary>valor = (1 - peso) * ruido + peso * gradiente vertical ondulado.</summary>
    private sealed class VerticalField
    {
        private readonly BlackyWorldGeography.PerlinNoise noise;
        private readonly BlackyWorldGeography.PerlinNoise warpNoise;
        private readonly float freq, shapeWeight, irregularity;
        private readonly int minX, minY;
        private readonly float invW, invH;
        private readonly bool highAtPositiveY;

        public VerticalField(ConfiguracionGeograficaData cfg, int seed, int w, int h,
            int minX, int minY, float frequency, bool highAtPositiveY)
        {
            noise = new BlackyWorldGeography.PerlinNoise(seed);
            warpNoise = new BlackyWorldGeography.PerlinNoise(seed ^ 0x6A09E667);
            shapeWeight = Math.Clamp(cfg.FormaPeso, 0f, 1f);
            irregularity = cfg.Sinuosidad;
            freq = frequency;
            this.minX = minX;
            this.minY = minY;
            this.highAtPositiveY = highAtPositiveY;
            invW = 1f / w;
            invH = 1f / h;
        }

        /// <summary>x, y son ÍNDICES de celda (0..w-1, 0..h-1); y crece con la Y de mundo.</summary>
        public float Evaluate(int x, int y)
        {
            float n = noise.Fbm((x + minX) * freq, (y + minY) * freq);
            if (shapeWeight <= 0f) return n;

            float u = (x + 0.5f) * invW;
            float v = (y + 0.5f) * invH;               // 0 en MinY ... 1 en MaxY

            float s = highAtPositiveY ? v : 1f - v;    // 1 = lo más alto
            // Ondulación de las franjas (lenta, independiente del detalle del ruido)
            float warp = Math.Clamp((warpNoise.Fbm(u * 3f + 5.1f, v * 3f + 9.7f) - 0.5f) * 4f, -1f, 1f);
            s = Math.Clamp(s + irregularity * 0.5f * warp, 0f, 1f);

            return (1f - shapeWeight) * n + shapeWeight * s;
        }
    }

    // ============================================================
    //  DEBUG: imagen PNG de la geografía
    // ============================================================

    /// <summary>
    /// Guarda la geografía como imagen PNG de colores (hasta 8192x8192 píxeles). Lee heightMapWorld.
    /// Colores: 0 agua (azul), 1 costa (arena), 2 verde, 3 ocre, 4 marrón; otra altura = magenta.
    /// Arriba-izquierda = (MinX, MaxY) si DebugYAxisUp (por defecto), o (MinX, MinY) si no.
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
             46,  94, 140,   // 0 agua (azul)
            230, 214, 150,   // 1 costa (arena)
             94, 158,  78,   // 2 verde
            176, 150,  80,   // 3 ocre
            120,  90,  65,   // 4 marrón
            255,   0, 255    // otra altura (magenta)
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
                int worldY = DebugYAxisUp ? (minY + h - 1) - ty * step : minY + ty * step;
                for (int tx = 0; tx < tilesX; tx++)
                {
                    int v = (int)heightMapWorld.GetTopHeight(minX + tx * step, worldY);
                    byte idx = (byte)(v >= 0 && v < Niveles ? v : Niveles);
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
}